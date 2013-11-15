using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LLT
{
	public class EMSwfUnhandledObject : EMSwfObject
	{
	    private byte[] _bytes;
	
	    public override void Read(EMSwfImporter importer, EMSwfTag tag, EMSwfBinaryReader reader)
	    {
	        _tag = tag;
	        _bytes = new byte[tag.Length];
	
	        reader.Read(_bytes, 0, _bytes.Length);
	    }
	
	    public override void Write(EMSwfBinaryWriter writer)
	    {
	        _tag.Write(writer);
	        writer.Write(_bytes);
	    }
	}
	
	public sealed class EMSwfFileAttributes : EMSwfUnhandledObject
	{
	}
	
	public sealed class EMSwfSetBackgroundColor : EMSwfUnhandledObject
	{
	}
	
	public sealed class EMSwfDefineBitsLossless2 : EMSwfUnhandledObject
	{
	}
	
	public sealed class EMSwfDefineBitsLossless : EMSwfUnhandledObject
	{
	}
	
	public sealed class EMSwfDefineBitsJPEG3 : EMSwfUnhandledObject
	{
	}
	
	public sealed class EMSwfDefineBits : EMSwfUnhandledObject
	{
	}
	
	public sealed class EMSwfJPEGTables : EMSwfUnhandledObject
	{
	}
}