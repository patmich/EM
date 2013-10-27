using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

public sealed class EMSwfTag
{
    private ushort _content;
    private uint _length;
	
    public EMSwfTag(uint type, uint length)
    {
        _content = (ushort)(type << 6);
        _content += (ushort)(length > 0x3F ? (ushort)0x3F : length);
        _length = length;
    }

    public EMSwfTag(EMSwfBinaryReader reader)
    {
        _content = reader.ReadUInt16();
        _length = (uint)_content & 0x3F;
        if (_length == 0x3F)
        {
            _length = reader.ReadUInt32();
        }
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write((ushort)_content);
        if (((uint)_content & 0x3F) == 0x3F)
        {
            writer.Write(_length);
        }
    }
	
    public uint Length
    {
        get
        {
            return _length;
        }
    }
	
    public uint TagType
    {
        get
        {
            return (uint)_content >> 6;
        }
    }
	
    public uint TagLength
    {
        get
        {
            if (_length >= 0x3F)
            {
                return sizeof(ushort) + sizeof(uint);
            }
            return sizeof(ushort);
        }
    }
}
