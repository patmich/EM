using System.Runtime.InteropServices;
namespace LLT
{
	[StructLayout(LayoutKind.Explicit, Size=76)]
	public struct EMSpriteStructLayout
	{
		[FieldOffset(0)]
		public EMTransformStructLayout Transform;
		[FieldOffset(36)]
		public EMTransformStructLayout LocalToWorld;
		[FieldOffset(70)]
		public ushort ClipCount;
		[FieldOffset(72)]
		public ushort AnimationId;
	}
}
