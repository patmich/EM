namespace LLT
{
	[TSLayout(typeof(int), "ChildIndex", 0)]
	[TSLayout(typeof(int), "Offset", 1)]
	[TSLayout(typeof(byte), "PropertyType", 2)]
	[TSLayout(typeof(float), "Value", 3)]
	public sealed partial class EMAnimationKeyframeValue : TSTreeStreamEntry
	{
		public new int FactoryTypeIndex 
		{
			get 
			{
				return (int)EMFactory.Type.EMAnimationKeyframeValue;
			}
		}
	}
}
