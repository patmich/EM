using System.Runtime.InteropServices;
namespace LLT
{
	[StructLayout(LayoutKind.Explicit, Size=8)]
	public struct EMAnimationKeyframeValueStructLayout
	{
		[FieldOffset(0)]
		public ushort ChildIndex;
		[FieldOffset(2)]
		public byte Offset;
		[FieldOffset(3)]
		public byte PropertyType;
		[FieldOffset(4)]
		public float Value;
	}
}
