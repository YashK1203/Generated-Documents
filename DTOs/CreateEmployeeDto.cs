using System;

namespace GeneratingDocs
{
    public class CreateEmployeeDto
    {
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
    }
}
