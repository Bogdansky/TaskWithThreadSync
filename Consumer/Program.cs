using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DbManipulation;
using DbManipulation.Models;

namespace Consumer
{
    internal class Program
    {
        private static Crud crud;

        static void Main(string[] args)
        {
            crud = new Crud();
            var consumersCount = 3;
            var bulkSize = 2;

            try
            {
                int.TryParse(args[0], out consumersCount);
                int.TryParse(args[1], out bulkSize);
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("Parameters were definited not correctly. One of them has null value.");
            }
            finally
            {
                //ConsumerWork(consumersCount, bulkSize);

                #region Statistics print

                ConsumerWorkStatistics();

                #endregion
            }
        }

        static void ConsumerWork(int consumersCount, int bulkSize)
        {
            var ids = Enumerable.Range(1, consumersCount);

            var threads = new List<Thread>();
            foreach(var i in ids)
            {
                var thread = new Thread(new ParameterizedThreadStart(TasksCheck));

                threads.Add(thread);
                thread.Start(new ThreadSets(i, bulkSize));
            }
        }

        /// <summary>
        /// Проверяет с определённой периодичностью базу данных на наличие новых тасков
        /// </summary>
        /// <param name="sets">Настройки поиска (id поставщика; число тасков, требующихся для проверки; периодичность проверки)</param>
        static void TasksCheck(object sets)
        {
            var threadSets = sets as ThreadSets;

            //добавить проверку и чтение тасков
            while(true)
            {
                var tasks = crud.GetEmptyTasks(threadSets.BulkSize, threadSets.ID);
                if (tasks.Count == 0)
                    break;

                tasks.ForEach(t => Console.WriteLine($"Consumer {threadSets.ID}: {t.TaskText}"));
                
                Thread.Sleep(threadSets.Periodicity);
            }
            Console.WriteLine($"Consumer {threadSets.ID} was finished");
        }

        static void ConsumerWorkStatistics()
        {
            PrintTaskStatusesStats();
            PrintAvgSuccessTime();
            PrintErrorTasksPercent();
        }

        static void PrintTaskStatusesStats()
        {
            var taskStatusesCount = crud.CalculateTaskStatuses();
            foreach(var key in taskStatusesCount.Keys)
            {
                Console.WriteLine($"{key} - {taskStatusesCount[key]}");
            }
        }

        static void PrintAvgSuccessTime()
        {
            Console.WriteLine("Average processing time of successfully executed tasks - {0:dd\\.hh\\:mm\\:ss} days", crud.AverageSuccessTime());
        }

        static void PrintErrorTasksPercent()
        {
            var percents = crud.PercentOfErrors() * 100;
            Console.WriteLine("{0:##.##} % of errors", percents);
        }
    }

    internal class ThreadSets
    {
        public int Periodicity;
        public int BulkSize;
        public int ID;

        public ThreadSets(int id, int bulkSize)
        {
            ID = id;
            BulkSize = bulkSize;
            Periodicity = 1000;
        }
    }
}