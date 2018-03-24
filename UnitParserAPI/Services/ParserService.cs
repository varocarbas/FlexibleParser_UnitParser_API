using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using FlexibleParser;
using UnitParserAPI.Models;

namespace UnitParserAPI.Services
{
	public class ParserService
	{
		public HttpResponseMessage GetParserOutput(string input, MediaTypeFormatter type)
		{
			UnitParserModel model = new UnitParserModel(input);

			HttpResponseMessage output = new HttpResponseMessage
			(
				model.UnitP.Error.Type == UnitP.ErrorTypes.None ?
				HttpStatusCode.OK : HttpStatusCode.BadRequest
			);

			output.Content = new ObjectContent
			(
				typeof(Dictionary<string, string>), GetStringValues(model.UnitP), type
			);

			return output;
		}

		private Dictionary<string, string> GetStringValues(UnitP unitP)
		{
			Dictionary<string, string> outDict = new Dictionary<string, string>();

			if (unitP.Error.Type != UnitP.ErrorTypes.None)
			{
				outDict.Add("Error", "Error");
			}
			else
			{
				outDict.Add("Value and unit", unitP.ValueAndUnitString);
				outDict.Add("Value", unitP.Value.ToString());
				outDict.Add
				(
					"Unit prefix", 
					(
						unitP.UnitPrefix.Factor == 1m ? "None" : 
						unitP.UnitPrefix.Name + " (" + unitP.UnitPrefix.Symbol + ")"
					)
				);
				outDict.Add
				(
					"Unit", 
					(
						unitP.Unit == Units.None ? "None" :
						unitP.Unit.ToString() + " (" + unitP.UnitString + ")"
					)
				);
				outDict.Add("Unit parts", GetUnitPartString(unitP));
				outDict.Add("System of units", unitP.UnitSystem.ToString());
				outDict.Add("Unit type", unitP.UnitType.ToString());
			}

			return outDict;
		}

		private string GetUnitPartString(UnitP unitP)
		{
			string output = "";

			foreach (UnitPart part in unitP.UnitParts)
			{
				if (output != "") output += " * ";

				if (part.Prefix.Factor != 1m) output += part.Prefix.Name;
				output += part.Unit.ToString();
				if (part.Exponent != 1m) output += part.Exponent.ToString();
			}

			return output;
		}
	}
}