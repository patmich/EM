namespace LLT
{
	public class EMSwfColorTransform
	{
	    private bool _hasAddTerms;
	    private bool _hasMultTerms;
	
	    public uint RedMultTerm { get; private set; }
	    public uint GreenMultTerm { get; private set; }
	    public uint BlueMultTerm { get; private set; }
	    public uint AlphaMultTerm { get; private set; }
	    public uint RedAddTerm { get; private set; }
	    public uint GreenAddTerm { get; private set; }
	    public uint BlueAddTerm { get; private set; }
	    public uint AlphaAddTerm { get; private set; }
	
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
	            RedMultTerm = reader.ReadBits(bitCount, true);
	            GreenMultTerm = reader.ReadBits(bitCount, true);
	            BlueMultTerm = reader.ReadBits(bitCount, true);
	
	            if (alpha)
	            {
	                AlphaMultTerm = reader.ReadBits(bitCount, true);
	            }
	            else
	            {
	                AlphaMultTerm = 0;
	            }
	        }
	        if (_hasMultTerms)
	        {
	            var bitCount = (int)reader.ReadBits(4, false);
	            RedAddTerm = reader.ReadBits(bitCount, true);
	            GreenAddTerm = reader.ReadBits(bitCount, true);
	            BlueAddTerm = reader.ReadBits(bitCount, true);
	
	            if (alpha)
	            {
	                AlphaAddTerm = reader.ReadBits(bitCount, true);
	            }
	            else
	            {
	                AlphaAddTerm = 0;
	            }
	        }
	
	        reader.Align(false);
	    }
	}
}