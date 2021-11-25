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
            var consumersCount = 2;
            var bulkSize = 1;

            try
            {
                int.TryParse(args[0], out consumersCount);
                int.TryParse(args[1], out bulkSize);
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("Parameters were definited not correctly. One of them has null value.");
            }

            var ids = Enumerable.Range(1, consumersCount);

            var threads = new List<Thread>();
            foreach(var i in ids)
            {
                var thread = new Thread(new ParameterizedThreadStart(TasksCheck));

                threads.Add(thread);
                thread.Start(new ThreadSets(i, bulkSize));
            }
            Console.WriteLine("Hello World!");
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
                var tasks = crud.GetEmptyTasks(threadSets.BulkSize);
                if (tasks.Count == 0)
                    break;
                tasks.ForEach(task => task.ConsumerId = threadSets.ID);
                ConsumerWork(tasks, threadSets.ID);

                Thread.Sleep(threadSets.Periodicity);
            }
            Console.WriteLine($"Thread {threadSets.ID} was finished");
        }
        /// <summary>
        /// Меняет у тасков ConsumerID и Status и сохраняет в БД
        /// </summary>
        /// <param name="tasks">Таски, для которых требуется внести изменения</param>
        static void ConsumerWork(List<Task> tasks, int threadId)
        {
            tasks.ForEach(t => Console.WriteLine($"Thread {threadId}: {t.TaskText}"));
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