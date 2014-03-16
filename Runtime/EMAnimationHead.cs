using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

namespace LLT
{
	[Serializable]
	public sealed class EMAnimationHead : IEMComponent, ICoreTimeline
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
		private EMAnimationTreeTextAsset _textAsset;
		private EMAnimationTreeStream _animationTree;

		private EMObject _object;
		private readonly List<int> _positions = new List<int>();

		private EMAnimationClip _animationClip;
		private TSTreeStreamTag _clipTag;
		
		[SerializeField]
		private bool _loop = true;
		private string _label;

		private Dictionary<string, IEnumerator> _wait;
		private int _keyframePosition;
		
		private float _time;
		private float _speed = 1f;
		
		private List<EMXYLinearInterpolation> _xyLinearInterpolation;

		public string Label { get { return _label; } }
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
		public bool Enabled { get; private set; }


		public void Init(EMObject obj)
		{	
			_object = obj;
			_positions.Clear();
			
			var childs = new TSTreeStreamSiblingEnumerator(_object.Tree);
			childs.Init(_object.Tag);
			
			while(childs.MoveNext())
			{
				_positions.Add(childs.Current.Position);
			}
			
			_animationTree = new EMAnimationTreeStream(_textAsset);
			
			var clipEnumerator = new TSTreeStreamSiblingEnumerator(_animationTree);
			clipEnumerator.Init(_animationTree.RootTag);
			clipEnumerator.MoveNext();
			
			_animationClip = new EMAnimationClip();
			_animationClip.Init(_animationTree.TextAsset);

			GotoAndPlay(_animationTree.GetName(clipEnumerator.Current));
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
			_clipTag = _animationTree.FindTag(label);
			if(_clipTag != null)
			{
				_label = label;
				
				_animationClip.Position = _clipTag.EntryPosition;         
				_keyframePosition = _clipTag.FirstChildPosition;

				Enabled = true;
				
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
		
		public void ExplicitUpdate(float deltaTime)
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
			
			_time += deltaTime * _speed;
			if(_time > _animationClip.Length)
			{
				if(_loop)
				{
					_time = _time % (_animationClip.Length);      
					_keyframePosition = _clipTag.FirstChildPosition;
					Seek(System.IO.SeekOrigin.Begin);
				}
				else
				{
					//enabled = false;
					_time = _animationClip.Length;
					Seek(System.IO.SeekOrigin.Current);
				}
			}
			else if(_time < _animationClip.Length)
			{
				Seek(System.IO.SeekOrigin.Current);
			}
		}
		
		
		private readonly List<EMObject> _childs = new List<EMObject>();
		
		private void Seek(System.IO.SeekOrigin origin)
		{
#if ALLOW_UNSAFE
			unsafe
			{
				if(origin == System.IO.SeekOrigin.Begin)
				{
					_keyframePosition = _clipTag.FirstChildPosition;
				}

				byte* displayTreePtr = (byte*)_object.Tree.TextAsset.AddrOfPinnedObject();
				byte* animationTreePtr = (byte*)_animationTree.TextAsset.AddrOfPinnedObject();
				TSTreeStreamTagStructLayout* keyframeTagPtr = (TSTreeStreamTagStructLayout*)(animationTreePtr + _keyframePosition);
				for(var keyframe = (EMAnimationKeyframeStructLayout*)((byte*)keyframeTagPtr + TSTreeStreamTag.TSTreeStreamTagSizeOf); keyframe->Time <= _time; keyframe = (EMAnimationKeyframeStructLayout*)((byte*)keyframeTagPtr + TSTreeStreamTag.TSTreeStreamTagSizeOf))
				{
					var keyframeValues =(EMAnimationKeyframeValueStructLayout*)((byte*)keyframe + EMAnimationKeyframe.EMAnimationKeyframeSizeOf);
					var count = (keyframeTagPtr->EntrySizeOf - EMAnimationKeyframe.EMAnimationKeyframeSizeOf) /EMAnimationKeyframeValue.EMAnimationKeyframeValueSizeOf;
					
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
					
					keyframeTagPtr = (TSTreeStreamTagStructLayout*)((byte*)keyframeTagPtr + TSTreeStreamTag.TSTreeStreamTagSizeOf + keyframeTagPtr->EntrySizeOf + keyframeTagPtr->SubTreeSizeOf);
					
					if((byte*)keyframeTagPtr == animationTreePtr + _clipTag.SiblingPosition)
					{
						keyframeTagPtr = (TSTreeStreamTagStructLayout*)(animationTreePtr + _clipTag.FirstChildPosition);
						if(keyframe->Time == 0)
						{
							_loop = false;
						}
						break;
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
					
					var loop = _loop;
					loop = UnityEditor.EditorGUILayout.Toggle("Loop: ", loop);
					if(_loop != loop)
					{
						_loop = loop;
						GotoAndPlay(Label, _time, _loop);
					}
					
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
				_animationTree = null;
			}

			_textAsset = null;
		}
		
		public void Play ()
		{
			lock(this)
			{
				Speed = 1;
			}
		}
		
		public void Pause ()
		{
			lock(this)
			{
				Speed = 0;
			}
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
				_keyframePosition = _clipTag.FirstChildPosition;
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
			var childs = _object.GetChilds();
			var xyLinearInterpolation = new List<EMXYLinearInterpolation>(childs.Count);
			var oldTime = Time;
			
			Time = 0;
			Seek(System.IO.SeekOrigin.Begin);
			
			for(var i = 0; i < childs.Count; i++)
			{
				xyLinearInterpolation.Add(new EMXYLinearInterpolation(){XStart = childs[i].Transform.M02, YStart = childs[i].Transform.M12});
			}
			
			Time = _animationClip.Length;
			Seek(System.IO.SeekOrigin.Begin);
			
			for(var i = 0; i < childs.Count; i++)
			{
				xyLinearInterpolation[i].XEnd = childs[i].Transform.M02;
				xyLinearInterpolation[i].YEnd = childs[i].Transform.M12;
			}
			
			Time = oldTime;
			Seek(System.IO.SeekOrigin.Begin);
			
			_xyLinearInterpolation = xyLinearInterpolation;
		}
		
		public Vector2 LerpXY(EMObject child, float t)
		{
			CoreAssert.Fatal(_xyLinearInterpolation != null, "Need to call ConvertXYToLinearInterpolation() first.");
			
			var childs = _object.GetChilds();
			var indexOf = childs.IndexOf(child);
			
			CoreAssert.Fatal(indexOf != -1);
			return _xyLinearInterpolation[indexOf].Lerp(t);
		}
	}
}