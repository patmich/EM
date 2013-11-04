using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LLT
{
	[ExecuteInEditMode]
	public sealed class EMRoot : EMMonoBehaviour 
	{
		private sealed class Drawcall
		{
			private class Indexing
			{
				public int ShapeIndex;
				public int Count;
			}
			
			private readonly List<Indexing> _indices = new List<Indexing>();
			private int _count;
			
			public Material Material { private set; get; }
			public ShaderType ShaderType { private set; get; }
			
			public int[] Indices
			{
				get
				{
					CoreAssert.Fatal(Count == _count);
					
					var indices = new int[_count * 4];
					for(var i = 0; i < _indices.Count; i++)
					{
						for(var j = 0; j < _indices[i].Count; j++)
						{
							var shapeIndex = _indices[i].ShapeIndex * 4;
							var baseIndex = (i + j) * 4;
							indices[baseIndex + 0] = shapeIndex + 0;
							indices[baseIndex + 1] = shapeIndex + 1;
							indices[baseIndex + 2] = shapeIndex + 2;
							indices[baseIndex + 3] = shapeIndex + 3;
						}
					}
					
					return indices;
				}
			}
			
			private int Count
			{
				get
				{
					var count = 0;
					for(var i = 0; i < _indices.Count; i++)
					{
						count += _indices[i].Count;
					}
					return count;
				}
			}
			
			public Drawcall(ShaderType shaderType, Material material)
			{
				ShaderType = shaderType;
				Material = material;
			}
			
			public void Add(int shapeIndex)
			{
				CoreAssert.Fatal(_indices.Count == 0 || _indices[_indices.Count - 1].ShapeIndex < shapeIndex);
				if(_indices.Count == 0 || _indices[_indices.Count - 1].ShapeIndex != shapeIndex + 1)
				{
					_indices.Add(new Indexing(){ShapeIndex = shapeIndex, Count = 1});
				}
				else
				{
					_indices[_indices.Count - 1].Count++;
				}
				_count++;
			}
		}
		
		private enum ShaderType
		{
			Transparent,
			StencilIncrement,
			StencilDecrement,
			Count,
		}
		
		private class MaskOperation
		{
			public int Start;
			public int End;
			public int StencilRef;
		}
		
		[SerializeField]
		private TextAsset _bytes;
		
		[SerializeField]
		private Texture2D _atlas;
		
		[SerializeField]
		private EMDisplayTreeStream _tree = new EMDisplayTreeStream();

		private Mesh _mesh;
		
		private Vector3[] _vertices;
		private Vector2[] _uv;
	
		private readonly List<Drawcall> _drawcalls = new List<Drawcall>();
		private ushort _shapeCount;
		
		// ToDo need as many shared material multiple as mask depth.
		private readonly Material[] _sharedMaterials = new Material[(int)ShaderType.Count * 2];
		private TSTreeStreamSiblingEnumerator _siblingEnumerator;
		private bool _updateDrawcalls;
		
		public ITSTreeStream DisplayTree
		{
			get
			{
				return _tree;
			}
		}
		
		public Mesh Mesh 
		{
			get
			{
				if(_mesh == null)
				{
					_mesh = new Mesh();
					_mesh.MarkDynamic();
				}
				return _mesh;
			}
		}
		
		private void Awake()
		{
			_updateDrawcalls = true;
			enabled = false;
			StartCoroutine(Init ());
		}
		
		public EMObject FindObject(params string[] path)
		{
			return _tree.FindObject(path);
		}
		
		private IEnumerator Init()
		{
			while(_bytes == null)
			{
				yield return null;
			}
			
			_tree.InitFromBytes(_bytes.bytes, null, new EMFactory());
			
			_siblingEnumerator = new TSTreeStreamSiblingEnumerator(_tree);
		    InitMesh();
			enabled = true;
		}
		
		private void Update()
		{
			InitMesh();
			
			var mesh = Mesh;
			_updateDrawcalls |= UpdateTransforms();
			
			mesh.vertices = _vertices;
			mesh.uv = _uv;
			
			//if(_updateDrawcalls)
			{
				UpdateDrawcalls();
				_updateDrawcalls = false;
			}
		}
		
		private void InitMesh()
		{
			_shapeCount = 0;

			var iter = new EMTreeStreamDFSEnumerator(this);
			while(iter.MoveNext(false))
			{
				if(iter.IsShape())
				{
					iter.Shape.ShapeIndex = _shapeCount++;
				}	
			}
			
			_vertices = new Vector3[_shapeCount * 4];
			_uv = new Vector2[_shapeCount * 4];
		}
		
		private void UpdateDrawcalls()
		{
			_drawcalls.Clear();
			_shapeCount = 0;
			
			AddDrawcall(ShaderType.Transparent, 0);
			
			var mask = new List<MaskOperation>();
			var masked = new List<MaskOperation>();
			
			var iter = new EMTreeStreamDFSEnumerator(this);
			while(iter.MoveNext(false))
			{
				var clipCount = 0;
				if(iter.IsSprite())
				{
					clipCount = iter.Sprite.ClipCount;
				}
				else if(iter.IsShape())
				{
					clipCount = iter.Shape.ClipCount;
				}	
				if(clipCount > 0)
				{
					_siblingEnumerator.Init(iter.ParentTag, iter.Current);
					_siblingEnumerator.MoveNext();
					
					for(var i = 0; i < clipCount; i++)
					{
						_siblingEnumerator.MoveNext();
					}
					
					mask.Add(new MaskOperation(){Start = (int)iter.Current.Position, End = iter.Current.SiblingPosition, StencilRef = mask.Count});
					masked.Add(new MaskOperation(){Start = (int)iter.Current.Position, End = _siblingEnumerator.Current.SiblingPosition, StencilRef = mask.Count});
				}
				
				if(iter.IsShape())
				{
					MaskOperation maskOperation = null;
					if(mask.Count > 0)
					{
						maskOperation = mask.FindLast(x=>x.Start <= iter.Current.Position && iter.Current.Position < x.End);
						
						if(maskOperation != null)
						{						
							AddDrawcall(ShaderType.StencilIncrement, maskOperation.StencilRef);
							_drawcalls[_drawcalls.Count - 1].Add(iter.Shape.ShapeIndex);
						}
					}
					
					if(maskOperation == null && masked.Count > 0)
					{
						maskOperation = masked.FindLast(x=>x.Start <= iter.Current.Position && iter.Current.Position < x.End);
						if(maskOperation != null)
						{
							AddDrawcall(ShaderType.Transparent, maskOperation.StencilRef);
							_drawcalls[_drawcalls.Count - 1].Add(iter.Shape.ShapeIndex);
							
							if(maskOperation.End <= iter.Current.SiblingPosition)
							{
								maskOperation = mask[mask.Count - 1];
									
								AddDrawcall(ShaderType.StencilDecrement, maskOperation.StencilRef + 1);							
								AddDrawcall(ShaderType.Transparent, 0);
								
								masked.Clear();
								mask.Clear();
							}
						}
					}
					
					if(maskOperation == null)
					{
						_drawcalls[_drawcalls.Count - 1].Add(iter.Shape.ShapeIndex);
					}
				}	
			}
			
			var mesh = Mesh;
			mesh.subMeshCount = _drawcalls.Count;	
			
			for(var i = 0; i < _drawcalls.Count; i++)
			{
				mesh.SetIndices(_drawcalls[i].Indices, MeshTopology.Quads, i);
			}
		}
		
		private bool UpdateTransforms()
		{
			var iter = new EMTreeStreamDFSEnumerator(this);
			iter.Parent.LocalToWorld.MakeIdentity();
			iter.Parent.LocalToWorld.M11 = -1f;
			
			var treePtr = _tree.Pin();
			var skipSubTree = false;
			
			var updateDrawCalls = false;
			
			while(iter.MoveNext(skipSubTree))
			{
				skipSubTree = false;
				if(iter.IsSprite())
				{	
#if UNITY_WEB || !ALLOW_UNSAFE
					iter.Sprite.LocalToWorld.Concat(iter.Parent.LocalToWorld, iter.Sprite.Transform);
#else				
					unsafe
					{
						byte* ptr = (byte*)treePtr.ToPointer();
						EMSpriteStructLayout* p0 = (EMSpriteStructLayout*)(ptr + iter.Parent.Position);
						EMSpriteStructLayout* p1 = (EMSpriteStructLayout*)(ptr + iter.Sprite.Position);
						
						p1->LocalToWorld.M00 = p0->LocalToWorld.M00 * p1->Transform.M00 + p0->LocalToWorld.M01 * p1->Transform.M10;
						p1->LocalToWorld.M01 = p0->LocalToWorld.M00 * p1->Transform.M01 + p0->LocalToWorld.M01 * p1->Transform.M11;
						p1->LocalToWorld.M10 = p0->LocalToWorld.M10 * p1->Transform.M00 + p0->LocalToWorld.M11 * p1->Transform.M10;
						p1->LocalToWorld.M11 = p0->LocalToWorld.M10 * p1->Transform.M01 + p0->LocalToWorld.M11 * p1->Transform.M11;
						p1->LocalToWorld.M02 = p0->LocalToWorld.M00 * p1->Transform.M02 + p0->LocalToWorld.M01 * p1->Transform.M12 + p0->LocalToWorld.M02;
						p1->LocalToWorld.M12 = p0->LocalToWorld.M10 * p1->Transform.M02 + p0->LocalToWorld.M11 * p1->Transform.M12 + p0->LocalToWorld.M12;
						
						var temp = p1->LocalToWorld.Placed;
						p1->LocalToWorld.Placed = (byte)(p0->LocalToWorld.Placed & p1->Transform.Placed);
						
						if(p1->LocalToWorld.Placed == 0 && temp == 0)
						{
							skipSubTree = true;
						}
						
						updateDrawCalls |= ((p1->UpdateFlag & 1) > 0);
						p1->UpdateFlag = 0;
					}
#endif
				}
				else if(iter.IsShape())
				{
#if UNITY_WEB || !ALLOW_UNSAFE
					iter.Shape.LocalToWorld.Concat(iter.Parent.LocalToWorld, iter.Shape.Transform);
#else
					unsafe
					{
						byte* ptr = (byte*)treePtr.ToPointer();
						EMSpriteStructLayout* p0 = (EMSpriteStructLayout*)(ptr + iter.Parent.Position);
						EMShapeStructLayout* p1 = (EMShapeStructLayout*)(ptr + iter.Shape.Position);
						
						p1->LocalToWorld.M00 = p0->LocalToWorld.M00 * p1->Transform.M00 + p0->LocalToWorld.M01 * p1->Transform.M10;
						p1->LocalToWorld.M01 = p0->LocalToWorld.M00 * p1->Transform.M01 + p0->LocalToWorld.M01 * p1->Transform.M11;
						p1->LocalToWorld.M10 = p0->LocalToWorld.M10 * p1->Transform.M00 + p0->LocalToWorld.M11 * p1->Transform.M10;
						p1->LocalToWorld.M11 = p0->LocalToWorld.M10 * p1->Transform.M01 + p0->LocalToWorld.M11 * p1->Transform.M11;
						p1->LocalToWorld.M02 = p0->LocalToWorld.M00 * p1->Transform.M02 + p0->LocalToWorld.M01 * p1->Transform.M12 + p0->LocalToWorld.M02;
						p1->LocalToWorld.M12 = p0->LocalToWorld.M10 * p1->Transform.M02 + p0->LocalToWorld.M11 * p1->Transform.M12 + p0->LocalToWorld.M12;
						
						var temp = p1->LocalToWorld.Placed;
						p1->LocalToWorld.Placed = (byte)(p0->LocalToWorld.Placed & p1->Transform.Placed);
						
						if(p1->LocalToWorld.Placed == 0 && temp == 0)
						{
							skipSubTree = true;
						}
						
						updateDrawCalls |= ((p1->UpdateFlag & 1) > 0);
						p1->UpdateFlag = 0;
					}
#endif
					if(!skipSubTree)
					{
						UpdateGeometry(iter.Shape, treePtr);
					}
				}
			}
			
			_tree.Release();
			return updateDrawCalls;
		}
		
		private void UpdateGeometry(EMShape shape, System.IntPtr treePtr)
		{
            int shapeIndex = shape.ShapeIndex;
			float m00, m01, m10, m11, m02, m12, xmin, ymin, xmax, ymax;
            
	#if UNITY_WEB || !ALLOW_UNSAFE		
			if(shape.LocalToWorld.Placed == 0)
			{
				m00 = 0f;
				m01 = 0f;
				m10 = 0f;
				m11 = 0f;
			}
			else
			{
				m00 = shape.LocalToWorld.M00;
				m01 = shape.LocalToWorld.M01;
				m10 = shape.LocalToWorld.M10;
				m11 = shape.LocalToWorld.M11;
			}
			m02 = shape.LocalToWorld.M02;
			m12 = shape.LocalToWorld.M12;
			
			xmin = shape.Rect.X;
			ymin = shape.Rect.Y;
			xmax = shape.Rect.X + shape.Rect.Width;
			ymax = shape.Rect.Y + shape.Rect.Height;
	#else
			unsafe
			{
				EMShapeStructLayout* ptr = (EMShapeStructLayout*)((byte*)treePtr.ToPointer() + shape.Position);
				
				if(shape.LocalToWorld.Placed == 0)
				{
					m00 = 0f;
					m01 = 0f;
					m10 = 0f;
					m11 = 0f;
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
			
			if(m00 * m11 < 0)
			{
				_vertices[shapeIndex * 4 + 0].x = m00 * xmin + m01 * ymax + m02;
				_vertices[shapeIndex * 4 + 0].y = m10 * xmin + m11 * ymax + m12;
				
				_vertices[shapeIndex * 4 + 1].x = m00 * xmin + m01 * ymin + m02;
				_vertices[shapeIndex * 4 + 1].y = m10 * xmin + m11 * ymin + m12;
				
				_vertices[shapeIndex * 4 + 2].x = m00 * xmax + m01 * ymin + m02;
				_vertices[shapeIndex * 4 + 2].y = m10 * xmax + m11 * ymin + m12;
				
				_vertices[shapeIndex * 4 + 3].x = m00 * xmax + m01 * ymax + m02;
				_vertices[shapeIndex * 4 + 3].y = m10 * xmax + m11 * ymax + m12;
				
				xmin = shape.Uv.X;
				ymin = shape.Uv.Y;
				xmax = xmin + shape.Uv.Width;
				ymax = ymin + shape.Uv.Height;
				
				_uv[shapeIndex * 4 + 0].x = xmin;
				_uv[shapeIndex * 4 + 0].y = ymin;
				
				_uv[shapeIndex * 4 + 1].x = xmin;
				_uv[shapeIndex * 4 + 1].y = ymax;
				
				_uv[shapeIndex * 4 + 2].x = xmax;
				_uv[shapeIndex * 4 + 2].y = ymax;
				
				_uv[shapeIndex * 4 + 3].x = xmax;
				_uv[shapeIndex * 4 + 3].y = ymin;
			}
			else
			{
				_vertices[shapeIndex * 4 + 0].x = m00 * xmin + m01 * ymin + m02;
				_vertices[shapeIndex * 4 + 0].y = m10 * xmin + m11 * ymin + m12;
				
				_vertices[shapeIndex * 4 + 1].x = m00 * xmin + m01 * ymax + m02;
				_vertices[shapeIndex * 4 + 1].y = m10 * xmin + m11 * ymax + m12;
				
				_vertices[shapeIndex * 4 + 2].x = m00 * xmax + m01 * ymax + m02;
				_vertices[shapeIndex * 4 + 2].y = m10 * xmax + m11 * ymax + m12;
				
				_vertices[shapeIndex * 4 + 3].x = m00 * xmax + m01 * ymin + m02;
				_vertices[shapeIndex * 4 + 3].y = m10 * xmax + m11 * ymin + m12;
				
				xmin = shape.Uv.X;
				ymin = shape.Uv.Y;
				xmax = xmin + shape.Uv.Width;
				ymax = ymin + shape.Uv.Height;
				
				_uv[shapeIndex * 4 + 0].x = xmin;
				_uv[shapeIndex * 4 + 0].y = ymax;
				
				_uv[shapeIndex * 4 + 1].x = xmin;
				_uv[shapeIndex * 4 + 1].y = ymin;
				
				_uv[shapeIndex * 4 + 2].x = xmax;
				_uv[shapeIndex * 4 + 2].y = ymin;
				
				_uv[shapeIndex * 4 + 3].x = xmax;
				_uv[shapeIndex * 4 + 3].y = ymax;
			}
		}
		
		private void AddDrawcall(ShaderType shaderType, int stencilRef)
		{
			var index = (int)shaderType + (int)ShaderType.Count * stencilRef;
			if(_sharedMaterials[index] == null)
			{
				var material = new Material (Shader.Find("Custom/" + shaderType));
				material.SetFloat("_Ref", stencilRef);
				material.mainTexture = _atlas;
				
				_sharedMaterials[index] = material;
			}
	
			if(_drawcalls.Count == 0 || _drawcalls[_drawcalls.Count - 1].Material != _sharedMaterials[index])
			{
				_drawcalls.Add(new Drawcall(shaderType, _sharedMaterials[index]));
			}
		}
		
		private void OnRenderObject() 
		{
            if((Camera.current.cullingMask & (1 << gameObject.layer)) > 0)
            {
    			var mat = transform.localToWorldMatrix;
    			var mesh = Mesh;
    			for(var drawcallIndex = 0; drawcallIndex < _drawcalls.Count; drawcallIndex++)
    			{
    				if(_drawcalls[drawcallIndex].ShaderType == ShaderType.StencilDecrement)
    				{
    					Graphics.Blit(null, _drawcalls[drawcallIndex].Material);
    				}
    				else
    				{
    					_drawcalls[drawcallIndex].Material.SetPass(0);
    					Graphics.DrawMeshNow(mesh, mat, drawcallIndex);
    				}
    			}
            }
		}
		
		private void OnDestroy()
		{
		}
	}
}
