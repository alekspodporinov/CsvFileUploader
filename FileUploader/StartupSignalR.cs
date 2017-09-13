using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(FileUploader.StartupSignalR))]

namespace FileUploader
{
	public class StartupSignalR
	{
		public void Configuration(IAppBuilder app)
		{
			app.MapSignalR();
		}
	}
}
