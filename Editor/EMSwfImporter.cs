
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
	    private string _temporaryFolder;
	    private string _adl;
	
	    private readonly List<EMSwfObject> _meta = new List<EMSwfObject>();
	    private readonly CoreDictionary<ushort, EMSwfObject> _objects = new CoreDictionary<ushort, EMSwfObject>();
	    private readonly CoreDictionary<string, EMSwfObject> _classes = new CoreDictionary<string, EMSwfObject>();
		
	    private EMSwfImporter(string source, string destination, string temporaryFolder, string adl)
	    {
	        _source = source;
	
			_destinationFolder = destination + Path.GetFileNameWithoutExtension(source) + "/";
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
			
			Directory.CreateDirectory(_destinationFolder);
			
			Raster();
			
			foreach (var sprite in GetObjects<EMSwfDefineSprite>())
			{
				using (var tree = new EMAnimationTreeStream())
	            {
					sprite.Expand();
					tree.InitFromTree(new EMSwfAnimation(header.FrameRate, sprite), null, new EMFactory());
					tree.WriteAllBytes(_destinationFolder + sprite.Id + ".anim.bytes");
					UnityEditor.AssetDatabase.ImportAsset(_destinationFolder + sprite.Id + ".anim.bytes");
				}
			}

			foreach (var root in _classes)
	        {
				var go = new GameObject();
				
				var defineSprite = root.Value as EMSwfDefineSprite;
				using (var tree = new EMDisplayTreeStream())
	            {
					var positions = tree.InitFromTree(new EMSwfDefineSpriteNode(root.Key, EMSwfMatrix.Identity, 0, defineSprite), null, new EMFactory());
					tree.WriteAllBytes(_destinationFolder + root.Key + ".bytes");
					
					UnityEditor.AssetDatabase.ImportAsset(_destinationFolder + root.Key + ".bytes");
					UnityEditor.AssetDatabase.ImportAsset(_destinationFolder + "atlas.png");
					UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.ForceSynchronousImport);
					
					var rootTextAsset = UnityEditor.AssetDatabase.LoadMainAssetAtPath(_destinationFolder + root.Key + ".bytes");
					var atlas = UnityEditor.AssetDatabase.LoadMainAssetAtPath(_destinationFolder + "atlas.png");
					
					var rootComponent = go.AddComponent<EMRoot>();
					
					var field = typeof(EMRoot).GetField("_bytes",  System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
					field.SetValue(rootComponent, rootTextAsset);
					
					field = typeof(EMRoot).GetField("_atlas",  System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
					field.SetValue(rootComponent, atlas);
			
					field = typeof(EMRoot).GetField("_tree",  System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
					var rootTree = field.GetValue(rootComponent) as EMDisplayTreeStream;
					
					for(var i = 0; i < positions.Count; i++)
					{
						var spriteNode = positions[i].Key as EMSwfDefineSpriteNode;
						if(spriteNode != null)
						{
							var obj = rootTree.GetObject(positions[i].Value);
							
							var animTextAsset = UnityEditor.AssetDatabase.LoadMainAssetAtPath(_destinationFolder + spriteNode.Id + ".anim.bytes");
							var animationHead = go.AddComponent<EMAnimationHead>();
							
							field = typeof(EMAnimationHead).GetField("_data", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
							field.SetValue(animationHead, animTextAsset);
							
							field = typeof(EMObject).GetField("_animationHead", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
							field.SetValue(obj, animationHead);
						}
					}
				}	
	
				UnityEditor.PrefabUtility.CreatePrefab(_destinationFolder + root.Key + ".prefab", go);
				GameObject.DestroyImmediate(go);
			}
	       
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
			
	        using (var stream = new EMSwfMemoryStream())
	        {
	            var binaryWriter = new EMSwfBinaryWriter(stream);
	
	            GetObject<EMSwfFileAttributes>().Write(binaryWriter);
	            GetObject<EMSwfSetBackgroundColor>().Write(binaryWriter);
				foreach(var obj in GetObjects<EMSwfDefineBitsLossless2>())
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
	        processStartInfo.WorkingDirectory = Path.GetFullPath("../Rasterizer/bin-debug/");
	        processStartInfo.FileName = _adl;
			processStartInfo.RedirectStandardError = true;
			processStartInfo.RedirectStandardOutput = true;
			processStartInfo.UseShellExecute = false;
	        processStartInfo.Arguments = string.Format("{0} -- file:/{1} {2} {3} {4} {5}",
	                                                   "Raster-app.xml",
	                                                   Path.GetFullPath(destination),
	                                                   Path.GetFullPath(_temporaryFolder),
	                                                   "72",
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
	
	        CoreTexture2D atlas = null;
	        var uv = CoreTexture2D.Pack(Directory.GetFiles(_temporaryFolder, "*.png", SearchOption.AllDirectories)
	                           	.Where(x=>!x.Contains("info"))
								.OrderBy(x=>int.Parse(Path.GetFileNameWithoutExtension(x)))
	                           	.ToList()
	                           	.ConvertAll(x=>new CoreTexture2D(x))
	                           	.ToArray(), out atlas, padding);
			
	        atlas.Save(string.Format("{0}/atlas.png", _destinationFolder));
			for(var i = 0; i < uv.Length; i++)
			{
				var current = new EMRectStructLayout();
				current.X = uv[i].X;
				current.Y = uv[i].Y;
				current.Width = uv[i].Width;
				current.Height = uv[i].Height;
				shapes[i].Uv = current;
			}
	    }
	}
}