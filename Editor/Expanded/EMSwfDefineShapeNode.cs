using System.Runtime.InteropServices;

namespace LLT
{
	public sealed class EMSwfDefineShapeNode : ITSTreeNode
	{
		private string _name;
	    private EMSwfMatrix _matrix;
	    private EMSwfDefineShape _defineShape;
		
		public int ClipDepth { private set; get; }
		public int ClipCount { set; get; }
		
	    internal EMSwfDefineShapeNode(string name, EMSwfMatrix matrix, int clipDepth, EMSwfDefineShape defineShape)
	    {
			_name = name;
	        _matrix = matrix;
			_defineShape = defineShape;
			ClipDepth = clipDepth;
	    }
		
	    public int FactoryTypeIndex
	    {
	        get
	        {
	            return (int)EMFactory.Type.EMShape;
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
		
		public int SizeOf 
		{
			get 
			{
				return EMShape.EMShapeSizeOf;
			}
		}
		public System.Collections.Generic.List<ITSTreeNode> Childs 
		{
			get 
			{
				return new System.Collections.Generic.List<ITSTreeNode>();
			}
		}
	
		public byte[] ToBytes ()
		{
			var EMShape = new EMShapeStructLayout();
			EMShape.Transform = new EMTransformStructLayout();
			EMShape.Transform.M00 = _matrix.M00;
			EMShape.Transform.M01 = _matrix.M01;
			EMShape.Transform.M02 = _matrix.M02;
			EMShape.Transform.M10 = _matrix.M10;
			EMShape.Transform.M11 = _matrix.M11;
			EMShape.Transform.M12 = _matrix.M12;
				
			EMShape.Rect = new EMRectStructLayout();
			EMShape.Rect.X = _defineShape.Bounds.XMin;
			EMShape.Rect.Y = _defineShape.Bounds.YMin;
			EMShape.Rect.Width = _defineShape.Bounds.XMax - _defineShape.Bounds.XMin;
			EMShape.Rect.Height = _defineShape.Bounds.YMax - _defineShape.Bounds.YMin;
			
			EMShape.Uv = _defineShape.Uv;
			
			CoreAssert.Fatal(ClipCount < ushort.MaxValue);
			EMShape.ClipCount = (ushort)ClipCount;
			
			var bytes = new byte[Marshal.SizeOf(EMShape.GetType())];
			
			var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
			Marshal.StructureToPtr(EMShape, handle.AddrOfPinnedObject(), false);
			handle.Free();
			
			return bytes;
		}
	}
}