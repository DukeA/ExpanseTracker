using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker;
public class ExpensesTrackerOptions
{
    public const string SectionName = "App";

    public string Path { get; set; } = string.Empty;
}
