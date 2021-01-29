// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "VolumeRendering/StandardNoDepth" {

	Properties
	{
		_Color("MainColor", Color) = (1,1,1,1)
	}
	SubShader
	{
		
		Tags{ "Queue" = "Transparent+5" }
		Pass
		{
			ZTest Off
			ZWrite Off
		
			CGPROGRAM

			#include "UnityCG.cginc"
			#pragma vertex vert  
			#pragma fragment frag 

			fixed4 _Color;

			struct appdata {
				float4 vertex : POSITION;
			};
			struct v2f {
				float4 pos : SV_POSITION;
			};

			v2f vert(appdata i)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(i.vertex);
				return o;
			}

			float4 frag(v2f i) : COLOR
			{ 
				return  _Color;
			}	

			ENDCG
		}
	}
}