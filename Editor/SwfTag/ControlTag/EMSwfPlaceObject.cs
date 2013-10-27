namespace LLT
{
	public sealed class EMSwfPlaceObject : EMSwfObject
	{
	    public ushort RefId { get; private set; }
	    public ushort Depth { get; private set; }
	    public EMSwfMatrix Matrix { get; private set; }
	    public EMSwfColorTransform CXForm { get; private set; }
	    
	    public override void Read(EMSwfImporter importer, EMSwfTag tag, EMSwfBinaryReader reader)
	    {
	        var position = reader.BaseStream.Position;
	
	        RefId = reader.ReadUInt16();
	        Depth = reader.ReadUInt16();
	        Matrix = new EMSwfMatrix(reader);
	
	        if (reader.BaseStream.Position - position <= tag.Length)
	        {
	            CXForm = new EMSwfColorTransform(reader, false);
	        }
	    }
	
	    public override void Write(EMSwfBinaryWriter writer)
	    {
	        throw new System.NotImplementedException();
	    }
	}
}