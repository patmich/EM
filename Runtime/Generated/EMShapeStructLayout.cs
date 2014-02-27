using System.Runtime.InteropServices;
namespace LLT
{
	[StructLayout(LayoutKind.Explicit, Size=116)]
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
		public ushort Depth;
		[FieldOffset(106)]
		public ushort ClipDepth;
		[FieldOffset(108)]
		public ushort DrawcallIndex;
		[FieldOffset(110)]
		public ushort ShapeIndex;
		[FieldOffset(112)]
		public byte TextureIndex;
		[FieldOffset(113)]
		public byte UpdateFlag;
	}
}
