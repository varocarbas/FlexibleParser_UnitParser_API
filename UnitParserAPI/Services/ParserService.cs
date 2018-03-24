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
				outDict.Add("Unit prefix", GetUnitPrefixNameAndSymbol(unitP.UnitPrefix));
				outDict.Add("Unit", GetUnitPrefixNameAndSymbol(unitP.Unit));
				outDict.Add("Unit parts", GetUnitPartString(unitP));
				outDict.Add("System of units", unitP.UnitSystem.ToString());
				outDict.Add("Unit type", unitP.UnitType.ToString());
			}

			return outDict;
		}

		private string GetUnitPrefixName(Units unit)
		{
			return unit.ToString();
		}

		private string GetUnitPrefixSymbol(Units unit)
		{
			string output = null;
			if (unit == Units.None) return output;

			var strings = UnitP.GetStringsForUnit(unit);
			if (strings != null && strings.Count > 0) { output = strings[0]; }

			return output;
		}

		private string GetUnitPrefixName(Prefix prefix)
		{
			return prefix.Name.ToString();
		}

		private string GetUnitPrefixSymbol(Prefix prefix)
		{
			return (prefix.Factor == 1m ? null : prefix.Symbol.ToString());
		}

		private string GetUnitPrefixNameAndSymbol(dynamic unitOrPrefix)
		{
			string symbol = GetUnitPrefixSymbol(unitOrPrefix);

			return 
			(
				GetUnitPrefixName(unitOrPrefix) + (symbol == null ? "" : " (" + symbol + ")")
			);
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