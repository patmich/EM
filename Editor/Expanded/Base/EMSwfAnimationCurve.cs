using System.Collections.Generic;

public sealed class EMSwfAnimationCurve
{
	public sealed class Keyframe
	{
		public int Index;
		public float Value;
		
		public override string ToString ()
		{
			return string.Format ("[Keyframe] Index={0}, Value={1}", Index, Value);
		}
	}
	
	private readonly List<Keyframe> _keyframes = new List<Keyframe>();
	
	public EMSwfAnimationCurve()
	{
	}
	
	public EMSwfAnimationCurve(float value)
	{
		Add(0, value);
	}
	
	public bool Add(int index, float value, bool removeDupe = true)
	{
		var indexOf = _keyframes.FindLastIndex((x)=>x.Index <= index);
		var newKeyframe = new Keyframe(){Index = index, Value = value};
		if(indexOf != -1)
		{
			if(_keyframes[indexOf].Index == index)
			{
				if(_keyframes[indexOf].Value != newKeyframe.Value)
				{
					_keyframes[indexOf] = newKeyframe;
					return true;
				}
			}
			else
			{
				if(_keyframes[indexOf].Value != value || !removeDupe)
				{
					_keyframes.Insert(indexOf + 1, newKeyframe);
					return true;
				}
			}
		}
		else
		{
			_keyframes.Add(newKeyframe);
			return true;
		}
		return false;
	}
	
	public float Sample(int index)
	{
		if(_keyframes.Count == 0)
		{
			return 0f;
		}
		
		var keyframe = _keyframes.FindLast((x)=>x.Index <= index);
		if(keyframe == null)
		{
			return 0f;
		}
		return keyframe.Value;
	}
	
	public bool HasValue(int index)
	{
		return _keyframes.FindIndex(x=>x.Index == index) != -1;
	}
	
	public int Count
	{
		get
		{
			return _keyframes.Count;
		}
	}
	
	public override string ToString ()
	{
		var retVal = string.Empty;
		_keyframes.ForEach(x=>retVal+=x + "\n");
		return retVal;
	}
}