using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using UnitParserAPI.Services;

namespace UnitParserAPI.Controllers
{
    public class ParserController : ApiController
    {
        private ParserService ParserService;
        public ParserController() { ParserService = new ParserService(); }

        [Route("xml")]
        public HttpResponseMessage Get(string input)
        {
			return ParserService.GetParserOutput(input, new XmlMediaTypeFormatter());
        }

		[Route("json")]
		public HttpResponseMessage Get(string input, bool placeholder = false)
		{
			return ParserService.GetParserOutput(input, new JsonMediaTypeFormatter());
		}
	}
}
