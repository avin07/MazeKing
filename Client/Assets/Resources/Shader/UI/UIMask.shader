// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'



Shader "UIMask" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_MaskTex("Mask", 2D) = "white"{}

    _StencilComp ("Stencil Comparison", Float) = 8
    _Stencil ("Stencil ID", Float) = 0
    _StencilOp ("Stencil Operation", Float) = 0
    _StencilWriteMask ("Stencil Write Mask", Float) = 255
    _StencilReadMask ("Stencil Read Mask", Float) = 255
}

SubShader {
	Pass {
		Blend SrcAlpha OneMinusSrcAlpha 
		Fog { Mode off }

		    Stencil
    {
        Ref [_Stencil]
        Comp [_StencilComp]
        Pass [_StencilOp] 
        ReadMask [_StencilReadMask]
        WriteMask [_StencilWriteMask]
    }

CGPROGRAM

#pragma fragmentoption ARB_precision_hint_fastest
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

uniform sampler2D _MainTex;
uniform sampler2D _MaskTex;

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
uniform float4 _MaskTex_ST;
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
		half4 mask = tex2D(_MaskTex, i.uv);
		
		return half4(original.r, original.g, original.b, mask.g) * i.color;
}

ENDCG
	}
}

Fallback off

}