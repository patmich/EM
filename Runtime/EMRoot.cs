using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LLT
{
	//[ExecuteInEditMode]
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
			public Texture Texture { private set; get; }
			public int Placed { get; set; }

			public int[] Indices
			{
				get
				{
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

			public int Count
			{
				get
				{
					return _count;
				}
			}

			public Drawcall(ShaderType shaderType, Texture texture, Material material)
			{
				ShaderType = shaderType;
				Material = material;
                Texture = texture;
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
		private Texture[] _textures;
		
		[SerializeField]
		private EMDisplayTreeStream _tree;

		private Mesh _mesh;
		
		private Vector3[] _vertices;
		private Vector2[] _uv;
	
        private Color32[] _colorAdd;
        private Vector4[] _colorMult;
        
		private readonly List<Drawcall> _drawcalls = new List<Drawcall>();
		private ushort _shapeCount;
		
		// ToDo need as many shared material multiple as mask depth.
		private readonly Material[] _sharedMaterials = new Material[(int)ShaderType.Count * 2];
		private EMDisplayTreeStreamDFSEnumerator _siblingEnumerator;
        private CoreTask _init;
        
#pragma warning disable 0414        
        private EMDisplayTreeStream _parent;
#pragma warning restore 0414
        
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
			_tree.UpdateFlag = EMUpdateFlag.Flag(EMUpdateFlag.Flags.InitMesh, EMUpdateFlag.Flags.UpdateDrawCalls);
            
			enabled = false;
            
            _init = CoreTask.Wrap(Init ());
			StartCoroutine(_init);
		}
		
		public EMObject FindObject(params string[] path)
		{
			return _tree.FindObject(path);
		}

		public Texture GetTexture(int index)
		{
			CoreAssert.Fatal(0 <= index && index < _textures.Length);
			return _textures[index];
		}

		private IEnumerator Init()
		{
			while(_bytes == null)
			{
				yield return null;
			}
			
			_tree.Init(this, _bytes.bytes);
			_siblingEnumerator = new EMDisplayTreeStreamDFSEnumerator(this);
			
			enabled = true;
		}
		
		private void Update()
		{
			var mesh = Mesh;

            if(EMUpdateFlag.Should(_tree.UpdateFlag, EMUpdateFlag.Flags.InitMesh))
            {
			    InitMesh();

				mesh.vertices = _vertices;
				mesh.uv = _uv;
				mesh.tangents = _colorMult;
				mesh.colors32 = _colorAdd;

				UpdateDrawcalls();
            }
			
			UpdateTransforms();

			mesh.vertices = _vertices;
			mesh.uv = _uv;
		    mesh.tangents = _colorMult;
            mesh.colors32 = _colorAdd;
            
            _tree.UpdateFlag = 0;
		}
		
		private void InitMesh()
		{
            ushort shapeCount = 0;
			
			var iter = _tree.Iter as EMDisplayTreeStreamDFSEnumerator;
			iter.Reset();
			
			while(iter.MoveNext(false))
			{
				if(iter.IsShape()) 
				{
					iter.Shape.ShapeIndex = shapeCount++;
				}	
			}
			
            if(shapeCount != _shapeCount)
            {
                _shapeCount = shapeCount;
			    _vertices = new Vector3[_shapeCount * 4];
			    _uv = new Vector2[_shapeCount * 4];
                _colorAdd = new Color32[_shapeCount * 4];
                _colorMult = new Vector4[_shapeCount * 4];
            }
		}
		
		private void UpdateDrawcalls()
		{
			_drawcalls.Clear();
			_shapeCount = 0;

			var mask = new List<MaskOperation>();
			var masked = new List<MaskOperation>();
			
			var iter = _tree.Iter as EMDisplayTreeStreamDFSEnumerator;
			iter.Reset();
			
			while(iter.MoveNext(false))
			{
				while(mask.Count > 0 && mask[mask.Count - 1].End <= iter.Current.Position)
				{
					mask.RemoveAt(mask.Count - 1);
				}
				while(masked.Count > 0 && masked[masked.Count - 1].End <= iter.Current.Position)
				{
					AddDrawcall(ShaderType.StencilDecrement, masked[masked.Count - 1].StencilRef, null);	
					masked.RemoveAt(masked.Count - 1);
				}

				var clipDepth = iter.ClipDepth;
				if(clipDepth > 0)
				{
					CoreAssert.Fatal(mask.Count == 0 || iter.Current.Position < mask[mask.Count - 1].End, "Expecting only one mask");
					if(mask.Count == 0 || mask[mask.Count - 1].End <= iter.Current.Position)
					{
						var depth = iter.Depth;

						_siblingEnumerator.Reset(iter.ParentTag, iter.Current);
						while(_siblingEnumerator.MoveNext(true) && _siblingEnumerator.Depth == depth);

						var startMasked = _siblingEnumerator.Current.Position;
						var endMasked = startMasked;

						mask.Add(new MaskOperation(){Start = (int)iter.Current.Position, End = startMasked, StencilRef = mask.Count});
					
						if(_siblingEnumerator.Depth <= clipDepth && _siblingEnumerator.ClipDepth == 0)
						{
							endMasked = _siblingEnumerator.Current.SiblingPosition;
						}
						while(_siblingEnumerator.MoveNext(true) && _siblingEnumerator.Depth <= clipDepth && _siblingEnumerator.ClipDepth == 0)
						{
							endMasked = _siblingEnumerator.Current.SiblingPosition;
						}
						masked.Add(new MaskOperation(){Start = startMasked, End = endMasked, StencilRef = mask.Count});
					}
				}
				
				if(iter.IsShape())
				{
					MaskOperation maskOperation = null;
					if(mask.Count > 0)
					{
						maskOperation = mask[mask.Count - 1];
						if(mask[mask.Count - 1].Start <= iter.Current.Position && iter.Current.Position < mask[mask.Count - 1].End)
						{
							AddDrawcall(ShaderType.StencilIncrement, maskOperation.StencilRef, iter.Texture);
							_drawcalls[_drawcalls.Count - 1].Add(iter.Shape.ShapeIndex);
						}
						else
						{
							maskOperation = null;
						}
					}
					
					if(maskOperation == null)
					{
						if(masked.Count > 0)
						{
							maskOperation = masked[masked.Count - 1];
							CoreAssert.Fatal(masked[masked.Count - 1].Start <= iter.Current.Position && iter.Current.Position < masked[masked.Count - 1].End);
							AddDrawcall(ShaderType.Transparent, maskOperation.StencilRef, iter.Texture);
							_drawcalls[_drawcalls.Count - 1].Add(iter.Shape.ShapeIndex);
						}
						else
						{
							AddDrawcall(ShaderType.Transparent, 0, iter.Texture);
							_drawcalls[_drawcalls.Count - 1].Add(iter.Shape.ShapeIndex);
						}
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
		
		private void UpdateTransforms()
		{
			var iter = _tree.Iter as EMDisplayTreeStreamDFSEnumerator;
			iter.Reset();

			iter.Sprite.LocalToWorld.MakeIdentity();
			iter.Sprite.LocalToWorld.M11 = -1f;

			var drawCallIndex = 0;

			while(_drawcalls[drawCallIndex].ShaderType == ShaderType.StencilDecrement)
			{
				drawCallIndex++;
			}

			var startIndex = 0;

			var skipSubTree = false;			
			while(iter.MoveNext(false))
			{
				skipSubTree = false;
				if(iter.IsSprite())
				{	
#if UNITY_WEB || !ALLOW_UNSAFE
					iter.Sprite.LocalToWorld.Concat(iter.Parent.LocalToWorld, iter.Sprite.Transform);
#else				
					unsafe
					{
						EMSpriteStructLayout* p0 = (EMSpriteStructLayout*)iter.ParentPtr.ToPointer();
						EMSpriteStructLayout* p1 = (EMSpriteStructLayout*)iter.CurrentPtr.ToPointer();
                        
						p1->LocalToWorld.M00 = p0->LocalToWorld.M00 * p1->Transform.M00 + p0->LocalToWorld.M01 * p1->Transform.M10;
						p1->LocalToWorld.M01 = p0->LocalToWorld.M00 * p1->Transform.M01 + p0->LocalToWorld.M01 * p1->Transform.M11;
						p1->LocalToWorld.M10 = p0->LocalToWorld.M10 * p1->Transform.M00 + p0->LocalToWorld.M11 * p1->Transform.M10;
						p1->LocalToWorld.M11 = p0->LocalToWorld.M10 * p1->Transform.M01 + p0->LocalToWorld.M11 * p1->Transform.M11;
						p1->LocalToWorld.M02 = p0->LocalToWorld.M00 * p1->Transform.M02 + p0->LocalToWorld.M01 * p1->Transform.M12 + p0->LocalToWorld.M02;
						p1->LocalToWorld.M12 = p0->LocalToWorld.M10 * p1->Transform.M02 + p0->LocalToWorld.M11 * p1->Transform.M12 + p0->LocalToWorld.M12;
						
                        p1->LocalToWorld.MA = (byte)(p0->LocalToWorld.MA * p1->Transform.MA/255f);
                        p1->LocalToWorld.MR = (byte)(p0->LocalToWorld.MR * p1->Transform.MR/255f);
                        p1->LocalToWorld.MG = (byte)(p0->LocalToWorld.MG * p1->Transform.MG/255f);
                        p1->LocalToWorld.MB = (byte)(p0->LocalToWorld.MB * p1->Transform.MB/255f);
                        
                        p1->LocalToWorld.OA = (byte)Mathf.Clamp(p0->LocalToWorld.OA + p1->Transform.OA, 0, byte.MaxValue);
                        p1->LocalToWorld.OR = (byte)Mathf.Clamp(p0->LocalToWorld.OR + p1->Transform.OR, 0, byte.MaxValue);
                        p1->LocalToWorld.OG = (byte)Mathf.Clamp(p0->LocalToWorld.OG + p1->Transform.OG, 0, byte.MaxValue);
                        p1->LocalToWorld.OB = (byte)Mathf.Clamp(p0->LocalToWorld.OB + p1->Transform.OB, 0, byte.MaxValue);
                        
						var temp = p1->LocalToWorld.Placed;
						p1->LocalToWorld.Placed = (byte)(p0->LocalToWorld.Placed & p1->Transform.Placed);
                        
						if(p1->LocalToWorld.Placed == 0 && temp == 0)
						{
							skipSubTree = true;
						}
						
						_tree.UpdateFlag |= p1->UpdateFlag;
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
						EMShapeStructLayout* p1 = (EMShapeStructLayout*)iter.CurrentPtr.ToPointer();
                        var temp = p1->LocalToWorld.Placed;
						
                        if(p1->Transform.Placed > 0)
                        {
							EMSpriteStructLayout* p0 = (EMSpriteStructLayout*)iter.ParentPtr.ToPointer();
    						p1->LocalToWorld.M00 = p0->LocalToWorld.M00 * p1->Transform.M00 + p0->LocalToWorld.M01 * p1->Transform.M10;
    						p1->LocalToWorld.M01 = p0->LocalToWorld.M00 * p1->Transform.M01 + p0->LocalToWorld.M01 * p1->Transform.M11;
    						p1->LocalToWorld.M10 = p0->LocalToWorld.M10 * p1->Transform.M00 + p0->LocalToWorld.M11 * p1->Transform.M10;
    						p1->LocalToWorld.M11 = p0->LocalToWorld.M10 * p1->Transform.M01 + p0->LocalToWorld.M11 * p1->Transform.M11;
    						p1->LocalToWorld.M02 = p0->LocalToWorld.M00 * p1->Transform.M02 + p0->LocalToWorld.M01 * p1->Transform.M12 + p0->LocalToWorld.M02;
    						p1->LocalToWorld.M12 = p0->LocalToWorld.M10 * p1->Transform.M02 + p0->LocalToWorld.M11 * p1->Transform.M12 + p0->LocalToWorld.M12;
							
                            p1->LocalToWorld.MA = (byte)(p0->LocalToWorld.MA * p1->Transform.MA/255f);
                            p1->LocalToWorld.MR = (byte)(p0->LocalToWorld.MR * p1->Transform.MR/255f);
                            p1->LocalToWorld.MG = (byte)(p0->LocalToWorld.MG * p1->Transform.MG/255f);
                            p1->LocalToWorld.MB = (byte)(p0->LocalToWorld.MB * p1->Transform.MB/255f);
                            
                            p1->LocalToWorld.OA = (byte)Mathf.Clamp(p0->LocalToWorld.OA + p1->Transform.OA, 0, byte.MaxValue);
                            p1->LocalToWorld.OR = (byte)Mathf.Clamp(p0->LocalToWorld.OR + p1->Transform.OR, 0, byte.MaxValue);
                            p1->LocalToWorld.OG = (byte)Mathf.Clamp(p0->LocalToWorld.OG + p1->Transform.OG, 0, byte.MaxValue);
                            p1->LocalToWorld.OB = (byte)Mathf.Clamp(p0->LocalToWorld.OB + p1->Transform.OB, 0, byte.MaxValue);
                            
							p1->LocalToWorld.Placed = (byte)(p0->LocalToWorld.Placed & p1->Transform.Placed);
						}
                        else
						{
							p1->LocalToWorld.Placed = 0;
						}

						if(p1->LocalToWorld.Placed != temp)
						{
							if(p1->LocalToWorld.Placed > 0)
							{
								_drawcalls[drawCallIndex].Placed++;
							}
							else
							{
								_drawcalls[drawCallIndex].Placed--;
							}
						}

						if(p1->LocalToWorld.Placed == 0 && temp == 0)
						{
							skipSubTree = true;
						}
						
						_tree.UpdateFlag |= p1->UpdateFlag;
						p1->UpdateFlag = 0;
					}
#endif
					if(!skipSubTree)
					{
						UpdateGeometry(iter);
					}

					if(iter.Shape.ShapeIndex == startIndex + _drawcalls[drawCallIndex].Count - 1)
					{
						startIndex = iter.Shape.ShapeIndex + 1;
						drawCallIndex++;

						while(drawCallIndex < _drawcalls.Count && _drawcalls[drawCallIndex].ShaderType == ShaderType.StencilDecrement)
						{
							drawCallIndex++;
						}
					}
				}
			}
		}
		
		private void UpdateGeometry(EMDisplayTreeStreamDFSEnumerator iter)
		{
            CoreAssert.Fatal(iter.IsShape());
            var shape = iter.Shape;
            int shapeIndex = shape.ShapeIndex;
			float m00, m01, m10, m11, m02, m12, xmin, ymin, xmax, ymax;
            
            Color32 colorAdd;
            Vector4 colorMult;
            
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
				EMShapeStructLayout* ptr = (EMShapeStructLayout*)iter.CurrentPtr.ToPointer();
				
				if(ptr->LocalToWorld.Placed == 0)
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
                
                colorAdd = new Color32(ptr->LocalToWorld.OR, ptr->LocalToWorld.OG, ptr->LocalToWorld.OB, ptr->LocalToWorld.OA);
                colorMult = new Vector4(ptr->LocalToWorld.MR/255f, ptr->LocalToWorld.MG/255f, ptr->LocalToWorld.MB/255f, ptr->LocalToWorld.MA/255f);
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
            
            _colorAdd[shapeIndex * 4 + 0] = colorAdd;
            _colorAdd[shapeIndex * 4 + 1] = colorAdd;
            _colorAdd[shapeIndex * 4 + 2] = colorAdd;
            _colorAdd[shapeIndex * 4 + 3] = colorAdd;
            
			
            _colorMult[shapeIndex * 4 + 0] = colorMult;
            _colorMult[shapeIndex * 4 + 1] = colorMult;
            _colorMult[shapeIndex * 4 + 2] = colorMult;
            _colorMult[shapeIndex * 4 + 3] = colorMult;
		}
		
		private void AddDrawcall(ShaderType shaderType, int stencilRef, Texture texture)
		{

			var index = (int)shaderType + (int)ShaderType.Count * stencilRef;
			CoreAssert.Fatal(index < _sharedMaterials.Length);
			if(_sharedMaterials[index] == null)
			{
				var material = new Material (Shader.Find("LLT/" + shaderType));
				material.SetFloat("_Ref", stencilRef);
				
				_sharedMaterials[index] = material;
			}
	
			if(_drawcalls.Count == 0 || _drawcalls[_drawcalls.Count - 1].Material != _sharedMaterials[index] || _drawcalls[_drawcalls.Count - 1].Texture != texture)
			{
				_drawcalls.Add(new Drawcall(shaderType, texture, _sharedMaterials[index]));
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
					else if(_drawcalls[drawcallIndex].Placed > 0)
    				{
                        _drawcalls[drawcallIndex].Material.mainTexture = _drawcalls[drawcallIndex].Texture;
    					_drawcalls[drawcallIndex].Material.SetPass(0);
                       
    					Graphics.DrawMeshNow(mesh, mat, drawcallIndex);
    				}
    			}
            }
		}
		
        public IEnumerator<EMDisplayTreeStreamDFSEnumerator> Link(EMDisplayTreeStream parent)
        {
            CoreAssert.Fatal(_parent == null);
            _parent = parent;
            
            if(!_init.Done)
            {
                _init.Stop();
                _init = CoreTask.Wrap(Init ());
                
                while(_init.MoveNext())yield return null;
            }
            else
            {
                Mesh.Destroy(_mesh);
                _mesh = null;
                _vertices = null;
                _shapeCount = 0;
            }
            
            enabled = false;
            yield return new EMDisplayTreeStreamDFSEnumerator(this);
        }
        
		private void OnDestroy()
		{
            _tree.Dispose();
		}
	}
}
