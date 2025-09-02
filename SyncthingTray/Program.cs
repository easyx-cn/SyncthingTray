using SyncthingTray.Properties;
using SyncthingTray.Utilities;
using System;
using System.Windows.Forms;

namespace SyncthingTray
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Application.Run(new UI.SyncthingTray());
            }
            catch (Exception ex)
            {
				Log.Error(ex.Message);
                MessageBox.Show(string.Format(strings.MainFatalError, ex.Message, Settings.Default.GitHubIssueUrl), strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
