using System.Runtime.InteropServices;
namespace LLT
{
	[StructLayout(LayoutKind.Explicit, Size=80)]
	public struct EMSpriteStructLayout
	{
		[FieldOffset(0)]
		public LLT.EMTransformStructLayout Transform;
		[FieldOffset(36)]
		public LLT.EMTransformStructLayout LocalToWorld;
		[FieldOffset(69)]
		public ushort Depth;
		[FieldOffset(72)]
		public ushort ClipDepth;
		[FieldOffset(74)]
		public byte UpdateFlag;
	}
}
