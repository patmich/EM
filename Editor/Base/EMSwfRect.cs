using System.IO;

public struct EMSwfRect
{
	private float _xmin;
    private float _xmax;
    private float _ymin;
    private float _ymax;

    public EMSwfRect(float xmin, float xmax, float ymin, float ymax)
    {
        _xmin = xmin;
        _xmax = xmax;
        _ymin = ymin;
        _ymax = ymax;
    }

	public EMSwfRect(EMSwfBinaryReader reader)
	{
        reader.Align(true);
        var count = (int)reader.ReadBits(5, false);
        _xmin = ((int)reader.ReadBits(count, true))/20f;
        _xmax = ((int)reader.ReadBits(count, true))/20f;
        _ymin = ((int)reader.ReadBits(count, true))/20f;
        _ymax = ((int)reader.ReadBits(count, true))/20f;
        reader.Align(false);
	}

    public void Write(EMSwfBinaryWriter writer)
    {
        writer.Align(true);
        var bitCount = writer.BitCount(true, (int)(_xmin * 20f), (int)(_xmax * 20f), (int)(_ymin * 20f), (int)(_ymax * 20f));
     
        writer.WriteBits((uint)bitCount, 5, false);
        writer.WriteBits((uint)(_xmin * 20f), bitCount, true);
        writer.WriteBits((uint)(_xmax * 20f), bitCount, true);
        writer.WriteBits((uint)(_ymin * 20f), bitCount, true);
        writer.WriteBits((uint)(_ymax * 20f), bitCount, true);
        writer.Align(false);
    }

    public float XMin
    {
        get
        {
            return _xmin;
        }
    }

    public float XMax
    {
        get
        {
            return _xmax;
        }
    }

    public float YMin
    {
        get
        {
            return _ymin;
        }
    }

    public float YMax
    {
        get
        {
            return _ymax;
        }
    }
}