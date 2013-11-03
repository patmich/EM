using System.Runtime.InteropServices;
namespace LLT
{
	[StructLayout(LayoutKind.Explicit, Size=76)]
	public struct EMSpriteStructLayout
	{
		[FieldOffset(0)]
		public LLT.EMTransformStructLayout Transform;
		[FieldOffset(36)]
		public LLT.EMTransformStructLayout LocalToWorld;
		[FieldOffset(69)]
		public ushort ClipCount;
		[FieldOffset(71)]
		public byte UpdateFlag;
	}
}
