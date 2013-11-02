using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LLT
{
	public sealed class EMSwfAnimationClipNode : ITSTreeNode
	{
		private readonly float _frameRate;
		private readonly List<EMSwfObject> _controlTags;
		private readonly string  _name;
		private readonly int _startIndex;
		private readonly List<KeyValuePair<EMSwfCurveKey, EMSwfAnimationCurve>> _curves;
		private readonly List<ITSTreeNode> _childs = new List<ITSTreeNode>();
		
		public EMSwfAnimationClipNode(float frameRate, string name, int startIndex, List<EMSwfObject> controlTags, List<KeyValuePair<EMSwfCurveKey, EMSwfAnimationCurve>> curves)
		{
			_frameRate = frameRate;
			_name = name;
			_startIndex = startIndex;
			_controlTags = controlTags;
			_curves = curves;
			
			var currentIndex = _startIndex;
			var endIndex = _startIndex;
			for(var i = 0; i < _controlTags.Count; i++)
			{
				var controlTag = _controlTags[i];
				if(controlTag is EMSwfShowFrame)
				{
					endIndex++;
				}
			}
			for(var i = 0; i < _controlTags.Count; i++)
			{
				var controlTag = _controlTags[i];
				if(controlTag is EMSwfShowFrame)
				{
					_childs.Add(new EMSwfAnimationKeyframeNode(_frameRate, _startIndex, endIndex - 1, currentIndex++, _curves));
				}
			}
		}
		
		public System.Collections.Generic.List<ITSTreeNode> Childs 
		{
			get 
			{
				return _childs;
			}
		}
		
		public int Length
		{
			get
			{
				var length = 0;
				for(var i = 0; i < _controlTags.Count; i++)
				{
					var controlTag = _controlTags[i];
					if(controlTag is EMSwfShowFrame)
					{
						length++;
					}
				}
				return length;
			}
		}

		public byte[] ToBytes ()
		{
			var EMAnimationClip = new EMAnimationClipStructLayout();
			EMAnimationClip.Length = Length / _frameRate;
			
			var bytes = new byte[Marshal.SizeOf(EMAnimationClip.GetType())];
			
			var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
			Marshal.StructureToPtr(EMAnimationClip, handle.AddrOfPinnedObject(), false);
			handle.Free();
			
			return bytes;
		}

		public int FactoryTypeIndex 
		{
			get 
			{
				return (int)EMFactory.Type.EMAnimationClip;
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
				throw new NotImplementedException ();
			}
		}

		public int SizeOf 
		{
			get 
			{
				return EMAnimationClip.EMAnimationClipSizeOf;
			}
		}
	}
}