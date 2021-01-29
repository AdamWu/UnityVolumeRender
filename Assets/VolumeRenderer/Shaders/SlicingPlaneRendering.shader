// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "VolumeRendering/SlicingPlaneRendering"
{
	Properties
	{
		_VolumeTex("Volume Texture",3D) = "black"{}
		_BorderColor("Border Color",Color) = (1,1,1,1)
    }
	SubShader
	{
		Tags { "Queue" = "Geometry" }
		LOD 100
        Cull Off

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
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 relVert : TEXCOORD1;
			};

			sampler3D _VolumeTex;
            uniform float4x4 _parentInverseMat;
            uniform float4x4 _planeMat;
			float4 _BorderColor;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
                float3 vert = float3(v.uv.x-0.5f, v.uv.y-0.5f, 0.0f);
                vert = mul(_planeMat, float4(vert, 1.0f));
				o.relVert = mul(_parentInverseMat, float4(vert, 1.0));
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float3 dataCoord = i.relVert + float3(0.5f, 0.5f, 0.5f);
                float v = tex3D(_VolumeTex, dataCoord).r;

                return float4(v, v, v, 1.0f);
			}
			ENDCG
		}
	}
}
