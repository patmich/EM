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
		
		private ITSTreeStream _tree;
		private TSTreeStreamTag _tag;
		
		public EMSprite Sprite { private set; get; }
		
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
			_tree = tree;
			
			_tag = new TSTreeStreamTag(_tree);
			_tag.Position = _position;
			
			_animationHead.Init(this);
			
			if((EMFactory.Type)_tag.TypeIndex == EMFactory.Type.EMSprite)
			{
				Sprite = new EMSprite();
				Sprite.Init(_tree);
				Sprite.Position = _tag.EntryPosition;
			}
		}
	}
}