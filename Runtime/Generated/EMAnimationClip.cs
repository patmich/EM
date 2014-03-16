namespace LLT
{
	public sealed partial class EMAnimationClip : LLT.TSTreeStreamEntry, LLT.ITSFactoryInstance
	{
		public const int Length_Offset = 0;
		public const int EMAnimationClipSizeOf = 4;

		public float Length
		{
			get
			{
				return _textAsset.ReadSingle(_position + 0);
			}
			set
			{
				_textAsset.Write(_position + 0, value);
			}
		}

		public override void Init(ITSTextAsset textAsset)
		{
			_textAsset = textAsset;
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
