Shader "Custom/StencilIncrement" 
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
		ColorMask 0
		AlphaTest Greater 0.5
    	
    	Stencil 
		{
			Ref [_Ref]
			Comp equal
			Pass IncrSat
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