using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Runtime.InteropServices;

public sealed class EMSwfMemoryStream : Stream, IDisposable
{
    private byte[] _bytes;
	private GCHandle _handle;

	public EMSwfMemoryStream(string path)
	{
#if !UNITY_WEBPLAYER
        Init(File.ReadAllBytes(path));
#endif
	}

    public EMSwfMemoryStream()
    {
        Init(new byte[32]);
    }
    public EMSwfMemoryStream(byte[] bytes)
    {
        Init(bytes);
    }

    public void Init(byte[] bytes)
    {
        Dispose();

        Position = 0;
        _bytes = bytes;
        _handle = GCHandle.Alloc(_bytes, GCHandleType.Pinned);
    }

	#region implemented abstract members of Stream
	public override void Flush ()
	{

	}
	public override long Seek (long offset, SeekOrigin origin)
	{
		throw new System.NotImplementedException ();
	}
	public override void SetLength (long value)
	{
		throw new System.NotImplementedException ();
	}
	public override int Read (byte[] buffer, int offset, int count)
	{
		count = Math.Min (count, (int)(Length - Position)); 
		Array.Copy(_bytes, Position + offset, buffer, 0, count);
		Position += offset + count;

		return count;
	}
	public override void Write (byte[] buffer, int offset, int count)
	{
        count = (buffer.Length - offset) > count ? count : buffer.Length - offset;
        while (_bytes.Length < Position + count)
        {
            Grow();
        }

        Buffer.BlockCopy(buffer, offset, _bytes, (int)Position, count);
        Position += count;
	}
	public override bool CanRead 
	{
		get 
		{
			return true;
		}
	}
	public override bool CanSeek 
	{
		get 
		{
			return false;
		}
	}
	public override bool CanWrite 
	{
		get 
		{
			return true;
		}
	}
	public override long Length 
	{
		get 
		{
			return _bytes.Length;
		}
	}
	public override long Position 
	{
		get;
		set;
	}
	public IntPtr Ptr
	{
		get
		{
			return new IntPtr (_handle.AddrOfPinnedObject().ToInt32() + (int)Position);
		}
	}

	#endregion

	~EMSwfMemoryStream()
	{
		Dispose ();
	}

	protected override void Dispose (bool disposing)
	{
		if (_handle.IsAllocated)
		{
			_handle.Free ();
		}
	}
	
    public uint GetBit(int bitPosition, bool bitfield)
    {
        return (uint)((_bytes[bitPosition / 8] >> (bitfield ? (7  - (bitPosition % 8)) : bitPosition % 8)) & 0x1);
    }

    public void SetBit(int bitPosition, bool bitfield)
    {
        if ((bitPosition / 8) >= Length)
        {
            Grow();
        }

        if (bitfield)
        {
            _bytes[bitPosition / 8] |= (byte)(0x1 << (7 - (bitPosition % 8)));
        }
        else
        {
            _bytes[bitPosition / 8] = (byte)(_bytes[bitPosition / 8] & (0xFF - (0x1 << (7 - (bitPosition % 8)))));
        }
    }

    public void Grow()
    {
        var bytes = new Byte[(_bytes.Length == 0 ? 32 : _bytes.Length << 1)];
        Buffer.BlockCopy(_bytes, 0, bytes, 0, _bytes.Length);
        _bytes = bytes;

        if (_handle.IsAllocated)
        {
            _handle.Free();
            _handle = GCHandle.Alloc(_bytes, GCHandleType.Pinned);
        }
    }
}