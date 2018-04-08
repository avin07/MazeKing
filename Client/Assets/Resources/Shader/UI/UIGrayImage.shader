// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'



Shader "UIGrayImage" {
Properties {
	[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

	_StencilComp ("Stencil Comparison", Float) = 8
	_Stencil ("Stencil ID", Float) = 0
	_StencilOp ("Stencil Operation", Float) = 0
	_StencilWriteMask ("Stencil Write Mask", Float) = 255
	_StencilReadMask ("Stencil Read Mask", Float) = 255
	_ColorMask ("Color Mask", Float) = 15
}

SubShader {

		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}
		ColorMask [_ColorMask]

	Pass {
		ZTest Always Cull Off ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha 
		Fog { Mode off }


CGPROGRAM

#pragma fragmentoption ARB_precision_hint_fastest
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

uniform sampler2D _MainTex;

struct appdata_t {
	float4 vertex : POSITION;
	fixed4 color : COLOR;
	float2 texcoord : TEXCOORD0;
};

struct v2f {
	float4 pos : POSITION;
	fixed4 color : COLOR;
	float2 uv : TEXCOORD0;
};

uniform float4 _MainTex_ST;
float IsGray;
v2f vert( appdata_t v )
{
	v2f o;
	o.pos = UnityObjectToClipPos (v.vertex);
	o.color = v.color;
	//o.uv.xy = v.texcoord.xy;
	o.uv = TRANSFORM_TEX(v.texcoord,_MainTex);
	
	return o;
}

half4 frag (v2f i) : COLOR
{
		half4 original = tex2D(_MainTex, i.uv);
		float luma = original.r * 0.7 + original.g * 0.2 + original.b * 0.1;
		return half4(luma, luma, luma, original.a) * i.color;
}

ENDCG
	}
}

Fallback off

}