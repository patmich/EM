using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

public sealed class EMSwfHeader
{
    private byte _signature1;
    private byte _signature2;
    private byte _signature3;
    private byte _version;
    private uint _fileLength;
    private EMSwfRect _rect;
    private float _frameRate;
    private ushort _frameCount;
	
    public EMSwfHeader(byte version, uint fileLength, EMSwfRect rect, ushort frameRate, ushort frameCount)
    {
        _signature1 = (byte)'F';
        _signature2 = (byte)'W';
        _signature3 = (byte)'S';
        _version = version;
        _fileLength = fileLength;
        _rect = rect;
        _frameRate = frameRate;
        _frameCount = frameCount;
    }

    public EMSwfHeader(EMSwfBinaryReader binaryReader)
    {
        _signature1 = binaryReader.ReadByte();
        _signature2 = binaryReader.ReadByte();
        _signature3 = binaryReader.ReadByte();
        _version = binaryReader.ReadByte();
        _fileLength = binaryReader.ReadUInt32();
    }

    public void Read(EMSwfBinaryReader binaryReader)
    {
        _rect = new EMSwfRect(binaryReader);
		
		binaryReader.Align(true);
        _frameRate = binaryReader.ReadFixed8();
		binaryReader.Align(false);

        _frameCount = binaryReader.ReadUInt16();
    }

    public void Write(EMSwfBinaryWriter writer)
    {
        MakeUncompressed();
        writer.Write(_signature1);
        writer.Write(_signature2);
        writer.Write(_signature3);
        writer.Write(_version);

        var pos = writer.BaseStream.Position;

        writer.Write((uint)0);
        _rect.Write(writer);
        writer.Write((ushort)15360);
        writer.Write(_frameCount);

        var endPos = writer.BaseStream.Position;
        uint realLength = _fileLength + (uint)writer.BaseStream.Position;
        writer.BaseStream.Position = pos;
        writer.Write(realLength);
        writer.BaseStream.Position = endPos;
    }
	
    public uint FileLength
    {
        get
        {
            return _fileLength;
        }
    }
	
	public float FrameRate
	{
		get
		{
			return _frameRate;
		}
	}
	
    public bool IsCompressed()
    {
        return (char)_signature1 == 'C' && (char)_signature2 == 'W' && (char)_signature3 == 'S';
    }

    public void MakeUncompressed()
    {
        _signature1 = (byte)'F';
    }
}
