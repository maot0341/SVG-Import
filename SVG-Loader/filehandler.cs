using System;
using System.Xml;
using System.Collections;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using CamBam.Geom;
using CamBam.CAD;
using CamBam.UI;

namespace SVGLoader
{
	public class Handler : CamBam.CAD.CADFileIO 
	{
		Adaptor _output;

		public Handler()
		{
			_output = new Adaptor (Plugin._ui);
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
		//-------------------------------------------------------------------
		void InsertEntity (Entity elem, string id) 
		{
			_output.draw (elem, id);
			//elem.Tag = id;
			//Plugin._ui.InsertEntity(elem);
		}
		//-------------------------------------------------------------------
		static string XmlAttr(XmlNode xml, string name, string value) 
		{
			if (xml==null || xml.Attributes==null || xml.Attributes.Count==0)
				return value;
			XmlNode n = xml.Attributes.GetNamedItem(name);
			if (n==null)
				return value;
			return n.Value;
		}
		//-------------------------------------------------------------------
		static Int32 XmlAttr(XmlNode xml, string name, Int32 value) 
		{
			if (xml==null || xml.Attributes==null || xml.Attributes.Count==0)
				return value;
			XmlNode n = xml.Attributes.GetNamedItem(name);
			if (n==null)
				return value;
			return Convert.ToInt32(n.Value);
		}
		//-------------------------------------------------------------------
		string normalize(string str) {
			string result = "";
			foreach (char c in str.ToCharArray())
				result += (char.IsWhiteSpace(c) ? ' ' : c);
			return result;
		}
		//-------------------------------------------------------------------
		ArrayList numbers(string str) 
		{
			ArrayList result = new ArrayList();
			Regex re = new Regex("([+-]?\\d*[.]?\\d+)");
			for (Match match=re.Match(str); match.Success; match = match.NextMatch())
			{
				if (match.Value.Length<1)
					continue;
				Double val = Convert.ToDouble(match.Value);
				result.Add(val);
			}
			return result;
		}
		//-------------------------------------------------------------------
		void ReadXML(XmlNode xml_root) 
		{
			string name = xml_root.Name.ToLower();
			string id = XmlAttr(xml_root, "id", "");
			Entity elem;
			if (name == "g") 
			{
				if (!CamBamUI.MainUI.ActiveView.CADFile.Layers.ContainsKey(id))
				{
					CamBamUI.MainUI.ActiveView.CADFile.CreateLayer(id);
					trace(">> layer created: {0}", id);
				}
				CamBamUI.MainUI.ActiveView.CADFile.SetActiveLayer(id);
				trace(">>g = group");
			}
			else if (name == "line") 
			{
				int x1 = XmlAttr(xml_root, "x1", 0);
				int y1 = XmlAttr(xml_root, "y1", 0);
				int x2 = XmlAttr(xml_root, "x2", 0);
				int y2 = XmlAttr(xml_root, "y2", 0);
				trace(">> line: a=[{0} {1}] b=[{2},{3}]", x1, y1, x2, y2);
				//trace(">> line: a=[{0} {1}] b=[{2},{3}].".Format(x1, y1, x2, y2));
				elem = new Line(new Point2F(x1, y1), new Point2F(x2, y2));
				InsertEntity(elem, id);
			}
			else if (name == "circle") {
				Int32 x = XmlAttr(xml_root, "cx", 0);
				Int32 y = XmlAttr(xml_root, "cy", 0);
				Int32 r = XmlAttr(xml_root, "r", 0);
				trace(">> circle: x={0} y={1} r={2}", x, y, r);
				elem = new Circle(new Point2F(x, y), r);
				InsertEntity(elem, id);
			}
			else if (name == "rect") {
				//String^ x = xml_root->Attributes->GetNamedItem("x")->Value;
				//String^ y = xml_root->Attributes->GetNamedItem("y")->Value;
				//XmlNode^n = xml_root->Attributes->GetNamedItem("width");
				//String^ w = xml_root->Attributes->GetNamedItem("width")->Value;
				//String^ h = xml_root->Attributes->GetNamedItem("height")->Value;
				Int32 x = XmlAttr(xml_root, "x", 0);
				Int32 y = XmlAttr(xml_root, "y", 0);
				Int32 w = XmlAttr(xml_root, "width", 0);
				Int32 h = XmlAttr(xml_root, "height", 0);
				trace(">> rect: w={0} h={1}", w, h);
				//trace("rect: " + w);
				Point3F point = new Point3F(x, y, 0);
				elem = new PolyRectangle(point, w, h);
				InsertEntity(elem, id);
			}
			else if (name == "path") 
			{
				string d = XmlAttr(xml_root, "d", "");
				trace(">> path: d={0}", d);
				string path = normalize (d);
				Regex pm = new Regex("([mMhHvVlLaAqQcC][^mMhHvVlLaAqQcC]+)");
				for (Match match=pm.Match(path); match.Success; match = match.NextMatch())
				{
					if (match.Value.Length==0)
						continue;
					string item = match.Value;
					char op = item[0];
					ArrayList pa = numbers(item);
				}
			}
			else 
			{
				trace(">>" + name);
			}

			//CamBam::ThisApplication::AddLogMessage(0, ">>" + name);
			foreach (XmlNode xml_node in xml_root.ChildNodes) 
			{
				ReadXML(xml_node);
			}
		}
	}
//-------------------------------------------------------
}
