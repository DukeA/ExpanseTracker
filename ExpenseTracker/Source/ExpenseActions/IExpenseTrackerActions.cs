using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker;

public interface IExpenseTrackerActions
{
    public string GetList();

    public string AddValues(string[] args);

    public string ShowSummary(string[] args);

    public string DeleteExpense(string[] args);

    public void SaveToFile();

}

