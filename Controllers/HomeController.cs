using System.Web.Mvc;

namespace UnitParserAPI.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			ViewBag.Title = "UnitParser Web API";

			return View();
		}
	}
}
