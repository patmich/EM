using System.IO;
using System;

public struct EMSwfMatrix
{
    private byte[] _bytes;

    private float _scaleX;
    private float _scaleY;

    private float _rotateSkewX;
    private float _rotateSkewY;

    private float _translateX;
    private float _translateY;
	
	private bool _hasScale;
	private bool _hasRotate;
	
    public EMSwfMatrix(EMSwfBinaryReader reader)
    {
        var position = reader.BaseStream.Position;
        int bitCount = 0;
        reader.Align(true);
        _hasScale = reader.ReadBits(1, false) == 0x1;
        if (_hasScale)
        {
            bitCount = (int)reader.ReadBits(5, false);
            _scaleX = reader.ReadFixed16(bitCount);
            _scaleY = reader.ReadFixed16(bitCount);
        }
        else
        {
            _scaleX = 1f;
            _scaleY = 1f;
        }

       	_hasRotate = reader.ReadBits(1, false) == 0x1;
        if (_hasRotate)
        {
            bitCount = (int)reader.ReadBits(5, false);
            _rotateSkewX = reader.ReadFixed16(bitCount);
            _rotateSkewY = reader.ReadFixed16(bitCount);
        }
        else
        {
            _rotateSkewX = 0f;
            _rotateSkewY = 0f;
        }

        bitCount = (int)reader.ReadBits(5, false);
        _translateX = ((int)reader.ReadBits(bitCount, true))/20f;
        _translateY = ((int)reader.ReadBits(bitCount, true))/20f;

        reader.Align(false);

        _bytes = new byte[reader.BaseStream.Position - position];
        reader.BaseStream.Position = position;
        reader.Read(_bytes, 0, _bytes.Length);
    }

    public void Write(EMSwfBinaryWriter writer)
    {
        var bitCount = 0;
        writer.Align(true);

        writer.WriteBits((uint)(HasScale ? 1 : 0), 1, false);
        if (HasScale)
        {
            bitCount = writer.BitCount(Math.Max(_scaleX, _scaleY));
            writer.WriteBits((uint)bitCount, 5, false);
            writer.WriteFloat(_scaleX);
            writer.WriteFloat(_scaleY);
        }

        writer.WriteBits((uint)(HasRotate ? 1 : 0), 1, false);
        if (HasRotate)
        {
            bitCount = writer.BitCount(Math.Max(_rotateSkewX, _rotateSkewY));
            writer.WriteBits((uint)bitCount, 5, false);
            writer.WriteFloat(_rotateSkewX);
            writer.WriteFloat(_rotateSkewY);
        }

        uint translateX = (uint)(_translateX * 20);
        uint translateY = (uint)(_translateY * 20);
        bitCount = writer.BitCount(true, (int)translateX, (int)translateY);
        writer.WriteBits((uint)bitCount, 5, false);
        writer.WriteBits(translateX, bitCount, true);
        writer.WriteBits(translateY, bitCount, true);

        writer.Align(false);
    }

    public static EMSwfMatrix Identity
    {
        get
        {
            return new EMSwfMatrix() { _scaleX = 1f, _scaleY = 1f, _rotateSkewX = 0f, _rotateSkewY = 0f, _translateX = 0, _translateY = 0 };
        }
    }

    public static EMSwfMatrix Zero
    {
        get
        {
            return new EMSwfMatrix() { _scaleX = 0f, _scaleY = 0f, _rotateSkewX = 0f, _rotateSkewY = 0f, _translateX = 0, _translateY = 0 };
        }
    }

    public float M00
    {
        get
        {
            return _scaleX;
        }
        set
        {
			_hasScale = _scaleX != value;
            _scaleX = value;
        }
    }

    public float M11
    {
        get
        {
            return _scaleY;
        }
        set
        {
			_hasScale = _scaleY != value;
            _scaleY = value;
        }
    }

    public float M01
    {
        get
        {
            return _rotateSkewY;
        }
        set
        {
			_hasRotate = _rotateSkewY != value;
            _rotateSkewY = value;
        }
    }

    public float M10
    {
        get
        {
            return _rotateSkewX;
        }
        set
        {
			_hasRotate = _rotateSkewX != value;
            _rotateSkewX = value;
        }
    }

    public float M02
    {
        get
        {
            return _translateX;
        }
        set
        {
            _translateX = value;
        }
    }

    public float M12
    {
        get
        {
            return _translateY;
        }
        set
        {
            _translateY = value;
        }
    }

    public bool HasRotate
    {
        get
        {
            return _hasRotate;
        }
    }

    public bool HasScale
    {
        get
        {
            return _hasScale;
        }
    }
}