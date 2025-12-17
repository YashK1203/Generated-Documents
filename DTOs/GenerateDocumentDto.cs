using System;

namespace GeneratingDocs
{
    public class GenerateDocumentDto
    {
        public Guid EmployeeId { get; set; }
        public string DocumentType { get; set; } = string.Empty;

        // Dates
        public DateTime? IssueDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // Salary Override Fields
        public decimal? Basic { get; set; }
        public decimal? HRA { get; set; }
        public decimal? Conveyance { get; set; }
        public decimal? SpecialAllowance { get; set; }
        public decimal? PT { get; set; }
        public decimal? PF { get; set; }
        public decimal? PFAdmin { get; set; }
        public decimal? MobileDeduction { get; set; }
        public decimal? HealthInsurance { get; set; }
        public decimal? TravelAllowance { get; set; }

        // NEW: Logo selection for Offer Letter
        public string? LogoFile { get; set; }
        public string? SignatureFile { get; set; }

    }
}
