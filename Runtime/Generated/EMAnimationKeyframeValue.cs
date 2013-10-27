namespace LLT
{
	public sealed partial class EMAnimationKeyframeValue : TSTreeStreamEntry, ITSFactoryInstance
	{
		public const int ChildIndex_Offset = 0;
		public const int Offset_Offset = 2;
		public const int PropertyType_Offset = 3;
		public const int Value_Offset = 4;
		public const int EMAnimationKeyframeValueSizeOf = 8;


		public ushort ChildIndex
		{
			get
			{
				return _tree.ReadUInt16(_position + 0);
			}
			set
			{
				_tree.Write(_position + 0, value);
			}
		}
		public byte Offset
		{
			get
			{
				return _tree.ReadByte(_position + 2);
			}
			set
			{
				_tree.Write(_position + 2, value);
			}
		}
		public byte PropertyType
		{
			get
			{
				return _tree.ReadByte(_position + 3);
			}
			set
			{
				_tree.Write(_position + 3, value);
			}
		}
		public float Value
		{
			get
			{
				return _tree.ReadSingle(_position + 4);
			}
			set
			{
				_tree.Write(_position + 4, value);
			}
		}

		public override int Position
		{
			get
			{
				return _position;
			}
			set
			{
				_position = value;
			}
		}

		public override void Init(ITSTreeStream tree)
		{
			_tree = tree;
		}

		public override int SizeOf
		{
			get
			{
				return 8;
			}
		}
	}
}
