using System;
using UnityEngine;

namespace LLT
{
	public sealed class EMDisplayTreeStreamDFSEnumerator : TSTreeStreamDFSEnumerator<EMSprite, EMDisplayTreeStreamDFSEnumerator>
	{
		private EMFactory.Type _currentTypeIndex;
		private readonly EMShape _shape = new EMShape();
		private readonly EMSprite _sprite = new EMSprite();
		private readonly EMRoot _root;

		public EMSprite Sprite
		{
			get
			{
				if(_link)
				{
					return _subEnumerator.Sprite;
				}
				
				CoreAssert.Fatal(_currentTypeIndex == EMFactory.Type.EMSprite);
				
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
				
				CoreAssert.Fatal(_currentTypeIndex == EMFactory.Type.EMShape);
				
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

		public EMDisplayTreeStreamDFSEnumerator(EMRoot root) : base(root.DisplayTree)
		{
			_root = root;
			_sprite.Init(_root.DisplayTree);
			_shape.Init(_root.DisplayTree);
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

		public Texture Texture
		{
			get
			{
				if(_link)
				{
					return _subEnumerator.Texture;
				}

				CoreAssert.Fatal(IsShape());
				return _root.GetTexture(Shape.TextureIndex);
			}
		}
	}
}

