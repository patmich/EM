namespace LLT
{
	public partial struct EMTransformStructLayout
	{
		public UnityEngine.Vector2 MultiplyPoint(UnityEngine.Vector2 point)
		{
			return new UnityEngine.Vector2( M00 * point.x + M01 * point.y + M02,  M10 * point.x + M11 * point.y + M12);
		}
	}
}
