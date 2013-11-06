using System.Collections.Generic;
using System.Linq;
using System;

namespace LLT
{
	public sealed class EMSwfDefineSprite : EMSwfObject
	{
	    private class ChildKey : IComparer<ChildKey>
	    {
	        private int _depth;
	        private int _firstFrame;
	        private string _name;
			private int _refId;
			
	        public string Name
	        {
	            get
	            {
					return _name;
	            }
	        }
			
			public int Depth
			{
				get
				{
					return _depth;
				}
			}
			
			public int FirstFrame
			{
				get
				{
					return _firstFrame;
				}
			}
			
			public int RefId
			{
				get
				{
					return _refId;
				}
			}
			
	        public ChildKey(int depth, int firstFrame, bool hasName, string name, int refId)
	        {
	            _depth = depth;
	            _firstFrame = firstFrame;
	            _name = name;
				_refId = refId;
	        }
	
	        public override int GetHashCode()
	        {
	            return _name.GetHashCode();
	        }
	
	        public override bool Equals(object obj)
	        {
	            var o = obj as ChildKey;
	            CoreAssert.Fatal(o != null);
	
	            return _depth == o._depth && _firstFrame == o._firstFrame;
	        }
	
	        public int Compare(ChildKey x, ChildKey y)
	        {
	            var retVal = x._depth.CompareTo(y._depth);
	            if (retVal == 0)
	            {
	                retVal = x._firstFrame.CompareTo(y._firstFrame);
	            }
	
	            return retVal;
	        }
	    }
		
		public enum PropertyId
		{
			Transform_M00_Offset,
			Transform_M01_Offset,
			Transform_M02_Offset,
			Transform_M10_Offset,
			Transform_M11_Offset,
			Transform_M12_Offset,
			Transform_MA_Offset,
			Transform_MR_Offset,
			Transform_MG_Offset,
			Transform_MB_Offset,
			Transform_OA_Offset,
			Transform_OR_Offset,
			Transform_OG_Offset,
			Transform_OB_Offset,
			Transform_Placed_Offset,
			ClipCount_Offset,
			UpdateFlag_Offset,
		}
		
	    private EMSwfImporter _importer;
	    private ushort _frameCount;
	    private readonly List<EMSwfObject> _controlTags = new List<EMSwfObject>();
		private readonly Dictionary<ChildKey, ITSTreeNode> _childs = new Dictionary<ChildKey, ITSTreeNode>();
		private List<KeyValuePair<EMSwfCurveKey, EMSwfAnimationCurve>> _animationCurves;
		
		public List<EMSwfObject> ControlTags
		{
			get
			{
				return _controlTags;
			}
		}
		
	    public override void Read(EMSwfImporter importer, EMSwfTag tag, EMSwfBinaryReader reader)
	    {
	        _importer = importer;
	        _tag = tag;
	        _id = reader.ReadUInt16();
	        _frameCount = reader.ReadUInt16();
	    }
	
	    public override void Write(EMSwfBinaryWriter writer)
	    {
	        _tag.Write(writer);
	        writer.Write(_id);
	        writer.Write(_frameCount);
	    }
	
	    public void AddControlTag(EMSwfObject obj)
	    {
	        _controlTags.Add(obj);
	    }
	
	    #region ICoreTreeNode implementation
		
		public void Expand()
		{
			_childs.Clear();
	        var currentFrame = 0;
	
	        for (var i = 0; i < _controlTags.Count; i++)
	        {
	            if (_controlTags[i] is EMSwfShowFrame)
	            {
	                currentFrame++;
	            }
	            else if (_controlTags[i] is EMSwfPlaceObject2)
	            {
	                var placeObject2 = _controlTags[i] as EMSwfPlaceObject2;
	                if (placeObject2.IsNewCharacter() || placeObject2.IsCharacterAtDepthReplaced())
	                {                  
	                    var key = new ChildKey(placeObject2.Depth, currentFrame, placeObject2.HasName(), placeObject2.Name, placeObject2.RefId);
	                    CoreAssert.Fatal(!_childs.ContainsKey(key));
						
	                    var defineSprite = _importer.GetObject<EMSwfDefineSprite>((ushort)placeObject2.RefId);
	                    if (defineSprite != null)
	                    {
							if(!_childs.ContainsKey(key))
							{
	                        	_childs.Add(key, new EMSwfDefineSpriteNode(key.Name, currentFrame == 0, placeObject2.Matrix, placeObject2.CXform, currentFrame == 0 ? placeObject2.ClipDepth : (ushort)0, defineSprite));
							}
	                    }
						var defineShape = _importer.GetObject<EMSwfDefineShape>((ushort)placeObject2.RefId);
	                    if (defineShape != null)
	                    {
							if(!_childs.ContainsKey(key))
							{
	                        	_childs.Add(key, new EMSwfDefineShapeNode(key.Name, currentFrame == 0, placeObject2.Matrix, placeObject2.CXform, currentFrame == 0 ? placeObject2.ClipDepth : (ushort)0, defineShape));
							}
	                    }
	                }
	            }                             
	        }
			
			var childsKey = _childs.OrderBy(x=>x.Key, new ChildKey(0, 0, false, null, 0)).Select(x=>x.Key).ToList();
			for(var i = 0; i < childsKey.Count; i++)
			{
				var child = _childs[childsKey[i]];
				
				if(child is EMSwfDefineShapeNode)
				{
					var node = child as EMSwfDefineShapeNode;
					
					if(node.ClipDepth > 0)
					{
						var clipCount = childsKey.FindLastIndex(x=>x.Depth < node.ClipDepth) - i;	
						node.ClipCount = clipCount;
					}
				}
				else if(child is EMSwfDefineSpriteNode)
				{
					var node = child as EMSwfDefineSpriteNode;
					if(node.ClipDepth > 0)
					{
						var clipCount = childsKey.FindLastIndex(x=>x.Depth < node.ClipDepth) - i;	
						node.ClipCount = clipCount;
					}
				}
			}
		}
	
		public List<ITSTreeNode> Childs
		{
			get
			{
				return _childs.OrderBy(x=>x.Key, new ChildKey(0,0,false, null,0)).Select(x=>x.Value).ToList();
			}
		}
		
		public bool HasAnimation()
		{
			return AnimationCurves.Count(x=>x.Value.Count > 1) > 0;
		}
		
		public List<KeyValuePair<EMSwfCurveKey, EMSwfAnimationCurve>> AnimationCurves
		{
			get
			{
				if(_animationCurves != null)
				{
					return _animationCurves;
				}
				
				var retVal = new CoreDictionary<EMSwfCurveKey, EMSwfAnimationCurve>();
				var childs = _childs.OrderBy(x=>x.Key, new ChildKey(0, 0, false, null,0)).Select(x=>x.Key).ToList();
				
				for(var i = 0; i < childs.Count; i++)
				{
					retVal[new EMSwfCurveKey(i, Offset(childs[i].RefId, PropertyId.Transform_M00_Offset))].Add(0, 1f);
					retVal[new EMSwfCurveKey(i, Offset(childs[i].RefId, PropertyId.Transform_M11_Offset))].Add(0, 1f);
					retVal[new EMSwfCurveKey(i, Offset(childs[i].RefId, PropertyId.Transform_M01_Offset))].Add(0, 0f);
					retVal[new EMSwfCurveKey(i, Offset(childs[i].RefId, PropertyId.Transform_M10_Offset))].Add(0, 0f);
					retVal[new EMSwfCurveKey(i, Offset(childs[i].RefId, PropertyId.Transform_M02_Offset))].Add(0, 0f);
					retVal[new EMSwfCurveKey(i, Offset(childs[i].RefId, PropertyId.Transform_M12_Offset))].Add(0, 0f);
					
					retVal[new EMSwfCurveKey(i, Offset(childs[i].RefId, PropertyId.Transform_MA_Offset), TSPropertyType._byte)].Add(0, 1f);
					retVal[new EMSwfCurveKey(i, Offset(childs[i].RefId, PropertyId.Transform_MR_Offset), TSPropertyType._byte)].Add(0, 1f);
					retVal[new EMSwfCurveKey(i, Offset(childs[i].RefId, PropertyId.Transform_MG_Offset), TSPropertyType._byte)].Add(0, 1f);
					retVal[new EMSwfCurveKey(i, Offset(childs[i].RefId, PropertyId.Transform_MB_Offset), TSPropertyType._byte)].Add(0, 1f);
					
					retVal[new EMSwfCurveKey(i, Offset(childs[i].RefId, PropertyId.Transform_OA_Offset), TSPropertyType._byte)].Add(0, 0f);
					retVal[new EMSwfCurveKey(i, Offset(childs[i].RefId, PropertyId.Transform_OR_Offset), TSPropertyType._byte)].Add(0, 0f);
					retVal[new EMSwfCurveKey(i, Offset(childs[i].RefId, PropertyId.Transform_OG_Offset), TSPropertyType._byte)].Add(0, 0f);
					retVal[new EMSwfCurveKey(i, Offset(childs[i].RefId, PropertyId.Transform_OB_Offset), TSPropertyType._byte)].Add(0, 0f);
					
					retVal[new EMSwfCurveKey(i, Offset(childs[i].RefId, PropertyId.ClipCount_Offset), TSPropertyType._ushort)].Add(0, 0f);
					retVal[new EMSwfCurveKey(i, Offset(childs[i].RefId, PropertyId.Transform_Placed_Offset), TSPropertyType._byte)].Add(0, 1f);
				}
				
				var currentFrame = 0;
				for(var i = 0; i < _controlTags.Count; i++)
				{
					if (_controlTags[i] is EMSwfShowFrame)
	                {
	                    currentFrame++;
	                }
	                else if (_controlTags[i] is EMSwfPlaceObject2)
	                {
	                    var placeObject2 = _controlTags[i] as EMSwfPlaceObject2;

						var oldChildIndex = childs.FindLastIndex(x=>x.Depth == placeObject2.Depth && x.FirstFrame <= currentFrame - 1);
						var childIndex = childs.FindLastIndex(x=>x.Depth == placeObject2.Depth && x.FirstFrame <= currentFrame);
						if(childIndex != -1)
						{
							var refId = childs[childIndex].RefId;
		                    if (placeObject2.IsNewCharacter() || placeObject2.IsCharacterAtDepthReplaced())
		                    {
								if(currentFrame > 0 && childs[childIndex].FirstFrame > 0)
								{
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_Placed_Offset), TSPropertyType._byte)].Add(0, 0f);
								}
								
								if(oldChildIndex != -1)
								{
									retVal[new EMSwfCurveKey(oldChildIndex, Offset(refId, PropertyId.Transform_Placed_Offset), TSPropertyType._byte)].Add(currentFrame, 0f);
									
									if(retVal[new EMSwfCurveKey(oldChildIndex, Offset(refId, PropertyId.ClipCount_Offset), TSPropertyType._ushort)].Add(currentFrame, 0f))
									{
										retVal[new EMSwfCurveKey(oldChildIndex, Offset(refId, PropertyId.UpdateFlag_Offset), TSPropertyType._byte)].Add(currentFrame, EMUpdateFlag.Flag(EMUpdateFlag.Flags.UpdateDrawCalls), false);
									}
								}
							}
							
							var clipCount = childs.FindLastIndex(x=>x.Depth < placeObject2.ClipDepth) - childIndex;	
							
							if(placeObject2.IsCharacterAtDepthReplaced())
							{
								if(placeObject2.HasMatrix())
								{
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_M01_Offset))].Add(currentFrame, placeObject2.Matrix.M01);
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_M10_Offset))].Add(currentFrame, placeObject2.Matrix.M10);
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_M00_Offset))].Add(currentFrame, placeObject2.Matrix.M00);
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_M11_Offset))].Add(currentFrame, placeObject2.Matrix.M11);					
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_M02_Offset))].Add(currentFrame, placeObject2.Matrix.M02);
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_M12_Offset))].Add(currentFrame, placeObject2.Matrix.M12);
								}
								else
								{
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_M01_Offset))].Add(currentFrame, retVal[new EMSwfCurveKey(oldChildIndex, Offset(refId, PropertyId.Transform_M01_Offset))].Sample(currentFrame - 1));
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_M10_Offset))].Add(currentFrame, retVal[new EMSwfCurveKey(oldChildIndex, Offset(refId, PropertyId.Transform_M10_Offset))].Sample(currentFrame - 1));
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_M00_Offset))].Add(currentFrame, retVal[new EMSwfCurveKey(oldChildIndex, Offset(refId, PropertyId.Transform_M00_Offset))].Sample(currentFrame - 1));
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_M11_Offset))].Add(currentFrame, retVal[new EMSwfCurveKey(oldChildIndex, Offset(refId, PropertyId.Transform_M11_Offset))].Sample(currentFrame - 1));
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_M02_Offset))].Add(currentFrame, retVal[new EMSwfCurveKey(oldChildIndex, Offset(refId, PropertyId.Transform_M02_Offset))].Sample(currentFrame - 1));
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_M12_Offset))].Add(currentFrame, retVal[new EMSwfCurveKey(oldChildIndex, Offset(refId, PropertyId.Transform_M12_Offset))].Sample(currentFrame - 1));
								}
								
								if(placeObject2.PlaceFlagHasColorTransform)
								{
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_MA_Offset), TSPropertyType._byte)].Add(currentFrame, placeObject2.CXform.AlphaMultTerm);
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_MR_Offset), TSPropertyType._byte)].Add(currentFrame, placeObject2.CXform.RedMultTerm);
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_MG_Offset), TSPropertyType._byte)].Add(currentFrame, placeObject2.CXform.GreenMultTerm);
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_MB_Offset), TSPropertyType._byte)].Add(currentFrame, placeObject2.CXform.BlueMultTerm);
									
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_OA_Offset), TSPropertyType._byte)].Add(currentFrame, placeObject2.CXform.AlphaAddTerm);
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_OR_Offset), TSPropertyType._byte)].Add(currentFrame, placeObject2.CXform.RedAddTerm);
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_OG_Offset), TSPropertyType._byte)].Add(currentFrame, placeObject2.CXform.GreenAddTerm);
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_OB_Offset), TSPropertyType._byte)].Add(currentFrame, placeObject2.CXform.BlueAddTerm);
								}
								else
								{
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_MA_Offset), TSPropertyType._byte)].Add(currentFrame, retVal[new EMSwfCurveKey(oldChildIndex, Offset(refId, PropertyId.Transform_MA_Offset), TSPropertyType._byte)].Sample(currentFrame - 1));
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_MR_Offset), TSPropertyType._byte)].Add(currentFrame, retVal[new EMSwfCurveKey(oldChildIndex, Offset(refId, PropertyId.Transform_MR_Offset), TSPropertyType._byte)].Sample(currentFrame - 1));
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_MG_Offset), TSPropertyType._byte)].Add(currentFrame, retVal[new EMSwfCurveKey(oldChildIndex, Offset(refId, PropertyId.Transform_MG_Offset), TSPropertyType._byte)].Sample(currentFrame - 1));
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_MB_Offset), TSPropertyType._byte)].Add(currentFrame, retVal[new EMSwfCurveKey(oldChildIndex, Offset(refId, PropertyId.Transform_MB_Offset), TSPropertyType._byte)].Sample(currentFrame - 1));
									
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_OA_Offset), TSPropertyType._byte)].Add(currentFrame, retVal[new EMSwfCurveKey(oldChildIndex, Offset(refId, PropertyId.Transform_OA_Offset), TSPropertyType._byte)].Sample(currentFrame - 1));
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_OR_Offset), TSPropertyType._byte)].Add(currentFrame, retVal[new EMSwfCurveKey(oldChildIndex, Offset(refId, PropertyId.Transform_OR_Offset), TSPropertyType._byte)].Sample(currentFrame - 1));
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_OG_Offset), TSPropertyType._byte)].Add(currentFrame, retVal[new EMSwfCurveKey(oldChildIndex, Offset(refId, PropertyId.Transform_OG_Offset), TSPropertyType._byte)].Sample(currentFrame - 1));
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_OB_Offset), TSPropertyType._byte)].Add(currentFrame, retVal[new EMSwfCurveKey(oldChildIndex, Offset(refId, PropertyId.Transform_OB_Offset), TSPropertyType._byte)].Sample(currentFrame - 1));
								}
	
								retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_Placed_Offset), TSPropertyType._byte)].Add(currentFrame, 1f);
								
								if(placeObject2.HasClipDepth())
								{
									if(retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.ClipCount_Offset), TSPropertyType._ushort)].Add(currentFrame, clipCount))
									{
										retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.UpdateFlag_Offset), TSPropertyType._byte)].Add(currentFrame, EMUpdateFlag.Flag(EMUpdateFlag.Flags.UpdateDrawCalls), false);
									}
								}
								else
								{
									if(retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.ClipCount_Offset), TSPropertyType._ushort)].Add(currentFrame, retVal[new EMSwfCurveKey(oldChildIndex, Offset(refId, PropertyId.ClipCount_Offset), TSPropertyType._ushort)].Sample(currentFrame - 1)))
									{
										retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.UpdateFlag_Offset), TSPropertyType._byte)].Add(currentFrame, EMUpdateFlag.Flag(EMUpdateFlag.Flags.UpdateDrawCalls), false);
									}
								}
							}
							else
							{
								if(placeObject2.HasMatrix())
								{
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_M01_Offset))].Add(currentFrame, placeObject2.Matrix.M01);
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_M10_Offset))].Add(currentFrame, placeObject2.Matrix.M10);
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_M00_Offset))].Add(currentFrame, placeObject2.Matrix.M00);
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_M11_Offset))].Add(currentFrame, placeObject2.Matrix.M11);
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_M02_Offset))].Add(currentFrame, placeObject2.Matrix.M02);
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_M12_Offset))].Add(currentFrame, placeObject2.Matrix.M12);
								}
								if(placeObject2.PlaceFlagHasColorTransform)
								{
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_MA_Offset), TSPropertyType._byte)].Add(currentFrame, placeObject2.CXform.AlphaMultTerm);
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_MR_Offset), TSPropertyType._byte)].Add(currentFrame, placeObject2.CXform.RedMultTerm);
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_MG_Offset), TSPropertyType._byte)].Add(currentFrame, placeObject2.CXform.GreenMultTerm);
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_MB_Offset), TSPropertyType._byte)].Add(currentFrame, placeObject2.CXform.BlueMultTerm);
									
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_OA_Offset), TSPropertyType._byte)].Add(currentFrame, placeObject2.CXform.AlphaAddTerm);
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_OR_Offset), TSPropertyType._byte)].Add(currentFrame, placeObject2.CXform.RedAddTerm);
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_OG_Offset), TSPropertyType._byte)].Add(currentFrame, placeObject2.CXform.GreenAddTerm);
									retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_OB_Offset), TSPropertyType._byte)].Add(currentFrame, placeObject2.CXform.BlueAddTerm);
								}
								
								retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_Placed_Offset), TSPropertyType._byte)].Add(currentFrame, 1f);
								
								if(placeObject2.HasClipDepth())
								{
									if(retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.ClipCount_Offset), TSPropertyType._ushort)].Add(currentFrame, clipCount))
									{
										retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.UpdateFlag_Offset), TSPropertyType._byte)].Add(currentFrame, EMUpdateFlag.Flag(EMUpdateFlag.Flags.UpdateDrawCalls), false);
									}
								}
							}
						}
					}
					else if(_controlTags[i] is EMSwfRemoveObject2)
					{
						var removeObject2 = _controlTags[i] as EMSwfRemoveObject2;
						
						var childIndex = childs.FindLastIndex(x=>x.Depth == removeObject2.Depth && x.FirstFrame <= currentFrame);
						if(childIndex != -1)
						{
							var refId = childs[childIndex].RefId;
							retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.Transform_Placed_Offset), TSPropertyType._byte)].Add(currentFrame, 0f);
							
							if(retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.ClipCount_Offset), TSPropertyType._ushort)].Add(currentFrame, 0f))
							{
								retVal[new EMSwfCurveKey(childIndex, Offset(refId, PropertyId.UpdateFlag_Offset), TSPropertyType._byte)].Add(currentFrame, EMUpdateFlag.Flag(EMUpdateFlag.Flags.UpdateDrawCalls), false);
							}
						}
					}
				}
				
				_animationCurves = retVal.Where(x=>x.Value.Count > 1).ToList();
				return _animationCurves;
			}
		}
		
		public int Offset(int refId, PropertyId propertyId)
		{
			var obj = _importer.GetObject<EMSwfObject>((ushort)refId);
			
			Type type = null;
			if(obj is EMSwfDefineShape)
			{
				type = typeof(EMShape);	
			}
			else if(obj is EMSwfDefineSprite)
			{
				type = typeof(EMSprite);
			}
			
			CoreAssert.Fatal(type != null, "Unknown Type");
			var field = type.GetField(propertyId.ToString());
			CoreAssert.Fatal(field != null);
			
			return (int)field.GetRawConstantValue();
		}
	    #endregion
	}
}