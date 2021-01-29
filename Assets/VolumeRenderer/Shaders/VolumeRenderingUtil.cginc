#ifndef VOLUME_RENDERING_UTIL_INCLUDED
#define VOLUME_RENDERING_UTIL_INCLUDED

#include "UnityCG.cginc"
#include "UnityStandardUtils.cginc"

sampler3D _VolumeTex;
float3 _VolumeTex_TexelSize;
sampler2D _TFTex;
matrix VolumeClipToObject;
sampler2D _NoiseTex;

float _IsoSurfaceThreshold;
float4 _IsoSurfaceColor;
float _Ambient;
float _Diffuse;
float _Specular;
float _SpecularPower;

float4 _SplitPlane;

#define MAX_SAMPLE_COUNT 1000
#define SAMPLE_STEP_SIZE (1.732f/MAX_SAMPLE_COUNT)

float4 getTFColor(float density)
{
	return tex2Dlod(_TFTex, float4(density, 0.0f, 0.0f, 0.0f));
}

float getDensity(float3 pos)
{
	return tex3Dlod(_VolumeTex, float4(pos, 0.0f)).r;
}

half3 GetGradientSobel(float3 pos) {
	float4 delta = 1.0 / _VolumeTex_TexelSize.x;
	delta.w = 0.0;
	float3 res = (0.0).xxx;
	res.x = tex3Dlod(_VolumeTex, float4(pos + delta.xww, 0)) - tex3Dlod(_VolumeTex, float4(pos - delta.xww, 0));
	res.y = tex3Dlod(_VolumeTex, float4(pos + delta.wxw, 0)) - tex3Dlod(_VolumeTex, float4(pos - delta.wxw, 0));
	res.z = tex3Dlod(_VolumeTex, float4(pos + delta.wwx, 0)) - tex3Dlod(_VolumeTex, float4(pos - delta.wwx, 0));
	return res;
}

float PointPlaneDistance(float3 pt, float4 plane) {
	plane.xyz = normalize(plane.xyz);
	return dot(plane, pt) + plane.w;
}

#endif