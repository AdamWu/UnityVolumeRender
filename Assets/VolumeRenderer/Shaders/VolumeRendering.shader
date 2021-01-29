Shader "VolumeRendering/VolumeRendering" 
{
	Properties{
		_VolumeTex("Volume Texture",3D) = "black"{}
		_VolumeTex_TexelSize("Volume Texture Size", Vector) = (0,0,0)
		_TFTex("Transfer Function Texture", 2D) = "" {}
		_NoiseTex("Noise Texture (Generated)", 2D) = "white" {}

		// iso surface render
		_IsoSurfaceThreshold("IsoSurfaceThreshold",Range(0,1)) = 0
		_IsoSurfaceColor("IsoSurfaceColor",Color) = (1,1,1,1)
		_Ambient("Ambient",Range(0,1)) = 0.5
		_Diffuse("Diffuse",Range(0,1)) = 0.5
		_Specular("Specular",Range(0,1)) = 0.5
		_SpecularPower("SpecularPower",Range(1,50)) = 20

		// ROI
		_SplitPlane("Split Plane",Vector) = (0,0,0,0)

		_MinVal("Min val", Range(0.0, 1.0)) = 0.0
		_MaxVal("Max val", Range(0.0, 1.0)) = 1.0
	}

	SubShader{
		Tags{ "Queue" = "Transparent" }
		Pass {
			Tags { "LightMode" = "ForwardBase" }

			Cull Back
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma target 3.0
			#pragma multi_compile MODE_DVR MODE_SURF 
			#pragma shader_feature SHADE
			#pragma vertex vert
			#pragma fragment frag
#ifdef MODE_DVR
			#include "DirectVolumeRendering.cginc"
#elif MODE_SURF
			#include "SurfaceVolumeRendering.cginc"
#endif
			ENDCG
		}
		
		Pass{
			//render the near clip.
			Cull Front
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma target 3.0
			#pragma multi_compile MODE_DVR MODE_SURF
			#pragma shader_feature SHADE
			#pragma vertex vert
			#pragma fragment frag
			#define VOLUME_BACKFACE
#ifdef MODE_DVR
			#include "DirectVolumeRendering.cginc"
#elif MODE_SURF
			#include "SurfaceVolumeRendering.cginc"
#endif
			ENDCG
		}
		
	}

}