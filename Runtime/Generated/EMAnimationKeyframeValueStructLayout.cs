using System.Runtime.InteropServices;
namespace LLT
{
	[StructLayout(LayoutKind.Explicit, Size=12)]
	public struct EMAnimationKeyframeValueStructLayout
	{
		[FieldOffset(0)]
		public int ChildIndex;
		[FieldOffset(4)]
		public byte Offset;
		[FieldOffset(5)]
		public byte PropertyType;
		[FieldOffset(8)]
		public float Value;
	}
}
