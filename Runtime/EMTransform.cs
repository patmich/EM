using UnityEngine;

namespace LLT
{
	[TSLayout(typeof(float), "M00", 0)]
	[TSLayout(typeof(float), "M01", 1)]
	[TSLayout(typeof(float), "M02", 2)]
	[TSLayout(typeof(float), "M10", 3)]
	[TSLayout(typeof(float), "M11", 4)]
	[TSLayout(typeof(float), "M12", 5)]
	[TSLayout(typeof(byte), "MA", 6)]
	[TSLayout(typeof(byte), "MR", 7)]
	[TSLayout(typeof(byte), "MG", 8)]
	[TSLayout(typeof(byte), "MB", 9)]
	[TSLayout(typeof(byte), "OA", 10)]
	[TSLayout(typeof(byte), "OR", 11)]
	[TSLayout(typeof(byte), "OG", 12)]
	[TSLayout(typeof(byte), "OB", 13)]
	[TSLayout(typeof(byte), "Placed", 14)]
	public sealed partial class EMTransform : TSTreeStreamEntry
	{
		public void MakeIdentity()
		{
			M00 = 1f;
			M01 = 0f;
			M02 = 0f;
			M10 = 0f;
			M11 = 1f;
			M12 = 0f;
            MA = 255;
            MR = 255;
            MG = 255;
            MB = 255;
            OA = 0;
            OR = 0;
            OG = 0;
            OB = 0;
			Placed = (byte)1;
		}
		
		public void Concat(EMTransform localToWorld, EMTransform transform)
		{
			M00 = localToWorld.M00 * transform.M00 + localToWorld.M01 * transform.M10;
			M01 = localToWorld.M00 * transform.M01 + localToWorld.M01 * transform.M11;
			M10 = localToWorld.M10 * transform.M00 + localToWorld.M11 * transform.M10;
			M11 = localToWorld.M10 * transform.M01 + localToWorld.M11 * transform.M11;
			M02 = localToWorld.M00 * transform.M02 + localToWorld.M01 * transform.M12 + localToWorld.M02;
			M12 = localToWorld.M10 * transform.M02 + localToWorld.M11 * transform.M12 + localToWorld.M12;
			Placed = (byte)(localToWorld.Placed & transform.Placed);
		}

        public UnityEngine.Matrix4x4 Inverse()
        {
            var mat = Matrix4x4.identity;
            mat.m00 = M00;
            mat.m01 = M01;
            mat.m03 = M02;
            mat.m10 = M10;
            mat.m11 = M11;
            mat.m13 = M12;
            return mat.inverse;
        }

        public void Set(Matrix4x4 mat)
        {
            M00 = mat.m00;
            M01 = mat.m01;
            M02 = mat.m02;
            M10 = mat.m10;
            M11 = mat.m11;
            M12 = mat.m12;
        }
	}
}
