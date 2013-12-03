using System;
using UnityEngine;
using LLT;
using System.Collections.Generic;

public static class EMCustomTransformUpdater
{
	public static void UpdateTransforms(IntPtr ptr, int rootTag, Vector3[] vertices, Color32[] colorAdds, Vector4[] colorMults)
	{ 
#if ALLOW_UNSAFE
		unsafe
		{
			int index = 0;

			TSTreeStreamTagStructLayout* rootTagPtr = (TSTreeStreamTagStructLayout*)((byte*)ptr.ToPointer() + rootTag);
			TSTreeStreamTagStructLayout debugRoot = *rootTagPtr;

			// ToDo: should be of variable size and shared instance of creating a new one every frame.
			TSTreeStreamTagStructLayout*[] parentTagsPtr = new TSTreeStreamTagStructLayout*[64];
			parentTagsPtr[0] = rootTagPtr;

			EMSpriteStructLayout* parentPtr = (EMSpriteStructLayout*)((byte*)rootTagPtr + TSTreeStreamTag.TSTreeStreamTagSizeOf);
			TSTreeStreamTagStructLayout* tagPtr = (TSTreeStreamTagStructLayout*)((byte*)parentPtr + rootTagPtr->EntrySizeOf);

			var count = 0;

			float m00, m01, m10, m11, m02, m12, xmin, ymin, xmax, ymax;
			
			Color32 colorAdd;
			Vector4 colorMult;

			while(true)
			{
				bool skipSubTree = false;

				parentPtr = (EMSpriteStructLayout*)((byte*)parentTagsPtr[index] + TSTreeStreamTag.TSTreeStreamTagSizeOf);
				if(tagPtr->TypeIndex == (int)EMFactory.Type.EMSprite)
				{
					EMSpriteStructLayout* currentPtr = (EMSpriteStructLayout*)((byte*)tagPtr + TSTreeStreamTag.TSTreeStreamTagSizeOf);

					currentPtr->LocalToWorld.M00 = parentPtr->LocalToWorld.M00 * currentPtr->Transform.M00 + parentPtr->LocalToWorld.M01 * currentPtr->Transform.M10;
					currentPtr->LocalToWorld.M01 = parentPtr->LocalToWorld.M00 * currentPtr->Transform.M01 + parentPtr->LocalToWorld.M01 * currentPtr->Transform.M11;
					currentPtr->LocalToWorld.M10 = parentPtr->LocalToWorld.M10 * currentPtr->Transform.M00 + parentPtr->LocalToWorld.M11 * currentPtr->Transform.M10;
					currentPtr->LocalToWorld.M11 = parentPtr->LocalToWorld.M10 * currentPtr->Transform.M01 + parentPtr->LocalToWorld.M11 * currentPtr->Transform.M11;
					currentPtr->LocalToWorld.M02 = parentPtr->LocalToWorld.M00 * currentPtr->Transform.M02 + parentPtr->LocalToWorld.M01 * currentPtr->Transform.M12 + parentPtr->LocalToWorld.M02;
					currentPtr->LocalToWorld.M12 = parentPtr->LocalToWorld.M10 * currentPtr->Transform.M02 + parentPtr->LocalToWorld.M11 * currentPtr->Transform.M12 + parentPtr->LocalToWorld.M12;
					
					currentPtr->LocalToWorld.MA = (byte)(parentPtr->LocalToWorld.MA * currentPtr->Transform.MA/255f);
					currentPtr->LocalToWorld.MR = (byte)(parentPtr->LocalToWorld.MR * currentPtr->Transform.MR/255f);
					currentPtr->LocalToWorld.MG = (byte)(parentPtr->LocalToWorld.MG * currentPtr->Transform.MG/255f);
					currentPtr->LocalToWorld.MB = (byte)(parentPtr->LocalToWorld.MB * currentPtr->Transform.MB/255f);
					
					int oa = parentPtr->LocalToWorld.OA + currentPtr->Transform.OA;
					int or = parentPtr->LocalToWorld.OR + currentPtr->Transform.OR;
					int og = parentPtr->LocalToWorld.OG + currentPtr->Transform.OG;
					int ob = parentPtr->LocalToWorld.OB + currentPtr->Transform.OB;
					currentPtr->LocalToWorld.OA = oa > byte.MaxValue ? byte.MaxValue : (byte)oa;
					currentPtr->LocalToWorld.OR = or > byte.MaxValue ? byte.MaxValue : (byte)or;
					currentPtr->LocalToWorld.OG = og > byte.MaxValue ? byte.MaxValue : (byte)og;
					currentPtr->LocalToWorld.OB = ob > byte.MaxValue ? byte.MaxValue : (byte)ob;
					
					var temp = currentPtr->LocalToWorld.Placed;
					currentPtr->LocalToWorld.Placed = (byte)(parentPtr->LocalToWorld.Placed & currentPtr->Transform.Placed);
					
					if(currentPtr->LocalToWorld.Placed == 0 && temp == 0)
					{
						skipSubTree = true;
					}
				}
				else if(tagPtr->TypeIndex == (int)EMFactory.Type.EMShape)
				{
					EMShapeStructLayout* currentPtr = (EMShapeStructLayout*)((byte*)tagPtr + TSTreeStreamTag.TSTreeStreamTagSizeOf);
					var temp = currentPtr->LocalToWorld.Placed;
					
					if(currentPtr->Transform.Placed > 0)
					{
						currentPtr->LocalToWorld.M00 = parentPtr->LocalToWorld.M00 * currentPtr->Transform.M00 + parentPtr->LocalToWorld.M01 * currentPtr->Transform.M10;
						currentPtr->LocalToWorld.M01 = parentPtr->LocalToWorld.M00 * currentPtr->Transform.M01 + parentPtr->LocalToWorld.M01 * currentPtr->Transform.M11;
						currentPtr->LocalToWorld.M10 = parentPtr->LocalToWorld.M10 * currentPtr->Transform.M00 + parentPtr->LocalToWorld.M11 * currentPtr->Transform.M10;
						currentPtr->LocalToWorld.M11 = parentPtr->LocalToWorld.M10 * currentPtr->Transform.M01 + parentPtr->LocalToWorld.M11 * currentPtr->Transform.M11;
						currentPtr->LocalToWorld.M02 = parentPtr->LocalToWorld.M00 * currentPtr->Transform.M02 + parentPtr->LocalToWorld.M01 * currentPtr->Transform.M12 + parentPtr->LocalToWorld.M02;
						currentPtr->LocalToWorld.M12 = parentPtr->LocalToWorld.M10 * currentPtr->Transform.M02 + parentPtr->LocalToWorld.M11 * currentPtr->Transform.M12 + parentPtr->LocalToWorld.M12;
						
						currentPtr->LocalToWorld.MA = (byte)(parentPtr->LocalToWorld.MA * currentPtr->Transform.MA/255f);
						currentPtr->LocalToWorld.MR = (byte)(parentPtr->LocalToWorld.MR * currentPtr->Transform.MR/255f);
						currentPtr->LocalToWorld.MG = (byte)(parentPtr->LocalToWorld.MG * currentPtr->Transform.MG/255f);
						currentPtr->LocalToWorld.MB = (byte)(parentPtr->LocalToWorld.MB * currentPtr->Transform.MB/255f);

						int oa = parentPtr->LocalToWorld.OA + currentPtr->Transform.OA;
						int or = parentPtr->LocalToWorld.OR + currentPtr->Transform.OR;
						int og = parentPtr->LocalToWorld.OG + currentPtr->Transform.OG;
						int ob = parentPtr->LocalToWorld.OB + currentPtr->Transform.OB;
						currentPtr->LocalToWorld.OA = oa > byte.MaxValue ? byte.MaxValue : (byte)oa;
						currentPtr->LocalToWorld.OR = or > byte.MaxValue ? byte.MaxValue : (byte)or;
						currentPtr->LocalToWorld.OG = og > byte.MaxValue ? byte.MaxValue : (byte)og;
						currentPtr->LocalToWorld.OB = ob > byte.MaxValue ? byte.MaxValue : (byte)ob;
						
						currentPtr->LocalToWorld.Placed = (byte)(parentPtr->LocalToWorld.Placed & currentPtr->Transform.Placed);
					}
					else
					{
						currentPtr->LocalToWorld.Placed = 0;
					}

					if(currentPtr->LocalToWorld.Placed == 0 && temp == 0)
					{
						skipSubTree = true;
					}

					if(!skipSubTree)
					{
						int shapeIndex = currentPtr->ShapeIndex;

						if(currentPtr->LocalToWorld.Placed == 0)
						{
							m00 = 0f;
							m01 = 0f;
							m10 = 0f;
							m11 = 0f;
						}
						else
						{
							m00 = currentPtr->LocalToWorld.M00;
							m01 = currentPtr->LocalToWorld.M01;
							m10 = currentPtr->LocalToWorld.M10;
							m11 = currentPtr->LocalToWorld.M11;
						}
						
						m02 = currentPtr->LocalToWorld.M02;
						m12 = currentPtr->LocalToWorld.M12;
						
						xmin = currentPtr->Rect.X;
						ymin = currentPtr->Rect.Y;
						xmax = currentPtr->Rect.X + currentPtr->Rect.Width;
						ymax = currentPtr->Rect.Y + currentPtr->Rect.Height;


						colorAdd.a = currentPtr->LocalToWorld.OA;
						colorAdd.r = currentPtr->LocalToWorld.OR;
						colorAdd.g = currentPtr->LocalToWorld.OG;
						colorAdd.b = currentPtr->LocalToWorld.OB;

						colorMult = new Vector4(currentPtr->LocalToWorld.MR/255f, currentPtr->LocalToWorld.MG/255f, currentPtr->LocalToWorld.MB/255f, currentPtr->LocalToWorld.MA/255f);
						
						vertices[shapeIndex * 4 + 0].x = m00 * xmin + m01 * ymax + m02;
						vertices[shapeIndex * 4 + 0].y = m10 * xmin + m11 * ymax + m12;
						
						vertices[shapeIndex * 4 + 1].x = m00 * xmin + m01 * ymin + m02;
						vertices[shapeIndex * 4 + 1].y = m10 * xmin + m11 * ymin + m12;
						
						vertices[shapeIndex * 4 + 2].x = m00 * xmax + m01 * ymin + m02;
						vertices[shapeIndex * 4 + 2].y = m10 * xmax + m11 * ymin + m12;
						
						vertices[shapeIndex * 4 + 3].x = m00 * xmax + m01 * ymax + m02;
						vertices[shapeIndex * 4 + 3].y = m10 * xmax + m11 * ymax + m12;

						colorAdds[shapeIndex * 4 + 0] = colorAdd;
						colorAdds[shapeIndex * 4 + 1] = colorAdd;
						colorAdds[shapeIndex * 4 + 2] = colorAdd;
						colorAdds[shapeIndex * 4 + 3] = colorAdd;

						colorMults[shapeIndex * 4 + 0] = colorMult;
						colorMults[shapeIndex * 4 + 1] = colorMult;
						colorMults[shapeIndex * 4 + 2] = colorMult;
						colorMults[shapeIndex * 4 + 3] = colorMult;
					}
				}
				
				TSTreeStreamTagStructLayout* previousTagPtr = tagPtr;
				tagPtr = (TSTreeStreamTagStructLayout*)((byte*)tagPtr + TSTreeStreamTag.TSTreeStreamTagSizeOf + tagPtr->EntrySizeOf);
				count++;
				
				if((byte*)tagPtr < (byte*)previousTagPtr + TSTreeStreamTag.TSTreeStreamTagSizeOf + previousTagPtr->EntrySizeOf + previousTagPtr->SubTreeSizeOf)
				{
					parentTagsPtr[++index] = previousTagPtr;
				}

				while(((byte*)tagPtr >= (byte*)parentTagsPtr[index] + TSTreeStreamTag.TSTreeStreamTagSizeOf + parentTagsPtr[index]->EntrySizeOf + parentTagsPtr[index]->SubTreeSizeOf))
				{
					if(--index == 0)
					{
						return;
					}
				}
			}
		}	
#endif
	}
}