/* ---------------------------------------------------------------------
 * Graphics Parser for simple graphics primitives.
 * (rect line etc.)
 * 
 * More complex SVG-entity 'path' is processed in 'polylines.cs'
 * ---------------------------------------------------------------------
 */
using System;
using System.Xml;
using System.Collections.Generic;
using CamBam.Geom;
using CamBam.CAD;
using CamBam.UI;


namespace SVGLoader
{
	public class Graphics
	{
		IOutput _output;
		Pathparser _pathparser;
		Stack<ITransform> _transforms;

		//-------------------------------------------------------------------
		public Graphics (IOutput output)
		{
			_output = output;
			_pathparser = new Pathparser (this);
			_transforms = new Stack<ITransform>();
		}
		//-------------------------------------------------------------------
		public void trace(string format, params object[] args)
		{
			_output.trace(4, format, args);
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
		public double[] calc(double x, double y) {
			double[] vector = new double[2] { x, y };
			foreach (ITransform tr in _transforms) {
				if (tr == null)
					continue;
				vector = tr.calc (vector);
			}
			return vector;
		}
		//-------------------------------------------------------------------
		public double[] scale(double x, double y) {
			double[] vector = new double[2] { x, y };
			foreach (ITransform tr in _transforms) 
			{
				if (tr == null)
					continue;
				vector = tr.scale (vector);
			}
			return vector;
		}
		//-------------------------------------------------------------------
		public void draw(Entity elem, string id=null)
		{
			_output.draw (elem, id);
		}
		//-------------------------------------------------------------------
		public void draw(XmlNode xml_root)
		{
			string name = xml_root.Name.ToLower ();

			// skip methadata
			if (name == "metadata") {
				trace (">> metadata: skip");
				return;
			} else if (name == "defs") {
				trace (">> defs: skip");
				return;
			}

			// transformation
			string id = attribute (xml_root, "id", "");
			ITransform tr = null;
			if (name == "svg") {
				string w = attribute (xml_root, "width", "");
				string h = attribute (xml_root, "height", "");
				string box = attribute (xml_root, "viewBox", "");
				double w_mm = Parser.unit_mm (w);
				double h_mm = Parser.unit_mm (h);
				double[] vbox = Parser.numbers (box);
				if (vbox.Length == 4 && !ViewBox.nop(vbox))
					tr = new ViewBox (vbox [0], vbox [1], vbox [2] / w_mm, vbox [3] / h_mm );
				_transforms.Push (tr);

			} else {
				string str = attribute (xml_root, "transform", "");
				tr = Parser.transform (str);
				_transforms.Push (tr);
			}

			Entity elem;
			if (name == "g") {
				_output.layer (id);
				trace (">>g = group");
			} else if (name == "line") {
				double x1 = attribute (xml_root, "x1", 0);
				double y1 = attribute (xml_root, "y1", 0);
				double x2 = attribute (xml_root, "x2", 0);
				double y2 = attribute (xml_root, "y2", 0);

				double[] p1 = calc(x1, y1);
				double[] p2 = calc(x2, y2);
				trace (">> line: a=[{0} {1}] b=[{2},{3}]", x1, y1, x2, y2);
				//trace(">> line: a=[{0} {1}] b=[{2},{3}].".Format(x1, y1, x2, y2));
				elem = new Line (new Point2F (p1[0], p1[1]), new Point2F (p2[0], p2[1]));
				_output.draw (elem, id);
			} else if (name == "circle") {
				double x = attribute (xml_root, "cx", 0);
				double y = attribute (xml_root, "cy", 0);
				double r = attribute (xml_root, "r", 0);
				trace (">> circle: x={0} y={1} r={2}", x, y, r);

				double [] cp = calc (x,y);
				double [] rr = scale (r, r);
				if (rr[0] != rr[1])
					trace ("Not (yet) supportet: scale x<>y");
				elem = new Circle (new Point2F (cp[0], cp[1]), rr[0]);
				_output.draw (elem, id);
			} else if (name == "rect") {
				int x = attribute (xml_root, "x", 0);
				int y = attribute (xml_root, "y", 0);
				int w = attribute (xml_root, "width", 0);
				int h = attribute (xml_root, "height", 0);
				trace (">> rect: w={0} h={1}", w, h);
				//trace("rect: " + w);

				double [] p = calc (x,y);
				double [] d = scale (w,h);
				Point3F point = new Point3F (p[0], p[1], 0);
				elem = new PolyRectangle (point, d[0], d[1]);
				_output.draw (elem, id);
			} else if (name == "path") {
				string d = attribute (xml_root, "d", "");
				//trace (">> path: d={0}", d);
				_pathparser.draw (d, id);
			} else {
				trace (">>" + name);
			}

			//CamBam::ThisApplication::AddLogMessage(0, ">>" + name);
			foreach (XmlNode xml_node in xml_root.ChildNodes) {
				draw (xml_node);
			}
			_transforms.Pop ();
		}
	}
}
