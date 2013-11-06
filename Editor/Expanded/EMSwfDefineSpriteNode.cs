using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LLT
{
	public sealed class EMSwfDefineSpriteNode : ITSTreeNode
	{
	    private string _name;
		private bool _placed;
		
	    private EMSwfMatrix _matrix;
		private EMSwfColorTransform _cxForm;
		
	    private EMSwfDefineSprite _defineSprite;
		
		public int ClipDepth { get; private set; }
		public int ClipCount { get; set; }
		
	    internal EMSwfDefineSpriteNode(string name, bool placed, EMSwfMatrix matrix, EMSwfColorTransform cxForm, int clipDepth, EMSwfDefineSprite defineSprite)
	    {
	        _name = name;
			_placed = placed;
	        _matrix = matrix;
			_cxForm = cxForm;
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
			var sprite = new EMSpriteStructLayout();
			sprite.Transform = new EMTransformStructLayout();
			sprite.Transform.M00 = _matrix.M00;
			sprite.Transform.M01 = _matrix.M01;
			sprite.Transform.M02 = _matrix.M02;
			sprite.Transform.M10 = _matrix.M10;
			sprite.Transform.M11 = _matrix.M11;
			sprite.Transform.M12 = _matrix.M12;
			
			sprite.Transform.MA = _cxForm.AlphaMultTerm;
			sprite.Transform.MR = _cxForm.RedMultTerm;
			sprite.Transform.MG = _cxForm.GreenMultTerm;
			sprite.Transform.MB = _cxForm.BlueMultTerm;
			
			sprite.Transform.OA = _cxForm.AlphaAddTerm;
			sprite.Transform.OR = _cxForm.RedAddTerm;
			sprite.Transform.OG = _cxForm.GreenAddTerm;
			sprite.Transform.OB = _cxForm.BlueAddTerm;
			
			sprite.Transform.Placed = (byte)(_placed ? 1 : 0);

			CoreAssert.Fatal(ClipCount < ushort.MaxValue);
			sprite.ClipCount = (ushort)ClipCount;
			
			var bytes = new byte[Marshal.SizeOf(sprite.GetType())];
			
			var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
			Marshal.StructureToPtr(sprite, handle.AddrOfPinnedObject(), false);
			handle.Free();
			
			return bytes;
		}
	}
}