using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ExpenseTracker.Source;

public class App(ILogger<ExpenseTrackerActions> _logger, IOptions<ExpensesTrackerOptions> options)
{
    public static IExpenseTrackerActions? ExpenseTracker;
    public async Task Execute(string[] args)
    {
        ExpenseTracker = new ExpenseTrackerActions(options, _logger);
        if (args.Length <= 0)
            return;
        Menu(args);

        _logger.LogInformation("Finished!");
        await Task.CompletedTask;
    }

    public void Menu(string[] args)
    {
        try
        {
            switch (args[0].ToLowerInvariant())
            {
                case "add":
                    Console.WriteLine(ExpenseTracker?.AddValues(args[1..]));
                    break;
                case "list":
                    Console.Write(ExpenseTracker?.GetList());
                    break;
                case "summary":
                    Console.WriteLine(ExpenseTracker?.ShowSummary(args[1..]));
                    break;
                case "delete":
                    Console.WriteLine(ExpenseTracker?.DeleteExpense(args[1..]));
                    break;
                default:
                    Console.WriteLine("Unknown Command");
                    break;

            }
            ExpenseTracker?.SaveToFile();
        }
        catch (Exception exp)
        {
            _logger.LogError("Exception Message:{exp}", exp.Message);
            throw new Exception(exp.Message);
        }

    }
}