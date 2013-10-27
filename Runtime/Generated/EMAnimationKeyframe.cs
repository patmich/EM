namespace LLT
{
	public sealed partial class EMAnimationKeyframe : TSTreeStreamEntry, ITSFactoryInstance
	{
		public const int Time_Offset = 0;
		public const int EMAnimationKeyframeSizeOf = 4;


		public float Time
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
				return 4;
			}
		}
	}
}
