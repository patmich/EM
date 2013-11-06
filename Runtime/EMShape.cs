using UnityEngine;

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
		
		public Bounds Bounds
		{
			get
			{
				float m00, m01, m10, m11, m02, m12, xmin, ymin, xmax, ymax;
            
#if UNITY_WEB || !ALLOW_UNSAFE		
				if(LocalToWorld.Placed == 0)
				{
					return new Bounds(Vector3.zero, Vector3.zero);
				}
				else
				{
					m00 = LocalToWorld.M00;
					m01 = LocalToWorld.M01;
					m10 = LocalToWorld.M10;
					m11 = LocalToWorld.M11;
				}
				
				m02 = LocalToWorld.M02;
				m12 = LocalToWorld.M12;
				
				xmin = Rect.X;
				ymin = Rect.Y;
				xmax = Rect.X + Rect.Width;
				ymax = Rect.Y + Rect.Height;
#else
				unsafe
				{
					EMShapeStructLayout* ptr = (EMShapeStructLayout*)((byte*)_tree.Ptr.ToPointer() + _position);
					
					if(ptr->LocalToWorld.Placed == 0)
					{
						return new Bounds(Vector3.zero, Vector3.zero);
					}
					else
					{
						m00 = ptr->LocalToWorld.M00;
						m01 = ptr->LocalToWorld.M01;
						m10 = ptr->LocalToWorld.M10;
						m11 = ptr->LocalToWorld.M11;
					}
	
					m02 = ptr->LocalToWorld.M02;
					m12 = ptr->LocalToWorld.M12;
					
					xmin = ptr->Rect.X;
					ymin = ptr->Rect.Y;
					xmax = ptr->Rect.X + ptr->Rect.Width;
					ymax = ptr->Rect.Y + ptr->Rect.Height;
				}
#endif
	
				var x1 = m00 * xmin + m01 * ymax + m02;
				var y1 = m10 * xmin + m11 * ymax + m12;
				
				var x2 = m00 * xmin + m01 * ymin + m02;
				var y2 = m10 * xmin + m11 * ymin + m12;
				
				var x3 = m00 * xmax + m01 * ymin + m02;
				var y3 = m10 * xmax + m11 * ymin + m12;
				
				var x4 = m00 * xmax + m01 * ymax + m02;
				var y4 = m10 * xmax + m11 * ymax + m12;
			
				var rect = new Rect(0f, 0f, 0f, 0f);
				rect.xMin = Mathf.Min(x1, x2, x3, x4);
				rect.xMax = Mathf.Max(x1, x2, x3, x4);
				rect.yMin = Mathf.Min(y1, y2, y3, y4);
				rect.yMax = Mathf.Max(y1, y2, y3, y4);
				
				return new Bounds(rect.center, new Vector2(rect.width, rect.height));
			}
		}
	}
}
