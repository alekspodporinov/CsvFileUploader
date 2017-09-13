using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FileUploader.Models
{
	public class Data
	{
		public int StartId { get; set; }
		public string TableName { get; set; }
		public List<List<ColumnsData>> Records { get; set; }
	}
}