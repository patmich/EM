using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;

namespace LLT
{
	public sealed class EMSwfAnimationKeyframeNode : ITSTreeNode
	{
		private readonly float _frameRate;
		private readonly int _startIndex;
		private readonly int _endIndex;
		private readonly int _index;
		private readonly List<KeyValuePair<EMSwfCurveKey, EMSwfAnimationCurve>> _curves;
		private readonly List<ITSTreeNode> _childs = new List<ITSTreeNode>();
		
		public EMSwfAnimationKeyframeNode(float frameRate, int startIndex, int endIndex, int index, List<KeyValuePair<EMSwfCurveKey, EMSwfAnimationCurve>> curves)
		{
			_frameRate = frameRate;
			_startIndex = startIndex;
			_endIndex = endIndex;
			_index = index;
			_curves = curves;

			for(var curveIndex = 0; curveIndex < _curves.Count; curveIndex++)
			{
				var curve = _curves[curveIndex];
				if(_index == _startIndex || curve.Value.HasValue(_index))
				{
					_childs.Add(new EMSwfAnimationKeyframeValueNode(curve.Key, curve.Value.Sample(_index)));
				}
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
				return string.Empty;
			}
			set
			{
				throw new System.NotImplementedException ();
			}
		}
		
		public int Index
		{
			get
			{
				return _index;
			}
		}

		public byte[] ToBytes ()
		{
			var animationKeyframe = new EMAnimationKeyframeStructLayout();
			animationKeyframe.Time = (Index - _startIndex) / _frameRate;
			
			var bytes = new byte[SizeOf];
			
			var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
			Marshal.StructureToPtr(animationKeyframe, handle.AddrOfPinnedObject(), false);
			handle.Free();
			
			var position = EMAnimationKeyframe.EMAnimationKeyframeSizeOf;
			for(var childIndex = 0; childIndex < _childs.Count; childIndex++)
			{
				System.Buffer.BlockCopy(_childs[childIndex].ToBytes(), 0, bytes, position, EMAnimationKeyframeValue.EMAnimationKeyframeValueSizeOf);
				position += EMAnimationKeyframeValue.EMAnimationKeyframeValueSizeOf;
			}

			return bytes;
		}

		public int FactoryTypeIndex
		{
			get 
			{
				return (int)EMFactory.Type.EMAnimationKeyframe;
			}
		}

		public int SizeOf 
		{
			get 
			{
				return EMAnimationKeyframe.EMAnimationKeyframeSizeOf + _childs.Count * EMAnimationKeyframeValue.EMAnimationKeyframeValueSizeOf;
			}
		}
	}
}