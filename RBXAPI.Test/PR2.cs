using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RBXAPI.Test
{
	class PR2
	{
		public static void Main(string[] args)
		{
			Regex EvtTarg = new Regex("<input type=\"hidden\" name=\"__EVENTTARGET\" id=\"__EVENTTARGET\" value=\"(.*)\" />");
			string input;
			while ((input = Console.ReadLine()) != null)
			{
				Match res = EvtTarg.Match(input);
				Console.WriteLine(res.Success);
				Console.WriteLine(res.Index);
				Console.WriteLine(res.Groups.Count);
				Console.WriteLine(res.Groups[res.Groups.Count - 1]);
			}
		}
	}
}
