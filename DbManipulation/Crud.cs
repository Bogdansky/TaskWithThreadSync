using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using DbManipulation.Models;
using System.Linq;

namespace DbManipulation
{
    public class Crud
    {
        internal SqlConnection sqlConnection;
        internal SqlCommand sqlCommand;

        public SqlConnection GetConnection() => sqlConnection == null && sqlConnection.State == System.Data.ConnectionState.Open? sqlConnection : InitConnection();

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

        public bool CloseConnection(SqlConnection connection)
        {
            try
            {
                if (connection.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
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

        public List<Task> GetLastestTasks(int[] ids)
        {
            var commandText = string.Join(',', ids.Select(i => i.ToString()));
            FillCommand(commandText);
            var reader = sqlCommand.ExecuteReader();
            var list = new List<Task>();
            while(reader.Read())
            {
                var task = new Task((int)reader["ID"], (int)reader["ConsumerID"], reader["TaskText"].ToString(), reader["CreationDate"], reader["ModificationTime"], Helper.GetEnumValue((short)reader["_Status"]));
                list.Add(task);
            }
            return list;
        }
    }
}
