Shader "Hidden/Tex3DSlicer"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Volumetex ("3D texture", 3D) = "white" {}
		_Slice ("Slice", Range(0,1) ) = 0
		//TODO: add wich dimension to fix
        [Enum(X,1,Y,2,Z,3)]
        _Axis ("Slicing axis", int) = 1
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
            int _Axis;

            fixed4 frag(v2f i) : SV_Target
            {
                float3 uvw;
            if (_Axis == 1)
                uvw = float3(_Slice, i.uv);
            else if (_Axis == 2)
                uvw = float3(i.uv.x, _Slice, i.uv.y);
            else
                uvw = float3(i.uv, _Slice);
                
				//i.uv.y = 1 - i.uv.y;
                fixed4 col = tex3D(_VolumeTex, uvw);
				return float4(col.x,col.x, col.x, 1.0);
            }
            ENDCG
        }
    }
}
