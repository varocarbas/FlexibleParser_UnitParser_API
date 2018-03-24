using FlexibleParser;

namespace UnitParserAPI.Models
{
	public class UnitParserModel
	{
		public UnitP UnitP { get; set; }

		public UnitParserModel(string input)
		{
			UnitP = new UnitP((input == null ? "" : input));
		}
	}
}