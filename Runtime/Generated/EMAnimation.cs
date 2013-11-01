namespace LLT
{
	public sealed partial class EMAnimation : LLT.TSTreeStreamEntry, LLT.ITSFactoryInstance
	{
		public const int EMAnimationSizeOf = 0;



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
