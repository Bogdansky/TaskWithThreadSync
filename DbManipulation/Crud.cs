using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using DbManipulation.Models;
using System.Linq;
using System.Globalization;

namespace DbManipulation
{
    public partial class Crud : IDisposable
    {
        internal SqlConnection sqlConnection;
        internal SqlCommand sqlCommand;
        readonly string format;
        readonly CultureInfo provider;

        public Crud()
        {
            InitConnection();
            format = "dd.MM.yyyy hh:mm:ss";
            provider = CultureInfo.InvariantCulture;
        }

        public SqlConnection GetConnection() => sqlConnection != null && sqlConnection.State == System.Data.ConnectionState.Open ? sqlConnection : InitConnection();

        private string GetConnectionString()
        {
            return new ConfigurationBuilder()
                                        .SetBasePath(Directory.GetCurrentDirectory())
                                        .AddJsonFile("appsettings.json")
                                        .Build()
                                        .GetConnectionString("ConnectionString");
        }

        private SqlConnection InitConnection()
        {
            var connectionString = GetConnectionString();
            try
            {
                sqlConnection = new SqlConnection(connectionString);
                sqlConnection.Open();
                if (sqlConnection.State == System.Data.ConnectionState.Open)
                {
                    return sqlConnection;
                }
                throw new Exception("Failed to open the connection.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        public bool CloseConnection()
        {
            try
            {
                if (sqlConnection.State == System.Data.ConnectionState.Open)
                {
                    sqlConnection.Close();
                }
                return true;
            }
            catch(SqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }

        public void FillCommand(string commandText)
        {
            if (sqlCommand == null)
                sqlCommand = new SqlCommand(commandText, GetConnection());

            if (sqlConnection.State == System.Data.ConnectionState.Closed)
                sqlConnection.Open();

            sqlCommand.CommandText = commandText;
        }

        public bool Add(Task task)
        {
            var commandText = $"INSERT INTO Tasks (CreationTime,ModificationTime,TaskText,_Status) values {task}";
            FillCommand(commandText);
            var res = sqlCommand.ExecuteNonQuery();
            return res == 1;
        }

        public bool AddBulk(List<Task> tasks)
        {
            var values = string.Join(",", tasks.Select(t => t.ToString()).ToArray());
            var commandText = $"INSERT INTO Tasks (CreationTime,ModificationTime,TaskText,_Status) values {values}";
            FillCommand(commandText);
            var res = sqlCommand.ExecuteNonQuery();
            return res == tasks.Count;
        }

        /// <summary>
        /// method that returns an array of latest tasks for each customer by CustomerID
        /// </summary>
        /// <param name="consumerIds"></param>
        /// <returns>pending or in progress tasks for each customer</returns>
        public List<Task> GetLastestTasks(int[] consumerIds)
        {
            var list = new List<Task>();

            var commandText = $"SELECT * FROM dbo.Tasks where ConsumerID in ({string.Join(',', consumerIds.Select(i => i.ToString()))})";
            FillCommand(commandText);
            var reader = sqlCommand.ExecuteReader();
            while(reader.Read())
            {
                var task = new Task((int)reader["ID"], (int)reader["ConsumerID"], reader["TaskText"].ToString(), 
                    DateTime.ParseExact(reader["CreationDate"].ToString(), format, provider), 
                    DateTime.ParseExact(reader["ModificationTime"].ToString(), format, provider), 
                    Helper.GetEnumValue((byte)reader["_Status"]));
                list.Add(task);
            }
            return list;
        }

        public void Dispose()
        {
            CloseConnection();
        }
    }
}
