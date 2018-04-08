// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "GrayImage" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
}

SubShader {
	Pass {
		ZTest Always Cull Off ZWrite Off
		Fog { Mode off }

CGPROGRAM

#pragma fragmentoption ARB_precision_hint_fastest
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

uniform sampler2D _MainTex;

struct v2f {
	float4 pos : POSITION;
	float2 uv : TEXCOORD0;
};

v2f vert( appdata_img v )
{
	v2f o;
	o.pos = UnityObjectToClipPos (v.vertex);
	o.uv.xy = v.texcoord.xy;
	
	return o;
}

half4 frag (v2f i) : COLOR
{
	half4 original = tex2D(_MainTex, i.uv);
	// float luma = original.r * 0.3 + original.g * 0.59 + original.b * 0.11;
	float luma = original.r * 0.5 + original.g * 0.3 + original.b * 0.2;
	return half4(luma, luma, luma, original.a);
}
ENDCG
	}
}

Fallback off

}