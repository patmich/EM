namespace LLT
{
	[TSLayout(typeof(EMTransform), "Transform", 0)]
	[TSLayout(typeof(EMTransform), "LocalToWorld", 1)]
	[TSLayout(typeof(EMRect), "Rect", 2)]
	[TSLayout(typeof(EMRect), "Uv", 3)]
	[TSLayout(typeof(ushort), "ClipCount", 4)]
    [TSLayout(typeof(ushort), "ShapeIndex", 5)]
	[TSLayout(typeof(byte), "UpdateFlag", 6)]
	public sealed partial class EMShape : TSTreeStreamEntry
	{
		public new int FactoryTypeIndex 
		{
			get 
			{
				return (int)EMFactory.Type.EMShape;
			}
		}
	}
}
