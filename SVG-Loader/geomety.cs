/* ---------------------------------------------------------------------
 * Heavy geometry maths here ...
 * 
 * ---------------------------------------------------------------------
 */
using System;

namespace SVGLoader
{
	public class Geometry
	{
		//-------------------------------------------------------------------
		// euclidic distance (p-2 norm)
		public static double distance(double [] x, double[] y) {
			double d, sum=0;
			int n = x.Length;
			for (int i=0; i<n; i++){
				d = x[i] - y[i];
				sum += d * d;
			}
			return Math.Sqrt(sum);
		}
		//-------------------------------------------------------------------
		public static double[] midpoint(double [] x, double[] y) {
			int n = x.Length;
			double[] m = new double[n];
			for (int i=0; i<n; i++)
				m[i] = (x[i] + y[i]) / 2.0;
			return m;
		}
		//-------------------------------------------------------------------
		// connect two point with an arc of radius <r>
		// ps    ... start point
		// pe    ... end point
		// r     ... circle radius
		// right ... solution selecor (left- or right-hand side)
		// return: circle center point
		public static double[] circle_center(double [] ps, double[] pe, double r, bool right=true) {
			double[] m = midpoint(ps, pe);
			double dx = pe [0] - ps [0];
			double dy = pe [1] - ps [1];
			double s = distance (ps, pe) / 2.0;
			double h = Math.Sqrt (r * r - s * s);
			double h2 = r * r - s * s;
			if (h2 == 0) return m;
			if (Math.Abs (dx) > Math.Abs (dy)) {
				double a = dy / dx;
				double yh = Math.Sqrt (h2/(a*a+1));
				double y1 = right ? +yh : -yh;
				double x1 = y1 * a;
				m [0] += x1;
				m [1] -= y1;
			} else {
				double a = dx / dy;
				double xh = Math.Sqrt (h2/(a*a+1));
				double x1 = right ? +xh : -xh;
				double y1 = x1 * a;
				m [0] += x1;
				m [1] -= y1;
			}
			// TODO: remove debug-test
			if (true) {
				double d1 = distance(ps, m);
				double d2 = distance(pe, m);
				double e1 = r - d1;
				double e2 = r - d2;
				//ok if: e1 == e2 == 0
			}
			return m;
		}
	}
}
