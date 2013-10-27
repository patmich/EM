namespace LLT
{
	[TSLayout(typeof(float), "Time", 0)]
	public sealed partial class EMAnimationKeyframe : TSTreeStreamEntry
	{
		public new int FactoryTypeIndex 
		{
			get 
			{
				return (int)EMFactory.Type.EMAnimationKeyframe;
			}
		}
	}
}
