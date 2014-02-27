using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace LLT
{
	public sealed class EMAnimationHead : EMComponent, ICoreTimeline
	{
		private class EMXYLinearInterpolation
		{
			public float XStart { get; set; }
			public float YStart { get; set; }
			public float XEnd { get; set; }
			public float YEnd { get; set; }

			public Vector2 Lerp(float t)
			{
				return new Vector2(Mathf.Lerp(XStart, XEnd, t), Mathf.Lerp(YStart, YEnd, t));
			}
		}

		[SerializeField]
		private EMAnimationHeadData _data;
		
		private TSTreeStreamEntry _entry;
        private readonly List<int> _positions = new List<int>();
		private EMAnimationTreeStream _animationTree;
		private EMAnimationClip _animationClip;
		private TSTreeStreamSiblingEnumerator _keyframesEnumerator;
  
        private string _label;
		public string Label { get { return _label; } }

        private float _realTimeSinceStartup;
       
        [SerializeField]
        private bool _loop = true;

        public bool Loop 
        {
            get
            {
                return _loop;
            }
            set
            {
                _loop = value;
            }
        }

        private Dictionary<string, IEnumerator> _wait;
        
#if !ALLOW_UNSAFE
#else
		private TSTreeStreamSiblingEnumerator _keyframeValuesEnumerator;
#endif
		
		private float _time;
		private float _speed = 1f;
        
		private List<EMXYLinearInterpolation> _xyLinearInterpolation;

		public override void Init(EMObject obj)
		{
            base.Init(obj);
		    
            if(_data == null)
            {
                return;
            }

			_entry = new TSTreeStreamEntry();
			_entry.Init(_object.Tree);
		
			_positions.Clear();
			
			var childs = new TSTreeStreamSiblingEnumerator(_object.Tree);
			childs.Init(_object.Tag);
			
			while(childs.MoveNext())
			{
				_positions.Add(childs.Current.Position);
			}
			
			_animationTree = new EMAnimationTreeStream();
			_animationTree.Init(_data.Bytes);
			
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

        public override void InitSerializedComponent(EMDisplayTreeStream tree)
        {
            _object = tree.GetObject(tree.CreateTag(_initialPosition)) as EMObject;
            _object.AddSerializedComponent(this);
        }
		
        public void GotoAndPlay(string label)
        {
            GotoAndPlay(label, 0f, true);
        }
        
        public void GotoAndPlay(string label, bool loop)
        {
			GotoAndPlay(label, 0f, loop);
        }
		public void GotoAndStop(float time)
		{
			_speed = 0f;
			GotoAndPlay(Label, time, true);
		}
		public void GotoAndPlay(string label, float time, bool loop)
		{
			var tag = _animationTree.FindTag(label);
			if(tag != null)
			{
				_label = label;
				
	            _animationClip.Position = tag.EntryPosition;
	            
	            _keyframesEnumerator.Init(tag);
	            enabled = _keyframesEnumerator.MoveNext();
	           
	            _loop = loop;
				_time = time;
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
            while(!_loop && _label == label && _time < _animationClip.Length)
			{
				yield return null;
			}
            _wait.Remove(_label);
        }
       
		public void ExplicitUpdate()
		{
            if(_object == null)
            {
                return;
            }

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

			Seek();
		}


        private readonly List<EMObject> _childs = new List<EMObject>();
		private void Seek()
		{
			#if !ALLOW_UNSAFE
			#else
			unsafe
			{
				var ptr = (byte*)_animationTree.Ptr.ToPointer();
				var displayTreePtr = (byte*)_object.Tree.Ptr.ToPointer();
				EMAnimationKeyframeStructLayout* keyframe = (EMAnimationKeyframeStructLayout*)(ptr + _keyframesEnumerator.Current.EntryPosition);
				
				while(!_keyframesEnumerator.Done && _time >= keyframe->Time)
				{
					EMAnimationKeyframeValueStructLayout* keyframeValues = (EMAnimationKeyframeValueStructLayout*)((byte*)ptr + _keyframesEnumerator.Current.EntryPosition + EMAnimationKeyframe.EMAnimationKeyframeSizeOf);
					var count = (_keyframesEnumerator.Current.EntrySizeOf - EMAnimationKeyframe.EMAnimationKeyframeSizeOf)/EMAnimationKeyframeValue.EMAnimationKeyframeValueSizeOf;
					
					for(var i = 0; i < count; i++)
					{
						EMAnimationKeyframeValueStructLayout* keyframeValue = keyframeValues + i;
						CoreAssert.Fatal(0 <= keyframeValue->ChildIndex && keyframeValue->ChildIndex < _positions.Count);

						var currentPtr = displayTreePtr + _positions[keyframeValue->ChildIndex] + keyframeValue->Offset + TSTreeStreamTag.TSTreeStreamTagSizeOf;
						switch((TSPropertyType)keyframeValue->PropertyType)
						{
						case TSPropertyType._byte:*currentPtr = (byte)keyframeValue->Value;break;
						case TSPropertyType._ushort:*(ushort*)currentPtr = (ushort)keyframeValue->Value;break;
						case TSPropertyType._int:*(int*)currentPtr = (int)keyframeValue->Value;break;
						case TSPropertyType._float:*(float*)currentPtr = (float)keyframeValue->Value;break;
						}
					}
					
					if(_keyframesEnumerator.MoveNext())
					{
						keyframe = (EMAnimationKeyframeStructLayout*)(ptr + _keyframesEnumerator.Current.EntryPosition);
					}
					else
					{
						if(keyframe->Time == 0)
						{
							_loop = false;
						}
					}
				}
			}
			#endif

			if(_xyLinearInterpolation != null)
			{
                _childs.Clear();
                _object.FillChilds(_childs);
				var t = _time/_animationClip.Length;

                CoreAssert.Fatal(_childs.Count == _xyLinearInterpolation.Count);
                for(var i = 0; i < _childs.Count; i++)
				{
					var vec = _xyLinearInterpolation[i].Lerp(t);
                    _childs[i].Transform.M02 = vec.x;
                    _childs[i].Transform.M12 = vec.y;
				}
			}
		}

#if UNITY_EDITOR

        public void OnInspectorGUI()
        {
            if(_object != null)
            {
                var path = string.Empty;
				if(_object.GetPath(out path) && !string.IsNullOrEmpty(path))
				{
					UnityEditor.EditorGUILayout.LabelField("Path: " + path);
					var time = _time;
					time = UnityEditor.EditorGUILayout.FloatField("Time: ", time);
					
					_speed = UnityEditor.EditorGUILayout.FloatField("Speed: ", _speed);
					_loop = UnityEditor.EditorGUILayout.Toggle("Loop: ", _loop);
					
					if(!_loop && GUILayout.Button("Play once"))
					{
						GotoAndPlay(_label, false);
					}
					
					if(_time != time)
					{
						_time = time;
						GotoAndPlay(Label, _time, _loop);
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
                _time = Mathf.Clamp(value, 0f, _animationClip.Length);
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

		public void ConvertXYToLinearInterpolation()
		{
			var childs = _object.Childs;
			var xyLinearInterpolation = new List<EMXYLinearInterpolation>(childs.Count);
			var oldTime = Time;

			Time = 0;
			Seek();

			for(var i = 0; i < childs.Count; i++)
			{
				xyLinearInterpolation.Add(new EMXYLinearInterpolation(){XStart = childs[i].Transform.M02, YStart = childs[i].Transform.M12});
			}

			Time = _animationClip.Length;
			Seek();

			for(var i = 0; i < childs.Count; i++)
			{
				xyLinearInterpolation[i].XEnd = childs[i].Transform.M02;
				xyLinearInterpolation[i].YEnd = childs[i].Transform.M12;
			}

			Time = oldTime;
			Seek();

			_xyLinearInterpolation = xyLinearInterpolation;
		}

		public Vector2 LerpXY(EMObject child, float t)
		{
			CoreAssert.Fatal(_xyLinearInterpolation != null, "Need to call ConvertXYToLinearInterpolation() first.");

			var childs = _object.Childs;
			var indexOf = childs.IndexOf(child);

			CoreAssert.Fatal(indexOf != -1);
			return _xyLinearInterpolation[indexOf].Lerp(t);
		}
	}
}