// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "DF/Transparent/Bumped Specular" {
Properties 
{
	_Color ("Main Color", Color) = (1,1,1,1)
	_Shininess ("Shininess", Range (0.03, 1)) = 0.078125
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	_BumpMap ("Normalmap", 2D) = "bump" {}
	_MaskTex ("Alpha Tex", 2D) = "white" {}
	
    _ShadowIntensity ("Shadow Intensity", Range (0, 1)) = 0.2
}

SubShader 
{
	Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 400

	CGPROGRAM
	//#pragma surface surf BlinnPhong alpha addshadow
	#pragma surface surf BlinnPhong alpha vertex:vert fullforwardshadows approxview
 
	sampler2D _MainTex;
	sampler2D _BumpMap;
	sampler2D _MaskTex;
	half4 _Color;
	half _Shininess;
	struct Input
	{
		half2 uv_MainTex;	
		half2 uv_MaskTex;
	};
 
		struct v2f { 
			V2F_SHADOW_CASTER; 
			float2 uv : TEXCOORD1;
		};
		v2f vert (inout appdata_full v) { 
			v2f o; 
			return o; 
		} 
	void surf (Input IN, inout SurfaceOutput o) 
	{
		half4 tex = tex2D(_MainTex, IN.uv_MainTex);
		half4 mask = tex2D(_MaskTex, IN.uv_MaskTex);
	
		o.Albedo = tex.rgb * _Color.rgb;
		o.Gloss = tex.a;
		o.Alpha = mask.a;
		o.Specular = _Shininess;
		o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
	}
	ENDCG
	 
         //Shadow Pass : Adding the shadows (from Directional Light)
         //by blending the light attenuation
        Pass 
	{
		Blend SrcAlpha OneMinusSrcAlpha
		Name "ShadowPass"
		Tags {"LightMode" = "ForwardBase"}
 
		CGPROGRAM
		// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members lightDir)
		#pragma exclude_renderers d3d11 xbox360
		#pragma vertex vert
		#pragma fragment frag
		#pragma multi_compile_fwdbase
		#pragma fragmentoption ARB_fog_exp2
		#pragma fragmentoption ARB_precision_hint_fastest
		#include "UnityCG.cginc"
		#include "AutoLight.cginc"
		struct v2f 
		{
			float2 uv_MainTex : TEXCOORD0;
			float4 pos : SV_POSITION;
			LIGHTING_COORDS(3,4)
			float4    lightDir : TEXCOORD1;
		};
 
		float4 _MainTex_ST;
		sampler2D _MainTex;
		sampler2D _MaskTex;
		float4 _Color;
		float _ShadowIntensity;
 
		v2f vert (appdata_full v)
		{
			v2f o;
			o.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
			o.pos = UnityObjectToClipPos (v.vertex);
			o.lightDir = float4(ObjSpaceLightDir( v.vertex ),0);
			TRANSFER_VERTEX_TO_FRAGMENT(o);
			return o;
		}
 
		float4 frag (v2f i) : COLOR
		{
			float atten = LIGHT_ATTENUATION(i);
			
			half4 c;
			c.rgb =  0;
			half a = tex2D(_MaskTex, i.uv_MainTex).a;
			if (a > 0)
				c.a = (1-atten) * _ShadowIntensity * a;
			else
				c.a = 0;
			return c;
		}
		ENDCG
        }
 }
FallBack "Transparent/VertexLit"
}