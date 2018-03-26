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
				outDict.Add("Unit prefix", StringRepresentations.GetUnitOrPrefix(unitP.UnitPrefix));
				outDict.Add("Unit", StringRepresentations.GetUnitOrPrefix(unitP.Unit));
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

			public static string GetUnitOrPrefix(dynamic unitOrPrefix)
			{
				string symbol = GetUnitPrefixSymbol(unitOrPrefix);

				return
				(
					GetUnitPrefixName(unitOrPrefix) + (symbol == null ? "" : " (" + symbol + ")")
				);
			}

			public static string GetUnitParts(UnitP unitP)
			{
				string output = "";

				foreach (UnitPart part in unitP.UnitParts)
				{
					if (output != "") output += ", ";

					if (part.Prefix.Factor != 1m) output += part.Prefix.Name.ToLower();
					output += part.Unit.ToString().ToLower();
					if (part.Exponent != 1m) output += "^" + part.Exponent.ToString();
				}

				return output;
			}

			private static string GetUnitPrefixName(Units unit)
			{
				return GetUnitPrefixNameInternal(unit.ToString());
			}

			private static string GetUnitPrefixName(Prefix prefix)
			{
				return GetUnitPrefixNameInternal(prefix.Name.ToString());
			}

			private static string GetUnitPrefixNameInternal(string input)
			{
				return (input == "None" ? input : input.ToLower());
			}

			private static string GetUnitPrefixSymbol(Units unit)
			{
				string output = null;
				if (unit == Units.None) return output;

				var strings = UnitP.GetStringsForUnit(unit);
				if (strings != null && strings.Count > 0) { output = strings[0]; }

				return output;
			}

			private static string GetUnitPrefixSymbol(Prefix prefix)
			{
				return (prefix.Factor == 1m ? null : prefix.Symbol.ToString());
			}
		}
	}
}