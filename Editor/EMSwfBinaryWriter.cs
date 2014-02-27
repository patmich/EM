using System.IO;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;

public sealed class EMSwfBinaryWriter : BinaryWriter
{
    private EMSwfMemoryStream _stream;
    private int _position;

    public EMSwfBinaryWriter(EMSwfMemoryStream stream) : base(stream)
    {
        _stream = stream;
    }
	
    public void WriteBits(uint value, int count, bool signed)
    {
        if (signed)
        {
            _stream.SetBit(_position++, (value & (1 << (count - 1))) > 0);
        }

        for (var i = signed ? 1 : 0; i < count; i++)
        {
            _stream.SetBit(_position++, (value & (1 << (count - i - 1))) > 0);
        }
    }

    public int BitCount(float value)
    {
        var high = (int)value;
        for (var i = 0; i < 16; i++)
        {
            if ((high & (1 << (15 - i))) > 0)
            {
                return 16 - i + 1 + 16;
            }
        }

        return 16 + 1;
    }

    public int BitCount(bool signed, params int[] values)
    {
        var value = values.ToList().Max(x => Math.Abs(x));

        for (var i = (signed ? 1 : 0); i < 32; i++)
        {
            if ((value & (1 << (31 - i))) > 0)
            {
                return 32 - i + (signed ? 1 : 0);
            }
        }

        return (signed ? 1 : 0);
    }

    public void WriteString(string value)
    {
        var bytes = System.Text.ASCIIEncoding.ASCII.GetBytes(value);
        _stream.Write(bytes, 0, bytes.Length);
        _stream.WriteByte(0);
    }

    public void WriteFloat(float value)
    {
        _stream.SetBit(_position++, value < 0);

        var high = (int)value;
        var started = false;
        for (var i = 0; i < 16; i++)
        {
            if ((high & (1 << (15 - i))) > 0 || started)
            {
                started = true;
                _stream.SetBit(_position++, (high & (1 << (15 - i))) > 0);
            }
        }

        uint low = (uint)((value - high) * Math.Pow(2, 16));
        var position = _position;
        _position += 16;

        for (var i = 15; i >= 0; i--)
        {
            _stream.SetBit(position + i, (low & (1 << 15 - i)) > 0);
        }
    }
    public void Align(bool start)
    {
        if (start)
        {
            _position = (int)_stream.Position * 8;
        }
        else
        {
			_stream.Position = _position / 8 + ((_position % 8 == 0) ? 0 : 1);
			_position = 0;
        }
    }
}