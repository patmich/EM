﻿namespace LLT
{
	public sealed class EMSwfDefineShape : EMSwfObject
	{
	    private EMSwfRect _bounds;
	    private byte[] _content;
	
		private float _scaleX = 0f;
		public float ScaleX
		{
			get
			{
				return _scaleX;
			}
			set
			{
				_scaleX = System.Math.Max(_scaleX, System.Math.Abs(value));
			}
		}

		private float _scaleY = 0f;
		public float ScaleY
		{
			get
			{
				return _scaleY;
			}
			set
			{
				_scaleY = System.Math.Max(_scaleY, System.Math.Abs(value));
			}
		}
	
		public bool Used { get; set; }

	    public EMSwfRect Bounds
	    {
	        get
	        {
	            return _bounds;
	        }
	    }
		
		internal EMRectStructLayout Uv { get; set; }
		internal int TextureIndex { get; set; }
		
	    public override void Read(EMSwfImporter importer, EMSwfTag tag, EMSwfBinaryReader reader)
	    {
	        var position = reader.BaseStream.Position;
	
	        _tag = tag;
	        _id = reader.ReadUInt16();
	        _bounds = new EMSwfRect(reader);
	
	        _content = new byte[tag.Length - (reader.BaseStream.Position - position)];
	        reader.Read(_content, 0, _content.Length);
	    }
	
	    /// <summary>
	    /// Write
	    /// </summary>
	    /// <param name="writer"></param>
	    public override void Write(EMSwfBinaryWriter writer)
	    {
	        _tag.Write(writer);
	        writer.Write(_id);
	        _bounds.Write(writer);
	        writer.Write(_content);
	    }
	}
}