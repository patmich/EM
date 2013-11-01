using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LLT
{
	public sealed class EMSwfDefineSpriteNode : ITSTreeNode
	{
	    private string _name;
		private bool _placed;
	    private EMSwfMatrix _matrix;
	    private EMSwfDefineSprite _defineSprite;
		
		public int ClipDepth { get; private set; }
		public int ClipCount { get; set; }
		
	    internal EMSwfDefineSpriteNode(string name, bool placed, EMSwfMatrix matrix, int clipDepth, EMSwfDefineSprite defineSprite)
	    {
	        _name = name;
			_placed = placed;
	        _matrix = matrix;
			ClipDepth = clipDepth;
	        _defineSprite = defineSprite;
			_defineSprite.Expand();
		}
	
	    public System.Collections.Generic.List<ITSTreeNode> Childs
	    {
	        get
	        {
	            return _defineSprite.Childs;
	        }
	    }
		
		public int Id
		{
			get
			{
				return _defineSprite.Id;
			}
		}
		
	    public string Name
	    {
	        get
	        {
	            return _name;
	        }
			set
			{
				_name = value;
			}
	    }
	
	    public int FactoryTypeIndex
	    {
	        get
	        {
	            return (int)EMFactory.Type.EMSprite;
	        }
	    }
		
		public int SizeOf 
		{
			get 
			{
				return EMSprite.EMSpriteSizeOf;	
			}
		}
	
		public byte[] ToBytes ()
		{
			var EMSprite = new EMSpriteStructLayout();
			EMSprite.Transform = new EMTransformStructLayout();
			EMSprite.Transform.M00 = _matrix.M00;
			EMSprite.Transform.M01 = _matrix.M01;
			EMSprite.Transform.M02 = _matrix.M02;
			EMSprite.Transform.M10 = _matrix.M10;
			EMSprite.Transform.M11 = _matrix.M11;
			EMSprite.Transform.M12 = _matrix.M12;
			EMSprite.Transform.Placed = (byte)(_placed ? 1 : 0);

			CoreAssert.Fatal(ClipCount < ushort.MaxValue);
			EMSprite.ClipCount = (ushort)ClipCount;
			
			var bytes = new byte[Marshal.SizeOf(EMSprite.GetType())];
			
			var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
			Marshal.StructureToPtr(EMSprite, handle.AddrOfPinnedObject(), false);
			handle.Free();
			
			return bytes;
		}
	}
}