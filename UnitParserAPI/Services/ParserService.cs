using System.Collections.Generic;
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
				outDict.Add("Unit prefix", StringRepresentations.GetIndividualUnitOrPrefix(unitP.UnitPrefix));
				outDict.Add("Unit", StringRepresentations.GetIndividualUnitOrPrefix(unitP.Unit));
				outDict.Add("Unit parts", StringRepresentations.GetUnitParts(unitP));
				outDict.Add("System of units", unitP.UnitSystem.ToString());
				outDict.Add("Unit type", unitP.UnitType.ToString());
			}

			return outDict;
		}

		private class StringRepresentations
		{
			//The methods in this class generate the final strings which are displayed to the user.
			//All of them follow these rules:
			//- The names of individual units/prefixes are always shown in lower case.
			//- The symbols/abbreviations of individual units/prefixes are shown as returned by UnitParser.
			//- The individual units/prefixes are represented via [name] ([symbol or most common abbreviation]).
			//- Each unit part is represented by [prefix name (if applicable)][unit name][^exponent (if different than 1)]. 
			//- Any other string is shown as returned by UnitParser.

			public static string GetIndividualUnitOrPrefix(dynamic unitOrPrefix)
			{
				string symbol = GetUnitOrPrefixSymbol(unitOrPrefix);

				return
				(
					GetUnitOrPrefixName(unitOrPrefix) + (symbol == null ? "" : " (" + symbol + ")")
				);
			}

			public static string GetUnitParts(UnitP unitP)
			{
				if (unitP.UnitParts.Count == 0) { return GetUnitOrPrefixName(unitP.Unit); }
				string output = "";

				foreach (UnitPart part in unitP.UnitParts)
				{
					if (output != "") output += ", ";

					if (part.Prefix.Factor != 1m) output += GetUnitOrPrefixName(part.Prefix);
					output += GetUnitOrPrefixName(part.Unit);
					if (part.Exponent != 1m) output += "^" + part.Exponent.ToString();
				}

				return output;
			}

			private static string GetUnitOrPrefixName(Units unit)
			{
				return GetUnitOrPrefixNameInternal(unit.ToString());
			}

			private static string GetUnitOrPrefixName(Prefix prefix)
			{
				return GetUnitOrPrefixNameInternal(prefix.Name.ToString());
			}

			private static string GetUnitOrPrefixNameInternal(string input)
			{
				return 
				(
					input == "None" || input == "Unitless" || 
					input.Contains("Valid") ? input : input.ToLower()
				);
			}

			private static string GetUnitOrPrefixSymbol(Units unit)
			{
				string output = null;
				if (unit == Units.None) return output;

				var strings = UnitP.GetStringsForUnit(unit);
				if (strings != null && strings.Count > 0) { output = strings[0]; }

				return output;
			}

			private static string GetUnitOrPrefixSymbol(Prefix prefix)
			{
				return (prefix.Factor == 1m ? null : prefix.Symbol.ToString());
			}
		}
	}
}