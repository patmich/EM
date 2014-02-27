using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace LLT
{
	public sealed class EMSwfTextNode : ITSTreeNode
	{
		private string _name;
		private bool _placed;
		private int _depth;
		private EMSwfMatrix _matrix;
		private EMSwfColorTransform _cxForm;
		private int _clipDepth;
		private EMSwfText _text;

		internal EMSwfTextNode(string name, bool placed, int depth, EMSwfMatrix matrix, EMSwfColorTransform cxForm, int clipDepth, EMSwfText text)
		{
			_name = name;
			_placed = placed;
			_depth = depth;
			_matrix = matrix;
			_cxForm = cxForm;
			_clipDepth = clipDepth;
			_text = text;
		}

		public int FactoryTypeIndex 
		{
			get 
			{
				return (int)EMFactory.Type.EMText;
			}
		}

		public System.Collections.Generic.List<ITSTreeNode> Childs 
		{
			get 
			{
				return new System.Collections.Generic.List<ITSTreeNode>();
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
				return EMText.EMTextSizeOf;	
			}
		}

		public byte[] ToBytes (List<string> lookup)
		{
			var text = new EMTextStructLayout();
			text.Transform = new EMTransformStructLayout();
			text.Transform.M00 = _matrix.M00;
			text.Transform.M01 = _matrix.M01;
			text.Transform.M02 = _matrix.M02;
			text.Transform.M10 = _matrix.M10;
			text.Transform.M11 = _matrix.M11;
			text.Transform.M12 = _matrix.M12;
			
			text.Transform.MA = _cxForm.AlphaMultTerm;
			text.Transform.MR = _cxForm.RedMultTerm;
			text.Transform.MG = _cxForm.GreenMultTerm;
			text.Transform.MB = _cxForm.BlueMultTerm;
			
			text.Transform.OA = _cxForm.AlphaAddTerm;
			text.Transform.OR = _cxForm.RedAddTerm;
			text.Transform.OG = _cxForm.GreenAddTerm;
			text.Transform.OB = _cxForm.BlueAddTerm;
			
			text.Transform.Placed = (byte)(_placed ? 1 : 0);
		
			CoreAssert.Fatal(_depth < ushort.MaxValue);
			text.Depth = (ushort)_depth;
			
			CoreAssert.Fatal(_clipDepth < ushort.MaxValue);
			text.ClipDepth = (ushort)_clipDepth;
		
			var index = lookup.IndexOf(_text.FontId);
			if(index == -1)
			{
				index = lookup.Count;
				lookup.Add(_text.FontId);
			}

			CoreAssert.Fatal(index < ushort.MaxValue);
			text.FontIdIndex = (ushort)index;

			index = lookup.IndexOf(_text.Content);
			if(index == -1)
			{
				index = lookup.Count;
				lookup.Add(_text.Content);
			}

			CoreAssert.Fatal(index < ushort.MaxValue);
			text.ContentIndex = (ushort)index;

			CoreAssert.Fatal(_text.MaxCharCount < ushort.MaxValue);
			text.MaxCharCount = (ushort)_text.MaxCharCount;

			var bytes = new byte[Marshal.SizeOf(text.GetType())];
			
			var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
			Marshal.StructureToPtr(text, handle.AddrOfPinnedObject(), false);
			handle.Free();
			
			return bytes;
		}
	}
}