using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace SVGLoader
{
	//-------------------------------------------------------
	public class Parser
	{
		static Regex _re_number;
		static Regex _re_scale;
		//-------------------------------------------------------------------
		public static double[] numbers(string str){
			if (_re_number == null)
				_re_number = new Regex("([+-]?\\d*[.]?\\d+)");
			Regex re = _re_number;
			List<double> result = new List<double>();
			for (Match match=re.Match(str); match.Success; match = match.NextMatch())
			{
				if (match.Value.Length<1)
					continue;
				double val = double.Parse(match.Value, System.Globalization.CultureInfo.InvariantCulture);
				result.Add(val);
			}
			return result.ToArray();
		}
		//-------------------------------------------------------------------
		public static double unit_mm(string str, double nvl=0) {
			str = str.Trim ();
			double[] vec = numbers (str);
			if (vec.Length < 1)
				return nvl;
			if (str.EndsWith ("mm"))
				return vec[0];
			if (str.EndsWith ("cm"))
				return vec[0] * 10;
			if (str.EndsWith ("dm"))
				return vec[0] * 100;
			if (str.EndsWith ("m"))
				return vec[0] * 1000;
			return vec[0];
		}
		//-------------------------------------------------------------------
		public static ITransform transform(string str) {
			str = str.Trim ();
			if (str == null || str == "")
				return null;
			if (_re_scale == null)
				_re_scale = new Regex (@"(translate|scale)\s*\([^a-zA-Z]+\)");
				//_re_scale = new Regex ("((translate|scale)\\s*\\(.*\\))");
			MultiTransform tr = new MultiTransform();
			str = str.Replace(",", " ");
			Regex re = _re_scale;
			for (Match match=re.Match(str); match.Success; match = match.NextMatch())
			{
				if (match.Value.Length==0)
					continue;
				string item = match.Value;
				double[] vec = numbers (item);
				if (item.StartsWith("scale") && !Scale.nop(vec))
					tr._items.Push(new Scale (vec));
				if (item.StartsWith("translate") && !Translate.nop(vec))
					tr._items.Push(new Translate (vec));
			}
			if (tr._items.Count < 1)
				return null;
			if (tr._items.Count == 1)
				return tr._items.Pop();
			return tr;
		}
		//-------------------------------------------------------------------
	}
	//-------------------------------------------------------
}
