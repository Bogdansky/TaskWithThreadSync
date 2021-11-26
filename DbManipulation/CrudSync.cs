using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;
using DbManipulation.Models;
using System.Linq;

namespace DbManipulation
{
    public partial class Crud
    {
        static object locker = new object();

        public List<Task> GetEmptyTasks(int count, int consumerId)
        {
            var tasks = new List<Task>();
            var command = $"SELECT TOP({count}) * FROM dbo.Tasks where _Status = {(byte)Enums.StatusEnum.Pending} and ConsumerID is NULL";

            lock(locker)
            {
                using var transaction = new TransactionScope();

                FillCommand(command);

                var reader = sqlCommand.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var task = new Task((int)reader["ID"], Convert.ToInt32(reader["ConsumerID"] as string), reader["TaskText"].ToString(),
                            DateTime.ParseExact(reader["CreationTime"].ToString(), format, provider),
                            DateTime.ParseExact(reader["ModificationTime"].ToString(), format, provider),
                            Helper.GetEnumValue((byte)reader["_Status"]));
                        tasks.Add(task);
                    }

                    var updateCommand = $"UPDATE dbo.Tasks set ConsumerId = {consumerId}, _Status = {random.Next(2, 4)}, ModificationTime='{DateTime.Now:yyyy-MM-dd hh:mm:ss}' WHERE ID in ({string.Join(',', tasks.Select(t => t.Id))})";

                    FillCommand(updateCommand);
                    sqlCommand.ExecuteNonQuery();
                }

                CloseConnection();
                transaction.Complete();
            }

            return tasks;
        }
    }
}
