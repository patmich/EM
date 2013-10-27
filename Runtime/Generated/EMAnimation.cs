namespace LLT
{
	public sealed partial class EMAnimation : TSTreeStreamEntry, ITSFactoryInstance
	{
		public const int EMAnimationSizeOf = 0;



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
				return 0;
			}
		}
	}
}
