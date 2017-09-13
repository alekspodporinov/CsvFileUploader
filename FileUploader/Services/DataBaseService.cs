using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using FileUploader.Models;

namespace FileUploader.Services
{
	public class DataBaseService : IDataBaseService
	{
		private List<ConfigurationFields> _initialFields;
		private readonly string _connectionString;

		public DataBaseService()
		{
			_initialFields = new XMLReaderService().GetConfigurationFields();
			_connectionString = ConfigurationManager.ConnectionStrings["DbConnection"].ToString();
		}
		public DbServiceMessage CheckDatabaseConnection()
		{
			
			var message = new DbServiceMessage();
			var connection = new SqlConnection(_connectionString);
			try
			{
				connection.Open();
				message.Message = "Connection successfully";
				message.IsSuccessful = true;
			} catch (SqlException se)
			{
				if (se.Number == 4060)
				{
					connection.Close();
					message = CreateDataBase();
				} else
				{
					message.Message = se.Message;
					message.IsSuccessful = false;
				}

			} finally
			{
				if(connection.State == ConnectionState.Open)
					connection.Close();
				connection.Dispose();
			}
			return message;
		}

		private void CreateTable(DataTable table)
		{
			var createTableSql = GetCreateFromDataTableSQL(table.TableName, table);

			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				var commandRowCount = new SqlCommand(createTableSql, connection);
					commandRowCount.ExecuteNonQuery();
			}
			
		}

		private void DropTable(string tableName)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				using(var command = new SqlCommand($"DROP TABLE {tableName}", connection))
					command.ExecuteNonQuery();
			}
		}

		public DbServiceMessage SaveDataToDataBase(Data data)
		{
			var message = new DbServiceMessage();
			var connection = new SqlConnection(_connectionString);
			try
			{
				DataTable table = MakeRows(data);

				if (!TableExists(data.TableName))
					CreateTable(table);
				connection.Open();
				using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.TableLock |
				                                                   SqlBulkCopyOptions.FireTriggers |
				                                                   SqlBulkCopyOptions.UseInternalTransaction,
					null))
				{
					bulkCopy.DestinationTableName = table.TableName;
					bulkCopy.WriteToServer(table);
				}
				message.Message = "all data was written";
				message.IsSuccessful = true;
			} catch (Exception se)
			{
				connection.Close();
				message.Message = se.Message;
				message.IsSuccessful = false;
				if (se is ConstraintException || se is SqlException)
					DropTable(data.TableName);

				
			} finally
			{
				if(connection.State == ConnectionState.Open)
					connection.Close();
				connection.Dispose();
				SqlConnection.ClearAllPools();
			}

			return message;
		}
	
		private DbServiceMessage CreateDataBase()
		{
			var message = new DbServiceMessage();
			SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(_connectionString);

			string auth;
			if (builder.IntegratedSecurity)
				auth = "Integrated Security=True;";
			else
			{
				auth = "User id=" + builder.UserID + ";";
				auth += "Password=" + builder.Password + ";";
			}
		
			var connString = $"Data Source={builder.DataSource};{auth}";
			var connection = new SqlConnection(connString);

			try
			{
				connection.Open();
				using (var cmdCreateDataBase = new SqlCommand($"CREATE DATABASE [{builder.InitialCatalog}]", connection))
					cmdCreateDataBase.ExecuteNonQuery();
				
				//задержка, нужна для того, чтоб БД успела создаться
				Thread.Sleep(5000);

				message.Message = "Database was created";
				message.IsSuccessful = true;
			} catch (SqlException se)
			{
				message.IsSuccessful = false;
				message.Message = $"Error connecting... \n\n{se.Message}";
			} finally
			{
				connection.Close();
				connection.Dispose();
			}
			return message;
		}
		
		private string SQLGetType(object type, int columnSize, int numericPrecision, int numericScale)
		{
			switch (type.ToString())
			{
				case "System.String":
					return "VARCHAR(" + ((columnSize == -1) ? 255 : columnSize) + ")";

				case "System.Decimal":
					if (numericScale > 0)
						return "REAL";
					else if (numericPrecision > 10)
						return "BIGINT";
					else
						return "INT";

				case "System.Double":
				case "System.Single":
					return "REAL";

				case "System.Int64":
					return "BIGINT";

				case "System.Int16":
				case "System.Int32":
					return "INT";

				case "System.DateTime":
					return "DATETIME";

				default:
					throw new Exception(type.ToString() + " not implemented.");
			}
		}

		private string SQLGetType(DataColumn column)
		{
			return SQLGetType(column.DataType, column.MaxLength, 10, 2);
		}

		private string GetCreateFromDataTableSQL(string tableName, DataTable table)
		{
			var sql = "CREATE TABLE [" + tableName + "] (\n";

			foreach (DataColumn column in table.Columns)
			{
				sql += "[" + column.ColumnName + "] " + 
					SQLGetType(column) + 
					(column.AllowDBNull ? "" :" NOT NULL") + 
					(column.Unique ? " UNIQUE" : "") + ",\n";
			}

			if (table.PrimaryKey.Length > 0)
			{
				sql += "CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED (";
				foreach (DataColumn column in table.PrimaryKey)
				{
					sql += "[" + column.ColumnName + "],";
				}
				sql = sql.TrimEnd(new char[] { ',' }) + "))\n";
			}

			return sql;
		}

		private bool TableExists(string tableName)
		{
			var count = 0;
			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				using (var cmd = new SqlCommand("select count(id) from sysobjects where name = @tableName and type = 'U'",
					connection))
				{
					cmd.Parameters.AddWithValue("@tableName", tableName);
					count = (int) cmd.ExecuteScalar();
				}
			}
			return count == 1;
		}

		private DataTable MakeRows(Data data)
		{
			var newTable = new DataTable(data.TableName);
			var Id = new DataColumn();
			Id.DataType = System.Type.GetType("System.Int32");
			Id.ColumnName = "Id";
			Id.AutoIncrementSeed = data.StartId;
			Id.AutoIncrementStep = 1;
			Id.AutoIncrement = true;
			newTable.Columns.Add(Id);

			DataColumn[] keys = new DataColumn[1];
			keys[0] = Id;
			newTable.PrimaryKey = keys;

			var columns = data.Records[0].Select(s => s.ColumnName).ToList();

			foreach (var column in columns)
			{
				var initialColumn = _initialFields.FirstOrDefault(f => f.Name.Contains(column));
				var col = new DataColumn();
				if (initialColumn != null)

					col.Unique = initialColumn.Unique;

				col.DataType = Type.GetType("System.String");
				col.ColumnName = column;
				newTable.Columns.Add(col);
			}

			foreach (var columnsData in data.Records)
			{
				var newRow = newTable.NewRow();
				foreach (var colData in columnsData)
				{
					newRow[colData.ColumnName] = colData.ColumnValue;
				}
				newTable.Rows.Add(newRow);
			}
			newTable.AcceptChanges();

			return newTable;
		}
	}
}