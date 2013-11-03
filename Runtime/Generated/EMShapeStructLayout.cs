using System.Runtime.InteropServices;
namespace LLT
{
	[StructLayout(LayoutKind.Explicit, Size=112)]
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
		public ushort ClipCount;
		[FieldOffset(106)]
		public ushort ShapeIndex;
		[FieldOffset(108)]
		public byte UpdateFlag;
	}
}
