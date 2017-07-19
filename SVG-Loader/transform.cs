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
		abstract public double [] tr (double [] p);

		public double[] tr(double x, double y, double z) 
		{
			double[] p = new double[3] {x, y, z};
			return tr(p);
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
		public override double[] tr(double [] p)
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
	}
	//-------------------------------------------------------
	public class Translate : ITransform
	{
		public double [] _vector;

		public Translate (double x=0, double y=0, double z=0)
		{
			_vector = new double[] { x, y, z };
		}
		public void set (double x, double y, double z=0)
		{
			_vector [0] = x;
			_vector [1] = y;
			_vector [2] = z;
		}
		public override double[] tr(double [] p)
		{
			p = (double[])p.Clone ();
			p [0] += _vector [0];
			p [1] += _vector [1];
			p [2] += _vector [2];
			return p;
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
		public void set (double x, double y, double z=0)
		{
			_vector [0] = x;
			_vector [1] = y;
			_vector [2] = z;
		}
		public override double[] tr(double [] p)
		{
			p = (double[])p.Clone ();
			p [0] *= _vector [0];
			p [1] *= _vector [1];
			p [2] *= _vector [2];
			return p;
		}
	}
	//-------------------------------------------------------
	public class MirrorX : ITransform
	{
		public override double[] tr(double [] p)
		{
			p = (double[])p.Clone ();
			p [0] = -p [0];
			return p;
		}
	}
	//-------------------------------------------------------
	public class MirrorY : ITransform
	{
		public override double[] tr(double [] p)
		{
			p = (double[])p.Clone ();
			p [1] = -p [1];
			return p;
		}
	}
	//-------------------------------------------------------
}
//-------------------------------------------------------

