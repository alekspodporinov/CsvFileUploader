using System.Web;
using System.Web.Optimization;

namespace FileUploader
{
	public class BundleConfig
	{
		// For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
		public static void RegisterBundles(BundleCollection bundles)
		{
			bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
						"~/Scripts/jquery-{version}.js"));

			bundles.Add(new ScriptBundle("~/bundles/jquery-csv").Include(
				"~/Scripts/jquery.csv.min.js"));

			bundles.Add(new ScriptBundle("~/bundles/jquery-signalR").Include(
				"~/Scripts/jquery.signalR-2.2.2.min.js"));

			bundles.Add(new ScriptBundle("~/bundles/angular").Include(
				"~/Scripts/angular.min.js"));

			bundles.Add(new ScriptBundle("~/bundles/materialize").Include(
					  "~/Scripts/materialize.min.js"));

			bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
				"~/Scripts/bootstrap.min.js"));

			bundles.Add(new StyleBundle("~/Content/css").Include(
					  "~/Content/bootstrap.min.css",
					  "~/Content/materialize.min.css",
					  "~/Content/site.css"));
		}
	}
}
