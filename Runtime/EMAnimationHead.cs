using UnityEngine;
using System.Collections.Generic;

namespace LLT
{
	public sealed class EMAnimationHead : EMComponent
	{
		[SerializeField]
		private TextAsset _data;
		
		private ITSTreeStream _tree;
		private TSTreeStreamEntry _entry;
		
		private TSTreeStreamSiblingEnumerator _childs;
		private readonly List<int> _positions = new List<int>();
		
		private EMAnimationTreeStream _animationTree;
		
		private EMAnimationClip _animationClip;
		
		private TSTreeStreamSiblingEnumerator _keyframesEnumerator;

#if !ALLOW_UNSAFE
#else
		private TSTreeStreamSiblingEnumerator _keyframeValuesEnumerator;
#endif
		
		private float _time;
		
		public override void Init(EMObject obj)
		{
			base.Init(obj);
			
			_entry = new TSTreeStreamEntry();
			_entry.Init(_object.Tree);
		
			_positions.Clear();
			
			_childs = new TSTreeStreamSiblingEnumerator(_object.Tree);
			_childs.Init(_object.Tag);
			
			while(_childs.MoveNext())
			{
				_positions.Add(_childs.Current.EntryPosition);
			}
			
			_animationTree = new EMAnimationTreeStream();
			_animationTree.InitFromBytes(_data.bytes, null, new EMFactory());
			
			var clipEnumerator = new TSTreeStreamSiblingEnumerator(_animationTree);
			clipEnumerator.Init(_animationTree.RootTag);
			clipEnumerator.MoveNext();
			
			_animationClip = new EMAnimationClip();
			_animationClip.Init(_animationTree);
			_animationClip.Position = clipEnumerator.Current.EntryPosition;
		
			_keyframesEnumerator = new TSTreeStreamSiblingEnumerator(_animationTree);
			_keyframesEnumerator.Init(clipEnumerator.Current);
						
			enabled = _keyframesEnumerator.MoveNext();
			
#if !ALLOW_UNSAFE
#else			
			_keyframeValuesEnumerator = new TSTreeStreamSiblingEnumerator(_animationTree);
#endif
		}
		
		public void GotoAndPlay(string label)
		{
			
			var tag = _animationTree.FindTag(label);
			_animationClip.Position = tag.EntryPosition;
			
			_keyframesEnumerator.Init(tag);
			enabled = _keyframesEnumerator.MoveNext();
			
			_time = 0f;
		}
		
		private void Update()
		{
			if(_animationClip == null)
			{
				return;
			}
			
			_time += UnityEngine.Time.deltaTime;
			if(_time > _animationClip.Length)
			{
				_time = _time % (_animationClip.Length);
				
				_keyframesEnumerator.Reset();
				_keyframesEnumerator.MoveNext();
			}
#if !ALLOW_UNSAFE
#else
			unsafe
			{
				var ptr = _animationTree.Pin();
				EMAnimationKeyframeStructLayout keyframe = *(EMAnimationKeyframeStructLayout*)((byte*)ptr.ToPointer() + _keyframesEnumerator.Current.EntryPosition);
				
				while(!_keyframesEnumerator.Done && _time > keyframe.Time)
				{
					_keyframeValuesEnumerator.Init(_keyframesEnumerator.Current);

					while(_keyframeValuesEnumerator.MoveNext())
					{
						EMAnimationKeyframeValueStructLayout keyframeValue = *(EMAnimationKeyframeValueStructLayout*)((byte*)ptr.ToPointer() + _keyframeValuesEnumerator.Current.EntryPosition);
					
						CoreAssert.Fatal(0 <= keyframeValue.ChildIndex && keyframeValue.ChildIndex < _positions.Count);
						
						_entry.Position = _positions[keyframeValue.ChildIndex];
						_entry.Affect((TSPropertyType)keyframeValue.PropertyType, keyframeValue.Offset, keyframeValue.Value);
					}
					
					if(!_keyframesEnumerator.MoveNext())
					{
						break;
					}
					
					keyframe = *(EMAnimationKeyframeStructLayout*)((byte*)ptr.ToPointer() + _keyframesEnumerator.Current.EntryPosition);
				}
				_animationTree.Release();
			}
#endif
		}
	}
}