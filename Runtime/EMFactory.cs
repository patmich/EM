namespace LLT
{
	public sealed class EMFactory : TSFactory
	{
		public enum Type
		{
			EMSprite,
			EMShape,
			EMText,
			EMAnimation,
			EMAnimationClip,
			EMAnimationKeyframe,
			EMAnimationKeyframeValue,
		}
		
		private static System.Type[] _factoryTypes = 
		{
			typeof(EMSprite),
			typeof(EMShape),
			typeof(EMText),
			typeof(EMAnimation),
			typeof(EMAnimationClip),
			typeof(EMAnimationKeyframe),
			typeof(EMAnimationKeyframeValue)
		};
		
		public override System.Type[] FactoryTypes 
		{
			get 
			{
				return _factoryTypes;
			}
		}
	}
}