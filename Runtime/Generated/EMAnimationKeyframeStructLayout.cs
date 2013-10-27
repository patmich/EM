using System.Runtime.InteropServices;
namespace LLT
{
	[StructLayout(LayoutKind.Explicit, Size=4)]
	public struct EMAnimationKeyframeStructLayout
	{
		[FieldOffset(0)]
		public float Time;
	}
}
