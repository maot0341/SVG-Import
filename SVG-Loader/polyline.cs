using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CamBam.Geom;
using CamBam.CAD;
using CamBam.UI;

namespace SVGLoader
{
	public class Pathparser
	{
		protected IOutput _output;
		protected Point2F _cursor;
		protected Polyline _poly;
		protected string _id;
		protected ITransform _tr;

		static Regex _regex_group;
		static Regex _regex_param;
		//-------------------------------------------------------------------
		public Pathparser(IOutput output)
		{
			_output = output;
			_cursor = new Point2F (0, 0);
		}
		//-------------------------------------------------------------------
		public static Regex regex_group()
		{
			if (_regex_group == null)
				_regex_group = new Regex ("([mMhHvVlLaAqQcCzZ][^mMhHvVlLaAqQcCzZ]*)");
			return _regex_group;
		}
		//-------------------------------------------------------------------
		public static Regex regex_param()
		{
			if (_regex_param == null)
				_regex_param = new Regex("([+-]?\\d*[.]?\\d+)");
			return _regex_param;
		}
		//-------------------------------------------------------------------
		string normalize(string str) {
			string result = "";
			foreach (char c in str.ToCharArray())
				result += (char.IsWhiteSpace(c) ? ' ' : c);
			return result;
		}
		//-------------------------------------------------------------------
		double[] numbers(string str) 
		{
			List<double> result = new List<double>();
			Regex re = new Regex("([+-]?\\d*[.]?\\d+)");
			//Regex re = regex_param();
			for (Match match=re.Match(str); match.Success; match = match.NextMatch())
			{
				if (match.Value.Length<1)
					continue;
				double val = double.Parse(match.Value, System.Globalization.CultureInfo.InvariantCulture);;
				//Double val = Convert.ToDouble(match.Value);
				//_output.trace (4, "val={0}", val);
				result.Add(val);
			}
			return result.ToArray();
		}
		//-------------------------------------------------------------------
		void trace(string format, params object[] args)
		{
			_output.trace(4, format, args);
		}
		//-------------------------------------------------------------------
		void flush()
		{
			if (_poly != null && _poly.Points.Count>0) 
				_output.draw(_poly, _id);
			_poly = new Polyline();
		}
		//-------------------------------------------------------------------
		double X(double x, double y, double z)
		{
			return x;
		}
		//-------------------------------------------------------------------
		double Y(double x, double y, double z)
		{
			return y;
		}
		//-------------------------------------------------------------------
		double Z(double x, double y, double z)
		{
			return z;
		}
		//-------------------------------------------------------------------
		Point3F cursor()
		{
			Point3F p = new Point3F();
			if (_tr == null) {
				p.X = _cursor.X;
				p.Y = _cursor.Y;
				p.Z = 0;
				return p;
			}				
			double[] v = new double[3] { _cursor.X, _cursor.Y, 0 };
			v = _tr.calc (v);
			p.X = v [0];
			p.Y = v [1];
			p.Z = v [2];
			return p;
		}
		//-------------------------------------------------------------------
		void draw(char op, double[] param)
		{
			int i;
			_output.trace (4, "path " + op + " ... " + string.Join (",", param));
			switch (op) {
			case 'M':
			case 'm':
				flush();
				for (i = 0; i < param.Length; i+=2) {
					_cursor.X = param[i+0];
					_cursor.Y = param[i+1];
					//_poly.Add(new Point3F (_cursor.X, _cursor.Y, 0));
					_poly.Add(cursor());
				}
				break;
			case 'H':
				for (i = 0; i < param.Length; i++) {
					_cursor.X = param[i];
					_poly.Add(cursor());
				}
				break;
			case 'h':
				for (i = 0; i < param.Length; i++) {
					_cursor.X += param[i];
					_poly.Add(cursor());
				}
				break;
			case 'V':
				for (i = 0; i < param.Length; i++) {
					_cursor.Y = param[i];
					_poly.Add(cursor());
				}
				break;
			case 'v':
				for (i = 0; i < param.Length; i++) {
					_poly.Add(cursor());
				}
				break;
			case 'L':
				for (i = 0; i < param.Length; i+=2) {
					_cursor.X = param[i+0];
					_cursor.Y = param[i+1];
					_poly.Add(cursor());
				}
				break;
			case 'l':
				for (i = 0; i < param.Length; i+=2) {
					_cursor.X += param[i+0];
					_cursor.Y += param[i+1];
					_poly.Add(cursor());
				}
				break;
			case 'A':
				for (i = 0; i < param.Length; i+=7) {
					_cursor.X = param[i+5];
					_cursor.Y = param[i+6];
					_poly.Add(cursor());
				}
				break;
			case 'a':
				for (i = 0; i < param.Length; i+=7) {
					_cursor.X += param[i+5];
					_cursor.Y += param[i+6];
					_poly.Add(cursor());
				}
				break;
			case 'C':
				for (i = 0; i < param.Length; i+=6) {
					_cursor.X = param[i+4];
					_cursor.Y = param[i+5];
					_poly.Add(cursor());
				}
				break;
			case 'c':
				for (i = 0; i < param.Length; i+=6) {
					_cursor.X += param[i+4];
					_cursor.Y += param[i+5];
					_poly.Add(cursor());
				}
				break;
			case 'Q':
				for (i = 0; i < param.Length; i+=4) {
					_cursor.X = param[i+2];
					_cursor.Y = param[i+3];
					_poly.Add(cursor());
				}
				break;
			case 'q':
				for (i = 0; i < param.Length; i+=4) {
					_cursor.X += param[i+2];
					_cursor.Y += param[i+3];
					_poly.Add(cursor());
				}
				break;
			case 'z':
			case 'Z':
				if (_poly.LastPoint != _poly.FirstPoint) {
					Point3F p = new Point3F (_poly.FirstPoint.X, _poly.FirstPoint.Y, 0);
					_poly.Add (p);
				}
				_poly.Closed = true;
				trace ("path Z: "+ op);
				break;
			}
		}
		//-------------------------------------------------------------------
		public void draw(string str, string id=null, ITransform tr=null)
		{
			_id = id;
			_tr = tr;
			str = normalize (str);
			//Regex re = regex_group();
			Regex re = new Regex ("([a-zA-Z][^a-zA-Z]*)"); 
			for (Match match=re.Match(str); match.Success; match = match.NextMatch())
			{
				if (match.Value.Length==0)
					continue;
				string item = match.Value;
				char op = item[0];
				double[] pa = numbers(item);
				draw(op, pa);
			}
			flush();
		}
	}
}

