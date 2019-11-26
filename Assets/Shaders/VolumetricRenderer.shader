Shader "Custom/VolumetricRenderer"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				float3 viewDir: TEXCOORD1;
            };

			struct ray
			{
				float3 origin;
				float3 direction;
			};

			/*float2 AABBRayIntersection(float3 bndMin, float3 bndMax, float3 origin, float3 direction)
			{
				float t0 = (bndMin - origin) / direction;
				float t1 = (bndMax - origin) / direction;

				float3 tmin = min(t0, t1);
				float3 tmax = max(t0, t1);

				float dA = max(max(tmin.x, tmin.y), tmin.z);
				float dB = min(tmax.x, min(tmax.y, tmax.z));


				//basically the t's are distances in each axis, distance from the origin to furthest/closest corner of the container
				//Normalized by the distance that the Ray will travel in that axis
				//the tmin holds the smallest distances and tmax the opposite
				//the DISTANCE TO THE BOX (nearest intersection point) is the biggest of all tmins, if it's less than 0 => the rayOrigin is inside the box
				//the DISTANCE INSIDE THE BOX is the difference between the two intersection distances

				float dToBox = max(0, dA);
				float dInBox = max(0, dB - dToBox);

				return float2(dToBox, dInBox);
				//return float2(dA, dB);
			}*/

			bool AABBRayIntersection(in ray r, in float3 cminb, in float3 cmaxb, out float dtobox, out float dinbox)
			{
				//https://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-box-intersection
				dtobox = 0;
				dinbox = 0;
				float3 bounds[2];
				bounds[0] = cminb;
				bounds[1] = cmaxb;
				float tmin, tmax, tymin, tymax, tzmin, tzmax;
				int sign[3];
				float3 invdir = 1 / r.direction;
				sign[0] = (invdir.x < 0);
				sign[1] = (invdir.y < 0);
				sign[2] = (invdir.z < 0);

				tmin = (bounds[sign[0]].x - r.origin.x) * invdir.x;
				tmax = (bounds[1 - sign[0]].x - r.origin.x) * invdir.x;
				tymin = (bounds[sign[1]].y - r.origin.y) * invdir.y;
				tymax = (bounds[1 - sign[1]].y - r.origin.y) * invdir.y;

				if ((tmin > tymax) || (tymin > tmax))
					return false;
				if (tymin > tmin)
					tmin = tymin;
				if (tymax < tmax)
					tmax = tymax;

				tzmin = (bounds[sign[2]].z - r.origin.z) * invdir.z;
				tzmax = (bounds[1 - sign[2]].z - r.origin.z) * invdir.z;

				if ((tmin > tzmax) || (tzmin > tmax))
					return false;
				if (tzmin > tmin)
					tmin = tzmin;
				if (tzmax < tmax)
					tmax = tzmax;

				dtobox = max(0, tmin);
				dinbox = max(0, tmax - dtobox);
				return true;
			}


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
				float3 viewVector = mul(unity_CameraInvProjection, float4(v.uv * 2 - 1, 0, -1));
				o.viewDir = mul(unity_CameraToWorld, float4(viewVector, 0));
                return o;
            }

            sampler2D _MainTex;
			float3 _ContainerMaxBounds;
			float3 _ContainerMinBounds;
			sampler2D _CameraDepthTexture;

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);

				ray r;
				r.origin = _WorldSpaceCameraPos;
				r.direction = normalize(i.viewDir);

				float dtobox; //distance to the box
				float dinbox; //distance inside the box

				//sampling depth texture
				float depthSample = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
				float depth = LinearEyeDepth(depthSample);
				bool hit = AABBRayIntersection(r, _ContainerMinBounds, _ContainerMaxBounds, dtobox, dinbox);

				if (hit && (((dtobox*i.viewDir).z -5) < depth))
					col = 0;

				return col;
            }
            ENDCG
        }
    }
	Fallback "Hidden/Content"
}
