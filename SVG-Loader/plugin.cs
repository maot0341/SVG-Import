using System;
using System.Windows.Forms;
using CamBam;
using CamBam.UI;

namespace SVGLoader
{
	public class Plugin
	{
		public static CamBamUI _ui;

		// This is the main entry point into the plugin.
		public static void InitPlugin(CamBamUI ui)
		{
			// Store a reference to the CamBamUI object passed to InitPlugin
			_ui = ui;

			// Create a new menu item in the top Plugins menu
			ToolStripMenuItem mi = new ToolStripMenuItem();
			mi.Text = "JV test plugin";
			mi.Click += new EventHandler(TestPlugin_Click);
			ui.Menus.mnuPlugins.DropDownItems.Add(mi);
			CamBamUI.CADFileHandlers.Add(new Handler());
			//ui.CADFileHandlers.Add(new Handler());
		}

		// Simple menu handler
		static void TestPlugin_Click(object sender, EventArgs e)
		{
			ThisApplication.MsgBox("Hallo there!");
		}
	}
}