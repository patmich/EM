using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LLT
{
	public sealed class EMSwfAnimationKeyframeNode : ITSTreeNode
	{
		private readonly float _frameRate;
		private readonly int _startIndex;
		private readonly int _index;
		private readonly List<KeyValuePair<EMSwfCurveKey, EMSwfAnimationCurve>> _curves;
		
		public EMSwfAnimationKeyframeNode(float frameRate, int startIndex, int index, List<KeyValuePair<EMSwfCurveKey, EMSwfAnimationCurve>> curves)
		{
			_frameRate = frameRate;
			_startIndex = startIndex;
			_index = index;
			_curves = curves;
		}
		
		public System.Collections.Generic.List<ITSTreeNode> Childs 
		{
			get 
			{
				var childs = new List<ITSTreeNode>();
				
				for(var curveIndex = 0; curveIndex < _curves.Count; curveIndex++)
				{
					var curve = _curves[curveIndex];
					if(_index == _startIndex || curve.Value.HasValue(_index))
					{
						childs.Add(new EMSwfAnimationKeyframeValueNode(curve.Key, curve.Value.Sample(_index)));
					}
				}
				
				return childs;
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
			var EMAnimationKeyframe = new EMAnimationKeyframeStructLayout();
			EMAnimationKeyframe.Time = Index / _frameRate;
			
			var bytes = new byte[SizeOf];
			var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
			Marshal.StructureToPtr(EMAnimationKeyframe, handle.AddrOfPinnedObject(), false);
			handle.Free();
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
				return EMAnimationKeyframe.EMAnimationKeyframeSizeOf;
			}
		}
	}
}