using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace LLT
{
	[Serializable]	
	public sealed class EMObject : ITSObject
	{
		[SerializeField]
		private int _position;
		
		[SerializeField]
		private EMAnimationHead _animationHead;
		
        private readonly List<EMComponent> _components = new List<EMComponent>();

		private EMDisplayTreeStream _tree;
		private TSTreeStreamTag _tag;

		private EMSprite _sprite;
		private EMShape _shape;

        private bool _disposed;

        public EMRoot Root
        {
            get
            {
                return _tree.Root;
            }
        }

		public EMTransform Transform
		{
			get
			{
				var type = (EMFactory.Type)_tag.TypeIndex;
				if(type == EMFactory.Type.EMShape)
				{
					if(_shape == null)
					{
						_shape = new EMShape();
						_shape.Init(_tree);
						_shape.Position = _tag.EntryPosition;
					}

					return _shape.Transform;
				}
				else if(type == EMFactory.Type.EMSprite)
				{
					return _sprite.Transform;
				}

				return null;
			}
		}

		public EMSprite Sprite 
		{
			get
			{
				CoreAssert.Fatal(_sprite != null);
				return _sprite;
			}
		}
		
        public bool HasAnimationHead
        {
            get
            {
                return _animationHead != null;
            }
        }

		public EMAnimationHead AnimationHead
		{
			get
			{
				CoreAssert.Fatal(_animationHead != null);
				return _animationHead;
			}
		}
		
		public int Position 
		{
			get 
			{
				return _position;
			}
			set 
			{
				_position = value;
				
				if(_tag != null)
				{
					_tag.Position = _position;
				}
			}
		}
		
		public ITSTreeStream Tree
		{
			get
			{
				return _tree;
			}
		}
		
		public TSTreeStreamTag Tag
		{
			get
			{
				return _tag;
			}
		}
		
		public void Init(ITSTreeStream tree)
		{
			_tree = tree as EMDisplayTreeStream;
			CoreAssert.Fatal(_tree != null);
			
			_tag = new TSTreeStreamTag(_tree);
			_tag.Position = _position;
			
            if(_animationHead != null)
            {
			    _animationHead.Init(this);
            }
			
			if((EMFactory.Type)_tag.TypeIndex == EMFactory.Type.EMSprite)
			{
				_sprite = new EMSprite();
				_sprite.Init(_tree);
				_sprite.Position = _tag.EntryPosition;
			}
		}
        
        public bool GetPath(out string path)
        {
            path = string.Empty;
            if(_tree != null && _tag != null)
            {
                return _tree.RebuildPath(_tag, out path);
            }
            return false;
        }

		public EMObject FindObject(string path)
		{
			return _tree.FindObject(_tag, path.Split('/'));
		}

		public EMObject FindObject(params string[] path)
		{
			return _tree.FindObject(_tag, path);
		}
		
        public EMObject FindFirstObject(string name)
        {
            return _tree.FindFirstObject(_tag, name);
        }

        public List<EMObject> Childs
        {
            get
            {
                return _tree.GetChilds(_tag);
            }
        }

        public void FillChilds(List<EMObject> childs)
        {
            _tree.FillChilds(_tag, childs);
        }

		public Bounds Bounds
		{
			get
			{
				var iter = _tree.Iter as EMDisplayTreeStreamDFSEnumerator;
				iter.Reset(_tag);
				
				if(iter.IsShape())
				{
					return iter.Shape.Bounds;
				}
				
				Bounds retVal = new Bounds(Vector3.zero, Vector3.zero);
				var skipSubTree = false;
				while(iter.MoveNext(skipSubTree))
				{
					skipSubTree = false;
					
					if(iter.IsShape())
					{
						var bounds = iter.Shape.Bounds;
						if(bounds.size != Vector3.zero)
						{
                            if(retVal.size == Vector3.zero)
                            {
                                retVal = bounds;
                            }
                            else
                            {
							    retVal.Encapsulate(bounds);
                            }
						}
					}
					else if(iter.Sprite.LocalToWorld.Placed == 0)
					{
						skipSubTree = true;
					}
				}
				
                if(retVal.size == Vector3.zero)
                {
                    retVal.center = new Vector3(iter.Sprite.LocalToWorld.M02, iter.Sprite.LocalToWorld.M12, 0f);
                }

                var absoluteRoot = AbsoluteRoot;
                retVal.min = absoluteRoot.transform.localToWorldMatrix.MultiplyPoint(retVal.min);
                retVal.max = absoluteRoot.transform.localToWorldMatrix.MultiplyPoint(retVal.max);
				return retVal;
			}
		}

        public T GetComponent<T>() 
            where T : EMComponent
        {
            return _components.Find(x=>x is T) as T;
        }

        public T AddComponent<T>()
            where T : EMComponent
        {
            var t = _tree.Root.gameObject.AddComponent<T>();
            _components.Add(t);
            t.Init(this);

            return t;
        }

        public void AddSerializedComponent(EMAnimationHead animationHead)
        {
            _components.Add(animationHead);
            _animationHead = animationHead;
            _animationHead.Init(this);
        }

        public void AddSerializedComponent(EMComponent component)
        {
            _components.Add(component);
            component.Init(this);
        }

        public void Destroy(EMComponent t)
        {
            _components.Remove(t);
            GameObject.Destroy(t);
        }

        public EMRoot AbsoluteRoot 
        {
            get
            {
                var root = _tree.Root;
                var parent = _tree.Root.Parent;
                while(parent != null)
                {
                    root = parent.Root;
                    parent = root.Parent;
                }

                return root;
            }
        }

        public string Name
        {
            get
            {
                return _tree.GetName(_tag);
            }
        }

        public int ChildCount
        {
            get
            {
                var iter = new EMDisplayTreeStreamDFSEnumerator(_tree.Root);
                iter.Reset(_tag);
                
                if(iter.IsShape())
                {
                    return 1;
                }

                var childCount = 0;
                var skipSubTree = false;
                while(iter.MoveNext(skipSubTree))
                {
                    skipSubTree = false;
                    childCount++;

                    if(iter.IsSprite())
                    {
                        skipSubTree = true;
                    }
                }

                return childCount;
            }
        }

        public void Dispose()
        {
            _disposed = true;
        }

        public static bool operator==(EMObject left, EMObject right)
        {
            if(System.Object.ReferenceEquals(left, right))
            {
                return true;
            }

            if(System.Object.ReferenceEquals(left, null))
            {
                return right._disposed;
            }

            if(System.Object.ReferenceEquals(right, null))
            {
                return left._disposed;
            }

            return false;
        }

        public static bool operator!=(EMObject left, EMObject right)
        {
            return !(left == right);
        }

        public override bool Equals (object obj)
        {
            return base.Equals (obj);
        }

        public override int GetHashCode ()
        {
            return base.GetHashCode ();
        }
	}
}