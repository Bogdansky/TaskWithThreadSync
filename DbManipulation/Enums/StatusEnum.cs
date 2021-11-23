using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace DbManipulation.Enums
{
    public enum StatusEnum
    {
        [Description("Waiting for Consumer")]
        Pending,
        [Description("One of the Consumers is handling the task")]
        InProgress,
        [Description("Task failed to be handled")]
        Error,
        [Description("Task completed successfully")]
        Done
    }
}
