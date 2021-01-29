// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

#ifndef SURFACE_VOLUME_RENDERING_INCLUDED
#define SURFACE_VOLUME_RENDERING_INCLUDED

#include "VolumeRenderingUtil.cginc"

struct appdata {
	float3 vertex : POSITION;
};

struct v2f {
	float4 pos : SV_POSITION;
#ifdef VOLUME_BACKFACE
	float4 clipPos : TEXCOORD1;
#else
	float3 localPos : TEXCOORD1;
	float3 worldPos : TEXCOORD2;
#endif
};

v2f vert(appdata i) {
	v2f o;
	o.pos = UnityObjectToClipPos(i.vertex);
#ifdef VOLUME_BACKFACE
	o.clipPos = o.pos;
#else
	o.localPos = i.vertex;
	o.worldPos = mul(unity_ObjectToWorld, float4(i.vertex, 1));
#endif
	return o;
}

float random(float2 uv)
{
	return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453123);
}

float4 frag(v2f i , out float depth : SV_DEPTH) :SV_TARGET{
	float4 firstHit = 0;
	float density = 0;
	float4 tfColor = 0;
	float4 gradient = 0;
#ifdef VOLUME_BACKFACE
	i.clipPos.y = - i.clipPos.y;
	float3 nearClipPlanePos = (i.clipPos / i.clipPos.w).xyz;
	nearClipPlanePos.z = -1;
	float4 localPos = mul(VolumeClipToObject, float4(nearClipPlanePos, 1));
	localPos /= localPos.w;
	float3 worldPos = mul(unity_ObjectToWorld, localPos);
#else
	float3 worldPos = i.worldPos;
	float4 localPos = float4(i.localPos,1);
#endif
	float3 viewDir = normalize(lerp(worldPos - _WorldSpaceCameraPos, -UNITY_MATRIX_V[2].xyz, UNITY_MATRIX_P[3][3]));
	float3 objViewDir = normalize(UnityWorldToObjectDir(viewDir));

	float3 startPosition = localPos;

	for (int i = 0; i < MAX_SAMPLE_COUNT; i++) {
		float3 uvw = startPosition + 0.5;
		if (uvw.x < -0.01 || uvw.y < -0.01 || uvw.z < -0.01 || uvw.x > 1.01 || uvw.y > 1.01 || uvw.z > 1.01) {
			firstHit = float4(uvw, 0);
			break;
		}
		density = getDensity(uvw);
		if (PointPlaneDistance(uvw, _SplitPlane) < 0)
			density = 0;

		if (density > _IsoSurfaceThreshold) {
			firstHit = float4(uvw, 1);
			break;
		}

		startPosition += objViewDir * SAMPLE_STEP_SIZE;
	}

	if (firstHit.a < 0.0001)
		discard;

	float3 localFirstHit = firstHit - 0.5;
	float4 opos = UnityObjectToClipPos(float4(localFirstHit, 1));
#if defined(SHADER_API_GLCORE) || defined(SHADER_API_GLES) || defined(SHADER_API_GLES3)
	depth = (opos.z / opos.w + 1.0 ) * 0.5;
#else
	depth = opos.z / opos.w;
#endif

	// 计算颜色
	float3 grad = GetGradientSobel(firstHit.xyz);
	gradient.w = length(grad);
	gradient.xyz = normalize(grad.xyz);
	
	float3 V = viewDir;
	float3 N = UnityObjectToWorldNormal(-gradient.xyz);
	float3 R = reflect(V, N);

	float3 L = normalize(float3(1, 0, -1));
	float3 F = reflect(L, N);

	float3 ambient = float3(_Ambient, _Ambient, _Ambient) * _IsoSurfaceColor;
	float3 diffuse = float3(_Diffuse, _Diffuse, _Diffuse) * max(dot(N, L), 0) * _IsoSurfaceColor;
	float3 specular = float3(_Specular, _Specular, _Specular) * pow(max(dot(V, F), 0), _SpecularPower) * _IsoSurfaceColor;
	
	float3 color = ambient + diffuse +specular;

	return float4(color, 1);

}
#endif