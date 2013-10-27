using System.Runtime.InteropServices;
namespace LLT
{
	[StructLayout(LayoutKind.Explicit, Size=16)]
	public struct EMRectStructLayout
	{
		[FieldOffset(0)]
		public float X;
		[FieldOffset(4)]
		public float Y;
		[FieldOffset(8)]
		public float Width;
		[FieldOffset(12)]
		public float Height;
	}
}
