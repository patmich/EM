using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LLT
{
	public sealed class EMSwfSymbolClass : EMSwfObject
	{
	    private ushort _numSymbols;
	    private readonly Dictionary<ushort, string> _map = new Dictionary<ushort,string>();
	
	    public Dictionary<ushort, string>  Map 
	    {
	        get
	        {
	            return _map;
	        }
	    }
	
	    public override void Read(EMSwfImporter importer, EMSwfTag tag, EMSwfBinaryReader reader)
	    {
	        _tag = tag;
	        _numSymbols = reader.ReadUInt16();
	        for (var i = 0; i < _numSymbols; i++)
	        {
	            var id = reader.ReadUInt16();
	            var name = reader.ReadString();
	            _map.Add(id, name);
	        }
	    }
	
	    public override void Write(EMSwfBinaryWriter writer)
	    {
	        throw new NotImplementedException();
	    }
	}
}