// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2019 Dan's Game Tools
// http://DansGameTools.com
// -------------------------------------------

Shader "Sprites/CF2-Soft-Light"
{ 
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		//_Color ("Tint", Color) = (1,1,1,1)
		//[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[MaterialToggle] AdvancedMode ("Advanced Mode", Float) = 0
		_ShadowScale ("Shadow Scale", Range(0, 1)) = 1.0
		_HighlightScale ("Highlite Scale", Range(0, 1)) = 1.0
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		//Blend One One //MinusSrcAlpha
		//Blend One OneMinusSrcAlpha	// OK...
		//Blend OneMinusSrcAlpha One 
		//Blend One SrcAlpha 

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			//#pragma multi_compile _ PIXELSNAP_ON
			#pragma multi_compile _ ADVANCEDMODE_ON
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
			};
			
			fixed4 _Color;

#ifdef ADVANCEDMODE_ON
			float _ShadowScale;
			float _HighlightScale;
#endif

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color; // * _Color;
				//OUT.color = IN.color;
			//	#ifdef PIXELSNAP_ON
			//	OUT.vertex = UnityPixelSnap (OUT.vertex);
			//	#endif

				return OUT;
			}

			sampler2D _MainTex;

			fixed4 frag(v2f IN) : SV_Target
			{
				//fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
				//c.rgb *= c.a;

				float4 c = tex2D(_MainTex, IN.texcoord); // * _Color; 
				//c.rgb *= 2;
				//fixed4 hilight = c - fixed4(1, 1, 1, 0);
				//c = clamp(c, 0, 1);
				float4 hilight = max(0, (c - float4(0.5, 0.5, 0.5, 0))) * 2;
				c.rgb = min(c.rgb, 0.5)  * 2;
				//c = clamp(c, 0, 1);

#ifdef ADVANCEDMODE_ON
				//c.rgb = float3(1,1,1) - ((float3(1,1,1) - c.rgb) * _ShadowScale);
				c.rgb = float3(1,1,1) - ((float3(1,1,1) - c.rgb) * _ShadowScale);

				hilight *= _HighlightScale;
#endif

				c *= IN.color;
				c.rgb += hilight.rgb; 

				//fixed4 outc = IN.color + hilight;
				//c *= outc;

		//		c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}
