﻿/* ---------------------------------------------------------------------
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

		public OutputCAD(CamBamUI ui)
		{
			_ui = ui;
		}
		public override void draw(Entity elem, string id=null)
		{
			elem.Tag = id;
			_ui.InsertEntity(elem);
		}
		public override void layer(string id)
		{
			if (!CamBamUI.MainUI.ActiveView.CADFile.Layers.ContainsKey (id)) {
				CamBamUI.MainUI.ActiveView.CADFile.CreateLayer (id);
				trace (4, ">> layer created: {0}", id);
			}
			CamBamUI.MainUI.ActiveView.CADFile.SetActiveLayer (id);
		}
		public override void trace(int level, string format, params object[] args)
		{
			CamBam.ThisApplication.AddLogMessage(level, string.Format(format, args));
		}
	}
	//-------------------------------------------------------------------
	// Implemtation of FileHandle (CamBam.CAD.CADFileIO)
	//-------------------------------------------------------------------
	public class Handler : CamBam.CAD.CADFileIO 
	{
		OutputCAD _output;

		//-------------------------------------------------------------------
		public Handler()
		{
			_output = new OutputCAD (Plugin._ui);
		}
		//-------------------------------------------------------------------
		public override string FileFilter 
		{
			get
			{
				return "SVG-File(jv) (*.svg)|*.svg";
			}
		}
		//-------------------------------------------------------------------
		public override bool ReadFile (string path)
		{
			//CamBam.ThisApplication.MsgBox("SVG: " + path);
			XmlDocument xml = new XmlDocument(); 
			xml.Load(path);
			//ThisApplication.AddLogMessage(0, "SVG [{0}] loaded".Format(path));
			trace ("SVG loaded:" + path);
			XmlElement xml_root = xml.DocumentElement;
			//ReadXML(xml_root);
			Graphics graphics = new Graphics (_output);
			graphics.draw (xml_root);
			return true;
		}
		//-------------------------------------------------------------------
		static void trace(string format, params object[] args)
		{
			CamBam.ThisApplication.AddLogMessage(4, string.Format(format, args));
		}
	}
	//-------------------------------------------------------------------
	// Plugin Initialisation (main)
	//-------------------------------------------------------------------
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
	//-------------------------------------------------------------------
}