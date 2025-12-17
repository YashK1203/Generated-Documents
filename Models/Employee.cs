public class Employee
{
    public Guid Id { get; set; }
    public string EmployeeNo { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public DateTime? JoiningDate { get; set; }
    public string? Designation { get; set; }
    public string? Department { get; set; }
    public string? PAN { get; set; }
    public string? Location { get; set; }
    public decimal MonthlyCTC { get; set; }
    public decimal AnnualCTC { get; set; }
    public string? UAN { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public ICollection<GeneratedDocument> Documents { get; set; } = new List<GeneratedDocument>();
}
