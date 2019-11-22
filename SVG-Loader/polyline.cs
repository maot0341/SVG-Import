/* ---------------------------------------------------------------------
 * Polyline Parser
 * SVG-Path Elements
 * ---------------------------------------------------------------------
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;
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
		protected string _repr;
		protected Bezier _bezier;

		//-------------------------------------------------------------------
		public Pathparser(Graphics parent)
		{
			_parent = parent;
			_cursor = new Point2F(0, 0);
			_bezier = new Bezier();
		}
		//-------------------------------------------------------------------
		public void setNodeNumber(int n)
		{
			Debug.Assert (n >= 10);
			_bezier._NodeNumber = n;
		}
		//-------------------------------------------------------------------
		public int getNodeNumber()
		{
			return _bezier._NodeNumber;
		}
		//-------------------------------------------------------------------
		public override string ToString()
		{
			return base.ToString()
			+ String.Format(" [{0}]:  path=[{1}]  cursor=[{2} {3}]"
			, _id, _repr, _cursor.X, _cursor.Y);
		}

		//-------------------------------------------------------------------
		public static string normalize(string str)
		{
			string result = "";
			foreach(char c in str.ToCharArray())
				result += (char.IsWhiteSpace(c) ? ' ' : c);
			return result;
		}
		//-------------------------------------------------------------------
		void trace(string format, params object[] args)
		{
			_parent.trace(format, args);
		}
		//-------------------------------------------------------------------
		static string ToString(double[] args)
		{
			string str = "";
			foreach(double x in args)
				str += String.Format(" {0:0.00}", x);
			return str.Trim();
		}
		//-------------------------------------------------------------------
		void flush()
		{
			if(_poly != null && _poly.Points.Count > 0)
				_parent.draw(_poly, _id);
			_poly = new Polyline();
			_repr = "";
		}
		//-------------------------------------------------------------------
		Point3F midpoint(Point3F p1, Point3F p2, double t = 0.5)
		{
			Point3F p0;
			p0.X = (1.0 - t) * p1.X + t * p2.X;
			p0.Y = (1.0 - t) * p1.Y + t * p2.Y;
			p0.Z = (1.0 - t) * p1.Z + t * p2.Z;
			return p0;
		}
		//-------------------------------------------------------------------
		void bezier_01(Point3F[] p)
		{
			int i, k, n = p.Length;
			for(k = n - 1; k > 0; k--) {
				Point3F p0 = p [0];
				trace("bezier: " + p0.ToString());
				_poly.Add(p0);
				for(i = 0; i < k; i++) {
					p [i] = midpoint(p [i], p [i + 1]);
				}
			}
			for(i = 0; i < n; i++) {
				Point3F p0 = p [i];
				trace("bezier: " + p0.ToString());
				_poly.Add(p0);
				//_poly.Add(p[i]);
			}
		}
		//-------------------------------------------------------------------
		void add(Point3F p)
		{
			Point3F last = _poly.LastPoint;
			if(last == p)
				trace("skip: " + p.ToString());
			else
				_poly.Add(p);
		}
		//-------------------------------------------------------------------
		void bezier(Point3F[] polygon)
		{
			int i, n = getNodeNumber();
			double t;
			for(i = 0; i <= n; i++) {
				t = (double)i / (double)n;
				Point3F p = _bezier.point(polygon, t);
				add(p);
			}
				
		}
		//-------------------------------------------------------------------
		Point3F point(double x, double y, double z = 0)
		{
			Point3F p = new Point3F();
			double[] v = _parent.calc(x, y);
			p.X = v [0];
			p.Y = v [1];
			p.Z = z;
			return p;
		}
		//-------------------------------------------------------------------
		Point3F line()
		{
			Point3F p = new Point3F();
			double[] v = _parent.calc(_cursor.X, _cursor.Y);
			p.X = v [0];
			p.Y = v [1];
			p.Z = 0;
			return p;
		}
		//-------------------------------------------------------------------
		Arc2F arc(double r, bool large, bool sweep)
		{
			Point3F aLast = _poly.LastPoint;
			double[] rr = _parent.scale(r, r);
			double[] p1 = new double[2] { aLast.X, aLast.Y };
			double[] p2 = _parent.calc(_cursor.X, _cursor.Y);
			bool left = (rr[0] == rr[1]) ? (sweep != large) : (sweep == large);
			double[] c1 = Geometry.circle_center(p1, p2, rr [0], left);
			double[] c2 = Geometry.circle_center(p1, p2, rr [0], !left);
			RotationDirection dir = (sweep == (rr[0] == rr[1])) ? RotationDirection.CCW : RotationDirection.CW;
			_parent.trace("arc    ... c1=[{0}]  c2=[{1}] p1=[{2}]  p2=[{3}] d={4}"
				, ToString(c1), ToString(c2)
				, ToString(p1), ToString(p2)
				, dir);
			//RotationDirection dir = (p1 [0] < p2 [0] && sweep) 
			//	? RotationDirection.CCW : RotationDirection.CW;
			Point2F aCC = new Point2F(c1 [0], c1 [1]);
			Point2F aP1 = new Point2F(p1 [0], p1 [1]);
			Point2F aP2 = new Point2F(p2 [0], p2 [1]);
			Arc2F arc = new Arc2F(aCC, aP1, aP2, dir);
			return arc;
		}
		//-------------------------------------------------------------------
		void draw(char op, double[] param)
		{
			int i;
			_parent.trace("path " + op + " ... " + string.Join(" ", param));
			switch(op) {
			case 'm':
				flush();
				for(i = 0; i < param.Length; i += 2) {
					_cursor.X += param [i + 0];
					_cursor.Y += param [i + 1];
					add(line());
				}
				break;
			case 'M':
				flush();
				for(i = 0; i < param.Length; i += 2) {
					_cursor.X = param [i + 0];
					_cursor.Y = param [i + 1];
					add(line());
				}
				break;
			case 'H':
				for(i = 0; i < param.Length; i++) {
					_cursor.X = param [i];
					add(line());
				}
				break;
			case 'h':
				for(i = 0; i < param.Length; i++) {
					_cursor.X += param [i];
					add(line());
				}
				break;
			case 'V':
				for(i = 0; i < param.Length; i++) {
					_cursor.Y = param [i];
					add(line());
				}
				break;
			case 'v':
				for(i = 0; i < param.Length; i++) {
					_cursor.Y += param [i];
					add(line());
				}
				break;
			case 'L':
				for(i = 0; i < param.Length; i += 2) {
					_cursor.X = param [i + 0];
					_cursor.Y = param [i + 1];
					add(line());
				}
				break;
			case 'l':
				for(i = 0; i < param.Length; i += 2) {
					_cursor.X += param [i + 0];
					_cursor.Y += param [i + 1];
					add(line());
				}
				break;
			case 'A':
				for(i = 0; i < param.Length; i += 7) {
					double r = param [i + 0];
					if(param [i + 0] != param [i + 1])
						trace("not supported: r1 <> r2");
					_cursor.X = param [i + 5];
					_cursor.Y = param [i + 6];
					bool large = (param [i + 3] > 0);
					bool sweep = (param [i + 4] > 0);
					_poly.Add(arc(r, large, sweep), 0.1);
				}
				break;
			case 'a':
				for(i = 0; i < param.Length; i += 7) {
					double r = param [i + 0];
					if(param [i + 0] != param [i + 1])
						trace("not supported: r1 <> r2");
					_cursor.X += param [i + 5];
					_cursor.Y += param [i + 6];
					bool large = (param [i + 3] > 0);
					bool sweep = (param [i + 4] > 0);
					// _cursor.X += param [i + 5];
					// _cursor.Y += param [i + 6];
					_poly.Add(arc(r, large, sweep), 0.1);
				}
				break;
			case 'C':
				for(i = 0; i < param.Length; i += 6) {
					Point3F[] polygon = new Point3F[4];
					polygon [0] = point(_cursor.X, _cursor.Y);
					polygon [1] = point(param [i + 0], param [i + 1]);
					polygon [2] = point(param [i + 2], param [i + 3]);
					polygon [3] = point(param [i + 4], param [i + 5]);
					bezier(polygon);
					_cursor.X = param [i + 4];
					_cursor.Y = param [i + 5];
					//_poly.Add(line());
				}
				break;
			case 'c':
				for(i = 0; i < param.Length; i += 6) {
					Point3F[] polygon = new Point3F[4];
					polygon [0] = point(_cursor.X, _cursor.Y);
					polygon [1] = point(_cursor.X + param [i + 0], _cursor.Y + param [i + 1]);
					polygon [2] = point(_cursor.X + param [i + 2], _cursor.Y + param [i + 3]);
					polygon [3] = point(_cursor.X + param [i + 4], _cursor.Y + param [i + 5]);
					bezier(polygon);
					_cursor.X += param [i + 4];
					_cursor.Y += param [i + 5];
					//_poly.Add(line());
				}
				break;
			case 'Q':
				for(i = 0; i < param.Length; i += 4) {
					Point3F[] polygon = new Point3F[3];
					polygon [0] = point(_cursor.X, _cursor.Y);
					polygon [1] = point(param [i + 0], param [i + 1]);
					polygon [2] = point(param [i + 2], param [i + 3]);
					bezier(polygon);
					_cursor.X = param [i + 2];
					_cursor.Y = param [i + 3];
					//_poly.Add(line());
				}
				break;
			case 'q':
				for(i = 0; i < param.Length; i += 4) {
					Point3F[] polygon = new Point3F[3];
					polygon [0] = point(_cursor.X, _cursor.Y);
					polygon [1] = point(_cursor.X + param [i + 0], _cursor.Y + param [i + 1]);
					polygon [2] = point(_cursor.X + param [i + 2], _cursor.Y + param [i + 3]);
					bezier(polygon);
					_cursor.X += param [i + 2];
					_cursor.Y += param [i + 3];
					//_poly.Add(line());
				}
				break;
			case 'z':
			case 'Z':
				add(_poly.FirstPoint);
				_poly.Closed = true;
				flush();
				break;
			}
		}
		//-------------------------------------------------------------------
		public void draw(string str, string id = null)
		{
			//TODO: exception on param# mismatch
			_id = id;
			_cursor.X = _cursor.Y = 0;
			str = normalize(str);
			Regex re = new Regex("([a-zA-Z][^a-zA-Z]*)"); 
			//Regex re = new Regex("([mMhHvVlLaAqQcCzZ][^mMhHvVlLaAqQcCzZ]*)");
			for(Match match = re.Match(str); match.Success; match = match.NextMatch()) {
				if(match.Value.Length == 0)
					continue;
				string item = match.Value;
				char op = item [0];
				double[] pa = Parser.numbers(item);
				draw(op, pa);
				_repr += op;
			}
			flush();
		}
		//-------------------------------------------------------------------
	}
	//-------------------------------------------------------------------
}

