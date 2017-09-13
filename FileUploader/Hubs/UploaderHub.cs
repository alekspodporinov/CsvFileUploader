using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using FileUploader.Models;
using FileUploader.Services;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;

namespace FileUploader.Hubs
{
	public class UploaderHub : Hub
	{
		private IDataBaseService _dataBaseService;

		public UploaderHub()
		{
			_dataBaseService = new DataBaseService();
		}
		public string CheckConnection()
		{
				var response = _dataBaseService.CheckDatabaseConnection();
				return JsonConvert.SerializeObject(response);
		}

		public string SaveData(Data data)
		{
				if (data.Records.Count == 0)
					return JsonConvert.SerializeObject(new DbServiceMessage { Message = "All data was written", IsSuccessful = true });
				var response = _dataBaseService.SaveDataToDataBase(data);

				return JsonConvert.SerializeObject(response);
		}

		public void Connect()
		{
			Clients.Caller.reponseMessage("Сonnection established");
		}

		public override Task OnDisconnected(bool stopCalled)
		{
			return base.OnDisconnected(stopCalled);
		}
	}
}