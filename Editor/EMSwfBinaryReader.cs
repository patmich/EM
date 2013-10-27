using System.IO;
using System.Runtime.InteropServices;
using System;
using System.Collections;
using System.Collections.Generic;

public class EMSwfBinaryReader : BinaryReader
{
	private EMSwfMemoryStream _stream;
    private int _position;

	public EMSwfBinaryReader(EMSwfMemoryStream stream) : base(stream)
	{
		_stream = stream;
	}

    public float ReadFixed16(int count)
    {
        uint high = 0;
        uint low = 0;

        // Read leading sign bit.
		count--;
        var negative = _stream.GetBit(_position++, true) == 1;
		
		uint val = 0;
		for (var i = 0; i < count; i++)
        {
            var bit = _stream.GetBit(_position++, true);
            val |= bit << count - 1 - i;
        }
		
		if(negative)
		{
			val = (val - 1) ^ (uint)((1 << count) - 1);
		}
		
		low = val & 0xFFFF;
		
		if(count > 16)
		{
			high = val >> 16;
		}
		
        var retVal = (high + (float)(low / (float)(1 << 16))) * (negative ? -1 : 1);
        return retVal;
    }
	
	public float ReadFixed8()
    {
        uint high = 0;
        uint low = 0;
		
		uint val = (uint)_stream.ReadByte();
		val |= (uint)_stream.ReadByte() << 8;
		_position += 16;
		
		var negative = (0x8000 & val) > 0;
		if(negative)
		{
			val = (val - 1) ^ (uint)((1 << 15) - 1);
		}
		
		low = val & 0xFF;
		high = val >> 8;
		
        var retVal = (high + (float)(low / (float)(1 << 8))) * (negative ? -1 : 1);
        return retVal;
    }
	
    public uint ReadBits(int count, bool signed)
    {
        uint retVal = 0;
        if (signed && _stream.GetBit(_position, true) == 1)
        {
            retVal = 0xFFFFFFFF - (0xFFFFFFFF >> (32 - count));
        }
        for (var i = 0; i < count; i++)
        {
            retVal |= _stream.GetBit(_position++, true) << (count - i - 1);
        }
        return retVal;
    }

    public void Align(bool start)
    {
        if (start)
        {
            _position = (int)_stream.Position * 8;
        }
        else
        {
            _stream.Position = (int)Math.Ceiling(_position / 8f);
            _position = 0;
        }
    }
    public new string ReadString()
    {
        var retVal = Marshal.PtrToStringAnsi(_stream.Ptr);
        while (_stream.ReadByte() != 0) ;
        return retVal;
    }
}
