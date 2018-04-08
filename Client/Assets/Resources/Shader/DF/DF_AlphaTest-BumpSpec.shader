Shader "DF/Transparent_Cutout/Bumped Specular" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_Shininess ("Shininess", Range (0.03, 1)) = 0.078125
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	_BumpMap ("Normalmap", 2D) = "bump" {}
	_MaskTex ("Alpha Tex", 2D) = "white" {}
		_Cutoff ("Alpha cutoff", Range(0,1)) = 0.1
}

SubShader {
		
	Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
	LOD 400

CGPROGRAM
#pragma surface surf BlinnPhong alphatest:_Cutoff

sampler2D _MainTex;
sampler2D _BumpMap;
sampler2D _MaskTex;
half4 _Color;
half _Shininess;
struct Input {
	half2 uv_MainTex;	
	half2 uv_MaskTex;	
};

void surf (Input IN, inout SurfaceOutput o) {
	half4 mask = tex2D(_MaskTex, IN.uv_MaskTex);
	
	half4 tex = tex2D(_MainTex, IN.uv_MainTex);
	o.Albedo = tex.rgb * _Color.rgb;
	o.Gloss = tex.a;
	o.Alpha = mask.a;
	o.Specular = _Shininess;
	o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
}
ENDCG
}

FallBack "Transparent/VertexLit"
}