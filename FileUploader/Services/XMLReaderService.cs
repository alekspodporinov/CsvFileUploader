using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using FileUploader.Models;

namespace FileUploader.Services
{
	public class XMLReaderService : IXMLReader
	{
		private readonly string _pathXML;

		public XMLReaderService()
		{
			_pathXML = HttpRuntime.AppDomainAppPath + "\\ColumnsConfig.xml";
		}

		public List<ConfigurationFields> GetConfigurationFields()
		{
			var xdoc = XDocument.Load(_pathXML);
			var columnsElements = xdoc.Element("columns");
			var listColumns = new List<ConfigurationFields>();
			if(columnsElements == null)
				throw new Exception("XML element 'columns' not found");

			foreach (var columnElement in columnsElements.Elements("column"))
			{
				var nameElement = columnElement.Element("name");
				var typeElement = columnElement.Element("type");
				var uploadElement = (bool?)columnElement.Element("upload");
				var requiredElement = (bool?)columnElement.Element("required");
				var aloneElement = (bool?)columnElement.Element("alone");
				var uniqueElement = (bool?)columnElement.Element("unique");
				

				if (nameElement == null &&
					uploadElement == null &&
				    requiredElement == null &&
				    aloneElement == null &&
				    uniqueElement == null &&
				    typeElement == null)
					throw new Exception("One of required elements not found, please check your xml file");

				listColumns.Add(new ConfigurationFields
				{
					Name = nameElement.Value,
					Alone = aloneElement.Value,
					Required = requiredElement.Value,
					Type = typeElement.Value,
					Unique = uniqueElement.Value,
					Upload = uploadElement.Value
					});
			}
			return listColumns;
		}
	}
}