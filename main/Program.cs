using System;
using System.Xml;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CamBam.Geom;
using CamBam.CAD;
using CamBam.UI;
using SVGLoader;

namespace main
{
	//-------------------------------------------------------------------
	public class Stdout : SVGLoader.IOutput
	{
		public override void layer(string id)
		{
			Console.WriteLine ("new layer: " + id);
		}
		public override void draw(Entity elem, string id=null)
		{
			if (elem == null) {
				Console.WriteLine ("<null>");
				return;
			}
			elem.Tag = id;
			Console.WriteLine (elem.ToString ());
		}
		public override void trace(int level, string format, params object[] args)
		{
			Console.WriteLine (string.Format(format, args));
		}
	}
	//-------------------------------------------------------------------
	class MainClass
	{
		public static void Main (string[] args)
		{
			double[] m;
			double[] x0 = new double[2] {  0, 0 };
			double[] x1 = new double[2] { -1, 0 };
			double[] x2 = new double[2] { +1, 0 };
			double[] x3 = new double[2] {  0,-1 };
			double[] x4 = new double[2] {  0,+1 };
			double[] x5 = new double[2] {  10, 5 };
			m = SVGLoader.Geometry.circle_center(x1, x2, 1);
			m = SVGLoader.Geometry.circle_center(x3, x4, 1);
			m = SVGLoader.Geometry.circle_center(x1, x4, 1);
			m = SVGLoader.Geometry.circle_center(x0, x5, 10, false);

			string str = "scale ( -.1) ";
			//string s_re = "scale";
			ITransform tr = SVGLoader.Parser.transform(str);

			SVGLoader.Transform t1 = new SVGLoader.Transform (Math.PI);
			double[] p0 = new double[3] { 1, 2, 3 };
			double[] p1 = t1.calc (p0);
			double[] p2 = t1.calc (1,2,3);
			SVGLoader.ITransform t2 = new SVGLoader.Translate (2,2);
			t1.rotate (0);
			t1.scale ();
			t1.scale (-1,1,3);
			p1 = t2.calc (p0);
			Stack<ITransform> t3 = new Stack<ITransform> ();
			t3.Push (t2);
			t3.Push (t1);
			p1 = (double[]) p0.Clone();
			foreach (ITransform t in t3)
				p1 = t.calc (p1);
			MultiTransform t4 = new MultiTransform (t3);
			p1 = t4.calc (p0);
			Queue<ITransform> queue = new Queue<ITransform> ();
			queue.Enqueue (t1);
			MultiTransform t5 = new MultiTransform (queue);
			p1 = t5.calc (p0);


			Stdout stdout = new Stdout ();
			SVGLoader.Graphics graphics = new SVGLoader.Graphics (stdout);

			string path = "/home/jvater/c#-prj/data/test.svg";
			//string path = "/home/jvater/c#-prj/data/Steg.svg";
			Console.WriteLine ("Hello World!");
			//CamBam.ThisApplication.MsgBox("SVG: " + path);
			XmlDocument xml = new XmlDocument(); 
			xml.Load(path);
			//ThisApplication.AddLogMessage(0, "SVG [{0}] loaded".Format(path));
			stdout.trace (0, "SVG loaded:" + path);
			XmlElement xml_root = xml.DocumentElement;
			graphics.draw (xml_root);
		}
	}
	//-------------------------------------------------------------------
}
