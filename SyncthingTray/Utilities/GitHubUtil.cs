using SyncthingTray.Properties;
using SyncthingTray.UI.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace SyncthingTray.Utilities
{
	public static class GitHubUtil
	{
		public static void GetLatestVersion()
		{
			var paDialog = new PendingActivityDialog
			{
				Text = "Autoupdate syncthing",
				SupportsCancellation = false,
				SupportsProgressVisualization = false
			};
			paDialog.BackgroundWorker.DoWork += delegate
			{
				paDialog.BackgroundWorker.ReportProgress(0, "Checking for latest version...");

				string url = Environment.Is64BitOperatingSystem
					? "https://github.com/syncthing/syncthing/releases/download/v2.0.3/syncthing-windows-amd64-v2.0.3.zip"
					: "https://github.com/syncthing/syncthing/releases/download/v2.0.3/syncthing-windows-386-v2.0.3.zip";

				using (var client = new HttpClient())
				{
					// 获取 syncthing 下载地址，却分 64 位 32 位
					var links = client.GetStringAsync("https://github.com/easyx-cn/SyncthingTray/releases/download/syncthing/syncthing_last_version.json").Result;
					Regex rex;
					if(Environment.Is64BitOperatingSystem)
						rex = new Regex($@"\[AMD64\](.+)\r|\n");
					else
						rex = new Regex($@"\[386\](.+)\r|\n");
					var ms = rex.Match(links);
					if (!ms.Success)
					{
						paDialog.BackgroundWorker.ReportProgress(0, "获取下载地址失败！");
						Thread.Sleep(5000);
						return;
					}
					string syncthing_download_address = ms.Groups[1].Value;

					using (var r = client.GetAsync(syncthing_download_address, HttpCompletionOption.ResponseHeadersRead).Result)
					{
						r.EnsureSuccessStatusCode();

						using (Stream re = r.Content.ReadAsStreamAsync().Result)
						{
							var filename = Path.GetTempFileName();
							using (FileStream sfile = File.Create(filename))
							{
								byte[] buffer = new byte[1024];
								int bytesRead;
								float readLength = 0;
								long totalLength = r.Content.Headers.ContentLength ?? -1;

								float currentProgress = 0.0f;
								while ((bytesRead = re.Read(buffer, 0, buffer.Length)) > 0)
								{
									readLength += bytesRead;

									float p = (float)Math.Round((decimal)(readLength / totalLength) * 100, 2);
									if (p > currentProgress + 0.2)
									{
										currentProgress = p;
										paDialog.BackgroundWorker.ReportProgress(50, $"Downloading ({p}%)");
									}
									sfile.Write(buffer, 0, bytesRead);
								}
							}
							string zipName = url.Substring(url.LastIndexOf('/') + 1);
							paDialog.BackgroundWorker.ReportProgress(99, $"Unzipping {zipName}...");

							System.IO.Compression.ZipFile.ExtractToDirectory(filename, Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath));
							Directory.Move(Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), Path.GetFileNameWithoutExtension(zipName)), Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "syncthing"));
							if (string.IsNullOrEmpty(Settings.Default.SyncthingPath))
							{
								Settings.Default.SyncthingPath = Path.Combine(Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "syncthing"), "syncthing.exe");
								Settings.Default.Save();
							}

							File.Delete(filename);
							paDialog.BackgroundWorker.ReportProgress(100, "Finished!");
							Thread.Sleep(500);
						}
					}
				}
			};
			paDialog.BackgroundWorker.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs args)
			{
				if (args.Error != null)
				{
					MessageBox.Show(args.Error.Message);
				}
			};
			paDialog.ShowDialog();
		}
	}
}
