using UnityEngine;
using System.Collections.Generic;
using System;

namespace LLT
{
	[Serializable]	
	public sealed class EMObject : ITSObject
	{
		[SerializeField]
		private int _position;
		
		[SerializeField]
		private EMAnimationHead _animationHead;
		
		private EMDisplayTreeStream _tree;
		private TSTreeStreamTag _tag;
		private EMSprite _sprite;
		
		public EMSprite Sprite 
		{
			get
			{
				CoreAssert.Fatal(_sprite != null);
				return _sprite;
			}
		}
		
		public EMAnimationHead AnimationHead
		{
			get
			{
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
			
			_animationHead.Init(this);
			
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
            if(_tree != null)
            {
                CoreAssert.Fatal(_tag != null);
                return _tree.RebuildPath(_tag, out path);
            }
            return false;
        }
		
		public EMObject FindObject(params string[] path)
		{
			return _tree.FindObject(_tag, path);
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
				
				var retVal = new Bounds(Vector3.zero, Vector3.zero);
				var skipSubTree = false;
				while(iter.MoveNext(skipSubTree))
				{
					skipSubTree = false;
					
					if(iter.IsShape())
					{
						var bounds = iter.Shape.Bounds;
						if(bounds.size != Vector3.zero)
						{
							retVal.Encapsulate(iter.Shape.Bounds);
						}
					}
					else if(iter.Sprite.LocalToWorld.Placed == 0)
					{
						skipSubTree = true;
					}
				}
				
				return retVal;
			}
		}
	}
}