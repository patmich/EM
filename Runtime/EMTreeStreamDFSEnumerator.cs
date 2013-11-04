using System;

namespace LLT
{
	public sealed class EMTreeStreamDFSEnumerator : TSTreeStreamDFSEnumerator<EMSprite>
	{
		private EMFactory.Type _currentTypeIndex;
		private readonly EMShape _shape = new EMShape();
		private readonly EMSprite _sprite = new EMSprite();
		
		public EMSprite Sprite
		{
			get
			{
				CoreAssert.Fatal(_currentTypeIndex == EMFactory.Type.EMSprite);
				return _sprite;
			}
		}
		public EMShape Shape
		{
			get
			{
				CoreAssert.Fatal(_currentTypeIndex == EMFactory.Type.EMShape);
				return _shape;
			}
		}
		
		public EMTreeStreamDFSEnumerator(ITSTreeStream tree) : base(tree)
		{
			_sprite.Init(tree);
			_shape.Init(tree);
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

