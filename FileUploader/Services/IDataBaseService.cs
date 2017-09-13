using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FileUploader.Models;

namespace FileUploader.Services
{
	interface IDataBaseService
	{
		DbServiceMessage CheckDatabaseConnection();
		DbServiceMessage SaveDataToDataBase(Data data);
	}
}