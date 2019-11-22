/* ---------------------------------------------------------------------
 * Entry Point MAIN
 * Plugin Initialisazion.
 * ---------------------------------------------------------------------
 */
using System;
using System.Windows.Forms;
using System.Xml;
using CamBam.Geom;
using CamBam.CAD;
using CamBam.UI;
using CamBam;

namespace SVGLoader
{
	//-------------------------------------------------------------------
	// Output: CamBam CAD
	//-------------------------------------------------------------------
	public class OutputCAD : IOutput
	{
		CamBamUI _ui;

		public OutputCAD (CamBamUI ui)
		{
			_ui = ui;
		}

		public override void draw (Entity elem, string id = null)
		{
			if (id != null && id.Length > 0)
				elem.Tag = String.Format("id={0}", id);
			_ui.InsertEntity (elem);
		}

		public override void layer (string id)
		{
			if (!CamBamUI.MainUI.ActiveView.CADFile.Layers.ContainsKey (id)) {
				CamBamUI.MainUI.ActiveView.CADFile.CreateLayer (id);
				trace (4, ">> layer created: {0}", id);
			}
			CamBamUI.MainUI.ActiveView.CADFile.SetActiveLayer (id);
		}

		public override void trace (int level, string format, params object[] args)
		{
			CamBam.ThisApplication.AddLogMessage (level, string.Format (format, args));
		}
	}
	//-------------------------------------------------------------------
	// Implemtation of FileHandle (CamBam.CAD.CADFileIO)
	//-------------------------------------------------------------------
	public class Handler : CamBam.CAD.CADFileIO
	{
		OutputCAD _output;

		//-------------------------------------------------------------------
		public Handler ()
		{
			_output = new OutputCAD (Plugin._ui);
		}
		//-------------------------------------------------------------------
		public override string FileFilter {
			get {
				return "SVG-File(jv) (*.svg)|*.svg";
			}
		}
		//-------------------------------------------------------------------
		static void trace (string format, params object[] args)
		{
			CamBam.ThisApplication.AddLogMessage (4, string.Format (format, args));
		}
		//-------------------------------------------------------------------
		public override bool ReadFile (string path)
		{
			//CamBam.ThisApplication.MsgBox("SVG: " + path);
			XmlDocument xml = new XmlDocument (); 
			xml.Load (path);
			trace ("SVG loaded: " + path);
			Graphics graphics = new Graphics (_output);
			graphics.draw (xml);
			return true;
		}
	}
	//-------------------------------------------------------------------
	// Plugin Setup (main)
	//-------------------------------------------------------------------
	public class Plugin
	{
		static string _version = "1.19";
		public static CamBamUI _ui;

		// This is the main entry point into the plugin.
		public static void InitPlugin (CamBamUI ui)
		{
			// Store a reference to the CamBamUI object passed to InitPlugin
			_ui = ui;

			// Create a new menu item in the top Plugins menu
			ToolStripMenuItem mi = null;
			/*
			mi = new ToolStripMenuItem ();
			mi.Text = "SVG-Loader (About)";
			mi.Click += new EventHandler (TestPlugin_Click);
			ui.Menus.mnuPlugins.DropDownItems.Add (mi);
			*/
			mi = new ToolStripMenuItem ();
			mi.Text = "SVG-Loader import...";
			mi.Click += new EventHandler (OpenSVG_Click);
			ui.Menus.mnuPlugins.DropDownItems.Add (mi);
			CamBamUI.CADFileHandlers.Add (new Handler ());
			//ui.CADFileHandlers.Add(new Handler());
		}

		// Simple menu handler
		static void TestPlugin_Click (object sender, EventArgs e)
		{
			ThisApplication.MsgBox (string.Format ("SVG Loader (c) j.vater 2019 version {0}", _version));
		}

		static void OpenSVG_Click (object sender, EventArgs e)
		{
			string path = string.Empty;
			string title = string.Format ("SVG Loader version {0}", _version);

			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Title = title;
			dlg.Filter = "SVG files (*.svg)|*.svg|All files (*.*)|*.*";
			if (dlg.ShowDialog () != DialogResult.OK)
				return;
			path = dlg.FileName;
			OutputCAD output = new OutputCAD (Plugin._ui);
			XmlDocument xml = new XmlDocument (); 
			xml.Load (path);
			Graphics graphics = new Graphics (output);
			graphics.draw (xml);
		}
	}
	//-------------------------------------------------------------------
}