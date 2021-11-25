using System;
using System.Collections.Generic;
using DbManipulation;
using DbManipulation.Enums;
using DbManipulation.Models;

namespace SomeTask
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var crud = new Crud();
                //var task = CreateTask();

                //crud.Add(task);
                var tasks = new List<Task>
                {
                    new Task("Know where you are and where you want to be."),
                    new Task("Get enough sleep."),
                    new Task("Get fit."),
                    new Task("Take steps toward a deadline."),
                    new Task("Use a prioritized checklist."),
                    new Task("Don''t overcommit."),
                    new Task("Close social media."),
                    new Task("Forget multitasking."),
                    new Task("Use the Pareto Principle."),
                    new Task("Focus on service."),
                    new Task("Use time blocks."),
                    new Task("Use a timer."),
                    new Task("Create routines."),
                    new Task("Change your environment."),
                    new Task("Work out midday."),
                    new Task("Capture fleeting thoughts."),
                    new Task("Replace \"I can''t\" with \"How?\""),
                    new Task("If you''re struggling, take a break."),
                    new Task("Handle paper and email once."),
                    new Task("Eat healthy snacks.")
                };

                crud.AddBulk(tasks);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static Task CreateTask()
        {
            Console.WriteLine("Enter task description!");
            var text = Console.ReadLine();
            return new Task(text);
        }
    }
}
