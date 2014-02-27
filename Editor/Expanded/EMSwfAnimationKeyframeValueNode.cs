using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LLT
{
	public sealed class EMSwfAnimationKeyframeValueNode : ITSTreeNode
	{
		private readonly EMSwfCurveKey _curveKey;
		private readonly float _value;
		
		public EMSwfAnimationKeyframeValueNode(EMSwfCurveKey curveKey, float value)
		{
			_curveKey = curveKey;
			_value = value;
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
				return string.Empty;
			}
			set
			{
				throw new System.NotImplementedException();
			}
		}
		
		public byte[] ToBytes (List<string> lookup)
		{
			var EMAnimationKeyframeValue = new EMAnimationKeyframeValueStructLayout();
			
			CoreAssert.Fatal(_curveKey.ChildIndex < ushort.MaxValue);
			EMAnimationKeyframeValue.ChildIndex = (ushort)_curveKey.ChildIndex;
			
			CoreAssert.Fatal((int)_curveKey.PropertyType < byte.MaxValue);
			EMAnimationKeyframeValue.PropertyType = (byte)_curveKey.PropertyType;
			
			EMAnimationKeyframeValue.Value = _value;
			
			CoreAssert.Fatal((int)_curveKey.Offset < byte.MaxValue);
			EMAnimationKeyframeValue.Offset = (byte)_curveKey.Offset;
			
			var bytes = new byte[SizeOf];
			var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
			Marshal.StructureToPtr(EMAnimationKeyframeValue, handle.AddrOfPinnedObject(), false);
			handle.Free();
			return bytes;
		}

		public int FactoryTypeIndex 
		{
			get 
			{
				return (int)EMFactory.Type.EMAnimationKeyframeValue;
			}
		}

		public int SizeOf 
		{
			get 
			{
				return EMAnimationKeyframeValue.EMAnimationKeyframeValueSizeOf;
			}
		}
		
		public override string ToString ()
		{
			return string.Format ("[EMSwfAnimationKeyframeValueNode: CurveKey={0}, Value={1}]", _curveKey, _value);
		}
	}
}