using System;
using System.Xml;
using CamBam.Geom;
using CamBam.CAD;
using CamBam.UI;

namespace SVGLoader
{
	public class Graphics
	{
		IOutput _out;
		Pathparser _polyline;

		//-------------------------------------------------------------------
		public Graphics (IOutput output)
		{
			_out = output;
			_polyline = new Pathparser (_out);
		}
		//-------------------------------------------------------------------
		void trace(string format, params object[] args)
		{
			_out.trace(4, format, args);
		}
		//-------------------------------------------------------------------
		static string attribute(XmlNode xml, string name, string value) 
		{
			if (xml==null || xml.Attributes==null || xml.Attributes.Count==0)
				return value;
			XmlNode n = xml.Attributes.GetNamedItem(name);
			if (n==null)
				return value;
			return n.Value;
		}
		//-------------------------------------------------------------------
		static int attribute(XmlNode xml, string name, int value) 
		{
			if (xml==null || xml.Attributes==null || xml.Attributes.Count==0)
				return value;
			XmlNode n = xml.Attributes.GetNamedItem(name);
			if (n==null)
				return value;
			return Convert.ToInt32(n.Value);
		}
		//-------------------------------------------------------------------
		public void draw(XmlNode xml_root)
		{
			string name = xml_root.Name.ToLower ();
			string id = attribute (xml_root, "id", "");
			Entity elem;
			if (name == "g") {
				_out.layer (id);
				trace (">>g = group");
			} else if (name == "line") {
				int x1 = +attribute (xml_root, "x1", 0);
				int y1 = -attribute (xml_root, "y1", 0);
				int x2 = +attribute (xml_root, "x2", 0);
				int y2 = -attribute (xml_root, "y2", 0);
				trace (">> line: a=[{0} {1}] b=[{2},{3}]", x1, y1, x2, y2);
				//trace(">> line: a=[{0} {1}] b=[{2},{3}].".Format(x1, y1, x2, y2));
				elem = new Line (new Point2F (x1, y1), new Point2F (x2, y2));
				_out.draw (elem, id);
			} else if (name == "circle") {
				int x = +attribute (xml_root, "cx", 0);
				int y = -attribute (xml_root, "cy", 0);
				int r = +attribute (xml_root, "r", 0);
				trace (">> circle: x={0} y={1} r={2}", x, y, r);
				elem = new Circle (new Point2F (x, y), r);
				_out.draw (elem, id);
			} else if (name == "rect") {
				int x = +attribute (xml_root, "x", 0);
				int y = -attribute (xml_root, "y", 0);
				int w = +attribute (xml_root, "width", 0);
				int h = -attribute (xml_root, "height", 0);
				trace (">> rect: w={0} h={1}", w, h);
				//trace("rect: " + w);
				Point3F point = new Point3F (x, y, 0);
				elem = new PolyRectangle (point, w, h);
				_out.draw (elem, id);
			} else if (name == "path") {
				string d = attribute (xml_root, "d", "");
				//trace (">> path: d={0}", d);
				_polyline.draw (d, id);
			} else if (name == "metadata") {
				trace (">> metadata: skip");
				return;
			} else if (name == "defs") {
				trace (">> defs: skip");
				return;
			} else {
				trace (">>" + name);
			}

			//CamBam::ThisApplication::AddLogMessage(0, ">>" + name);
			foreach (XmlNode xml_node in xml_root.ChildNodes) {
				draw (xml_node);
			}
		}
	}
}
