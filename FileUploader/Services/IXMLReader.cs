using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileUploader.Models;

namespace FileUploader.Services
{
	interface IXMLReader
	{
		List<ConfigurationFields> GetConfigurationFields();
	}
}
