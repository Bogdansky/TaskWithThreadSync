using System;
using DbManipulation;
using DbManipulation.Enums;

namespace SomeTask
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var date = new DateTime(2012, 10, 30, 23, 23, 59);
            Console.WriteLine($"{date:yyyy-MM-dd hh:mm:ss}");
        }

        static void AddTask()
        {
            Console.WriteLine("Enter task description!");
            var text = Console.ReadLine();
            Console.WriteLine($"Choose task status:\n1.{StatusEnum.}");
        }
    }
}
