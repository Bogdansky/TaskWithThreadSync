using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using DbManipulation.Models;
using System.Linq;
using System.Globalization;
using DbManipulation.Enums;

namespace DbManipulation
{
    public partial class Crud : IDisposable
    {
        internal SqlConnection sqlConnection;
        internal SqlCommand sqlCommand;
        readonly string format;
        readonly CultureInfo provider;
        readonly Random random; 

        public Crud()
        {
            InitConnection();

            format = "dd.MM.yyyy hh:mm:ss";
            provider = CultureInfo.InvariantCulture;
            random = new Random();
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
        /// <summary>
        /// Fills SqlCommand object and opens SqlConnection if needed
        /// </summary>
        /// <param name="commandText">Command text</param>
        public void FillCommand(string commandText)
        {
            if (sqlCommand == null)
                sqlCommand = new SqlCommand(commandText, GetConnection());

            if (sqlConnection.State == System.Data.ConnectionState.Open)
                CloseConnection();

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

        #region Statistics methods
        /// <summary>
        /// Calculate how many tasks in each status
        /// </summary>
        public Dictionary<StatusEnum, int> CalculateTaskStatuses()
        {
            var result = new Dictionary<StatusEnum, int>();
            var command = "SELECT _Status, count(id) as [Count] FROM Tasks GROUP BY _Status";
            var statuses = new StatusEnum[] { StatusEnum.Pending, StatusEnum.InProgress, StatusEnum.Error, StatusEnum.Done };

            FillCommand(command);

            var reader = sqlCommand.ExecuteReader();

            if (reader.HasRows)
            {
                while(reader.Read())
                {
                    result.Add(Helper.GetEnumValue((byte)reader["_Status"]), (int)reader["Count"]);
                }
                var keys = result.Keys.ToArray();
                var emptyStatuses = statuses.Except(keys);
                foreach (var status in emptyStatuses)
                {
                    result.Add(status, 0);
                }
            }
            else
            {
                result.Add(StatusEnum.Pending, 0);
                result.Add(StatusEnum.InProgress, 0);
                result.Add(StatusEnum.Error, 0);
                result.Add(StatusEnum.Done, 0);
            }

            CloseConnection();
            return result;
        }

        /// <summary>
        /// Calculate average processing time of successfully executed tasks
        /// </summary>
        public TimeSpan AverageSuccessTime()
        {
            var ts = new List<long>();
            var command = $"SELECT CreationTime,ModificationTime FROM dbo.Tasks where _Status={(byte)StatusEnum.Done}";
            FillCommand(command);

            var reader = sqlCommand.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    var ct = DateTime.ParseExact(reader["CreationTime"].ToString(), format, provider);
                    var mt = DateTime.ParseExact(reader["ModificationTime"].ToString(), format, provider);
                    ts.Add((mt - ct).Ticks);
                }
            }

            CloseConnection();
            return ts.Count == 0 ? TimeSpan.Zero : TimeSpan.FromTicks(Convert.ToInt64(ts.Average()));
        }

        public double PercentOfErrors()
        {
            var command = $"select (select count(id) from Tasks where _Status={(int)StatusEnum.Error}) as [Error],(select count(id) from Tasks) as [Common]";
            var commonTasks = 0;
            var errorTasks = 0;

            FillCommand(command);

            var reader = sqlCommand.ExecuteReader();
            while(reader.Read())
            {
                commonTasks = (int)reader["Common"];
                errorTasks = (int)reader["Error"];
            }
            return errorTasks / (double)commonTasks;
        }
        #endregion

        public void Dispose()
        {
            CloseConnection();
        }
    }
}
