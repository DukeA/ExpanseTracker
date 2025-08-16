using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ExpenseTracker;

public class ExpenseTrackerActions : IExpenseTrackerActions
{

    public List<ExpenseValue> Expenses;
    public string Path;

    public ILogger<ExpenseTrackerActions> _logger;

    public ExpenseTrackerActions(IOptions<ExpensesTrackerOptions> options, ILogger<ExpenseTrackerActions> logger)
    {
        _logger = logger;
        Path = options.Value.Path;
        Expenses = ReadFromFile(Path);
    }

    private List<ExpenseValue> ReadFromFile(string Path)
    {
        try
        {
            if (!File.Exists(Path))
                return new List<ExpenseValue>();

            var lines = File.ReadAllLines(Path).Skip(1); // Skip header
            var values = new List<ExpenseValue>();
            foreach (var line in lines)
            {
                if (TryParseExpense(line, out ExpenseValue expense))
                {
                    values.Add(expense);
                }
            }
            return values;
        }
        catch (Exception exp)
        {
            _logger.LogError("Exception message:{exp}", exp.Message);
            throw new Exception(exp.Message);
        }
    }

    private bool TryParseExpense(string line, out ExpenseValue expense)
    {
        expense = null!;
        var parts = line.Split(',');
        if (parts.Length != 4)
        {
            return false;
        }

        bool idParsed = int.TryParse(parts[0], out int id);
        bool dateParsed = DateTime.TryParse(parts[1], out DateTime date);
        bool amountParsed = int.TryParse(parts[3], out int amount);

        if (!idParsed)
        {
            return false;
        }
        if (!dateParsed)
        {
            return false;
        }
        if (!amountParsed)
        {
            return false;
        }
        expense = new ExpenseValue(id, date, parts[2], amount);
        return true;
    }

    public string AddValues(string[] args)
    {
        try
        {
            if (!args.Any())
            return "Invalid command should be --description xxx --amount yyy";

            string description = "";
            int amount = 0;

            for (int i = 0; i < args.Length; i++)
            {
            if (args[i] == "--description" && i + 1 < args.Length)
                description = args[i + 1];
            if (args[i] == "--amount" && i + 1 < args.Length)
                int.TryParse(args[i + 1], out amount);
            }

            if (string.IsNullOrWhiteSpace(description))
            return "Description is required";
            if (amount <= 0)
            return "Invalid amount for the expenses";

            int nextId = Expenses.Any() ? Expenses.Max(x => x.Id) + 1 : 1;
            Expenses.Add(new ExpenseValue(nextId, DateTime.Now, description, amount));
            return $"# Expense added successfully (ID: {nextId})";
        }
        catch (Exception exp)
        {
            _logger.LogError("Exception message:{exp}", exp.Message);
            return "Could not complete request";
        }
    }

    public string DeleteExpense(string[] args)
    {
        try
        {
            string idExist = args.FirstOrDefault(x => x.Equals("--id"))!;
            if (string.IsNullOrEmpty(idExist))
                return $"Invalid Command should be --id x";
            var id = int.Parse(args.Last());
            Expenses = Expenses.Where(x => x.Id != id).ToList();
            return "# Expense deleted successfully";
        }
        catch (Exception exp)
        {
            _logger.LogError("Exception message:{exp}", exp.Message);
            return "Could not complete request";
        }

    }

    public string GetList()
    {
        try
        {
            string header = $"# {"ID",-3} {"Date",-10} {"Description",-12} {"Amount"}\n";
            string rows = string.Join("\n",
                Expenses.Select(x =>
                    $"# {x.Id,-3} {x.CurrentDate:yyyy-MM-dd}  {x.Description,-12} ${x.Amount}"));
            return header + rows;
        }
        catch (Exception exp)
        {
            _logger.LogError("Exception message:{exp}", exp.Message);
            return "Could not complete request";
        }
    }

    public string ShowSummary(string[] args)
    {
        try
        {
            int total = 0;
            if (!args.Any())
            {
                total = Expenses.Sum(x => x.Amount);
                return $"# Total expenses: ${total}";
            }
            int.TryParse(args[^1], out int month);
            total = Expenses.Where(x => x.CurrentDate.Month.Equals(month)).Sum(x => x.Amount);
            string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month).ToUpper();
            return $"# Total expenses for {monthName} : ${total}";
        }
        catch (Exception exp)
        {
            _logger.LogError("Exception message:{exp}", exp.Message);
            return "Could not complete request";
        }
    }

    public void SaveToFile()
    {
        try
        {
            using (var writer = new StreamWriter(Path, false))
            {
                writer.WriteLine("Id,Date,Description,Amount");
                foreach (var expense in Expenses)
                {
                    writer.WriteLine($"{expense.Id},{expense.CurrentDate:yyyy-MM-dd},{expense.Description},{expense.Amount}");
                }
            }
        }
        catch (Exception exp)
        {
            _logger.LogError("Exception message:{exp}", exp.Message);
            throw new Exception(exp.Message);
        }
    }
}
