using System.Runtime.InteropServices;
namespace LLT
{
	[StructLayout(LayoutKind.Explicit, Size=4)]
	public struct EMAnimationClipStructLayout
	{
		[FieldOffset(0)]
		public float Length;
	}
}
