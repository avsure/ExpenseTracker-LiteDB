💼 Expense & Budget Tracker — LiteDB + .NET Console App

A lightweight personal finance tracker built with C# (.NET) and LiteDB —
a fast, embedded NoSQL database for console applications.

This project demonstrates real-world data handling concepts like CRUD,
LINQ queries, indexing, multi-key fields, BSON documents all inside a simple console app.

| Feature                                 | Description                                               |
| --------------------------------------- | --------------------------------------------------------- |
| 💾 **LiteDB Integration**               | Serverless NoSQL DB — no setup or external server         |
| 🧩 **CRUD Operations**                  | Full create, read, update, delete support                 |
| 🔍 **Querying & Filtering**             | LINQ and typed query syntax                               |
| 📊 **Aggregation (SUM, AVG, GROUP BY)** | Implemented via LINQ to mimic SQL-style analytics         |
| 🏷️ **Multi-Key Indexing**               | Demonstrates indexing of array fields (`Tags`)            |    
| 🧾 **BSON Data Handling**               | Displaying raw BSON documents in console                  |
| 🗂️ **CSV Export**                       | Export collections into `.csv` files for reporting        |
| 📁 **File Management**                  | Database and CSV created at project root (same as `.sln`) |


⚡ How to Run

Clone the repo:

git clone https://github.com/avsure/ExpenseTracker-LiteDB.git
cd ExpenseTracker-LiteDB

Build and run:
dotnet run

Inspect results:

Database: BudgetsDb.db (open in LiteDB Studio)
CSV export: expenses_summary.csv (view in Excel or VS Code)

🧩 Key Concepts Implemented:

🧮 1. CRUD Operations
🔍 2. LINQ Queries & Aggregation (SUM / AVG),SQL-Like Syntax
📊 3. Run Concurrency Simulation
🏷️ 3. Multi-Key Indexing (Array Field)
🧾 4. BSON Document Display
🗂️ 5. CSV Export


🧰 Tech Stack:

- C# / .NET 9
- LiteDB 5.x
- LiteDB Studio (visual DB tool)
- LINQ for in-memory querying

💡 Learning Outcomes

This project demonstrates:

How to use LiteDB as a lightweight embedded data store
How to perform LINQ-based analytics (SUM, AVG, GROUP BY)
How to implement multi-key indexes for array fields
How to export and visualize local data effectively
