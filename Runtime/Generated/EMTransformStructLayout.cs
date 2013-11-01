using System.Runtime.InteropServices;
namespace LLT
{
	[StructLayout(LayoutKind.Explicit, Size=36)]
	public struct EMTransformStructLayout
	{
		[FieldOffset(0)]
		public float M00;
		[FieldOffset(4)]
		public float M01;
		[FieldOffset(8)]
		public float M02;
		[FieldOffset(12)]
		public float M10;
		[FieldOffset(16)]
		public float M11;
		[FieldOffset(20)]
		public float M12;
		[FieldOffset(24)]
		public byte MA;
		[FieldOffset(25)]
		public byte MR;
		[FieldOffset(26)]
		public byte MG;
		[FieldOffset(27)]
		public byte MB;
		[FieldOffset(28)]
		public byte OA;
		[FieldOffset(29)]
		public byte OR;
		[FieldOffset(30)]
		public byte OG;
		[FieldOffset(31)]
		public byte OB;
		[FieldOffset(32)]
		public byte Placed;
	}
}
