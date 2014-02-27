Shader "LLT/StencilZero" 
{
	Properties 
	{
	    _Color ("Main Color", Color) = (0,1,0,1)
	}
	SubShader 
	{ 
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		ColorMask 0
		Cull Off
		
		Stencil 
		{
			PassFront Zero
			PassBack Zero
		}

	    Pass
	    {
			SetTexture[_MainTex]
			{
				constantColor[_Color]
				combine constant 
			}
		}
    }
} 