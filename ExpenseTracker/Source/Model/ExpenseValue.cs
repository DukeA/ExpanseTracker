using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker;

public record ExpenseValue
(
    int Id,
    DateTime CurrentDate,
    string Description,
    int Amount
);