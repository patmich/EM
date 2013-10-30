using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

namespace LLT
{
	public class EMSettings
	{
		private const string _fileName = "EMSettings.xml";
		private static string _path;
		
		private static IEnumerator<string> _depencyCheck;
		
		public string FlexSDKSource { get; private set; }
		public string FlexSDK { get; private set; }
		
		public static EMSettings Instance
		{
			get
			{
				if(_path == null)
				{
					_path = Directory.GetFiles(Directory.GetCurrentDirectory(), _fileName, SearchOption.AllDirectories).FirstOrDefault();
				}
				var xmlSerializer = new XmlSerializer(typeof(EMSettings)); 
				
				if(File.Exists(_path))
				{
					using(var stream = new FileStream(_path, FileMode.Open))
					{
						return xmlSerializer.Deserialize(stream) as EMSettings;
					}
				}
				
				CoreAssert.Fatal(false, "Counld not find settings.");
				return null;
			}
		}
		
		public IEnumerator<string> DependencyCheck()
		{
			UnityEditor.EditorApplication.CallbackFunction update = null;
			
			if(_depencyCheck == null)
			{
				_depencyCheck = DependencyCheckInternal();
				
				update = ()=>
				{
					if(_depencyCheck != null)
					{
						_depencyCheck.MoveNext();
					}
				};
				
				UnityEditor.EditorApplication.update += update;
			}
			
			while(_depencyCheck != null && _depencyCheck.MoveNext())yield return _depencyCheck.Current;
			
			UnityEditor.EditorApplication.update -= update;
			_depencyCheck = null;
		}
		
		private IEnumerator<string> DependencyCheckInternal()
		{
			if(!Directory.Exists(FlexSDK))
			{
				var www = new WWW(FlexSDKSource);
				while(!www.isDone)yield return string.Format("Missing depencies: Downloading flex sdk ({0}%)", www.progress * 100);
				
				if(!string.IsNullOrEmpty(www.error))
				{
					yield break;	
				}
				
				var bytes = www.bytes;
				if(Application.platform == RuntimePlatform.OSXEditor)
				{
					var tempPath = Path.GetFullPath(UnityEditor.FileUtil.GetUniqueTempPathInProject());
					System.Threading.ThreadPool.QueueUserWorkItem((x)=>
					{
						File.WriteAllBytes(tempPath, bytes);
						
						Directory.CreateDirectory(FlexSDK);
						var processStartInfo = new ProcessStartInfo("unzip", tempPath);
						processStartInfo.UseShellExecute = false;
						processStartInfo.RedirectStandardError = true;
						processStartInfo.RedirectStandardOutput = true;
						processStartInfo.WorkingDirectory = FlexSDK;
						
						using(var process = Process.Start(processStartInfo))
						{
							process.WaitForExit();
							
							if(process.ExitCode != 0)
							{
								UnityEngine.Debug.LogError(process.StandardError.ReadToEnd());
							}
							else
							{
								UnityEngine.Debug.Log(process.StandardOutput.ReadToEnd());
							}
						}

						bytes = null;
					});
					
				}
				else
				{
					System.Threading.ThreadPool.QueueUserWorkItem((x)=>
					{
						using(var stream = new MemoryStream(bytes))
						{
							var zipFile = Ionic.Zip.ZipFile.Read(stream);
							zipFile.ExtractAll(FlexSDK);
						}
						
						bytes = null;
					});
				}
				while(bytes != null)yield return "Missing depencies: Unziping flex sdk.";
				
				if(Directory.Exists(FlexSDK))
				{
					UnityEngine.Debug.LogWarning("Depencies resolved, reimporting all.");
					foreach(var swf in Directory.GetFiles("Assets", "*.swf", SearchOption.AllDirectories))
					{
						UnityEditor.AssetDatabase.ImportAsset(swf);
					}
				}
			}
		}
	}
}

