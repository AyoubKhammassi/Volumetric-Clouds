Shader "Custom/VolumetricRenderer"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_ContainerMaxBounds("top near right corner", Vector) = (0,0,0)
		_ContainerMinBounds("bottom far left corner", Vector) = (0,0,0)
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
				float3 direction: COLOR0;
            };

			struct ray
			{
				float3 origin;
				float3 direction;
			};

			float2 AABBRayIntersection(float3 bndMin, float3 bndMax, ray r)
			{
				float t0 = (bndMin - r.origin) / r.direction;
				float t1 = (bndMax - r.origin) / r.direction;

				float3 tmin = min(t0, t1);
				float3 tmax = max(t0, t1);

				float dA = max(max(tmin.x, tmin.y), tmin.z);
				float dB = min(min(tmax.x, tmax.y), tmax.z);


				//basically the t's are distances in each axis, distance from the origin to furthest/closest corner of the container
				//Normalized by the distance that the Ray will travel in that axis
				//the tmin holds the smallest distances and tmax the opposite
				//the DISTANCE TO THE BOX (nearest intersection point) is the biggest of all tmins, if it's less than 0 => the rayOrigin is inside the box
				//the DISTANCE INSIDE THE BOX is the difference between the two intersection distances

				float d2box = max(0, dA);
				float dInBox = max(0, dB - d2box);

				return float2(d2box, dInBox);
			}

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
				o.direction = mul(unity_WorldToObject, o.vertex).xyz - _WorldSpaceCameraPos;
                return o;
            }

            sampler2D _MainTex;
			float3 _ContainerMaxBounds;
			float3 _ContainerMinBounds;

			fixed4 frag(v2f i) : SV_Target
			{
				ray r;
				r.origin = _WorldSpaceCameraPos;
				r.direction = i.direction;

				float2 ds = AABBRayIntersection(_ContainerMinBounds, _ContainerMaxBounds, r);

				fixed4 col = tex2D(_MainTex, i.uv);

				bool hitBox = ds.y > 0;
				if (!hitBox)
					col = 0;

				return float4(i.direction,1.0);
            }
            ENDCG
        }
    }
	Fallback "Hidden/Content"
}
