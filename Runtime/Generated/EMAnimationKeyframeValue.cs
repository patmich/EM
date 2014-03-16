namespace LLT
{
	public sealed partial class EMAnimationKeyframeValue : LLT.TSTreeStreamEntry, LLT.ITSFactoryInstance
	{
		public const int ChildIndex_Offset = 0;
		public const int Offset_Offset = 4;
		public const int PropertyType_Offset = 5;
		public const int Value_Offset = 8;
		public const int EMAnimationKeyframeValueSizeOf = 12;


		public int ChildIndex
		{
			get
			{
				return _textAsset.ReadInt32(_position + 0);
			}
			set
			{
				_textAsset.Write(_position + 0, value);
			}
		}
		public byte Offset
		{
			get
			{
				return _textAsset.ReadByte(_position + 4);
			}
			set
			{
				_textAsset.Write(_position + 4, value);
			}
		}
		public byte PropertyType
		{
			get
			{
				return _textAsset.ReadByte(_position + 5);
			}
			set
			{
				_textAsset.Write(_position + 5, value);
			}
		}
		public float Value
		{
			get
			{
				return _textAsset.ReadSingle(_position + 8);
			}
			set
			{
				_textAsset.Write(_position + 8, value);
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
				return 12;
			}
		}
	}
}
