public class GeneratedDocument
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string? DocumentType { get; set; }
    public byte[]? PdfBytes { get; set; }
    public DateTime GeneratedOn { get; set; }

    // Navigation Property
    public Employee Employee { get; set; }
}
