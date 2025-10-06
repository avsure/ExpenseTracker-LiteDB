using LiteDB;

namespace ExpenseTracker
{
    internal class Program
    {
        private static string dbPath = Path.Combine(Directory.GetCurrentDirectory(), "Finance.db");
        static void Main()
        {
            // Get the project (solution) root directory
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectDir = Path.GetFullPath(Path.Combine(baseDir, @"..\..\.."));

            string dbPath = Path.Combine(projectDir, "BudgetsDb.db");

            using var db = new LiteDatabase(dbPath);
            var expenseCollection = db.GetCollection<Expense>("expenses");
            var budgetCollection = db.GetCollection<Budget>("budgets");


            // --- LiteDB Indexing ---
            expenseCollection.EnsureIndex(x => x.Category);
            expenseCollection.EnsureIndex(x => x.Date);
            expenseCollection.EnsureIndex(x => x.Amount);

            // Create multi-key index on Budget.Tags
            budgetCollection.EnsureIndex(x => x.Tags);

            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("\n--- Personal Expense Tracker ---");
                Console.WriteLine("1. Add Sample Data");
                Console.WriteLine("2. Add Expense");
                Console.WriteLine("3. Update Expense");
                Console.WriteLine("4. Delete Expense");
                Console.WriteLine("5. View Expenses");
                Console.WriteLine("6. Query by Category");
                Console.WriteLine("7. SQL-like Summary");
                Console.WriteLine("8. Export to CSV");
                Console.WriteLine("9. Run Concurrency Simulation");
                Console.WriteLine("10. Show BSON Representation");
                Console.WriteLine("11. Working With Files");
                Console.WriteLine("12. Multi-Key Index example");
                Console.WriteLine("13. Expressions and Functions");
                Console.WriteLine("0. Exit");
                Console.Write("Choose an option: ");

                var input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        AddSampleData(expenseCollection, budgetCollection);
                        break;
                    case "2":
                        AddExpense(expenseCollection);
                        break;
                    case "3":
                        UpdateExpense(expenseCollection);
                        break;
                    case "4":
                        DeleteExpense(expenseCollection);
                        break;
                    case "5":
                        ViewExpenses(expenseCollection);
                        break;
                    case "6":
                        QueryByCategory(expenseCollection);
                        break;
                    case "7":
                        SqlLikeSummary(expenseCollection);
                        break;
                    case "8":
                        ExportExpensesToCsv(expenseCollection.FindAll());
                        Console.WriteLine("Expenses exported to expenses_summary.csv");
                        break;
                    case "9":
                        RunConcurrencySimulation(expenseCollection);
                        break;
                    case "10":
                        // Insert sample data if empty
                        if (expenseCollection.Count() == 0)
                        {
                            expenseCollection.Insert(new Expense { Date = DateTime.Now, Amount = 250, Category = "Food", Description = "Lunch" });
                            expenseCollection.Insert(new Expense { Date = DateTime.Now, Amount = 100, Category = "Transport", Description = "Taxi" });
                        }

                        Console.WriteLine("=== BSON Representation ===\n");

                        // Get raw BsonDocument instead of mapped class
                        var bsonDocs = db.GetCollection("expenses").FindAll();

                        foreach (var doc in bsonDocs)
                        {
                            Console.WriteLine(doc.ToString());
                            Console.WriteLine("----------------------------");
                        }
                        break;
                    case "11":
                        Console.WriteLine("=== Working with files ===\n");
                        // Get file storage with Int Id
                        var storage = db.GetStorage<int>();

                        // Upload a file from file system to database
                        storage.Upload(123, @"C:\Temp\Hobbit.jpg");

                        // And download later
                        storage.Download(123, @"C:\Temp\copy-of-Hobbit.jpg", true);
                        break;
                    case "12":
                        MultiKeyIndex(budgetCollection);
                        break;
                    case "13":
                        ExpressionsAndFunctions(budgetCollection);
                        break;
                    case "0":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }
        }

        private static void ExpressionsAndFunctions(ILiteCollection<Budget> budgetCollection)
        {
            Console.WriteLine("\n************* SUM *************");
            Console.WriteLine("Aggregate Functions: SUM Example");

            Console.WriteLine("\nBudget Collection: ");
            // Get all documents
            var allBudgets = budgetCollection.FindAll().ToList();

            // ✅ Calculate total sum (in memory)
            decimal totalLimit = allBudgets.Sum(b => b.Limit);

            Console.WriteLine($"Total Budget Limit: ₹{totalLimit}");
            Console.WriteLine("Equivalent to SQL's: SELECT SUM(Limit) FROM budgets;");
            Console.WriteLine("\nInfo:LiteDB loads all budget records into memory, and\n then .Sum() computes the total — not the database engine.");


            Console.WriteLine("\n\nFiltered Aggregation: SUM Example");
            var monthlyBudgets = budgetCollection.Find(b => b.Tags.Contains("Monthly")).ToList();
            var monthlyTotal = monthlyBudgets.Sum(b => b.Limit);
            Console.WriteLine($"\nTotal Monthly Budget: ₹{monthlyTotal}");
            Console.WriteLine("Equivalent to SQL's: SELECT SUM(Limit) FROM budgets WHERE Tags CONTAINS 'Monthly'");


            Console.WriteLine("\n************* AVG *************");
            Console.WriteLine("\nAggregate Functions: AVG Example");
            var avgByCategory = budgetCollection.FindAll()
                           .GroupBy(b => b.Category)
                           .Select(g => new
                           {
                               Category = g.Key,
                               AverageLimit = g.Average(x => x.Limit)
                           });

            Console.WriteLine("\nAverage Limit by Category:");
            foreach (var a in avgByCategory)
                Console.WriteLine($"{a.Category,-15} | Avg: ₹{a.AverageLimit}");
            Console.WriteLine("Equivalant to SQL's: SELECT Category, AVG(Limit) FROM budgets GROUP BY Category;");
            Console.WriteLine("\nInfo:This grouping and averaging happens in C#, not at DB-level.");

            Console.WriteLine("\nLiteDB trades SQL engine power for C# simplicity." +
                "\r\nIt doesn’t have built-in SQL-style aggregation commands — but since it stores data locally (in-process)," +
                " using LINQ for aggregation is fast enough for most small-to-medium apps.");
        }

        private static void MultiKeyIndex(ILiteCollection<Budget> budgetCol)
        {
            // Insert sample budgets 

            budgetCol.Insert(new Budget
            {
                Category = "Monthly Groceries",
                Limit = 5000,
                Month = new DateTime(2025, 10, 1),
                Tags = new[] { "food", "monthly", "home" }
            });

            budgetCol.Insert(new Budget
            {
                Category = "Entertainment Budget",
                Limit = 2000,
                Month = new DateTime(2025, 10, 1),
                Tags = new[] { "fun", "monthly", "leisure" }
            });

            budgetCol.Insert(new Budget
            {
                Category = "Office Expenses",
                Limit = 10000,
                Month = new DateTime(2025, 10, 1),
                Tags = new[] { "office", "work", "monthly" }
            });


            // Query budgets by tag
            var foodBudgets = budgetCol.Query()
                                           .Where(x => x.Tags.Contains("food"))
                                           .ToList();

            Console.WriteLine("Budgets with tag 'food':");
            foreach (var b in foodBudgets)
                Console.WriteLine($"- {b.Category} (Limit: {b.Limit})");
        }

        static void AddSampleData(ILiteCollection<Expense> collection, ILiteCollection<Budget> budgetCollection)
        {
            var sampleExpenses = new List<Expense>
                {
                    new Expense { Date = DateTime.Parse("2025-09-01"), Amount = 150, Category = "Food", Description = "Breakfast" },
                    new Expense { Date = DateTime.Parse("2025-09-01"), Amount = 50, Category = "Transport", Description = "Bus fare" },
                    new Expense { Date = DateTime.Parse("2025-09-02"), Amount = 200, Category = "Food", Description = "Lunch" },
                    new Expense { Date = DateTime.Parse("2025-09-02"), Amount = 300, Category = "Shopping", Description = "Groceries" },
                    new Expense { Date = DateTime.Parse("2025-09-03"), Amount = 120, Category = "Food", Description = "Dinner" },
                    new Expense { Date = DateTime.Parse("2025-09-03"), Amount = 75, Category = "Transport", Description = "Taxi" },
                    new Expense { Date = DateTime.Parse("2025-09-04"), Amount = 500, Category = "Shopping", Description = "Clothes" },
                    new Expense { Date = DateTime.Parse("2025-09-04"), Amount = 250, Category = "Health", Description = "Medicines" },
                    new Expense { Date = DateTime.Parse("2025-09-05"), Amount = 80, Category = "Food", Description = "Breakfast" },
                    new Expense { Date = DateTime.Parse("2025-09-05"), Amount = 60, Category = "Transport", Description = "Metro" },
                    new Expense { Date = DateTime.Parse("2025-09-06"), Amount = 150, Category = "Food", Description = "Lunch" },
                    new Expense { Date = DateTime.Parse("2025-09-06"), Amount = 100, Category = "Entertainment", Description = "Movie" },
                    new Expense { Date = DateTime.Parse("2025-09-07"), Amount = 200, Category = "Food", Description = "Dinner" },
                    new Expense { Date = DateTime.Parse("2025-09-07"), Amount = 300, Category = "Shopping", Description = "Electronics" },
                    new Expense { Date = DateTime.Parse("2025-09-08"), Amount = 120, Category = "Health", Description = "Doctor visit" },
                    new Expense { Date = DateTime.Parse("2025-09-08"), Amount = 50, Category = "Transport", Description = "Bus fare" },
                    new Expense { Date = DateTime.Parse("2025-09-09"), Amount = 400, Category = "Shopping", Description = "Shoes" },
                    new Expense { Date = DateTime.Parse("2025-09-09"), Amount = 90, Category = "Food", Description = "Lunch" },
                    new Expense { Date = DateTime.Parse("2025-09-10"), Amount = 60, Category = "Transport", Description = "Taxi" },
                    new Expense { Date = DateTime.Parse("2025-09-10"), Amount = 180, Category = "Food", Description = "Dinner" },
                    new Expense { Date = DateTime.Parse("2025-09-11"), Amount = 120, Category = "Entertainment", Description = "Concert" },
                    new Expense { Date = DateTime.Parse("2025-09-11"), Amount = 300, Category = "Shopping", Description = "Groceries" },
                    new Expense { Date = DateTime.Parse("2025-09-12"), Amount = 50, Category = "Transport", Description = "Metro" },
                    new Expense { Date = DateTime.Parse("2025-09-12"), Amount = 100, Category = "Food", Description = "Breakfast" },
                    new Expense { Date = DateTime.Parse("2025-09-13"), Amount = 150, Category = "Food", Description = "Lunch" },
                };


            // Insert sample data
            collection.InsertBulk(sampleExpenses);
            Console.WriteLine("Sample expenses inserted successfully.");

            var sampleBudget = new List<Budget>
            {
                new Budget { Category = "Food", Limit = 8000, Month = new DateTime(2025, 10, 1), Tags = new []{"Monthly", "Essential"} },
                new Budget { Category = "Travel", Limit = 5000, Month = new DateTime(2025, 10, 1), Tags = new []{"Monthly", "Optional"} },
                new Budget { Category = "Entertainment", Limit = 3000, Month = new DateTime(2025, 10, 1), Tags = new []{"Optional"} },
                new Budget { Category = "Health", Limit = 4000, Month = new DateTime(2025, 10, 1), Tags = new []{"Essential"} },
                new Budget { Category = "Savings", Limit = 10000, Month = new DateTime(2025, 10, 1), Tags = new []{"Monthly", "Goal"} }
            };

            budgetCollection.InsertBulk(sampleBudget);
            Console.WriteLine("Sample budget inserted successfully.");
        }

        static void AddExpense(ILiteCollection<Expense> collection)
        {
            Console.Write("Date (yyyy-MM-dd): ");
            DateTime date = DateTime.Parse(Console.ReadLine());
            Console.Write("Amount: ");
            decimal amount = decimal.Parse(Console.ReadLine());
            Console.Write("Category: ");
            string category = Console.ReadLine();
            Console.Write("Description: ");
            string description = Console.ReadLine();

            collection.Insert(new Expense
            {
                Date = date,
                Amount = amount,
                Category = category,
                Description = description
            });
            Console.WriteLine("Expense added successfully.");
        }

        static void UpdateExpense(ILiteCollection<Expense> collection)
        {
            Console.Write("Enter Expense ID to update: ");
            int id = int.Parse(Console.ReadLine());
            var expense = collection.FindById(id);
            if (expense != null)
            {
                Console.Write("New Amount: ");
                expense.Amount = decimal.Parse(Console.ReadLine());
                Console.Write("New Category: ");
                expense.Category = Console.ReadLine();
                Console.Write("New Description: ");
                expense.Description = Console.ReadLine();

                collection.Update(expense);
                Console.WriteLine("Expense updated successfully.");
            }
            else
            {
                Console.WriteLine("Expense not found.");
            }
        }

        static void DeleteExpense(ILiteCollection<Expense> collection)
        {
            Console.Write("Enter Expense ID to delete: ");
            int id = int.Parse(Console.ReadLine());
            if (collection.Delete(id))
                Console.WriteLine("Expense deleted.");
            else
                Console.WriteLine("Expense not found.");
        }

        static void ViewExpenses(ILiteCollection<Expense> collection)
        {
            var expenses = collection.FindAll().ToList();
            Console.WriteLine("\nID | Date       | Category   | Amount | Description");
            foreach (var e in expenses)
            {
                Console.WriteLine($"{e.Id} | {e.Date:yyyy-MM-dd} | {e.Category} | {e.Amount} | {e.Description}");
            }
        }

        static void QueryByCategory(ILiteCollection<Expense> collection)
        {
            Console.Write("Enter category to filter: ");
            string category = Console.ReadLine();
            var filtered = collection.Find(x => x.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
            Console.WriteLine($"\nExpenses in category '{category}':");
            foreach (var e in filtered)
            {
                Console.WriteLine($"{e.Id} | {e.Date:yyyy-MM-dd} | {e.Amount} | {e.Description}");
            }
        }

        static void SqlLikeSummary(ILiteCollection<Expense> collection)
        {
            var summary = collection.Query()
                .ToEnumerable()
                .GroupBy(x => x.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Total = g.Sum(x => x.Amount)
                })
                .OrderByDescending(x => x.Total);

            Console.WriteLine("\nSQL-like Summary (Total per Category):");
            foreach (var row in summary)
            {
                Console.WriteLine($"{row.Category,-15} ₹{row.Total}");
            }
        }

        static void RunConcurrencySimulation(ILiteCollection<Expense> collection)
        {
            var tasks = new List<Task>();

            // Writer
            tasks.Add(Task.Run(() =>
            {
                for (int i = 0; i < 5; i++)
                {
                    collection.Insert(new Expense
                    {
                        Date = DateTime.Now,
                        Amount = new Random().Next(50, 500),
                        Category = "Food",
                        Description = $"Lunch {i + 1}"
                    });
                    Thread.Sleep(100);
                }
                Console.WriteLine("Writer task completed.");
            }));

            // Readers
            for (int i = 0; i < 2; i++)
            {
                int readerId = i + 1;
                tasks.Add(Task.Run(() =>
                {
                    for (int j = 0; j < 3; j++)
                    {
                        var expenses = collection.FindAll().ToList();
                        Console.WriteLine($"Reader {readerId} read {expenses.Count} expenses.");
                        Thread.Sleep(150);
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());
            Console.WriteLine("Concurrency simulation completed.");
        }

        static void ExportExpensesToCsv(IEnumerable<Expense> expenses)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectDir = Path.GetFullPath(Path.Combine(baseDir, @"..\..\.."));

            string csvPath = Path.Combine(projectDir, "expenses_summary.csv");

            using var writer = new StreamWriter(csvPath);
            writer.WriteLine("Date,Category,Amount,Description");
            foreach (var exp in expenses)
            {
                writer.WriteLine($"{exp.Date:yyyy-MM-dd},{exp.Category},{exp.Amount},{exp.Description}");
            }
        }
    }
}
