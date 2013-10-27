using System.IO;

namespace LLT
{
	public class EMSwfObject
	{
	    protected EMSwfTag _tag;
	    protected ushort _id;
	
	    public ushort Id
	    {
	        get
	        {
	            return _id;
	        }
	    }
	
	    public virtual void Read(EMSwfImporter importer, EMSwfTag tag, EMSwfBinaryReader reader)
		{
		}
	    public virtual void Write(EMSwfBinaryWriter writer)
		{
		}
	
	    public uint Length
	    {
	        get
	        {
	            return _tag.TagLength + _tag.Length;
	        }
	    }
		
		public EMSwfObject()
		{
		}
	}
}