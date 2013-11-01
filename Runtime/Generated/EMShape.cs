namespace LLT
{
	public sealed partial class EMShape : LLT.TSTreeStreamEntry, LLT.ITSFactoryInstance
	{
		public const int Transform_M00_Offset = 0;
		public const int Transform_M01_Offset = 4;
		public const int Transform_M02_Offset = 8;
		public const int Transform_M10_Offset = 12;
		public const int Transform_M11_Offset = 16;
		public const int Transform_M12_Offset = 20;
		public const int Transform_MA_Offset = 24;
		public const int Transform_MR_Offset = 25;
		public const int Transform_MG_Offset = 26;
		public const int Transform_MB_Offset = 27;
		public const int Transform_OA_Offset = 28;
		public const int Transform_OR_Offset = 29;
		public const int Transform_OG_Offset = 30;
		public const int Transform_OB_Offset = 31;
		public const int Transform_Placed_Offset = 32;
		public const int LocalToWorld_M00_Offset = 36;
		public const int LocalToWorld_M01_Offset = 40;
		public const int LocalToWorld_M02_Offset = 44;
		public const int LocalToWorld_M10_Offset = 48;
		public const int LocalToWorld_M11_Offset = 52;
		public const int LocalToWorld_M12_Offset = 56;
		public const int LocalToWorld_MA_Offset = 60;
		public const int LocalToWorld_MR_Offset = 61;
		public const int LocalToWorld_MG_Offset = 62;
		public const int LocalToWorld_MB_Offset = 63;
		public const int LocalToWorld_OA_Offset = 64;
		public const int LocalToWorld_OR_Offset = 65;
		public const int LocalToWorld_OG_Offset = 66;
		public const int LocalToWorld_OB_Offset = 67;
		public const int LocalToWorld_Placed_Offset = 68;
		public const int Rect_X_Offset = 72;
		public const int Rect_Y_Offset = 76;
		public const int Rect_Width_Offset = 80;
		public const int Rect_Height_Offset = 84;
		public const int Uv_X_Offset = 88;
		public const int Uv_Y_Offset = 92;
		public const int Uv_Width_Offset = 96;
		public const int Uv_Height_Offset = 100;
		public const int ClipCount_Offset = 104;
		public const int EMShapeSizeOf = 108;

		public readonly LLT.EMTransform Transform = new LLT.EMTransform();
		public readonly LLT.EMTransform LocalToWorld = new LLT.EMTransform();
		public readonly LLT.EMRect Rect = new LLT.EMRect();
		public readonly LLT.EMRect Uv = new LLT.EMRect();

		public ushort ClipCount
		{
			get
			{
				return _tree.ReadUInt16(_position + 104);
			}
			set
			{
				_tree.Write(_position + 104, value);
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
				Transform.Position = _position + 0;
				LocalToWorld.Position = _position + 36;
				Rect.Position = _position + 72;
				Uv.Position = _position + 88;
			}
		}

		public override void Init(ITSTreeStream tree)
		{
			_tree = tree;
			Transform.Init(_tree);
			LocalToWorld.Init(_tree);
			Rect.Init(_tree);
			Uv.Init(_tree);
		}

		public override int SizeOf
		{
			get
			{
				return 108;
			}
		}
	}
}
