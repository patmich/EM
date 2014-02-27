
using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using UnityEngine;

namespace LLT
{
	public sealed class EMSwfImporter
	{
	    private string _source;
		private string _destinationFolder;
		private string _destination;
	    private string _temporaryFolder;
	    private string _adl;
	
	    private readonly List<EMSwfObject> _meta = new List<EMSwfObject>();
	    private readonly CoreDictionary<ushort, EMSwfObject> _objects = new CoreDictionary<ushort, EMSwfObject>();
	    private readonly CoreDictionary<string, EMSwfObject> _classes = new CoreDictionary<string, EMSwfObject>();
		
	    private EMSwfImporter(string source, string destinationFolder, string temporaryFolder, string adl)
	    {
	        _source = source;
			_destinationFolder = destinationFolder;

			Directory.CreateDirectory(_destinationFolder);
			
	        _temporaryFolder = string.Format("{0}/{1}/", temporaryFolder, Path.GetFileNameWithoutExtension(source));
	        if(Path.GetFullPath(_temporaryFolder).Contains(Directory.GetCurrentDirectory()))
	        {
	            if (Directory.Exists(_temporaryFolder))
	            {
	                Directory.GetFiles(_temporaryFolder, "*.png").ToList().ForEach(x => File.Delete(x));
	                Directory.Delete(_temporaryFolder, true);
	            }
	        }
	
	        _adl = adl;
	    }
	
	    public static IEnumerator Import(string source, string destination, string temporaryFolder, string adl)
	    {
	        var importer = new EMSwfImporter(source, destination, temporaryFolder, adl);
	        return importer.ImportInternal();
	    }
	
	    private IEnumerator ImportInternal()
	    {
			EMSwfHeader header = null;
	        foreach (System.Object obj in EMSwfFileReader.Open(this, _source))
	        {
	            if(obj is EMSwfHeader)
	            {
	                header = obj as EMSwfHeader;
	            }
				else
				{
					CoreAssert.Fatal(header != null);
		           	if (obj is EMSwfObject)
		            {
		                var swfObject = obj as EMSwfObject;
		
		                if (swfObject.Id != 0)
		                {
		                    _objects[swfObject.Id] = swfObject;
		                }
		                else if (swfObject is EMSwfSymbolClass)
		                {
		                    var symbolClass = swfObject as EMSwfSymbolClass;
		                    foreach (KeyValuePair<ushort, string> class_ in symbolClass.Map)
		                    {
		                        _classes[class_.Value] = _objects[class_.Key];
		                    }
		                }
		                else
		                {
		                    _meta.Add(swfObject);
		                }
		            }
				}
	        }

			foreach (var root in _classes)
			{
				var defineSprite = root.Value as EMSwfDefineSprite;
				defineSprite.Expand(1f, 1f);
			}

			Raster();
			
			// Create text asset and wait for reference to exist.
			foreach (var sprite in GetObjects<EMSwfDefineSprite>())
			{
				if(sprite.AnimationCurves.Count > 0)
				{
					using (var tree = new EMAnimationTreeStream())
		            {
						byte[] buffer;
						List<KeyValuePair<ITSTreeNode, int>> positions;

						TSTreeStreamBuilder.Build(new EMSwfAnimation(header.FrameRate, sprite), null, new EMFactory(), out buffer, out positions);

						var animationHeadData = ScriptableObject.CreateInstance<EMAnimationHeadData>();
						animationHeadData.Bytes = tree.GetAllBytes();
						UnityEditor.AssetDatabase.CreateAsset(animationHeadData, _destinationFolder + sprite.Id + ".anim.asset");
					}
				}
			}
			UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.ForceSynchronousImport);
			foreach (var sprite in GetObjects<EMSwfDefineSprite>())
			{
				if(sprite.AnimationCurves.Count > 0)
				{
					var wait = WaitAsset(_destinationFolder + sprite.Id + ".anim.asset");
					while(wait.MoveNext())yield return null;
				}
			}
			
			foreach (var root in _classes)
	        {
				var prefabPath = _destinationFolder + root.Key + ".prefab";
				var prefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath(prefabPath) as GameObject;
				if(prefab == null)
				{
					var temp = new GameObject();
					prefab = UnityEditor.PrefabUtility.CreatePrefab(prefabPath, temp) as GameObject;
					GameObject.DestroyImmediate(temp);
				}
				else
				{
					foreach(var comp in prefab.GetComponents<MonoBehaviour>().Where(x=>x is EMAnimationHead))
					{
						MonoBehaviour.DestroyImmediate(comp, true);
					}
				}

				var defineSprite = root.Value as EMSwfDefineSprite;

				if(defineSprite != null)
				{
					using (var tree = new EMDisplayTreeStream())
		            {
						var rootComponent = prefab.GetComponent<EMRoot>();
						if(rootComponent == null)
						{
							rootComponent = prefab.AddComponent<EMRoot>();
						}
						rootComponent.enabled = false;

						byte[] buffer;
						List<KeyValuePair<ITSTreeNode, int>> positions;

						TSTreeStreamBuilder.Build(new EMSwfDefineSpriteNode(root.Key, true, 0, EMSwfMatrix.Identity, EMSwfColorTransform.Identity, 0, defineSprite, 1f, 1f), null, new EMFactory(), out buffer, out positions);

						var index = 0;
						var atlas = string.Empty;
						var textures = new List<Texture>();

						while((atlas = string.Format("{0}atlas{1}.png", _destinationFolder, index++)) != string.Empty && File.Exists(atlas))
						{
							var waitAtlas = WaitAsset(atlas);
							while(waitAtlas.MoveNext())yield return null;

							textures.Add(UnityEditor.AssetDatabase.LoadMainAssetAtPath(atlas) as Texture);
						}

						var field = typeof(EMRoot).GetField("_textures",  System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
						field.SetValue(rootComponent, textures.ToArray());
				
						field = typeof(EMRoot).GetField("_tree",  System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
						field.SetValue(rootComponent, tree);
						
						for(var i = 0; i < positions.Count; i++)
						{
							var spriteNode = positions[i].Key as EMSwfDefineSpriteNode;
							if(spriteNode != null && GetObject<EMSwfDefineSprite>((ushort)spriteNode.Id).AnimationCurves.Count > 0)
							{
								var obj = new EMObject();//tree.GetObject(tree.CreateTag(positions[i].Value));
								obj.Position = positions[i].Value;
								obj.Init(tree);

								var animationHeadData = UnityEditor.AssetDatabase.LoadMainAssetAtPath(_destinationFolder + spriteNode.Id + ".anim.asset");

								var animationHead = obj.AddComponent<EMAnimationHead>();
								animationHead.hideFlags = HideFlags.HideInInspector;

								field = typeof(EMAnimationHead).GetField("_data", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
								field.SetValue(animationHead, animationHeadData);
								
								field = typeof(EMObject).GetField("_animationHead", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
								field.SetValue(obj, animationHead);

								animationHead.enabled = false;
							}
						}
						
						var rootTextAssetPath = _destinationFolder + root.Key + ".bytes";
						tree.WriteAllBytes(rootTextAssetPath);
						
						UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.ForceSynchronousImport);
						
						var wait = WaitAsset(rootTextAssetPath);
						while(wait.MoveNext())yield return null;
						
						field = typeof(EMRoot).GetField("_bytes",  System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
						field.SetValue(rootComponent, UnityEditor.AssetDatabase.LoadMainAssetAtPath(rootTextAssetPath) as TextAsset);
					}	
				}
			}

			UnityEditor.EditorUtility.UnloadUnusedAssets();
	        yield return null;
	    }
	
	    public List<T> GetObjects<T>() where T : EMSwfObject
	    {
	        return _meta.FindAll(x=>x is T).ConvertAll(x=>x as T).Concat(_objects.Values.ToList().FindAll(x => x is T).ToList().ConvertAll(x => x as T)).ToList();
	    }
	
	    public T GetObject<T>() where T : EMSwfObject
	    {
	        var all = GetObjects<T>();
	        CoreAssert.Fatal(all.Count == 1);
	        return all[0] as T;
	    }
	
	    public T GetObject<T>(ushort id) where T : EMSwfObject
	    {
	        CoreAssert.Fatal(_objects.ContainsKey(id) && _objects[id] is T);
	        return _objects[id] as T;
	    }
	
	    private void Raster()
	    {
	        var destination = string.Format("{0}/{1}.swf", _temporaryFolder, Path.GetFileNameWithoutExtension(_source));
	        Directory.CreateDirectory(_temporaryFolder);
			
			var shapes = GetObjects<EMSwfDefineShape>().OrderBy(x=>x.Id).ToArray();
			var swfSettings = EMSwfSettings.Get(_source);
	        using (var stream = new EMSwfMemoryStream())
	        {
	            var binaryWriter = new EMSwfBinaryWriter(stream);
	
	            GetObject<EMSwfFileAttributes>().Write(binaryWriter);
	            GetObject<EMSwfSetBackgroundColor>().Write(binaryWriter);

				var objects = new List<EMSwfObject>();
				objects.AddRange(GetObjects<EMSwfJPEGTables>().Cast<EMSwfObject>());
				objects.AddRange(GetObjects<EMSwfDefineBits>().Cast<EMSwfObject>());
				objects.AddRange(GetObjects<EMSwfDefineBitsLossless2>().Cast<EMSwfObject>());
				objects.AddRange(GetObjects<EMSwfDefineBitsLossless>().Cast<EMSwfObject>());
				objects.AddRange(GetObjects<EMSwfDefineBitsJPEG3>().Cast<EMSwfObject>());
				foreach(var obj in objects.OrderBy(x=>x.Id))
				{
					obj.Write(binaryWriter);
				}
				
	            ushort frameCount = 0;

				// ToDO: Add dependency check.
	            foreach (var shape in shapes)
	            {
	                (new EMSwfFrameLabel(shape.Id.ToString())).Write(binaryWriter);
	
	                shape.Write(binaryWriter);
	                var matrix = EMSwfMatrix.Identity;

					if(swfSettings.Dpi < 0)
					{
						matrix.M00 = Mathf.Max(shape.ScaleX, shape.ScaleY);
						matrix.M11 = Mathf.Max(shape.ScaleX, shape.ScaleY);
					}
	                matrix.M02 = -shape.Bounds.XMin;
	                matrix.M12 = -shape.Bounds.YMin;
	                (new EMSwfPlaceObject2(0, shape.Id, matrix)).Write(binaryWriter);
	                (new EMSwfShowFrame()).Write(binaryWriter);
	                frameCount++;
	
	                (new EMSwfRemoveObject2(0)).Write(binaryWriter);
	            }
	
	            (new EMSwfShowFrame()).Write(binaryWriter);
	            (new EMSwfEnd()).Write(binaryWriter);
	
	            var bytes = new Byte[stream.Position];
	            stream.Position = 0;
	            stream.Read(bytes, 0, bytes.Length);
	
	            stream.Position = 0;
	
	            (new EMSwfHeader((byte)19, (uint)bytes.Length, new EMSwfRect(0,0,4096,4096), (ushort)15360, frameCount)).Write(binaryWriter);
	            binaryWriter.Write(bytes);
	
	            bytes = new Byte[stream.Position];
	            stream.Position = 0;
	            stream.Read(bytes, 0, bytes.Length);
	
	            File.WriteAllBytes(destination, bytes);
	        }
	
			var padding = 5;
		
	        ProcessStartInfo processStartInfo = new ProcessStartInfo();
	        processStartInfo.WorkingDirectory = Path.GetFullPath("Tools/Rasterizer/bin-debug/");
	        processStartInfo.FileName = _adl;
			processStartInfo.RedirectStandardError = true;
			processStartInfo.RedirectStandardOutput = true;
			processStartInfo.UseShellExecute = false;
	        processStartInfo.Arguments = string.Format("{0} -- file:/{1} {2} {3} {4} {5}",
	                                                   "Raster-app.xml",
	                                                   Path.GetFullPath(destination),
	                                                   Path.GetFullPath(_temporaryFolder),
			                                           swfSettings.Dpi > 0 ? swfSettings.Dpi : 72,
	                                                   padding.ToString(),
	                                                   "10"
	                                                   );

	        Process process = Process.Start(processStartInfo);
	        process.WaitForExit();
			
			if(process.ExitCode != 0)
			{
				UnityEngine.Debug.LogError(process.StandardError.ReadToEnd());
			}
			else
			{
				UnityEngine.Debug.Log(process.StandardOutput.ReadToEnd());
			}
	
			foreach(var file in Directory.GetFiles(_temporaryFolder, "*.png", SearchOption.AllDirectories).Where(x=>!x.Contains("-info")))
			{
				var original = CoreTexture2D.PngDecoder.Invoke(file);
				var info = CoreTexture2D.PngDecoder.Invoke(Path.GetDirectoryName(file) + "/" + Path.GetFileNameWithoutExtension(file) + "-info.png");
				
				for(var i = 0; i < original.ARGB.Length; i++)
				{
					if((original.ARGB[i] >> 24) == 0)
					{
						original.ARGB[i] = info.ARGB[i] & 0x00FFFFFF;
					}
				}

				original.Save(file);
			}
			
			var textures = Directory.GetFiles(_temporaryFolder, "*.png", SearchOption.AllDirectories)
	                           	.Where(x=>!x.Contains("info"))
								.OrderBy(x=>int.Parse(Path.GetFileNameWithoutExtension(x)))
	                           	.ToList()
	                           	.ConvertAll(x=>new CoreTexture2D(x))
	                           	.ToArray();

			var textureIndex = 0;
			while(textures.Count(x => x != null) > 0)
			{
		        CoreTexture2D atlas = null;
				CoreRect[] uv;
		
				textures = CoreTexture2D.Pack(textures, padding, out atlas, out uv);
			
				for(var i = 0; i < uv.Length; i++)
				{
					if(uv[i] != null)
					{
						for(var j = 1; j < padding; j++)
						{
							var startY = -j + Mathf.RoundToInt(uv[i].Y * atlas.Height);
							var endY = startY + Mathf.RoundToInt(uv[i].Height * atlas.Height) + 2 * j - 1;
							var temp = atlas.ARGB.ToArray();
							for(var y = startY; y <= endY; y++)
							{
								var startX = -j + Mathf.RoundToInt(uv[i].X * atlas.Width);
								var endX = startX + Mathf.RoundToInt(uv[i].Width * atlas.Width) + 2 * j - 1;
								for(var x = startX; x <= endX; x++)
								{
									if(x == startX || y == startY || x == endX || y == endY)
									{
										var r0 = atlas.ARGB[(x - 1) + y * atlas.Width];
										var r1 = atlas.ARGB[(x + 1) + y * atlas.Width];
										var r2 = atlas.ARGB[(x) + (y - 1) * atlas.Width];
										var r3 = atlas.ARGB[(x) + (y + 1) * atlas.Width];
										var r4 = 0;
										var r5 = 0;
										var r6 = 0;
										var r7 = 0;

										if((x == startX && y == startY) || (x == startX && y == endY) || (x == endX && y == startY) || (x == endX && y == endY))
										{
											r4 = atlas.ARGB[(x - 1) + (y - 1) * atlas.Width];
											r5 = atlas.ARGB[(x + 1) + (y + 1) * atlas.Width];
											r6 = atlas.ARGB[(x + 1) + (y - 1) * atlas.Width];
											r7 = atlas.ARGB[(x - 1) + (y + 1) * atlas.Width];
										}

										var c0 = (Color)(new Color32((byte)((r0 >> 16) & 0xFF), (byte)((r0 >> 8) & 0xFF), (byte)((r0 >> 0) & 0xFF), (byte)((r0 >> 24) & 0xFF)));
										var c1 = (Color)(new Color32((byte)((r1 >> 16) & 0xFF), (byte)((r1 >> 8) & 0xFF), (byte)((r1 >> 0) & 0xFF), (byte)((r1 >> 24) & 0xFF)));
										var c2 = (Color)(new Color32((byte)((r2 >> 16) & 0xFF), (byte)((r2 >> 8) & 0xFF), (byte)((r2 >> 0) & 0xFF), (byte)((r2 >> 24) & 0xFF)));
										var c3 = (Color)(new Color32((byte)((r3 >> 16) & 0xFF), (byte)((r3 >> 8) & 0xFF), (byte)((r3 >> 0) & 0xFF), (byte)((r3 >> 24) & 0xFF)));
										var c4 = (Color)(new Color32((byte)((r4 >> 16) & 0xFF), (byte)((r4 >> 8) & 0xFF), (byte)((r4 >> 0) & 0xFF), (byte)((r4 >> 24) & 0xFF)));
										var c5 = (Color)(new Color32((byte)((r5 >> 16) & 0xFF), (byte)((r5 >> 8) & 0xFF), (byte)((r5 >> 0) & 0xFF), (byte)((r5 >> 24) & 0xFF)));
										var c6 = (Color)(new Color32((byte)((r6 >> 16) & 0xFF), (byte)((r6 >> 8) & 0xFF), (byte)((r6 >> 0) & 0xFF), (byte)((r6 >> 24) & 0xFF)));
										var c7 = (Color)(new Color32((byte)((r7 >> 16) & 0xFF), (byte)((r7 >> 8) & 0xFF), (byte)((r7 >> 0) & 0xFF), (byte)((r7 >> 24) & 0xFF)));
										var a = c0.a + c1.a + c2.a + c3.a + c4.a + c5.a + c6.a + c7.a;
										var o = (Color32)((c0 * c0.a + c1 * c1.a + c2 * c2.a + c3 * c3.a + c4 * c4.a + c5 * c5.a + c6 * c6.a + c7 * c7.a)/a);
										temp[x + y * atlas.Width] = (o.a << 24) | (o.r << 16) | (o.g << 8) | o.b;
									}
								}
							}
							Array.Copy(temp, 0, atlas.ARGB, 0, temp.Length);
						}
					}
				}
		        atlas.Save(string.Format("{0}/atlas{1}.png", _destinationFolder, textureIndex));
				
				for(var i = 0; i < uv.Length; i++)
				{
					if(uv[i] != null)
					{
						var current = new EMRectStructLayout();
						current.X = uv[i].X;
						current.Y = uv[i].Y;
						current.Width = uv[i].Width;
						current.Height = uv[i].Height;
						
						shapes[i].TextureIndex = textureIndex;
						shapes[i].Uv = current;
					}
				}

				textureIndex++;
			}
	    }
		
		private IEnumerator WaitAsset(string path)
		{
			while(UnityEditor.AssetDatabase.LoadMainAssetAtPath(path) == null)
			{
				yield return null;
			}
		}
	}
}