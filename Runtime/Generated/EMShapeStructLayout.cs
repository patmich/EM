using System.Runtime.InteropServices;
namespace LLT
{
	[StructLayout(LayoutKind.Explicit, Size=124)]
	public struct EMShapeStructLayout
	{
		[FieldOffset(0)]
		public LLT.EMTransformStructLayout Transform;
		[FieldOffset(36)]
		public LLT.EMTransformStructLayout LocalToWorld;
		[FieldOffset(72)]
		public LLT.EMRectStructLayout Rect;
		[FieldOffset(88)]
		public LLT.EMRectStructLayout Uv;
		[FieldOffset(104)]
		public int Depth;
		[FieldOffset(108)]
		public int ClipDepth;
		[FieldOffset(112)]
		public int ShapeIndex;
		[FieldOffset(116)]
		public int DrawcallIndex;
		[FieldOffset(120)]
		public byte TextureIndex;
		[FieldOffset(121)]
		public byte UpdateFlag;
	}
}
