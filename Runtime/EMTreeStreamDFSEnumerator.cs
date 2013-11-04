using System;

namespace LLT
{
	public sealed class EMTreeStreamDFSEnumerator : TSTreeStreamDFSEnumerator<EMRoot, EMSprite, EMTreeStreamDFSEnumerator>
	{
		private EMFactory.Type _currentTypeIndex;
		private readonly EMShape _shape = new EMShape();
		private readonly EMSprite _sprite = new EMSprite();
		
		public EMSprite Sprite
		{
			get
			{
				if(_link)
				{
					return _subEnumerator.Sprite;
				}
				
				CoreAssert.Fatal(_currentTypeIndex == EMFactory.Type.EMSprite);
				return _sprite;
			}
		}
		public EMShape Shape
		{
			get
			{
				if(_link)
				{
					return _subEnumerator.Shape;
				}
				
				CoreAssert.Fatal(_currentTypeIndex == EMFactory.Type.EMShape);
				return _shape;
			}
		}
        
		public EMTreeStreamDFSEnumerator(EMRoot root) : base(root, root.DisplayTree)
		{
			_sprite.Init(root.DisplayTree);
			_shape.Init(root.DisplayTree);
		}
		
		public override bool MoveNext (bool skipSubTree)
		{
			var retVal = base.MoveNext (skipSubTree);
			var current = Current;
			_currentTypeIndex = (EMFactory.Type)current.TypeIndex;
			
			if(_currentTypeIndex == EMFactory.Type.EMSprite)
			{
				_sprite.Position = current.EntryPosition;
			}
			else if(_currentTypeIndex == EMFactory.Type.EMShape)
			{
				_shape.Position = current.EntryPosition;
			}
			
			return retVal;
		}
		
		public bool IsShape()
		{
			return _currentTypeIndex == EMFactory.Type.EMShape;
		}
		
		public bool IsSprite()
		{
			return _currentTypeIndex == EMFactory.Type.EMSprite;
		}
	}
}

