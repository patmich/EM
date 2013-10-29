namespace LLT
{
	public sealed class EMSwfFrameLabel : EMSwfObject
	{
	    private string _label;
		public string Label 
		{
			get
			{
				return _label;
			}
		}
		
		public EMSwfFrameLabel()
	    {
	    }
		
	    public EMSwfFrameLabel(string label)
	    {
	        _tag = new EMSwfTag(43, (ushort)(label.Length + 1));
	        _label = label;
	    }
	
	    public override void Read(EMSwfImporter importer, EMSwfTag tag, EMSwfBinaryReader reader)
	    {
	        _tag = tag;
	        _label = reader.ReadString();
	    }
	
	    public override void Write(EMSwfBinaryWriter writer)
	    {
	        _tag.Write(writer);
	        writer.WriteString(_label);
	    }
	}
}