using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace LLT
{
	public sealed class EMAnimationHead : EMComponent, ICoreTimeline
	{
		[SerializeField]
		private TextAsset _data;
		
		private TSTreeStreamEntry _entry;
        private readonly List<int> _positions = new List<int>();
		private EMAnimationTreeStream _animationTree;
		private EMAnimationClip _animationClip;
		private TSTreeStreamSiblingEnumerator _keyframesEnumerator;
  
        private string _label;
        private float _realTimeSinceStartup;
       
        [SerializeField]
        private bool _loop = true;
        
        private Dictionary<string, IEnumerator> _wait;
        
#if !ALLOW_UNSAFE
#else
		private TSTreeStreamSiblingEnumerator _keyframeValuesEnumerator;
#endif
		
		private float _time;
		private float _speed;
        
		public override void Init(EMObject obj)
		{
			base.Init(obj);
			
			_entry = new TSTreeStreamEntry();
			_entry.Init(_object.Tree);
		
			_positions.Clear();
			
			var childs = new TSTreeStreamSiblingEnumerator(_object.Tree);
			childs.Init(_object.Tag);
			
			while(childs.MoveNext())
			{
				_positions.Add(childs.Current.EntryPosition);
			}
			
			_animationTree = new EMAnimationTreeStream();
			_animationTree.Init(_data.bytes);
			
			var clipEnumerator = new TSTreeStreamSiblingEnumerator(_animationTree);
			clipEnumerator.Init(_animationTree.RootTag);
			clipEnumerator.MoveNext();

            _animationClip = new EMAnimationClip();
			_animationClip.Init(_animationTree);
            _keyframesEnumerator = new TSTreeStreamSiblingEnumerator(_animationTree);
            
			GotoAndPlay(_animationTree.GetName(clipEnumerator.Current));

#if !ALLOW_UNSAFE
#else			
			_keyframeValuesEnumerator = new TSTreeStreamSiblingEnumerator(_animationTree);
#endif
		}
		
        public void GotoAndPlay(string label)
        {
            GotoAndPlay(label, true);
        }
        
        public void GotoAndPlay(string label, bool loop)
        {
            var tag = _animationTree.FindTag(label);
			if(tag != null)
			{
				_label = label;
				
	            _animationClip.Position = tag.EntryPosition;
	            
	            _keyframesEnumerator.Init(tag);
	            enabled = _keyframesEnumerator.MoveNext();
	           
	            _loop = loop;
	            _time = 0f;
			}
        }
        
		public IEnumerator GotoAndPlayWait(string label)
		{
            if(_wait == null)
            {
                _wait = new Dictionary<string, IEnumerator>();
            }
            
            GotoAndPlay(label, false);
            
            IEnumerator wait = null;
            if(!_wait.TryGetValue(_label, out wait))
            {
                wait = Wait (_label);
                _wait.Add(_label, wait);
            }
            
            return wait;
		}
		
        private IEnumerator Wait(string label)
        {
            while(!_loop && _label == label && _time < _animationClip.Length)yield return null;
            _wait.Remove(_label);
        }
       
		private void Update()
		{
			CoreAssert.Fatal(_object.Sprite != null);
			if(_object.Sprite.LocalToWorld.Placed == 0)
			{
				//return;
			}
			if(_animationClip == null)
			{
				return;
			}
			
            _time += UnityEngine.Time.deltaTime * _speed;
			if(_time > _animationClip.Length)
			{
                if(_loop)
                {
                    _time = _time % (_animationClip.Length);
    				_keyframesEnumerator.Reset();
    				_keyframesEnumerator.MoveNext();
                }
                else
                {
                    _time = _animationClip.Length;
                }
			}
#if !ALLOW_UNSAFE
#else
			unsafe
			{
				var ptr = _animationTree.Ptr;
				EMAnimationKeyframeStructLayout keyframe = *(EMAnimationKeyframeStructLayout*)((byte*)ptr.ToPointer() + _keyframesEnumerator.Current.EntryPosition);
				
				while(!_keyframesEnumerator.Done && _time >= keyframe.Time)
				{
					EMAnimationKeyframeValueStructLayout* keyframeValues = (EMAnimationKeyframeValueStructLayout*)((byte*)ptr.ToPointer() + _keyframesEnumerator.Current.EntryPosition + EMAnimationKeyframe.EMAnimationKeyframeSizeOf);
					var count = (_keyframesEnumerator.Current.EntrySizeOf - EMAnimationKeyframe.EMAnimationKeyframeSizeOf)/EMAnimationKeyframeValue.EMAnimationKeyframeValueSizeOf;
			
					for(var i = 0; i < count; i++)
					{
						EMAnimationKeyframeValueStructLayout keyframeValue = keyframeValues[i];
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
			}
#endif
		}
#if UNITY_EDITOR
        public void OnInspectorGUI()
        {
            if(_object != null)
            {
                var path = string.Empty;
                if(_object.GetPath(out path))
                {
                    UnityEditor.EditorGUILayout.LabelField("Path: " + path);
                }
                
                _loop = UnityEditor.EditorGUILayout.Toggle("Loop: ", _loop);
                
                if(!_loop && GUILayout.Button("Play once"))
                {
                    GotoAndPlay(_label, false);
                }
                    
                if(_animationTree != null)
                {
                    var clipEnumerator = new TSTreeStreamSiblingEnumerator(_animationTree);
                    clipEnumerator.Init(_animationTree.RootTag);
                    
                    var labels = new List<string>();
                    while(clipEnumerator.MoveNext())
                    {
                        labels.Add(_animationTree.GetName(clipEnumerator.Current));
                    }
                    
                    if(labels.Count > 0)
                    {
                        var oldIndex = labels.IndexOf(_label);
                        var newIndex = UnityEditor.EditorGUILayout.Popup(oldIndex == -1 ? 0 : oldIndex, labels.ToArray());
                        if(oldIndex != newIndex)
                        {
                            GotoAndPlay(labels[newIndex]);
                        }
                    }
                }
            }
        }
#endif
        
        private void OnDestroy()
        {
            if(_animationTree != null)
            {
                _animationTree.Dispose();    
            }
        }

        public void Play ()
        {
            Speed = 1;
        }

        public void Pause ()
        {
            Speed = 0;
        }

        public float Time 
        {
            get 
            {
                return _time;
            }
            set
            {
                _time = value % (_animationClip.Length);
                _keyframesEnumerator.Reset();
                _keyframesEnumerator.MoveNext();
            }
        }

        public float Length 
        {
            get 
            {
                return _animationClip.Length;
            }
        }

        public float Speed 
        {
            set
            {
                _speed = value;
            }
        }
	}
}