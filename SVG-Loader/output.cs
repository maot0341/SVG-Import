using System;
using CamBam.Geom;
using CamBam.CAD;
using CamBam.UI;

namespace SVGLoader
{
	abstract public class IOutput
	{
		abstract public void draw(Entity elem, string id=null, ITransform tr=null);
		abstract public void layer(string id=null);
		abstract public void trace(int level, string format, params object[] args);
		abstract public double X (double x);
		abstract public double Y (double y);
		abstract public double Z (double z);
	}
}

