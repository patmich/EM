Shader "LLT/StencilDecrement" 
{
	Properties 
	{
	    _Color ("Main Color", Color) = (0,1,0,1)
	    _Ref ("Ref", Float) = 0
	}
	SubShader 
	{ 
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		ColorMask 0
		Cull Off
		
		Stencil 
		{
			Ref [_Ref]
			CompFront equal
			CompBack equal
			PassFront DecrSat
			PassBack DecrSat
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