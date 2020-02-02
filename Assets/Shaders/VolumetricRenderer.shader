Shader "Custom/VolumetricRenderer"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_VolumeOffset("Volume Offse", Vector) =(0,0,0)
		_VolumeScale("Volume Scale", float) = 1.0
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
			#include "UnityLightingCommon.cginc"

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

			sampler2D _MainTex;
			sampler2D _CameraDepthTexture;

			//Handeled by the script
			//Container metadata
			float3 _ContainerMaxBounds;
			float3 _ContainerMinBounds;
			//The world to object matrix of the  container
			float4x4 _ContainerMatrix;

			//Texture coming from the Tex3D Generator
			Texture3D<float4> SSVolume;
			SamplerState samplerSSVolume; //The sampler sate for the texture
			SamplerState trilinear_repeat_samplerSSVolume;
			float3 _VolumeOffset; //The volume offset inside the container; used when sampling density
			float _VolumeScale; //the scale of the volume inside the container
			float3 _VolumeColor;
			float _Density;
			float _MinDensity;
			//Sampling
			//The step between each point of sampling inside the container, the smaller the step the more samples
			float _Step;

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
				dinbox = max(0, tmax);
				return true;
			}

			//Sample the density in the position pos inside the container
			float4 sampleDensity(float3 pos)
			{
				//the pos origin is the center of the container
				float3 uvw = pos / _VolumeScale + _VolumeOffset;
				uvw += +float3(0.5, 0.5, 0.5);


				clamp(uvw, float3(0, 0, 0), float3(1, 1, 1));

				/*if (uvw.x < 0.0 || uvw.y < 0.0 || uvw.z < 0.0)
					return float4(0, 0, 0, 0);
				if (uvw.x > 1.0 || uvw.y > 1.0 || uvw.z > 1.0)
					return float4(0, 0, 0, 0);*/
				float4 density = SSVolume.Sample(samplerSSVolume, uvw);
				return density;
			}

			float lightQuantity(float3 pos)
			{
				ray r;
				//the current position we're sampling
				r.origin = pos;
				//the direction to the light source
				r.direction = normalize(_WorldSpaceLightPos0.xyz - pos);

				float dinbox, dtobox, density;
				//we know for sure that we're inside the container, we're only interested in the dinbox value
				bool hit = AABBRayIntersection(r, _ContainerMinBounds, _ContainerMaxBounds, dtobox, dinbox);

				density = 0;
				[unroll(30)]
				while (dinbox > 0.0)
				{
					pos += r.direction * _Step;
					density += sampleDensity(pos).x * _Step;
					dinbox -= _Step;
				}
				density = exp(-density * _Density);
				return density;
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


			fixed4 frag(v2f i) : SV_Target
			{
				//Get the original rendered image
				fixed4 col = tex2D(_MainTex, i.uv);

				
				ray r;
				r.origin = _WorldSpaceCameraPos;
				r.direction = normalize(i.viewDir);

				float dtobox; //distance to the box
				float dinbox; //distance inside the box

				//sampling depth texture
				float depthSample = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
				float depth = LinearEyeDepth(depthSample) * length(i.viewDir); //NOT BETWEEN  0 AND 1
				bool hit = AABBRayIntersection(r, _ContainerMinBounds, _ContainerMaxBounds, dtobox, dinbox);

				//Testing if the Container is in front of the Camera
				hit = (hit && (dinbox > 0));
				//Testing to see if the container is being culled by another object in the scene
				hit = (hit && (dtobox - _ProjectionParams.y <= depth));


				//_ProjectionParams is the camera near plane
				if (hit)
				{
					float4 worldPos = float4(r.origin + r.direction * dtobox, 1.0);
					//we need to find the position of the hit point in the local space of the container
					float3 pos = mul(_ContainerMatrix, worldPos).xyz;
					pos = normalize(pos);

					//If there's an object inside the volume, the depth is as far as we march
					dinbox = min((dinbox - _ProjectionParams.y), depth);
					fixed accumDensity = 0.0;
					fixed light = 0;
					fixed dist = 0;
					[unroll(30)]
					while (dist < dinbox)
					{

						light += lightQuantity(pos) * _Step * _Density * accumDensity;
						accumDensity += sampleDensity(pos).x * _Step;
						//Advance one step in the ray direction inside the container
						dist += _Step;
						pos += r.direction * _Step;
					}



					accumDensity = exp(-(accumDensity*_Density));
					float opacity = 1 - accumDensity;
					float3 cloudCol = light * _LightColor0 * _VolumeColor;
					float3 mixedCol = col.xyz * accumDensity + cloudCol; //_VolumeColor * opacity + col.xyz * (1 - opacity);
					col = float4(mixedCol, 1.0);
				}

				return col;
            }
            ENDCG
        }
    }
	Fallback "Hidden/Content"
}
