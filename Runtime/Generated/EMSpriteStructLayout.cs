using System.Runtime.InteropServices;
namespace LLT
{
	[StructLayout(LayoutKind.Explicit, Size=88)]
	public struct EMSpriteStructLayout
	{
		[FieldOffset(0)]
		public LLT.EMTransformStructLayout Transform;
		[FieldOffset(36)]
		public LLT.EMTransformStructLayout LocalToWorld;
		[FieldOffset(72)]
		public int Depth;
		[FieldOffset(76)]
		public int ClipDepth;
		[FieldOffset(80)]
		public int SpriteIndex;
		[FieldOffset(84)]
		public byte UpdateFlag;
	}
}
