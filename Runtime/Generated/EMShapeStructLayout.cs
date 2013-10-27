using System.Runtime.InteropServices;
namespace LLT
{
	[StructLayout(LayoutKind.Explicit, Size=108)]
	public struct EMShapeStructLayout
	{
		[FieldOffset(0)]
		public EMTransformStructLayout Transform;
		[FieldOffset(36)]
		public EMTransformStructLayout LocalToWorld;
		[FieldOffset(72)]
		public EMRectStructLayout Rect;
		[FieldOffset(88)]
		public EMRectStructLayout Uv;
		[FieldOffset(104)]
		public ushort ClipCount;
	}
}
