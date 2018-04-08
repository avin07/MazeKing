Shader "DF/Bumped Diffuse Gray" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_BumpMap ("Normalmap", 2D) = "bump" {}
	_GrayScale("GrayScale", Range(0,1)) = 1
}

SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 300

CGPROGRAM
#pragma surface surf Lambert

sampler2D _MainTex;
sampler2D _BumpMap;
fixed4 _Color;
half _GrayScale;
struct Input {
	float2 uv_MainTex;
	float2 uv_BumpMap;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex)* _Color;
	if (_GrayScale < 1)
	{
		half c = tex.r * 0.7 + tex.g * 0.2 + tex.b * 0.1;
		tex.r = c * _GrayScale;
		tex.g = c* _GrayScale;
		tex.b= c* _GrayScale;
	}
	o.Albedo = tex;
	o.Alpha = tex.a;
	o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
}
ENDCG  
}

FallBack "Diffuse"
}
