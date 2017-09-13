using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FileUploader.Models
{
	public class DbServiceMessage
	{
		[JsonProperty(PropertyName = "message")]
		public string Message { get; set; }
		[JsonProperty(PropertyName = "isSuccessful")]
		public bool IsSuccessful { get; set; }
	}
}