using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

using CamBam;
using CamBam.UI;
using CamBam.Util;
using CamBam.CAD;

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