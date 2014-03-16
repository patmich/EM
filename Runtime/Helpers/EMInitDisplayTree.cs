using System;
using System.Collections.Generic;

namespace LLT
{

	public static partial class EMHelpers
	{
#if ALLOW_UNSAFE
		private unsafe sealed class EMIteratorDFS
		{
			private readonly Stack<IntPtr> _tags = new Stack<IntPtr>();

			private TSTreeStreamTagStructLayout* _currentTagPtr;
			private TSTreeStreamTagStructLayout* _parentTagPtr;
			private EMSpriteStructLayout* _parentPtr;
			private void* _currentPtr;
			
			public EMSpriteStructLayout* ParentPtr
			{
				get
				{
					return _parentPtr;
				}
			}

			public EMSpriteStructLayout* SpritePtr
			{
				get
				{
					CoreAssert.Fatal(IsSprite());
					return (EMSpriteStructLayout*)_currentPtr;
				}
			}

			public EMShapeStructLayout* ShapePtr
			{
				get
				{
					CoreAssert.Fatal(IsShape());
					return (EMShapeStructLayout*)_currentPtr;
				}
			}

			public bool IsShape()
			{
				return (EMFactory.Type)_currentTagPtr->TypeIndex == EMFactory.Type.EMShape;
			}

			public bool IsSprite()
			{
				return (EMFactory.Type)_currentTagPtr->TypeIndex == EMFactory.Type.EMSprite;
			}

			public void Init(TSTreeStreamTagStructLayout* rootTagPtr)
			{
				_currentTagPtr = rootTagPtr;
				_currentPtr = null;

				_parentTagPtr = rootTagPtr;
				_parentPtr = null;
			}

			public bool MoveNext()
			{
				TSTreeStreamTagStructLayout* tagPtr = (TSTreeStreamTagStructLayout*)((byte*)_currentTagPtr + TSTreeStreamTag.TSTreeStreamTagSizeOf + _currentTagPtr->EntrySizeOf);
				if((byte*)tagPtr < (byte*)_currentTagPtr + TSTreeStreamTag.TSTreeStreamTagSizeOf + _currentTagPtr->EntrySizeOf + _currentTagPtr->SubTreeSizeOf)
				{
					_parentTagPtr = _currentTagPtr;
					_parentPtr = (EMSpriteStructLayout*)_parentTagPtr + TSTreeStreamTag.TSTreeStreamTagSizeOf;
					
					_tags.Push((IntPtr)_parentTagPtr);
					_currentTagPtr = tagPtr;
					_currentPtr = tagPtr;
					return true;
				}
				else
				{
					_currentTagPtr = tagPtr;
					_currentPtr = tagPtr;
				}

				while(((byte*)_currentTagPtr >= (byte*)_parentTagPtr + TSTreeStreamTag.TSTreeStreamTagSizeOf + _parentTagPtr->EntrySizeOf + _parentTagPtr->SubTreeSizeOf))
				{
					if(_tags.Count == 0)
					{
						return false;
					}

					_parentTagPtr = (TSTreeStreamTagStructLayout*)_tags.Pop();
					_parentPtr = (EMSpriteStructLayout*)_parentTagPtr + TSTreeStreamTag.TSTreeStreamTagSizeOf;
				}

				return true;
			}
		}
#endif
		public static void InitDisplayTree(IntPtr rbo, out int spriteCount, out int shapeCount)
		{
			spriteCount = 0;
			shapeCount = 0;

#if ALLOW_UNSAFE
			unsafe
			{
				var iter = new EMIteratorDFS();
				iter.Init((TSTreeStreamTagStructLayout*)rbo);

				while(iter.MoveNext())
				{
					if(iter.IsSprite())
					{
						iter.SpritePtr->SpriteIndex = spriteCount++;
					}
					else if(iter.IsShape())
					{
						iter.ShapePtr->ShapeIndex = shapeCount++;
					}
				}
			}
#endif
		}
	}
}