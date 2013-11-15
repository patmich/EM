namespace LLT
{
	public class EMSwfColorTransform
	{
	    private bool _hasAddTerms;
	    private bool _hasMultTerms;
	
	    public byte RedMultTerm { get; private set; }
	    public byte GreenMultTerm { get; private set; }
	    public byte BlueMultTerm { get; private set; }
	    public byte AlphaMultTerm { get; private set; }
	    public byte RedAddTerm { get; private set; }
	    public byte GreenAddTerm { get; private set; }
	    public byte BlueAddTerm { get; private set; }
	    public byte AlphaAddTerm { get; private set; }
	
		public EMSwfColorTransform()
	    {
			_hasAddTerms = true;
			_hasMultTerms = true;
		}
	    public EMSwfColorTransform(EMSwfBinaryReader reader, bool alpha)
	    {
	        reader.Align(true);
	
	        _hasAddTerms = reader.ReadBits(1, false) == 0x1;
	        _hasMultTerms = reader.ReadBits(1, false) == 0x1;
	        
	        RedMultTerm = 255;
	        GreenMultTerm = 255;
	        BlueMultTerm = 255;
	        AlphaMultTerm = 255;
	
	        RedAddTerm = 0;
	        GreenAddTerm = 0;
	        BlueAddTerm = 0;
	        AlphaAddTerm = 0;
	
			var bitCount = (int)reader.ReadBits(4, false);
			
	        if (_hasMultTerms)
	        {
	            RedMultTerm = (byte)UnityEngine.Mathf.Clamp(reader.ReadBits(bitCount, true), 0, 255);
	            GreenMultTerm = (byte)UnityEngine.Mathf.Clamp(reader.ReadBits(bitCount, true), 0, 255);
	            BlueMultTerm = (byte)UnityEngine.Mathf.Clamp(reader.ReadBits(bitCount, true), 0, 255);
	
	            if (alpha)
	            {
	                AlphaMultTerm = (byte)UnityEngine.Mathf.Clamp(reader.ReadBits(bitCount, true), 0, 255);
	            }
	            else
	            {
	                AlphaMultTerm = 0;
	            }
	        }
	        if (_hasAddTerms)
	        {
	            RedAddTerm = (byte)UnityEngine.Mathf.Clamp(reader.ReadBits(bitCount, true), 0, 255);
	            GreenAddTerm = (byte)UnityEngine.Mathf.Clamp(reader.ReadBits(bitCount, true), 0, 255);
	            BlueAddTerm = (byte)UnityEngine.Mathf.Clamp(reader.ReadBits(bitCount, true), 0, 255);
	
	            if (alpha)
	            {
	                AlphaAddTerm = (byte)UnityEngine.Mathf.Clamp(reader.ReadBits(bitCount, true), 0, 255);
	            }
	            else
	            {
	                AlphaAddTerm = 0;
	            }
	        }
	
	        reader.Align(false);
	    }
		
		public static EMSwfColorTransform Identity
		{
			get
			{
				return new EMSwfColorTransform(){RedMultTerm = 255, BlueMultTerm = 255, GreenMultTerm = 255, AlphaMultTerm = 255};
			}
		}
		
		public override string ToString ()
		{
			return string.Format ("[EMSwfColorTransform: RedMultTerm={0}, GreenMultTerm={1}, BlueMultTerm={2}, AlphaMultTerm={3}, RedAddTerm={4}, GreenAddTerm={5}, BlueAddTerm={6}, AlphaAddTerm={7}]", RedMultTerm, GreenMultTerm, BlueMultTerm, AlphaMultTerm, RedAddTerm, GreenAddTerm, BlueAddTerm, AlphaAddTerm);
		}
	}
}