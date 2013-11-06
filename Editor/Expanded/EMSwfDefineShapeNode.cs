using System.Runtime.InteropServices;

namespace LLT
{
	public sealed class EMSwfDefineShapeNode : ITSTreeNode
	{
		private string _name;
	    private bool _placed;
		private EMSwfMatrix _matrix;
		private EMSwfColorTransform _cxForm;
	    private EMSwfDefineShape _defineShape;
		
		public int ClipDepth { private set; get; }
		public int ClipCount { set; get; }
		
	    internal EMSwfDefineShapeNode(string name, bool placed, EMSwfMatrix matrix, EMSwfColorTransform cxForm, int clipDepth, EMSwfDefineShape defineShape)
	    {
			_name = name;
			_placed = placed;
	        _matrix = matrix;
			_cxForm = cxForm;
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
			var shape = new EMShapeStructLayout();
			shape.Transform = new EMTransformStructLayout();
			shape.Transform.M00 = _matrix.M00;
			shape.Transform.M01 = _matrix.M01;
			shape.Transform.M02 = _matrix.M02;
			shape.Transform.M10 = _matrix.M10;
			shape.Transform.M11 = _matrix.M11;
			shape.Transform.M12 = _matrix.M12;
			
			shape.Transform.MA = _cxForm.AlphaMultTerm;
			shape.Transform.MR = _cxForm.RedMultTerm;
			shape.Transform.MG = _cxForm.GreenMultTerm;
			shape.Transform.MB = _cxForm.BlueMultTerm;
			
			shape.Transform.OA = _cxForm.AlphaAddTerm;
			shape.Transform.OR = _cxForm.RedAddTerm;
			shape.Transform.OG = _cxForm.GreenAddTerm;
			shape.Transform.OB = _cxForm.BlueAddTerm;
			
			shape.Transform.Placed = (byte)(_placed ? 1 : 0);
			
			shape.Rect = new EMRectStructLayout();
			shape.Rect.X = _defineShape.Bounds.XMin;
			shape.Rect.Y = _defineShape.Bounds.YMin;
			shape.Rect.Width = _defineShape.Bounds.XMax - _defineShape.Bounds.XMin;
			shape.Rect.Height = _defineShape.Bounds.YMax - _defineShape.Bounds.YMin;
			
			shape.Uv = _defineShape.Uv;
			
			CoreAssert.Fatal(ClipCount < ushort.MaxValue);
			shape.ClipCount = (ushort)ClipCount;
			
			var bytes = new byte[Marshal.SizeOf(shape.GetType())];
			
			var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
			Marshal.StructureToPtr(shape, handle.AddrOfPinnedObject(), false);
			handle.Free();
			
			return bytes;
		}
	}
}