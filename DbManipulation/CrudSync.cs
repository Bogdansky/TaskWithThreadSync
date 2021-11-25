using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;
using DbManipulation.Models;

namespace DbManipulation
{
    public partial class Crud
    {
        static object locker = new object();

        public List<Task> GetEmptyTasks(int count)
        {
            var tasks = new List<Task>();
            var command = $"SELECT TOP({count}) * FROM dbo.Tasks where _Status = {(byte)Enums.StatusEnum.Pending} and ConsumerID is NULL";

            lock(locker)
            {
                using var transaction = new TransactionScope();

                FillCommand(command);

                var reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    var task = new Task((int)reader["ID"], Convert.ToInt32(reader["ConsumerID"] as string), reader["TaskText"].ToString(),
                        DateTime.ParseExact(reader["CreationTime"].ToString(), format, provider),
                        DateTime.ParseExact(reader["ModificationTime"].ToString(), format, provider),
                        Helper.GetEnumValue((byte)reader["_Status"]));
                    tasks.Add(task);
                }

                transaction.Complete();
                CloseConnection();
            }

            return tasks;
        }
    }
}
