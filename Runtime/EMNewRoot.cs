using UnityEngine;
using System.Collections.Generic;

namespace LLT
{
	//[ExecuteInEditMode]
	public sealed class EMNewRoot : MonoBehaviour, IEMRoot
	{
		[SerializeField]
		private MeshFilter _meshFilter;

		[SerializeField]
		private Texture2D[] _textures;

		[SerializeField]
		private EMDisplayTreeTextAsset _bufferObject;

		[SerializeField]
		private List<EMObject> _objects;

		private EMDisplayTreeStream _displayTree;
		private readonly EMMesh _workerMesh = new EMMesh();

		private void Awake()
		{
			_displayTree = new EMDisplayTreeStream(this, _bufferObject, _objects);

			for(var i = 0; i < _textures.Length; i++)
			{
				EMAssetManager.Instance.RegisterTexture(_displayTree.TextAsset, i, _textures[i]);
			}

			_objects = null;

			InitDisplayTree();
		}

		private void InitDisplayTree()
		{
			int spriteCount, shapeCount;
			EMHelpers.InitDisplayTree(_displayTree.TextAsset.AddrOfPinnedObject(), out spriteCount, out shapeCount);
			_workerMesh.Init(shapeCount);
		}

		private void OnDestroy()
		{
			for(var i = 0; i < _textures.Length; i++)
			{
				EMAssetManager.Instance.UnregisterTexture(_displayTree.TextAsset, i, _textures[i]);
			}

			_displayTree = null;
			_bufferObject = null;
			_textures = null;
		}

		public EMTransformStructLayout Transform
		{
			get
			{
				throw new System.NotImplementedException();
			}
		}
	}
}

/*
 * using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;

namespace LLT
{
	[ExecuteInEditMode]
	public sealed class EMRoot : EMMonoBehaviour 
	{
        private sealed class MaterialId
        {
            private ShaderType _shaderType;
            private int _stencilRef;
            private Texture _texture;

            public MaterialId(ShaderType shaderType, int stencilRef, Texture texture)
            {
                _shaderType = shaderType;
                _stencilRef = stencilRef;
                _texture = texture;
            }

            public override bool Equals (object obj)
            {
                CoreAssert.Fatal(obj is MaterialId);
                var materialId = obj as MaterialId;
                return _shaderType == materialId._shaderType && _stencilRef == materialId._stencilRef && _texture == materialId._texture;
            }

            public override int GetHashCode ()
            {
                return (((int)_shaderType & 0xF) << 28) | (((int)_stencilRef & 0xF) << 24) | (_texture == null ? 0 : _texture.GetInstanceID() & 0x00FFFFFF);
            }
        }
		private sealed class Drawcall
		{
			private class Indexing
			{
				public int ShapeIndex;
				public int Count;
			}
			
			private readonly List<Indexing> _indexing = new List<Indexing>();
            private int[] _indices = new int[0];
			private int _count;
			
			public Material Material { private set; get; }
            public ShaderType ShaderType { private set; get; }

			public int[] Indices
			{
				get
				{
                    var oldCount = _indices.Length/4;
                    if(oldCount != _count)
                    {
                        Array.Resize(ref _indices, _count * 4);
                        for(var i = 0; i < _indexing.Count; i++)
    					{
                            for(var j = 0; j < _indexing[i].Count; j++)
    						{
                                var shapeIndex = _indexing[i].ShapeIndex * 4;
    							var baseIndex = (i + j) * 4;
                                _indices[baseIndex + 0] = shapeIndex + 0;
                                _indices[baseIndex + 1] = shapeIndex + 1;
                                _indices[baseIndex + 2] = shapeIndex + 2;
                                _indices[baseIndex + 3] = shapeIndex + 3;
    						}
    					}
                    }
                    return _indices;
				}
			}

			public int Count
			{
				get
				{
					return _count;
				}
			}

			public Drawcall(ShaderType shaderType, Material material)
			{
                ShaderType = shaderType;
				Material = material;
			}
			
			public void Add(int shapeIndex)
			{
                CoreAssert.Fatal(_indexing.Count == 0 || _indexing[_indexing.Count - 1].ShapeIndex <= shapeIndex);
                if(_indexing.Count == 0 || _indexing[_indexing.Count - 1].ShapeIndex != shapeIndex + 1)
				{
                    _indexing.Add(new Indexing(){ShapeIndex = shapeIndex, Count = 1});
				}
				else
				{
                    _indexing[_indexing.Count - 1].Count++;
				}
				_count++;
			}
		}
		
		private enum ShaderType
		{
			Transparent,
			StencilIncrement,
			StencilDecrement,
            StencilZero,
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
		
		private EMDisplayTreeStream _tree;

        [SerializeField]
        private Camera _camera;

		private Vector3[] _vertices;
		private Vector2[] _uv;
	
        private Color32[] _colorAdd;
        private Vector4[] _colorMult;

        private int[] _placed;

		private readonly List<Drawcall> _drawcalls = new List<Drawcall>();
		private ushort _shapeCount;
		
        private readonly Dictionary<MaterialId, Material> _sharedMaterials = new Dictionary<MaterialId, Material>();
        private Material[] _materials;

        private CoreTask _init;

		[SerializeField]
		private bool _useCustomTransformUpdater;

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;

        private EMInputManager _inputManager;

        private EMAnimationHead[] _animationHeads;

        private bool _linked;

        [SerializeField]
        private bool _clearStencilBuffer;

        public Camera Camera 
        {
            get
            {
                return _camera;
            }
        }
             
        public EMInputManager InputManager
        {
            get
            {
                if(_inputManager == null)
                {
                    _inputManager = _camera.GetComponent<EMInputManager>();
                }

                CoreAssert.Fatal(_camera.GetComponent<EMInputManager>() == _inputManager);
                return _inputManager;
            }
        }

        private EMDisplayTreeStream _parent;

        public EMDisplayTreeStream Parent 
        {
            get
            {
                return _parent;
            }
        }

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
                return _meshFilter.sharedMesh;
			}
		}
		
		private void Awake()
		{
			enabled = false;
            
            _init = CoreTask.Wrap(Init ());
			StartCoroutine(_init);
		}
		
        public EMObject FindObject(string path)
        {
            return _tree.FindObject(path.Split('/'));
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
			
            _tree = new EMDisplayTreeStream();
            _tree.UpdateFlag = EMUpdateFlag.Flag(EMUpdateFlag.Flags.InitMesh, EMUpdateFlag.Flags.UpdateDrawCalls);
			_tree.Init(this, _bytes.bytes);

			var iter = _tree.Iter as EMDisplayTreeStreamDFSEnumerator;
			iter.Reset();
			
            _meshFilter = GetComponent<MeshFilter>();
            if(_meshFilter == null)
            {
                _meshFilter = gameObject.AddComponent<MeshFilter>();
            }

            _meshFilter.sharedMesh = new Mesh();
            _meshFilter.sharedMesh.MarkDynamic();
            _meshFilter.sharedMesh.Clear();

            _meshRenderer = GetComponent<MeshRenderer>();
            if(_meshRenderer == null)
            {
                _meshRenderer = gameObject.AddComponent<MeshRenderer>();
            }

			iter.Sprite.LocalToWorld.MakeIdentity();
			iter.Sprite.LocalToWorld.M11 = -1f;

            _animationHeads = GetComponents<EMAnimationHead>();
			enabled = true;
		}
		
		public void Update()
		{
			var mesh = Mesh;

            if(!_linked && EMUpdateFlag.Should(_tree.UpdateFlag, EMUpdateFlag.Flags.InitMesh))
            {
			    InitMesh();

				mesh.vertices = _vertices;
				mesh.uv = _uv;
				mesh.tangents = _colorMult;
				mesh.colors32 = _colorAdd;

                UpdateDrawcalls();
            }

            for(var i = 0; i < _animationHeads.Length; i++)
            {
                if(_animationHeads[i].enabled)
                {
                    _animationHeads[i].ExplicitUpdate();
                }
            }

            if(!_linked)
            {
    			if(_useCustomTransformUpdater)
    			{
    				EMCustomTransformUpdater.UpdateTransforms(_tree.Ptr, _tree.RootTag.Position, _vertices, _colorAdd, _colorMult, _placed);
    			}
    			else
    			{
    				UpdateTransforms();
    			}

    			mesh.vertices = _vertices;
    		    mesh.tangents = _colorMult;
                mesh.colors32 = _colorAdd;
                mesh.RecalculateBounds();

                for(var i = 0; i < _drawcalls.Count; i++)
                {
                    if(_placed[i] > 0 || _drawcalls[i].ShaderType == ShaderType.StencilDecrement || _drawcalls[i].ShaderType == ShaderType.StencilZero)
                    {
                        _materials[i] = _drawcalls[i].Material;
                    }
                    else
                    {
                        _materials[i] = null;
                    }
                }
                _meshRenderer.sharedMaterials = _materials;
            }

            _tree.UpdateFlag = 0;
		}
		
		private void InitMesh()
		{
            ushort shapeCount = 0;
            ushort spriteCount = 0;
			var iter = _tree.Iter as EMDisplayTreeStreamDFSEnumerator;
			iter.Reset();
			
			while(iter.MoveNext(false))
			{
				if(iter.IsShape()) 
				{
					iter.Shape.ShapeIndex = shapeCount++;
				}	
                else if(iter.IsSprite())
                {
                    iter.Sprite.SpriteIndex = spriteCount++;
                }
			}
            shapeCount++;

            CoreAssert.Fatal(shapeCount < ushort.MaxValue && spriteCount < ushort.MaxValue);

            if(shapeCount != _shapeCount)
            {
                _shapeCount = shapeCount;
			    _vertices = new Vector3[_shapeCount * 4];
			    _uv = new Vector2[_shapeCount * 4];
                _colorAdd = new Color32[_shapeCount * 4];
                _colorMult = new Vector4[_shapeCount * 4];
            }

            var shapeIndex = 0;
			while(iter.MoveNext(false))
			{
				if(iter.IsShape()) 
				{
					var shape = iter.Shape;
					shapeIndex = iter.Shape.ShapeIndex;
					var xmin = shape.Uv.X;
					var ymin = shape.Uv.Y;
					var xmax = xmin + shape.Uv.Width;
					var ymax = ymin + shape.Uv.Height;
					
					_uv[shapeIndex * 4 + 0].x = xmin;
					_uv[shapeIndex * 4 + 0].y = ymin;
					
					_uv[shapeIndex * 4 + 1].x = xmin;
					_uv[shapeIndex * 4 + 1].y = ymax;
					
					_uv[shapeIndex * 4 + 2].x = xmax;
					_uv[shapeIndex * 4 + 2].y = ymax;
					
					_uv[shapeIndex * 4 + 3].x = xmax;
					_uv[shapeIndex * 4 + 3].y = ymin;
				}	
			}

            shapeIndex = _shapeCount - 1;
            _vertices[shapeIndex * 4 + 0].x = -4096;
            _vertices[shapeIndex * 4 + 0].y = 4096;
            
            _vertices[shapeIndex * 4 + 1].x = -4096;
            _vertices[shapeIndex * 4 + 1].y = -4096;
            
            _vertices[shapeIndex * 4 + 2].x = 4096;
            _vertices[shapeIndex * 4 + 2].y = -4096;
            
            _vertices[shapeIndex * 4 + 3].x = 4096;
            _vertices[shapeIndex * 4 + 3].y = 4096;
		}
		
		private void UpdateDrawcalls()
		{
			_drawcalls.Clear();

			var mask = new List<MaskOperation>();
			var masked = new List<MaskOperation>();
			
			var iter = _tree.Iter as EMDisplayTreeStreamDFSEnumerator;
			iter.Reset();

			var absolutePosition = 0;

            if(_clearStencilBuffer)
            {
                AddDrawcall(ShaderType.StencilZero, 0, null);    
                _drawcalls[_drawcalls.Count - 1].Add(_shapeCount - 1);
            }

			while(iter.MoveNext(false))
			{
				while(mask.Count > 0 && mask[mask.Count - 1].End <= absolutePosition)
				{
					mask.RemoveAt(mask.Count - 1);
				}
				while(masked.Count > 0 && masked[masked.Count - 1].End <= absolutePosition)
				{
					AddDrawcall(ShaderType.StencilDecrement, masked[masked.Count - 1].StencilRef, null);	
                    _drawcalls[_drawcalls.Count - 1].Add(_shapeCount - 1);
					masked.RemoveAt(masked.Count - 1);
				}

				var clipDepth = iter.ClipDepth;
				if(clipDepth > 0)
				{
					CoreAssert.Fatal(mask.Count == 0 || absolutePosition < mask[mask.Count - 1].End, "Expecting only one mask");
					if(mask.Count == 0 || mask[mask.Count - 1].End <= absolutePosition)
					{
						var depth = iter.Depth;
      
						var tempAbsolutePosition = absolutePosition;

                        var siblingEnumerator = new EMDisplayTreeStreamDFSEnumerator(this, iter.Tree);
						siblingEnumerator.Reset(iter.ParentTag, iter.Current);
                        
						while(siblingEnumerator.MoveNext(false))
						{
							tempAbsolutePosition += siblingEnumerator.Current.EntrySizeOf + TSTreeStreamTag.TSTreeStreamTagSizeOf;

							if(siblingEnumerator.Index == 1 && siblingEnumerator.Depth != depth)
							{
								break;
							}
						}

						var startMasked = tempAbsolutePosition;
						mask.Add(new MaskOperation(){Start = absolutePosition, End = tempAbsolutePosition, StencilRef = mask.Count});
					
						while(siblingEnumerator.MoveNext(false))
						{
							tempAbsolutePosition += siblingEnumerator.Current.EntrySizeOf + TSTreeStreamTag.TSTreeStreamTagSizeOf;

							if(siblingEnumerator.Index == 1 && (clipDepth < siblingEnumerator.Depth || siblingEnumerator.ClipDepth != 0))
							{
								break;
							}
						}
						masked.Add(new MaskOperation(){Start = startMasked, End = tempAbsolutePosition, StencilRef = mask.Count});
					}
				}
				
				if(iter.IsShape())
				{
					MaskOperation maskOperation = null;
					if(mask.Count > 0)
					{
						maskOperation = mask[mask.Count - 1];
						if(mask[mask.Count - 1].Start <= absolutePosition && absolutePosition < mask[mask.Count - 1].End)
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
							CoreAssert.Fatal(masked[masked.Count - 1].Start <= absolutePosition && absolutePosition < masked[masked.Count - 1].End);
							AddDrawcall(ShaderType.Transparent, maskOperation.StencilRef, iter.Texture);
							_drawcalls[_drawcalls.Count - 1].Add(iter.Shape.ShapeIndex);
						}
						else
						{
							AddDrawcall(ShaderType.Transparent, 0, iter.Texture);
							_drawcalls[_drawcalls.Count - 1].Add(iter.Shape.ShapeIndex);
						}
					}

                    iter.Shape.DrawcallIndex = (ushort)(_drawcalls.Count - 1);
                    iter.Shape.LocalToWorld.Placed = 0;
				}	

				absolutePosition += iter.Current.EntrySizeOf + TSTreeStreamTag.TSTreeStreamTagSizeOf;
			}
			
            while(masked.Count > 0)
            {
                AddDrawcall(ShaderType.StencilDecrement, masked[masked.Count - 1].StencilRef, null);    
                _drawcalls[_drawcalls.Count - 1].Add(_shapeCount - 1);
                masked.RemoveAt(masked.Count - 1);
            }

			var mesh = Mesh;
			mesh.subMeshCount = _drawcalls.Count;	
			
            _materials = new Material[_drawcalls.Count];
            for(var drawcallIndex = 0; drawcallIndex < _drawcalls.Count; drawcallIndex++)
            {
                mesh.SetIndices(_drawcalls[drawcallIndex].Indices, MeshTopology.Quads, drawcallIndex);
            }

            _placed = new int[_drawcalls.Count];
		}
		
		private void UpdateTransforms()
		{
			var iter = _tree.Iter as EMDisplayTreeStreamDFSEnumerator;
			iter.Reset();

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
                            CoreAssert.Fatal(p1->DrawcallIndex < _placed.Length);
							if(p1->LocalToWorld.Placed > 0)
							{
                                _placed[p1->DrawcallIndex]++;
							}
							else
							{
                                _placed[p1->DrawcallIndex]--;
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
           
			_vertices[shapeIndex * 4 + 0].x = m00 * xmin + m01 * ymax + m02;
			_vertices[shapeIndex * 4 + 0].y = m10 * xmin + m11 * ymax + m12;
			
			_vertices[shapeIndex * 4 + 1].x = m00 * xmin + m01 * ymin + m02;
			_vertices[shapeIndex * 4 + 1].y = m10 * xmin + m11 * ymin + m12;
			
			_vertices[shapeIndex * 4 + 2].x = m00 * xmax + m01 * ymin + m02;
			_vertices[shapeIndex * 4 + 2].y = m10 * xmax + m11 * ymin + m12;
			
			_vertices[shapeIndex * 4 + 3].x = m00 * xmax + m01 * ymax + m02;
			_vertices[shapeIndex * 4 + 3].y = m10 * xmax + m11 * ymax + m12;
            
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
            var materialId = new MaterialId(shaderType, stencilRef, texture);
            Material material = null;

            if(!_sharedMaterials.TryGetValue(materialId, out material))
            {
                material = new Material (Shader.Find("LLT/" + shaderType));
                material.SetFloat("_Ref", stencilRef);
                material.mainTexture = texture;
                _sharedMaterials.Add(materialId, material);
            }

            if(_drawcalls.Count == 0 || _drawcalls[_drawcalls.Count - 1].Material != material)
			{
                _drawcalls.Add(new Drawcall(shaderType, material));
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
                Mesh.Destroy(_meshFilter.mesh);
                _meshFilter.mesh = null;
                _vertices = null;
                _shapeCount = 0;
            }
            
            _linked = true;
            yield return new EMDisplayTreeStreamDFSEnumerator(this);
        }
        
		private void OnDestroy()
		{
            if(_tree != null)
            {
                _tree.Dispose();
            }
		}
	}
}

*/