namespace LLT
{
	public sealed class EMSwfEnd : EMSwfObject
	{
	    public EMSwfEnd()
	    {
	        _tag = new EMSwfTag(0, 0);
	    }
	
	    public override void Read(EMSwfImporter importer, EMSwfTag tag, EMSwfBinaryReader reader)
	    {
	        _tag = tag;
	    }
	
	    public override void Write(EMSwfBinaryWriter writer)
	    {
	        _tag.Write(writer);
	    }
	}
}