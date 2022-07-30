// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// -------------------------------------------
// Control Freak 2
// Copyright (C) 2013-2019 Dan's Game Tools
// http://DansGameTools.com
// -------------------------------------------

Shader "Sprites/CF2-Additive"
{ 
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
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
		Blend One One //MinusSrcAlpha
		//Blend One OneMinusSrcAlpha	// OK...
		//Blend OneMinusSrcAlpha One 
		//Blend One SrcAlpha 

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
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

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _MainTex;

			fixed4 frag(v2f IN) : SV_Target
			{
				//fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
				//c.rgb *= c.a;

				fixed4 c = tex2D(_MainTex, IN.texcoord);
				c *= IN.color;
//				c.rgb += IN.color.rgb;
//				//c.rgb *= IN.color.rgb;
//				c.rgb = min(c.rgb, 1.0);
				c.rgb *= c.a;
//				c.rgb *= IN.color.a;
//				c.a *= IN.color.a;
				return c;
			}
		ENDCG
		}
	}
}
