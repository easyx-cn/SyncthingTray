using SyncthingTray.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SyncthingTray.Utilities
{
	public class Log
	{
		public static void Info(string msg)
		{
			Write(msg, "INFO");
		}
		public static void Error(string msg)
		{
			Write(msg, "ERROR");
		}

		static void Write(string msg, string type)
		{
			string path = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), Settings.Default.LogPath);
			var info = new FileInfo(path);
			if (!info.Directory.Exists)
				Directory.CreateDirectory(info.Directory.FullName);

			var dir = new DirectoryInfo(info.Directory.FullName);
			var files = dir.GetFiles();

			// 获取纯文件名，不带后缀
			string onlyname = info.Name;
			var e = info.Extension;
			if (!string.IsNullOrEmpty(e))
				onlyname = info.Name.Replace(e, "");

			Regex rex = new Regex(onlyname + "(\\d+)" + e);
			if (info.Exists && info.Length > 1024 * 1024)
			{
				int count = 0, maxIndex = 0;
				foreach (var f in files)
				{
					var m = rex.Match(f.Name);
					if (m.Success)
					{
						count++;
						int a = int.Parse(m.Groups[1].ToString());
						maxIndex = maxIndex == 0 ? a : Math.Max(maxIndex, a);
					}
				}

				// 保留三个旧日志文件
				File.Move(path, path.Replace(info.Name, $"{onlyname}{maxIndex + 1}{e}"));
				if (count == 3)
					File.Delete($"{dir.FullName}\\{onlyname}{maxIndex - 2}{e}");
			}

			using (var fs = new FileStream(path, FileMode.Append))
			{
				using (var sw = new StreamWriter(fs, Encoding.UTF8))
				{
					sw.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - {type} {msg}");
				}
			}
		}
	}
}
