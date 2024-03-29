﻿Shader "LLT/StencilIncrement" 
{
	Properties 
	{
	    _Color ("Main Color", Color) = (1,1,1,0.5)
	    _MainTex ("Texture", 2D) = "white" { } 
	    _Ref ("Ref", Float) = 0
	}
	SubShader 
	{ 
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		ColorMask 0
		AlphaTest Greater 0.5
    	Cull Off
    	
    	Stencil 
		{
			Ref [_Ref]
			CompFront equal
			CompBack equal
			PassFront IncrSat
			PassBack IncrSat
		}

	    Pass
	    {
			SetTexture[_MainTex]
			{
				combine texture 
			}
		}
    }
} 