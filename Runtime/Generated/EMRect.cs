namespace LLT
{
	public sealed partial class EMRect : TSTreeStreamEntry, ITSFactoryInstance
	{
		public const int X_Offset = 0;
		public const int Y_Offset = 4;
		public const int Width_Offset = 8;
		public const int Height_Offset = 12;
		public const int EMRectSizeOf = 16;


		public float X
		{
			get
			{
				return _tree.ReadSingle(_position + 0);
			}
			set
			{
				_tree.Write(_position + 0, value);
			}
		}
		public float Y
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
		public float Width
		{
			get
			{
				return _tree.ReadSingle(_position + 8);
			}
			set
			{
				_tree.Write(_position + 8, value);
			}
		}
		public float Height
		{
			get
			{
				return _tree.ReadSingle(_position + 12);
			}
			set
			{
				_tree.Write(_position + 12, value);
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
				return 16;
			}
		}
	}
}
