public sealed class EMSwfTransform
{
	public enum Component
	{
		M00,
		M01,
		M02,
		M10,
		M11,
		M12,
		ClipDepth,
		CXAddA,
		CXAddR,
		CXAddG,
		CXAddB,
		CXMulA,
		CXMulR,
		CXMulG,
		CXMulB,
		
		Num,
	}
	
	public static float[] InitialValues =
	{
		1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 0
	};
	
	public bool IsCXAdd(Component comp)
	{
		return Component.CXAddA <= comp && comp <= Component.CXAddB;
	}
	
	public bool IsCXMul(Component comp)
	{
		return Component.CXMulA <= comp && comp <= Component.CXMulB;
	}
}