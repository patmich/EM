namespace LLT
{
	public sealed class EMSwfShowFrame : EMSwfObject
	{
	    public EMSwfShowFrame()
	    {
	        _tag = new EMSwfTag(1, 0);
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