// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "TransparentRange" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_MainColor ("Main Color", Color) = (0, 0, 0, 0)
    _Range ("Range", Float) = 10
	_MaxRange ("MaxRange", Float) = 30
}
SubShader {
Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}

	Pass {
		Blend SrcAlpha OneMinusSrcAlpha 
		Fog { Mode off }

CGPROGRAM

#pragma fragmentoption ARB_precision_hint_fastest
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
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
uniform half _Range;
uniform half _MaxRange;
uniform fixed4 _MainColor;
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
		half4 color = half4(original.r, original.g, original.b, original.a) * i.color;
                const half edge = 0.01;

		half minR = (1 - _Range / _MaxRange- edge) * 0.5 ;
		half maxR = (1 + _Range / _MaxRange + edge) * 0.5 ;

		if (i.uv.x <minR|| i.uv.x > maxR || i.uv.y < minR|| i.uv.y > maxR)
		{
			return half4(_MainColor.r, _MainColor.g, _MainColor.b, _MainColor.a) * i.color;
		}
		else
		{
			half alphaX = 0;
			if (i.uv.x >= minR && i.uv.x <= minR + edge)
			{
				alphaX = 1 - (i.uv.x - minR) / edge;
			}
			else if (i.uv.x <= maxR && i.uv.x >=  maxR - edge)
			{
				alphaX = 1 - (maxR - i.uv.x) / edge;
			}

			if (i.uv.y >= minR && i.uv.y <= minR + edge)
			{
				if ((1 - (i.uv.y - minR) / edge) > alphaX)
				{
					alphaX = (1 - (i.uv.y - minR) / edge);
				}
			}
			else if (i.uv.y <= maxR && i.uv.y >= maxR - edge)
			{
				if ((1 - (maxR - i.uv.y) / edge) > alphaX)
				{
					alphaX = (1 - (maxR - i.uv.y) / edge);
				}
			}
			if (alphaX <0)
			{
				alphaX = 0;
			}
			else if (alphaX > 1)
			{
				alphaX = 1;
			}
			return half4(_MainColor.r, _MainColor.g, _MainColor.b, _MainColor.a * alphaX) * i.color;
		}


		
}

ENDCG
	}
}

Fallback off

}