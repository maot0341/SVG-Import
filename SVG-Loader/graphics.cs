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
	//-------------------------------------------------------------------
	class TransformLevel: IDisposable
	{
		Stack<ITransform> _transforms;

		public TransformLevel (Stack<ITransform> stack, ITransform tr)
		{
			_transforms = stack;
			_transforms.Push (tr);
		}

		public void Dispose ()
		{
			_transforms.Pop ();
		}
	}
	//-------------------------------------------------------------------

	public class Graphics
	{
		IOutput _output;
		Pathparser _pathparser;
		Stack<ITransform> _transforms;
		XmlDocument _xml;

		//-------------------------------------------------------------------
		public Graphics (IOutput output)
		{
			_xml = null;
			_output = output;
			_pathparser = new Pathparser (this);
			_transforms = new Stack<ITransform> ();
		}
		//-------------------------------------------------------------------
		public void trace (string format, params object[] args)
		{
			_output.trace (4, format, args);
		}
		//-------------------------------------------------------------------
		static string attribute (XmlNode xml, string name, string value)
		{
			if (xml == null || xml.Attributes == null || xml.Attributes.Count == 0)
				return value;
			XmlNode n = xml.Attributes.GetNamedItem (name);
			if (n == null)
				return value;
			return n.Value;
		}
		//-------------------------------------------------------------------
		static double attribute (XmlNode xml, string name, double value)
		{
			if (xml == null || xml.Attributes == null || xml.Attributes.Count == 0)
				return value;
			XmlNode n = xml.Attributes.GetNamedItem (name);
			if (n == null)
				return value;
			return Convert.ToDouble (n.Value);
		}
		//-------------------------------------------------------------------
		public double[] calc (double x, double y)
		{
			double[] vector = new double[2] { x, y };
			foreach (ITransform tr in _transforms) {
				if (tr == null)
					continue;
				vector = tr.calc (vector);
			}
			return vector;
		}
		//-------------------------------------------------------------------
		public double[] scale (double x, double y)
		{
			double[] vector = new double[2] { x, y };
			foreach (ITransform tr in _transforms) {
				if (tr == null)
					continue;
				vector = tr.scale (vector);
			}
			return vector;
		}
		//-------------------------------------------------------------------
		public void draw (XmlDocument xml)
		{
			_xml = xml;
			XmlElement xml_root = xml.DocumentElement;
			draw (xml_root);
		}
		//-------------------------------------------------------------------
		public void draw (Entity elem, string id = null)
		{
			_output.draw (elem, id);
		}
		//-------------------------------------------------------------------
		void draw (XmlNode xml_root)
		{
			string name = xml_root.Name.ToLower ();

			// skip methadata
			if (name == "metadata") {
				trace ("metadata: skip");
				return;
			} else if (name == "defs") {
				trace ("defs: skip");
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
				if (vbox.Length == 4 && !ViewBox.nop (vbox))
					tr = new Scale (w_mm / vbox [2], h_mm / vbox [3]);
				//tr = new ViewBox (vbox [0], vbox [1], vbox [2] / w_mm, vbox [3] / h_mm);
				_transforms.Push (tr);
			} else {
				string str = attribute (xml_root, "transform", "");
				tr = Parser.transform (str);
				_transforms.Push (tr);
			}

			//TODO: ensure balanced Push(tr)/Pop
			using (var aTR = new TransformLevel (_transforms, null)) {
				trace ("transform stack (level): {0}", _transforms.Count);
			}
			;
			trace ("transform stack: {0}", _transforms.Count);

			// draw element
			Entity elem;
			if (name == "#comment")
				trace ("comment");
			else if (name == "svg") {
				trace ("svg (root element)");
			} else if (name == "g") {
				trace ("g = group");
				_output.layer (id);
			} else if (name == "line") {
				double x1 = attribute (xml_root, "x1", 0);
				double y1 = attribute (xml_root, "y1", 0);
				double x2 = attribute (xml_root, "x2", 0);
				double y2 = attribute (xml_root, "y2", 0);

				double[] p1 = calc (x1, y1);
				double[] p2 = calc (x2, y2);
				trace ("line a=[{0} {1}] b=[{2},{3}]", x1, y1, x2, y2);
				//trace(">> line: a=[{0} {1}] b=[{2},{3}].".Format(x1, y1, x2, y2));
				elem = new Line (new Point2F (p1 [0], p1 [1]), new Point2F (p2 [0], p2 [1]));
				_output.draw (elem, id);
			} else if (name == "circle") {
				double x = attribute (xml_root, "cx", 0);
				double y = attribute (xml_root, "cy", 0);
				double r = attribute (xml_root, "r", 0);
				trace ("circle x={0} y={1} r={2}", x, y, r);

				double[] cp = calc (x, y);
				double[] rr = scale (r, r);
				if (rr [0] != rr [1])
					trace ("Not (yet) supportet: scale x<>y");
				elem = new Circle (new Point2F (cp [0], cp [1]), rr [0]);
				_output.draw (elem, id);
			} else if (name == "rect") {
				double x = attribute (xml_root, "x", 0);
				double y = attribute (xml_root, "y", 0);
				double w = attribute (xml_root, "width", 0);
				double h = attribute (xml_root, "height", 0);
				trace ("rect w={0} h={1}", w, h);

				double[] p = calc (x, y);
				double[] d = scale (w, h);
				Point3F point = new Point3F (p [0], p [1], 0);
				elem = new PolyRectangle (point, d [0], d [1]);
				_output.draw (elem, id);
			} else if (name == "path") {
				string d = attribute (xml_root, "d", "");
				//TODO: trace ("path  d={0}", d);
				_pathparser.draw (d, id);
			} else if (name == "use") {
				double x = attribute (xml_root, "x", 0);
				double y = attribute (xml_root, "y", 0);
				string link = attribute (xml_root, "xlink:href", "");
				trace ("use x={0}  y={1}  xlink:href=[{2}]", x, y, link);
				if (!String.IsNullOrEmpty (link) && link [0] == '#')
					link = link.Substring (1);
				string xpath = String.Format ("//*[@id='{0}']", link);
				XmlElement xml = (XmlElement)_xml.SelectSingleNode (xpath);
				if (xml == null)
					trace ("use [{0}]?!", link);
				else {
					tr = null;
					double[] v = new double[2] { x, y };
					if (!Translate.nop (v))
						tr = new Translate (x, y);
					_transforms.Push (tr);
					draw (xml);
					_transforms.Pop ();
				}
			} else {
				trace ("unknown: [{0}]", name);
			}

			// draw childs
			foreach (XmlNode xml_node in xml_root.ChildNodes)
				draw (xml_node);

			// transformation - switch back
			_transforms.Pop ();
		}
	}
}
