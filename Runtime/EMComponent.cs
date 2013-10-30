using UnityEngine;

namespace LLT
{
	public abstract class EMComponent : EMMonoBehaviour
	{
		protected EMObject _object;
		
		public virtual void Init(EMObject obj)
		{
			_object = obj;
		}
	}
}