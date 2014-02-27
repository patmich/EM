using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace LLT
{	
	public static class EMADLInvoke
	{
		public static IEnumerator<string> Invoke(string appXml, string arguments)
		{
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.WorkingDirectory = Path.GetFullPath(Path.GetDirectoryName(appXml));
			processStartInfo.FileName = EMSettings.Instance.AirSDK + "/bin/adl";
			processStartInfo.RedirectStandardError = true;
			processStartInfo.RedirectStandardOutput = true;
			processStartInfo.UseShellExecute = false;
			processStartInfo.Arguments = string.Format("{0} -- {1}", Path.GetFileName(appXml), arguments);
			
			Process process = Process.Start(processStartInfo);
			while(!process.HasExited)yield return null;
			
			if(process.ExitCode != 0)
			{
				yield return process.StandardError.ReadToEnd();
			}
			else
			{
				yield return process.StandardOutput.ReadToEnd();
			}
		}
	}
}