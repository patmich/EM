Shader "LLT/Transparent" 
{
	Properties 
	{
	    _Color ("Main Color", Color) = (1,1,1,0.5)
	    _MainTex ("Texture", 2D) = "white" { } 
	    _Ref ("Ref", Float) = 0
	}
	SubShader 
	{ 
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		Cull Off
		Stencil 
		{
			Ref [_Ref]
			CompFront equal
			CompBack equal
		}
	
	    Pass
	    {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			#pragma glsl_no_auto_normalization
			#include "UnityCG.cginc"	
			
			float4 _Color;
			sampler2D _MainTex;
			
			struct appdata
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;	
				fixed4 color : COLOR;	
				float4 tangent : TANGENT;
			};
			
			struct v2f 
			{
			    float4 pos : SV_POSITION;
			    float2 uv : TEXCOORD0;
			    fixed4 add : COLOR0;
			    fixed4 mult : COLOR1;
			};
			
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
			    v2f o;
			    
			    o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			    o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
			    o.add = v.color;
			    o.mult = v.tangent;
			    
			    return o;
			}
			
			fixed4 frag (v2f i) : COLOR
			{
			   	return i.mult * tex2D (_MainTex, i.uv) + i.add;
			}
			ENDCG
		}
    }
} 