Shader "DF/Bumped Specular Gray" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_Shininess ("Shininess", Range (0.03, 1)) = 0.078125
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	_BumpMap ("Normalmap", 2D) = "bump" {}
	_GrayScale("GrayScale", Range(0,1)) = 1
}
SubShader { 
	Tags { "RenderType"="Opaque" }
	LOD 400
CGPROGRAM
#pragma surface surf BlinnPhong


sampler2D _MainTex;
sampler2D _BumpMap;
half4 _Color;
half _Shininess;
half _GrayScale;
struct Input {
	half2 uv_MainTex;
	half2 uv_BumpMap;
};

void surf (Input IN, inout SurfaceOutput o) {
	half4 tex = tex2D(_MainTex, IN.uv_MainTex);

	if (_GrayScale < 1)
	{
		half c = (tex.r * 0.7 + tex.g * 0.2 + tex.b * 0.1)*_GrayScale;
		tex.r = c;
		tex.g = c;
		tex.b = c;
	}
	o.Albedo = tex.rgb * _Color.rgb;
	o.Gloss = tex.a;
	o.Alpha = tex.a * _Color.a;
	o.Specular = _Shininess;
	o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
}
ENDCG
}

FallBack "Specular"
}
