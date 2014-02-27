using System;
using System.Collections.Generic;
using System.Linq;

namespace LLT
{
	public sealed class EMSwfAnimation : ITSTreeNode
	{
		private float _frameRate;
		private EMSwfDefineSprite _sprite;
		private List<ITSTreeNode> _childs;
		
		public EMSwfAnimation(float frameRate, EMSwfDefineSprite sprite)
		{
			_frameRate = frameRate;
			_sprite = sprite;
		}
		
		public System.Collections.Generic.List<ITSTreeNode> Childs 
		{
			get 
			{
				if(_childs != null)
				{
					return _childs;
				}
				
				_childs = new List<ITSTreeNode>();
				var curves = _sprite.AnimationCurves;
				var label = "default";
				var start = 0;
				
				var index = 0;
				var startIndex = 0;
				
				var i = 0;
				for(i = 0; i < _sprite.ControlTags.Count; i++)
				{
					var controlTag = _sprite.ControlTags[i];
					if(controlTag is EMSwfShowFrame)
					{
						break;
					}
					else if(controlTag is EMSwfFrameLabel)
					{
						label = (controlTag as EMSwfFrameLabel).Label;
						start = i + 1;
					}
				}
				for(;i < _sprite.ControlTags.Count; i++)
				{
					var controlTag = _sprite.ControlTags[i];
					
					if(controlTag is EMSwfFrameLabel)
					{
						_childs.Add(new EMSwfAnimationClipNode(_frameRate, label, startIndex, _sprite.ControlTags.GetRange(start, i - start), curves));
						label = (controlTag as EMSwfFrameLabel).Label;
						
						startIndex = index;
						start = i + 1;
					}
					else if(controlTag is EMSwfShowFrame)
					{
						index++;
					}
				}
				
				if(i > start)
				{
					_childs.Add(new EMSwfAnimationClipNode(_frameRate, label, startIndex, _sprite.ControlTags.GetRange(start, i - start), curves));
				}
				
				_childs.RemoveAll((x) => x.Childs.Count == 0);
				
				return _childs;
			}
		}

		public byte[] ToBytes (List<string> lookup)
		{
			return new byte[]{};
		}

		public int FactoryTypeIndex 
		{
			get 
			{
				return (int)EMFactory.Type.EMAnimation;
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
				throw new NotImplementedException ();
			}
		}

		public int SizeOf 
		{
			get 
			{
				return EMAnimation.EMAnimationSizeOf;
			}
		}
	}
}