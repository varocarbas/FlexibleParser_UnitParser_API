﻿using System.Collections.Generic;
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
				outDict.Add
				(
					"System of units", 
					(
						unitP.Unit == Units.None || unitP.Unit == Units.Unitless ? 
						UnitSystems.None : unitP.UnitSystem
					)
					.ToString()
				);
				outDict.Add("Unit type", unitP.UnitType.ToString());
			}

			return outDict;
		}

		private class StringRepresentations
		{
			//The methods in this class generate the final strings which are displayed to the user.
			//All of them follow these rules:
			//- The individual units/prefixes are represented via [name] ([symbol or most common abbreviation]).
			//- Each unit part is represented by [prefix name (if applicable)][unit name][^exponent (if different than 1)]. 

			public static string GetIndividualUnitOrPrefix(dynamic unitOrPrefix)
			{
				string outString = "";
				if (unitOrPrefix.GetType() == typeof(Prefix) && unitOrPrefix.Factor != 1m)
				{
					outString = unitOrPrefix.Factor.ToString() + " ";
				}

				string symbol = GetUnitOrPrefixSymbol(unitOrPrefix);

				return
				(
					outString + GetUnitOrPrefixName(unitOrPrefix) + (symbol == null ? "" : " (" + symbol + ")")
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
				return unit.ToString();
			}

			private static string GetUnitOrPrefixName(Prefix prefix)
			{
				return prefix.Name.ToString();
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