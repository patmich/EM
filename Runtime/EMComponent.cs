using UnityEngine;

namespace LLT
{
	public abstract class EMComponent : EMMonoBehaviour
	{
        [SerializeField]
        public int _initialPosition = -1;
		protected EMObject _object;
		
		public virtual void Init(EMObject obj)
		{
			_object = obj;
            _initialPosition = _object.Tag.Position;
		}

        public virtual void InitSerializedComponent(EMDisplayTreeStream tree)
        {
            CoreAssert.Fatal(_initialPosition != -1);
            _object = tree.GetObject(tree.CreateTag(_initialPosition)) as EMObject;
            _object.AddSerializedComponent(this);
        }
	}
}