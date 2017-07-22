/* ---------------------------------------------------------------------
 * Transformations
 * Roration,Translate etc.
 * 
 * Ther are to transformation calculation types:
 * - 'scale' for relative messures like 'with' and 'heigth' 
 * - 'calc' for points (absolute coordinates)
 * Beside of common interface methods,
 * the static methode 'nop(vector)' checks for 'nothig-to-do'
 * before creating a transformation - for simplcity reasons.
 * ---------------------------------------------------------------------
 */
using System;
using System.Collections.Generic;
using CamBam.Geom;
using CamBam.CAD;
using CamBam.UI;

namespace SVGLoader
{
	//-------------------------------------------------------
	abstract public class ITransform
	{
		abstract public double [] calc (double [] p);
		abstract public double [] scale (double [] p);

		public double[] calc(double x, double y, double z) 
		{
			double[] p = new double[3] {x, y, z};
			return calc(p);
		}
	}
	//-------------------------------------------------------
	public class Transform : ITransform
	{
		public double [,] _matrix;

		public Transform()
		{
			_matrix = new double [3,3];
			_matrix [0,0] = 1;
			_matrix [1,1] = 1;
			_matrix [2,2] = 1;
		}
		public Transform(double angle)
		{
			_matrix = new double [3,4];
			_matrix [2,2] = 1;
			rotate (angle);
		}
		public void rotate(double angle)
		{
			double cos=1, sin=0;
			if (angle != 0) {
				cos = Math.Cos (angle);
				sin = Math.Sin (angle);
			}
			_matrix [0, 0] = cos;
			_matrix [0, 1] = sin;
			_matrix [1, 0] = sin;
			_matrix [1, 1] = cos;
		}
		public void scale(double x=1, double y=1, double z=1)
		{
			Array.Clear (_matrix, 0, _matrix.Length);
			_matrix [0, 0] = x;
			_matrix [1, 1] = y;
			_matrix [2, 2] = z;
		}
		public override double[] calc(double [] p)
		{
			int i, j, n=p.Length;
			double[] p1 = new double[3];
			for (i = 0; i < n; i++) {
				for (j = 0; j < n; j++) {
					p1 [i] += _matrix [i, j] * p [j];
				}
			}
			return p1;
		}
		public override double[] scale(double [] p)
		{
			return p;
		}
	}
	//-------------------------------------------------------
	public class Translate : ITransform
	{
		public double [] _vector;

		public Translate (double x=0, double y=0, double z=0)
		{
			_vector = new double[] { x, y, z };
		}
		public Translate (double[] v)
		{
			_vector = new double[3];
			for (int i=0; i<v.Length; i++)
				_vector[i] = v[i];
		}
		public override double[] calc(double [] p)
		{
			p = (double[])p.Clone ();
			for (int i=0; i<p.Length; i++)
				p [i] += _vector [i];
			return p;
		}
		public override double[] scale(double [] p)
		{
			return p;
		}
		public static bool nop(double [] p)
		{
			for(int i=0; i<p.Length; i++)
				if (p[i] != 0) return false;
			return true;
		}
	}
	//-------------------------------------------------------
	public class Scale : ITransform
	{
		public double [] _vector;

		public Scale (double x=0, double y=0, double z=0)
		{
			_vector = new double[] { x, y, z };
		}
		public Scale (double [] v)
		{
			_vector = new double[3];
			for (int i=0; i<v.Length; i++)
				_vector[i] = v[i];
		}
		public void set (double x, double y, double z=0)
		{
			_vector [0] = x;
			_vector [1] = y;
			_vector [2] = z;
		}
		public override double[] calc(double [] p)
		{
			p = (double[])p.Clone ();
			for (int i=0; i < p.Length; i++)
				p [i] *= _vector [i];
			return p;
		}
		public override double[] scale(double [] p)
		{
			return calc(p);
		}
		public static bool nop(double [] p)
		{
			for(int i=0; i<p.Length; i++)
				if (p[i]!= 1) return false;
			return true;
		}
	}
	//-------------------------------------------------------
	public class ViewBox : ITransform
	{
		public double [] _scale;
		public double [] _origin;

		public ViewBox(double x, double y, double xres, double yres)
		{
			_origin = new double [2] {x, y};
			_scale = new double [2] {xres, yres};
		}
		public override double[] calc(double [] p)
		{
			double[] p1 = (double[])p.Clone ();
			p1 [0] -= _origin [0];
			p1 [1] -= _origin [1];
			p1 [0] /= _scale [0];
			p1 [1] /= _scale [1];
			return p1;
		}
		public override double[] scale(double [] p)
		{
			double[] p1 = (double[])p.Clone ();
			p1 [0] /= _scale [0];
			p1 [1] /= _scale [1];
			return p1;
		}
		public static bool nop(double [] v)
		{
			if (v.Length < 4)
				return true;
			if (v [0] != 0 || v [1] != 0)
				return false;
			if (v [2] == 1 && v [3] == 1)
				return false;
			return true;
		}
	}
	//-------------------------------------------------------
	public class MultiTransform : ITransform
	{
		public Stack<ITransform> _items; 

		public MultiTransform() 
		{
			_items = new Stack<ITransform> ();
		}
		public MultiTransform(IEnumerable<ITransform> items) 
		{
			_items = new Stack<ITransform> ();
			foreach(ITransform t in items)
				_items.Push(t);
		}
		public override double[] calc(double[] p) 
		{
			foreach (ITransform t in _items)
				p = t.calc (p);
			return p;
		}
		public override double[] scale(double[] p) 
		{
			foreach (ITransform t in _items)
				p = t.scale (p);
			return p;
		}
	}
	//-------------------------------------------------------
}
//-------------------------------------------------------

