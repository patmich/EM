namespace LLT
{
	public sealed partial class EMAnimationClip : TSTreeStreamEntry, ITSFactoryInstance
	{
		public const int Length_Offset = 0;
		public const int EMAnimationClipSizeOf = 4;


		public float Length
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
