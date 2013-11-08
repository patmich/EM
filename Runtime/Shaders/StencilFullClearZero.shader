Shader "LLT/StencilFullClearZero" 
{
	SubShader 
	{ 
		Stencil 
		{
			Comp always
			Pass zero
		}
	    Pass
	    {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			
			struct v2f 
			{
			    float4  pos : SV_POSITION;
			};
			
			float4 _MainTex_ST;
			
			v2f vert (appdata_base v)
			{
			    v2f o;
			    o.pos = v.vertex;
			    return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
			    return half4(0,0,0,0);
			}
			
			ENDCG
		}
    }
} 