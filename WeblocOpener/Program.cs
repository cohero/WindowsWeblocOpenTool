using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Microsoft.Win32;

namespace WeblocOpener
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args != null && args.Length > 0 && Path.GetExtension(args[0]).ToLower() != ".webloc")
			{
				string url = "";

				XmlTextReader reader = new XmlTextReader(args[0]);
				while (reader.Read())
				{
					if (reader.NodeType == XmlNodeType.Element && reader.Name == "string")
					{
						reader.Read();
						url = reader.Value;
						break;
					}
				}

				if (!String.IsNullOrEmpty(url))
					Process.Start(url);			
			}
			else
			{
				MessageBox.Show("Associate this program with .webloc extension through Windows Explorer, then simply open any .webloc file.");
			}
		}

		// TODO check and if need associate this executable with extension .webloc
		// http://stackoverflow.com/questions/9540051/is-an-application-associated-with-a-given-extension
		// http://stackoverflow.com/questions/2681878/associate-file-extension-with-application

		[DllImport("shell32.dll")]
		static extern int FindExecutable(string lpFile, string lpDirectory, [Out] StringBuilder lpResult);

		[DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

		public static void SetAssociation(string Extension, string KeyName, string OpenWith, string FileDescription)
		{
			RegistryKey BaseKey;
			RegistryKey OpenMethod;
			RegistryKey Shell;
			RegistryKey CurrentUser;

			BaseKey = Registry.ClassesRoot.CreateSubKey(Extension);
			BaseKey.SetValue("", KeyName);

			OpenMethod = Registry.ClassesRoot.CreateSubKey(KeyName);
			OpenMethod.SetValue("", FileDescription);
			OpenMethod.CreateSubKey("DefaultIcon").SetValue("", "\"" + OpenWith + "\",0");
			Shell = OpenMethod.CreateSubKey("Shell");
			Shell.CreateSubKey("edit").CreateSubKey("command").SetValue("", "\"" + OpenWith + "\"" + " \"%1\"");
			Shell.CreateSubKey("open").CreateSubKey("command").SetValue("", "\"" + OpenWith + "\"" + " \"%1\"");
			BaseKey.Close();
			OpenMethod.Close();
			Shell.Close();


			// Delete the key instead of trying to change it
			CurrentUser = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\.ucs", true);
			CurrentUser.DeleteSubKey("UserChoice", false);
			CurrentUser.Close();

			// Tell explorer the file association has been changed
			SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
		}
	}
}