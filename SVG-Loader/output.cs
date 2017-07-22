/* ---------------------------------------------------------------------
 * Abstract Interface (data sink)
 * 
 * Output/Drawing of graphical primitives (rect, circl etc.)
 * Usually it's the CamBam program.
 * ---------------------------------------------------------------------
 */
using System;
using CamBam.Geom;
using CamBam.CAD;
using CamBam.UI;

namespace SVGLoader
{
	abstract public class IOutput
	{
		abstract public void draw(Entity elem, string id=null);
		abstract public void layer(string id=null);
		abstract public void trace(int level, string format, params object[] args);
	}
}

