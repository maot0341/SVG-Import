/* ---------------------------------------------------------------------
 * Polyline Parser
 * SVG-Path Elements
 * ---------------------------------------------------------------------
 */
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
		Graphics _parent;
		protected Point2F _cursor;
		protected Polyline _poly;
		protected string _id;

		//-------------------------------------------------------------------
		public Pathparser(Graphics parent)
		{
			_parent = parent;
			_cursor = new Point2F (0, 0);
		}
		//-------------------------------------------------------------------
		public static string normalize(string str) {
			string result = "";
			foreach (char c in str.ToCharArray())
				result += (char.IsWhiteSpace(c) ? ' ' : c);
			return result;
		}
		//-------------------------------------------------------------------
		void trace(string format, params object[] args)
		{
			_parent.trace(format, args);
		}
		//-------------------------------------------------------------------
		void flush()
		{
			if (_poly != null && _poly.Points.Count>0) 
				_parent.draw(_poly, _id);
			_poly = new Polyline();
		}
		//-------------------------------------------------------------------
		Point3F cursor()
		{
			Point3F p = new Point3F();
			double[] v = _parent.calc (_cursor.X, _cursor.Y);
			p.X = v [0];
			p.Y = v [1];
			p.Z = 0;
			return p;
		}
		//-------------------------------------------------------------------
		void draw(char op, double[] param)
		{
			int i;
			_parent.trace ("path " + op + " ... " + string.Join (",", param));
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
		public void draw(string str, string id=null)
		{
			//TODO: exception on param# mismatch
			_id = id;
			str = normalize (str);
			Regex re = new Regex ("([a-zA-Z][^a-zA-Z]*)"); 
			//Regex re = new Regex ("([mMhHvVlLaAqQcCzZ][^mMhHvVlLaAqQcCzZ]*)");
			for (Match match=re.Match(str); match.Success; match = match.NextMatch())
			{
				if (match.Value.Length==0)
					continue;
				string item = match.Value;
				char op = item[0];
				double[] pa = Parser.numbers(item);
				draw(op, pa);
			}
			flush();
		}
		//-------------------------------------------------------------------
	}
	//-------------------------------------------------------------------
}

