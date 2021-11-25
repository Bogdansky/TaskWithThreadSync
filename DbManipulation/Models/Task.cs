using System;
using DbManipulation.Enums;

namespace DbManipulation.Models
{
    public class Task
    {
        public int Id { get; set; }
        public int? ConsumerId { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime ModificationTime { get; set; }
        public string TaskText{ get; set; }
        public byte Status{ get; set; }

        public Task(string text, StatusEnum status = StatusEnum.Pending)
        {
            TaskText = text;
            Status = (byte)status;
            CreationTime = ModificationTime = DateTime.Now;
        }

        public Task(int id, int? consumerId, string text, DateTime creationTime, DateTime modificationTime, StatusEnum status)
        {
            Id = id;
            ConsumerId = consumerId;
            TaskText = text;
            CreationTime = creationTime;
            ModificationTime = modificationTime;
            Status = (byte)status;
        }

        public override string ToString()
        {
            return $"('{CreationTime:yyyy-MM-dd hh:mm:ss}', '{ModificationTime:yyyy-MM-dd hh:mm:ss}', '{TaskText}', {Status})";
        }
    }
}
