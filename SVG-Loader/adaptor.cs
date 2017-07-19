using System;
using CamBam.Geom;
using CamBam.CAD;
using CamBam.UI;

namespace SVGLoader
{
	public class Adaptor : IOutput
	{
		CamBamUI _ui;

		public Adaptor(CamBamUI ui)
		{
			_ui = ui;
		}
		public override double X (double x) {
			return x;
		}
		public override double Y (double y) {
			return -y;
		}
		public override double Z (double z) {
			return 0;
		}

		public override void draw(Entity elem, string id=null)
		{
			elem.Tag = id;
			_ui.InsertEntity(elem);
		}
		public override void layer(string id)
		{
			if (!CamBamUI.MainUI.ActiveView.CADFile.Layers.ContainsKey (id)) {
				CamBamUI.MainUI.ActiveView.CADFile.CreateLayer (id);
				trace (0, ">> layer created: {0}", id);
			}
			CamBamUI.MainUI.ActiveView.CADFile.SetActiveLayer (id);
		}

		public override void trace(int level, string format, params object[] args)
		{
			CamBam.ThisApplication.AddLogMessage(level, string.Format(format, args));
		}
	}

}

