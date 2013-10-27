namespace LLT
{
	public sealed class EMSwfPlaceObject2 : EMSwfObject
	{
		private string _name;
		
	    public bool PlaceFlagHasClipActions { get; private set; }
	    public bool PlaceFlagHasClipDepth { get; private set; }
	    public bool PlaceFlagHasName { get; private set; }
	    public bool PlaceFlagHasRatio { get; private set; }
	    public bool PlaceFlagHasColorTransform { get; private set; }
	    public bool PlaceFlagHasMatrix { get; private set; }
	    public bool PlaceFlagHasCharacter { get; private set; }
	    public bool PlaceFlagMove { get; private set; }
	    public ushort Depth { get; private set; }
	    public ushort RefId { get; private set; }
	    public EMSwfMatrix Matrix { get; private set; }
	    public EMSwfColorTransform CXform { get; private set; }
	    public ushort Ratio { get; private set; }
	    public ushort ClipDepth { get; private set; }
	    public byte[] Bytes { get; private set; }
		
	    public string Name
	    {
	        get
	        {
	            if (!PlaceFlagHasName)
	            {
	                return string.Format("{0}-{1}", RefId, Depth);
	            }
	            return _name;
	        }
	    }
	
		public bool HasName()
		{
			return PlaceFlagHasName;
		}
		
	    public bool IsNewCharacter()
	    {
	        return PlaceFlagHasCharacter && !PlaceFlagMove;
	    }
	
	    public bool IsCharacterAtDepthModified()
	    {
	        return !PlaceFlagHasCharacter && PlaceFlagMove;
	    }
		
		public bool HasMatrix()
		{
			return PlaceFlagHasMatrix;
		}
		
		public bool HasMove()
		{
			return PlaceFlagMove;
		}
		
	    public bool IsCharacterAtDepthReplaced()
	    {
	        return PlaceFlagHasCharacter && PlaceFlagMove;
	    }
		
		public bool HasClipDepth()
		{
			return PlaceFlagHasClipDepth;
		}
		
	    public EMSwfPlaceObject2()
	    {
	    }
	
	    public EMSwfPlaceObject2(ushort depth, ushort refId, EMSwfMatrix matrix)
	    {
	        PlaceFlagHasClipActions = false;
	        PlaceFlagHasClipDepth = false;
	        PlaceFlagHasName = false;
	        PlaceFlagHasRatio = false;
	        PlaceFlagHasColorTransform = false;
	        PlaceFlagHasMatrix = true;
	        PlaceFlagHasCharacter = true;
	        PlaceFlagMove = false;
	
	        RefId = refId;
	        Matrix = matrix;
	
	        using (var stream = new EMSwfMemoryStream())
	        {
	            var binaryWriter = new EMSwfBinaryWriter(stream);
	            Matrix.Write(binaryWriter);
	
	            _tag = new EMSwfTag(26, (ushort)(5 + binaryWriter.BaseStream.Position));
	        }
	    }
	
	    public override void Read(EMSwfImporter importer, EMSwfTag tag, EMSwfBinaryReader reader)
	    {
	        _tag = tag;
	
	        var position = reader.BaseStream.Position;
	
	        reader.Align(true);
	        PlaceFlagHasClipActions = reader.ReadBits(1, false) == 0x1;
	        PlaceFlagHasClipDepth = reader.ReadBits(1, false) == 0x1;
	        PlaceFlagHasName = reader.ReadBits(1, false) == 0x1;
	        PlaceFlagHasRatio = reader.ReadBits(1, false) == 0x1;
	        PlaceFlagHasColorTransform = reader.ReadBits(1, false) == 0x1;
	        PlaceFlagHasMatrix = reader.ReadBits(1, false) == 0x1;
	        PlaceFlagHasCharacter = reader.ReadBits(1, false) == 0x1;
	        PlaceFlagMove = reader.ReadBits(1, false) == 0x1;
	        reader.Align(false);
	
	        Depth = reader.ReadUInt16();
	
	        if (PlaceFlagHasCharacter)
	        {
	            RefId = reader.ReadUInt16();
	        }
			
	        if (PlaceFlagHasMatrix)
	        {
	            Matrix = new EMSwfMatrix(reader);
	        }
	
	        if (PlaceFlagHasColorTransform)
	        {
	            CXform = new EMSwfColorTransform(reader, true);
	        }
	        if (PlaceFlagHasRatio)
	        {
	            Ratio = reader.ReadUInt16();
	        }
	        if (PlaceFlagHasName)
	        {
	            _name = reader.ReadString();
	        }
	        if (PlaceFlagHasClipDepth)
	        {
	            ClipDepth = reader.ReadUInt16();
	        }
			
	        Bytes = reader.ReadBytes((int)(_tag.Length - (reader.BaseStream.Position - position)));
	    }
	
	    public override void Write(EMSwfBinaryWriter writer)
	    {
	       _tag.Write(writer);
	
	        writer.Align(true);
	        writer.WriteBits((uint)(PlaceFlagHasClipActions ? 1 : 0), 1, false);
	        writer.WriteBits((uint)(PlaceFlagHasClipDepth ? 1 : 0), 1, false);
	        writer.WriteBits((uint)(PlaceFlagHasName ? 1 : 0), 1, false);
	        writer.WriteBits((uint)(PlaceFlagHasRatio ? 1 : 0), 1, false);
	        writer.WriteBits((uint)(PlaceFlagHasColorTransform ? 1 : 0), 1, false);
	        writer.WriteBits((uint)(PlaceFlagHasMatrix ? 1 : 0), 1, false);
	        writer.WriteBits((uint)(PlaceFlagHasCharacter ? 1 : 0), 1, false);
	        writer.WriteBits((uint)(PlaceFlagMove ? 1 : 0), 1, false);
	        writer.Align(false);
	
	        writer.Write(Depth);
	
	        if(PlaceFlagHasCharacter)
	        {
	            writer.Write(RefId);
	        }
	
	        if (PlaceFlagHasMatrix)
	        {
	            Matrix.Write(writer);
	        }
	
	        if (PlaceFlagHasColorTransform)
	        {
	            throw new System.Exception("Need Support");
	        }
	
	        if (PlaceFlagHasRatio)
	        {
	            writer.Write(Ratio);
	        }
	
	        if (PlaceFlagHasName)
	        {
	            writer.Write(_name);
	        }
	
	        if (PlaceFlagHasClipDepth)
	        {
	            writer.Write(ClipDepth);
	        }
	    }
	}
}
