namespace LLT
{
	public sealed partial class EMAnimation : LLT.TSTreeStreamEntry, LLT.ITSFactoryInstance
	{
		public const int EMAnimationSizeOf = 0;

		public override void Init(ITSTextAsset textAsset)
		{
			_textAsset = textAsset;
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
