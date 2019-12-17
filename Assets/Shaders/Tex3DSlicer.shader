Shader "Hidden/Tex3DSlicer"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Volumetex ("3D texture", 3D) = "white" {}
		_Slice ("Slice", Range(0,1) ) = 0
		//TODO: add wich dimension to fix
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
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
			sampler3D _VolumeTex;
			float _Slice;

            fixed4 frag (v2f i) : SV_Target
            {
				i.uv.y = 1 - i.uv.y;
                fixed4 col = tex3D(_VolumeTex, float3(i.uv, _Slice).zyx);

				return float4(col.x,col.x, col.x, 1.0);
            }
            ENDCG
        }
    }
}
