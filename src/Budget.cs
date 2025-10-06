namespace ExpenseTracker
{
    public class Budget
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public decimal Limit { get; set; }
        public DateTime Month { get; set; }

        // Multi-key field
        public string[] Tags { get; set; }
    }
}
