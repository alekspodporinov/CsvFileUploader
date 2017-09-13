using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace FileUploader.Models
{
	public class ConfigurationFields
	{
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }

		[JsonIgnore]
		public string Type { get; set; }

		[JsonProperty(PropertyName = "upload")]
		public bool Upload { get; set; }

		[JsonProperty(PropertyName = "required")]
		public bool Required { get; set; }

		[JsonProperty(PropertyName = "alone")]
		public bool Alone { get; set; }

		[JsonProperty(PropertyName = "unique")]
		public bool Unique { get; set; }
	}
}