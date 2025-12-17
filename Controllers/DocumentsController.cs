using GeneratingDocs;
using GeneratingDocs.Data;
using GeneratingDocs.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeneratingDocs.Controllers
{
    [ApiController]
    [Route("api/documents")]
    public class DocumentsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly PdfService _pdfService;

        public DocumentsController(ApplicationDbContext db, PdfService pdfService)
        {
            _db = db;
            _pdfService = pdfService;
        }

        private DateTime? Normalize(DateTime? d)
        {
            if (!d.HasValue) return null;
            return DateTime.SpecifyKind(d.Value, DateTimeKind.Utc);
        }

        // =========================================================
        // CREATE EMPLOYEE
        // =========================================================
        [HttpPost("employee")]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeDto dto)
        {
            if (dto == null) return BadRequest("Invalid data");

            var emp = new Employee
            {
                Id = Guid.NewGuid(),
                EmployeeNo = dto.EmployeeNo,
                Name = dto.Name,
                JoiningDate = Normalize(dto.JoiningDate),
                Designation = dto.Designation,
                Department = dto.Department,
                Location = dto.Location,
                PAN = dto.PAN,
                UAN = dto.UAN,
                AnnualCTC = dto.AnnualCTC,
                MonthlyCTC = dto.MonthlyCTC,
                CreatedAt = DateTime.UtcNow
            };

            _db.Employees.Add(emp);
            await _db.SaveChangesAsync();

            return Ok(emp);
        }

        // =========================================================
        // GET ALL EMPLOYEES
        // =========================================================
        [HttpGet("employees")]
        public async Task<IActionResult> GetEmployees()
        {
            var list = await _db.Employees
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return Ok(list);
        }

        // =========================================================
        // GET EMPLOYEE BY ID
        // =========================================================
        [HttpGet("employee/{id}")]
        public async Task<IActionResult> GetEmployeeById(Guid id)
        {
            var emp = await _db.Employees.FirstOrDefaultAsync(e => e.Id == id);
            if (emp == null) return NotFound("Employee not found");

            return Ok(emp);
        }

        // =========================================================
        // UPDATE EMPLOYEE  ✅ (FIXES YOUR NotFound ISSUE)
        // =========================================================
        [HttpPut("employee/{id}")]
        public async Task<IActionResult> UpdateEmployee(Guid id, [FromBody] CreateEmployeeDto dto)
        {
            var emp = await _db.Employees.FirstOrDefaultAsync(e => e.Id == id);
            if (emp == null) return NotFound("Employee not found");

            emp.EmployeeNo = dto.EmployeeNo;
            emp.Name = dto.Name;
            emp.Designation = dto.Designation;
            emp.Department = dto.Department;
            emp.Location = dto.Location;
            emp.PAN = dto.PAN;
            emp.UAN = dto.UAN;
            emp.JoiningDate = Normalize(dto.JoiningDate);
            emp.AnnualCTC = dto.AnnualCTC;
            emp.MonthlyCTC = dto.MonthlyCTC;

            await _db.SaveChangesAsync();

            return Ok(emp);
        }

        // =========================================================
        // DELETE EMPLOYEE
        // =========================================================
        [HttpDelete("employee/{id}")]
        public async Task<IActionResult> DeleteEmployee(Guid id)
        {
            var emp = await _db.Employees.FirstOrDefaultAsync(e => e.Id == id);
            if (emp == null) return NotFound("Employee not found");

            _db.Employees.Remove(emp);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Employee deleted successfully" });
        }

        // =========================================================
        // GENERATE DOCUMENT (OFFER / PAYSLIP / EXPERIENCE / RELIEVING)
        // =========================================================
        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] GenerateDocumentDto dto)
        {
            var emp = await _db.Employees.FirstOrDefaultAsync(e => e.Id == dto.EmployeeId);
            if (emp == null) return NotFound("Employee not found");

            var overrides = new SalaryBreakup
            {
                Basic = dto.Basic ?? 0,
                HRA = dto.HRA ?? 0,
                Conveyance = dto.Conveyance ?? 0,
                SpecialAllowance = dto.SpecialAllowance ?? 0,
                PT = dto.PT ?? 0,
                PF = dto.PF ?? 0,
                PFAdmin = dto.PFAdmin ?? 0,
                MobileDeduction = dto.MobileDeduction ?? 0,
                HealthInsurance = dto.HealthInsurance ?? 0,
                TravelAllowance = dto.TravelAllowance ?? -1
            };

            byte[] pdf;

            switch (dto.DocumentType.ToLower())
            {
                case "offer":
                    pdf = _pdfService.GenerateOfferLetter(
                        emp,
                        Normalize(dto.IssueDate) ?? DateTime.UtcNow,
                        Normalize(dto.StartDate) ?? emp.JoiningDate ?? DateTime.UtcNow,
                        overrides,
                        dto.LogoFile,
                        dto.SignatureFile
                    );
                    break;

                case "payslip":
                    pdf = _pdfService.GeneratePayslip(
                        emp,
                        Normalize(dto.IssueDate) ?? DateTime.UtcNow,
                        overrides,
                        dto.LogoFile
                    );
                    break;

                case "experience":
                    pdf = _pdfService.GenerateExperienceLetter(
                        emp,
                        Normalize(dto.FromDate) ?? emp.JoiningDate ?? DateTime.UtcNow,
                        Normalize(dto.ToDate) ?? DateTime.UtcNow,
                        Normalize(dto.IssueDate) ?? DateTime.UtcNow
                    );
                    break;

                case "relieving":
                    pdf = _pdfService.GenerateRelievingLetter(
                        emp,
                        Normalize(dto.ToDate) ?? DateTime.UtcNow,
                        Normalize(dto.IssueDate) ?? DateTime.UtcNow
                    );
                    break;

                default:
                    return BadRequest("Invalid document type");
            }

            var record = new GeneratedDocument
            {
                Id = Guid.NewGuid(),
                EmployeeId = emp.Id,
                DocumentType = dto.DocumentType,
                GeneratedOn = DateTime.UtcNow,
                PdfBytes = pdf
            };

            _db.GeneratedDocuments.Add(record);
            await _db.SaveChangesAsync();

            return File(pdf, "application/pdf", $"{dto.DocumentType}_{emp.EmployeeNo}.pdf");
        }
    }
}
