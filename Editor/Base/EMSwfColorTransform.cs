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
	
	        if (_hasAddTerms)
	        {
	            var bitCount = (int)reader.ReadBits(4, false);
	            RedMultTerm = (byte)reader.ReadBits(bitCount, true);
	            GreenMultTerm = (byte)reader.ReadBits(bitCount, true);
	            BlueMultTerm = (byte)reader.ReadBits(bitCount, true);
	
	            if (alpha)
	            {
	                AlphaMultTerm = (byte)reader.ReadBits(bitCount, true);
	            }
	            else
	            {
	                AlphaMultTerm = 0;
	            }
	        }
	        if (_hasMultTerms)
	        {
	            var bitCount = (int)reader.ReadBits(4, false);
	            RedAddTerm = (byte)reader.ReadBits(bitCount, true);
	            GreenAddTerm = (byte)reader.ReadBits(bitCount, true);
	            BlueAddTerm = (byte)reader.ReadBits(bitCount, true);
	
	            if (alpha)
	            {
	                AlphaAddTerm = (byte)reader.ReadBits(bitCount, true);
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
	}
}