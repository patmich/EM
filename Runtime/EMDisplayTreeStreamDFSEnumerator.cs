using System;
using UnityEngine;

namespace LLT
{
	public sealed class EMDisplayTreeStreamDFSEnumerator : TSTreeStreamDFSEnumerator<EMSprite, EMDisplayTreeStreamDFSEnumerator>
	{
		private EMFactory.Type _currentTypeIndex;
		private readonly EMShape _shape = new EMShape();
		private readonly EMSprite _sprite = new EMSprite();
		private readonly EMDisplayTreeStream _displayTree;

		public EMSprite Sprite
		{
			get
			{
				if(_link)
				{
					return _subEnumerator.Sprite;
				}
				
                CoreAssert.Fatal((EMFactory.Type)Current.TypeIndex == EMFactory.Type.EMSprite);
				
				var current = Current;
				if(_sprite.Position != current.EntryPosition)
				{
					_sprite.Position = current.EntryPosition;
				}
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
				
                CoreAssert.Fatal((EMFactory.Type)Current.TypeIndex == EMFactory.Type.EMShape);
				
				var current = Current;
				if(_shape.Position != current.EntryPosition)
				{
					_shape.Position = current.EntryPosition;
				}
				return _shape;
			}
		}

		public int Depth
		{
			get
			{
				if(IsShape())
				{
					return Shape.Depth;
				}
				else
				{
					CoreAssert.Fatal(IsSprite());
					return Sprite.Depth;
				}
			}
		}

		public int ClipDepth
		{
			get
			{
				if(IsShape())
				{
					return Shape.ClipDepth;
				}
				else
				{
					CoreAssert.Fatal(IsSprite());
					return Sprite.ClipDepth;
				}
			}
		}
  
		public EMDisplayTreeStreamDFSEnumerator(EMDisplayTreeStream displayTree) : base(displayTree)
        {
			_displayTree = displayTree;
			_sprite.Init(_displayTree.TextAsset);
			_shape.Init(_displayTree.TextAsset);
        }
		
		public override bool MoveNext (bool skipSubTree)
		{
			var retVal = base.MoveNext (skipSubTree);
			_currentTypeIndex = (EMFactory.Type)Current.TypeIndex;
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

		public int DisplayTreeInstanceId
		{
			get
			{
				if(_link)
				{
					return _subEnumerator.DisplayTreeInstanceId;
				}

				return _displayTree.GetInstanceID();
			}
		}
	}
}

