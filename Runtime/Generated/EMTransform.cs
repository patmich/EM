namespace LLT
{
	public sealed partial class EMTransform : LLT.TSTreeStreamEntry, LLT.ITSFactoryInstance
	{
		public const int M00_Offset = 0;
		public const int M01_Offset = 4;
		public const int M02_Offset = 8;
		public const int M10_Offset = 12;
		public const int M11_Offset = 16;
		public const int M12_Offset = 20;
		public const int MA_Offset = 24;
		public const int MR_Offset = 25;
		public const int MG_Offset = 26;
		public const int MB_Offset = 27;
		public const int OA_Offset = 28;
		public const int OR_Offset = 29;
		public const int OG_Offset = 30;
		public const int OB_Offset = 31;
		public const int Placed_Offset = 32;
		public const int EMTransformSizeOf = 36;


		public float M00
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
		public float M01
		{
			get
			{
				return _textAsset.ReadSingle(_position + 4);
			}
			set
			{
				_textAsset.Write(_position + 4, value);
			}
		}
		public float M02
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
		public float M10
		{
			get
			{
				return _textAsset.ReadSingle(_position + 12);
			}
			set
			{
				_textAsset.Write(_position + 12, value);
			}
		}
		public float M11
		{
			get
			{
				return _textAsset.ReadSingle(_position + 16);
			}
			set
			{
				_textAsset.Write(_position + 16, value);
			}
		}
		public float M12
		{
			get
			{
				return _textAsset.ReadSingle(_position + 20);
			}
			set
			{
				_textAsset.Write(_position + 20, value);
			}
		}
		public byte MA
		{
			get
			{
				return _textAsset.ReadByte(_position + 24);
			}
			set
			{
				_textAsset.Write(_position + 24, value);
			}
		}
		public byte MR
		{
			get
			{
				return _textAsset.ReadByte(_position + 25);
			}
			set
			{
				_textAsset.Write(_position + 25, value);
			}
		}
		public byte MG
		{
			get
			{
				return _textAsset.ReadByte(_position + 26);
			}
			set
			{
				_textAsset.Write(_position + 26, value);
			}
		}
		public byte MB
		{
			get
			{
				return _textAsset.ReadByte(_position + 27);
			}
			set
			{
				_textAsset.Write(_position + 27, value);
			}
		}
		public byte OA
		{
			get
			{
				return _textAsset.ReadByte(_position + 28);
			}
			set
			{
				_textAsset.Write(_position + 28, value);
			}
		}
		public byte OR
		{
			get
			{
				return _textAsset.ReadByte(_position + 29);
			}
			set
			{
				_textAsset.Write(_position + 29, value);
			}
		}
		public byte OG
		{
			get
			{
				return _textAsset.ReadByte(_position + 30);
			}
			set
			{
				_textAsset.Write(_position + 30, value);
			}
		}
		public byte OB
		{
			get
			{
				return _textAsset.ReadByte(_position + 31);
			}
			set
			{
				_textAsset.Write(_position + 31, value);
			}
		}
		public byte Placed
		{
			get
			{
				return _textAsset.ReadByte(_position + 32);
			}
			set
			{
				_textAsset.Write(_position + 32, value);
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
				return 36;
			}
		}
	}
}
