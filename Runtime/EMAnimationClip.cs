namespace LLT
{
	[TSLayout(typeof(float), "Length", 0)]
	public sealed partial class EMAnimationClip : TSTreeStreamEntry
	{
		public new int FactoryTypeIndex 
		{
			get 
			{
				return (int)EMFactory.Type.EMAnimationClip;
			}
		}
	}
}
