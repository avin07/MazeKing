Shader "DF/FadeoutDissolve" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	_BumpMap ("Normalmap", 2D) = "bump" {}
	_DissolveTex ("DissolveTex", 2D) = "white" {}
	_MaskTex ("MaskTex", 2D) = "white" {}
	
	_IsDissolve("IsDissolve", Range(0,1)) = 1
	_Shininess ("Shininess", Range (0.03, 1)) = 0.078125
}
SubShader { 
	Tags { "RenderType"="Opaque" }
	LOD 400
	
CGPROGRAM
#pragma surface surf BlinnPhong


sampler2D _MainTex;
sampler2D _MaskTex;
sampler2D _DissolveTex;
sampler2D _BumpMap;
fixed4 _Color;
half _Shininess;
half _IsDissolve;
half4 _OffsetVec;
half _PlayerPos;
struct Input {
	half2 uv_MainTex;
	//float2 uv_BumpMap;	
	half2 uv_DissolveTex;
	half4 screenPos;
	half3 worldPos;
};

void surf (Input IN, inout SurfaceOutput o) {

	//if (_PlayerPos > IN.worldPos.z + 0.5)
	{
			half g = tex2D(_DissolveTex, IN.uv_DissolveTex).r;
			half2 screenPos = IN.screenPos.xy/IN.screenPos.w;
			
			//screenPos.x = screenPos.x*_OffsetVec.z + _OffsetVec.x;
			//screenPos.y = screenPos.y*_OffsetVec.w + _OffsetVec.y;
			
			clip(_IsDissolve + g - tex2D(_MaskTex, screenPos).a);
	}
	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
	o.Albedo = tex.rgb * _Color.rgb;
	o.Gloss = tex.a;
	o.Alpha = tex.a * _Color.a;
	o.Specular = _Shininess;
	o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
}
ENDCG
}

FallBack "Specular"
}
