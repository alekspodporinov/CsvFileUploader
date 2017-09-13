using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FileUploader.Services;
using Newtonsoft.Json;

namespace FileUploader.Controllers
{
	public class HomeController : Controller
	{
		private IXMLReader _xmlReader;
		public HomeController()
		{
			_xmlReader = new XMLReaderService();
		}
		public ActionResult Index()
		{
			return View();
		}

		public string GetAccessibleColumns()
		{
			var responseData = _xmlReader.GetConfigurationFields();
			var v = JsonConvert.SerializeObject(responseData);

			return v;
		}
	}
}