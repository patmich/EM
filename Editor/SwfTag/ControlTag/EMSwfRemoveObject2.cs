namespace LLT
{
	public sealed class EMSwfRemoveObject2 : EMSwfObject
	{
	    private ushort _depth;
		
		public float Depth
		{
			get
			{
				return _depth;
			}
		}
		
	    public EMSwfRemoveObject2()
	    {
	    }
	
	    public EMSwfRemoveObject2(ushort depth)
	    {
	        _tag = new EMSwfTag(28, 2);
	        _depth = depth;
	    }
	
	    public override void Read(EMSwfImporter importer, EMSwfTag tag, EMSwfBinaryReader reader)
	    {
	        _tag = tag;
	        _depth = reader.ReadUInt16();
	    }
	
	    public override void Write(EMSwfBinaryWriter writer)
	    {
	        _tag.Write(writer);
	        writer.Write(_depth);
	    }
	}
}