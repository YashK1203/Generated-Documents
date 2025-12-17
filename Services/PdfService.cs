using GeneratingDocs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace GeneratingDocs.Services
{
    public class PdfService
    {
        private readonly SalaryCalculator _calculator;
        private readonly IWebHostEnvironment _env;
        private static IContainer Box(IContainer c) =>
        c.Border(1).Padding(6);

        private static IContainer BoxNoBottom(IContainer c) =>
        c.BorderTop(1).BorderLeft(1).BorderRight(1).Padding(6);


        public PdfService(SalaryCalculator calculator, IWebHostEnvironment env)
        {
            _calculator = calculator;
            _env = env;
            QuestPDF.Settings.License = LicenseType.Community;
        }
        private static string NumberToWords(long number)
        {
            if (number == 0)
                return "Zero";

            if (number < 0)
                return "Minus " + NumberToWords(Math.Abs(number));

            string words = "";

            if ((number / 10000000) > 0)
            {
                words += NumberToWords(number / 10000000) + " Crore ";
                number %= 10000000;
            }

            if ((number / 100000) > 0)
            {
                words += NumberToWords(number / 100000) + " Lakh ";
                number %= 100000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " Thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " Hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                var unitsMap = new[]
                {
                    "Zero","One","Two","Three","Four","Five","Six","Seven","Eight","Nine","Ten",
                    "Eleven","Twelve","Thirteen","Fourteen","Fifteen","Sixteen","Seventeen","Eighteen","Nineteen"
                };
                var tensMap = new[]
                {
                    "Zero","Ten","Twenty","Thirty","Forty","Fifty","Sixty","Seventy","Eighty","Ninety"
                };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += " " + unitsMap[number % 10];
                }
            }

            return words.Trim();
        }


        // ---------------------------------------------------------------------------
        // OFFER LETTER WITH LOGO SUPPORT + ADDRESS UNDER LOGO
        // ---------------------------------------------------------------------------
        public byte[] GenerateOfferLetter(Employee emp, DateTime offerDate, DateTime startDate, SalaryBreakup? overrides, string? logoFile, string? signatureFile )
        {
            var sb = _calculator.Calculate(emp.MonthlyCTC, overrides);

            // Load logo bytes if provided
            byte[]? logoBytes = null;
            if (!string.IsNullOrWhiteSpace(logoFile))
            {
                try
                {
                    var path = Path.Combine(_env.WebRootPath ?? "", "images", logoFile);
                    if (File.Exists(path))
                        logoBytes = File.ReadAllBytes(path);
                }
                catch { logoBytes = null; }
            }

            // Load signature bytes if provided
            byte[]? signatureBytes = null;
            if (!string.IsNullOrWhiteSpace(signatureFile))
            {
                try
                {
                    var path = Path.Combine(_env.WebRootPath ?? "", "images", signatureFile);
                    if (File.Exists(path))
                        signatureBytes = File.ReadAllBytes(path);
                }
                catch { signatureBytes = null; }
            }

            using var ms = new MemoryStream();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontColor(Colors.Black));

                    // ----------------------------- HEADER -----------------------------
                    page.Header().Row(r =>
                    {
                        r.RelativeItem().AlignLeft().Column(left =>
                        {
                            if (logoBytes != null)
                                left.Item().Width(160).Image(logoBytes);

                            left.Item().PaddingTop(6).Text(t =>
                            {
                                // Show company name ONLY for old logo
                                if (!string.IsNullOrWhiteSpace(logoFile) &&
                                    logoFile.Equals("Metrolabs_old.png", StringComparison.OrdinalIgnoreCase))
                                {
                                    t.Line("METROLABS SERVICES PVT.LTD")
                                    .SemiBold()
                                    .FontSize(12);
                                }

                                t.Line("1-90/2/46/1, 4th floor, Sriram Plaza,");
                                t.Line("Vial Rao Nagar, Madhapur, Hyderabad,");
                                t.Line("Telangana, India, 500081");
                            });
                        });

                        r.RelativeItem().AlignRight().Text("");
                    });

                    // ----------------------------- FOOTER (APPEARS ON ALL PAGES) -----------------------------
                    // `.Text(...)` overload with a lambda returns void, so do not chain styling after it.
                    // Keep footer simple; it will inherit default text style.
                    page.Footer().AlignCenter().PaddingTop(10).Text(t =>
                    {
                        t.Span("Phone : +91-9951222468,   ")
                        .FontColor(Colors.Black);

                        t.Span("Email : hr@metrolabsservices.com,   ")
                        .FontColor(Colors.Black);

                        t.Span("Web : www.metrolabsservices.com")
                        .FontColor(Colors.Black);
                    });


                    // ----------------------------- CONTENT -----------------------------
                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Item().PaddingTop(8).AlignCenter().Text("OFFER LETTER").FontSize(16).SemiBold().Underline();
                        col.Item().PaddingBottom(25);   // Two-line gap under title ✔

                        // ------------------ BASIC DETAILS TABLE ------------------
                        col.Item().PaddingTop(12).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(140);
                                columns.ConstantColumn(15);
                                columns.RelativeColumn();
                            });

                            void Row(string label, string value)
                            {
                                table.Cell().Element(CellPadding).Text(label);
                                table.Cell().Element(CellPadding).Text(":");
                                table.Cell().Element(CellPadding).Text(value);
                            }

                            Row("Date", offerDate.ToString("dd-MMM-yyyy"));
                            Row("Name", emp.Name);
                            Row("Work location", emp.Location);
                        });

                        // ------------------ BODY PARAGRAPHS ------------------
                        col.Item().PaddingTop(12).Column(section =>
                        {
                            section.Item().Text($"Dear {emp.Name},");
                            section.Item().Text("");

                            section.Item().Text(
                                $"With reference to your appointment with Metrolabs Services Pvt.Ltd, we have pleasure in offering you " +
                                $"the position as \"{emp.Designation}\" in our organization on a fixed term contract basis. " +
                                $"The detail of the offer is as follow:"
                            );
                            section.Item().Text("");

                            // ------------------ ALIGNED CTC DETAILS ------------------
                            section.Item().Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.ConstantColumn(180);
                                    cols.ConstantColumn(10);
                                    cols.RelativeColumn();
                                });

                                void Row(string label, string value)
                                {
                                    table.Cell().Element(CellPadding).Text(label);
                                    table.Cell().Element(CellPadding).Text(":");
                                    table.Cell().Element(CellPadding).Text(value);
                                }

                                Row("Start date of Assignment", startDate.ToString("dd MMM yyyy"));
                                Row("Monthly CTC", $"{emp.MonthlyCTC:N2}/-");
                                Row("Annual CTC", $"{emp.AnnualCTC:N2}/-");
                            });

                            section.Item().Text("");

                            // ------------------ PARAGRAPH 1 ------------------
                            section.Item().Text(
                                "Any statutory dues like PF, ESIC, Bonus etc, if applicable, will be paid / Deducted as per law " +
                                "after probation period of one (1) year from the date of joining. All taxes will be deducted as applicable by law."
                            );

                            section.Item().Text("");

                            // ------------------ PARAGRAPH 2 ------------------
                            section.Item().Text(
                                "If you wish to accept this offer, kindly send the accepted copy of the same along with a copy of your resignation letter " +
                                "or relieving letter (if applicable). In case the signed acceptance and required documents are not received by Metrolabs " +
                                "Services Pvt.Ltd within 48 hours of the offer date, Metrolabs Services Pvt.Ltd at their discretion reserve their right to " +
                                "treat this offer as withdrawn automatically without further notice."
                            );
                        });

                        // ----------------------------- PAGE BREAK BEFORE ANNEXURE -----------------------------
                        col.Item().PageBreak();

                       // ---------------- ANNEXURE-I (FINAL – POLISHED & PERFECT) ----------------
                        col.Item().PaddingTop(18).Column(ann =>
                        {
                            // Annexure Title
                            ann.Item()
                            .AlignCenter()
                            .Text("Annexure-I: Fixed Annual CTC")
                            .Bold()
                            .FontSize(14);

                            // ---- TWO LINE GAP BELOW TITLE ----
                            ann.Item().Height(40);

                            ann.Item()
                            .PaddingHorizontal(30)
                            .Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn(2);   // Particulars / Name
                                    cols.RelativeColumn();    // Monthly
                                    cols.RelativeColumn();    // Yearly
                                });

                                // ----------- Salary Breakup Header (BOLD) -----------
                                table.Cell().ColumnSpan(3)
                                    .Element(Box)
                                    .AlignCenter()
                                    .Text("Salary Breakup")
                                    .Bold();

                                // ----------- Name (RowSpan = 2, CENTER + BOLD) -----------
                                table.Cell()
                                    .RowSpan(2)
                                    .Element(Box)
                                    .AlignMiddle()
                                    .AlignCenter()
                                    .Text(emp.Name)
                                    .Bold();

                                // ----------- Designation (BOLD) -----------
                                table.Cell().ColumnSpan(2)
                                    .Element(Box)
                                    .AlignCenter()
                                    .Text(emp.Designation)
                                    .Bold();

                                // ----------- Monthly / Yearly Headers (BOLD) -----------
                                table.Cell().Element(Box).AlignCenter().Text("Monthly").Bold();
                                table.Cell().Element(Box).AlignCenter().Text("Yearly").Bold();

                                // ----------- Body Rows (CENTERED) -----------
                                void Row(string label, decimal monthly, decimal yearly)
                                {
                                    table.Cell().Element(Box).AlignCenter().Text(label);
                                    table.Cell().Element(Box).AlignCenter().Text($"{monthly:N2}");
                                    table.Cell().Element(Box).AlignCenter().Text($"{yearly:N2}");
                                }

                                Row("Basic", sb.Basic, sb.Basic * 12);
                                Row("HRA", sb.HRA, sb.HRA * 12);
                                Row("Conveyance", sb.Conveyance, sb.Conveyance * 12);
                                Row("Special Allowances", sb.SpecialAllowance, sb.SpecialAllowance * 12);
                                Row("CTC", sb.MonthlyCTC, sb.AnnualCTC);
                                Row("PT", sb.PT, sb.PT * 12);

                                var netMonthly = Math.Round(sb.MonthlyCTC - (sb.PT + sb.PF), 2);
                                var netYearly = netMonthly * 12;
                                Row("Net Take Home Salary", netMonthly, netYearly);
                            });

                            // ---- ONE LINE GAP BETWEEN TABLE & NOTE ----
                            ann.Item().Height(16);

                            // ----------- NOTE (LEFT ALIGNED – NOT CENTERED) -----------
                            ann.Item()
                            .PaddingTop(8)
                            .PaddingHorizontal(10)
                            .Row(row =>
                            {
                                // Bullet column
                                row.ConstantItem(12)
                                    .AlignTop()
                                    .Text("•")
                                    .Bold();

                                // Text column (wrapped text aligns properly)
                                row.RelativeItem()
                                    .Text("Income Tax and Professional tax as applicable will be deducted. " +
                                        "All taxes will be deducted as applicable by law. Your salary is strictly confidential.")
                                    .Bold();
                            });
                        });


                        col.Item().PaddingTop(20).Row(r =>
                        {
                            // ---------- LEFT SIDE (NO EXTRA LEFT SPACE) ----------
                            r.RelativeItem().PaddingLeft(10).Column(left =>
                            {
                                // Top text (same alignment as header)
                                left.Item()
                                    .Text("For Metrolabs Services Pvt.Ltd.");

                                // Small gap before image
                                left.Item().PaddingTop(6);

                                // Signature / Stamp Image
                                if (signatureBytes != null)
                                {
                                    left.Item()
                                        .Width(120)              // 🔑 same visual column width
                                        .AlignCenter()           // centers image INSIDE the column
                                        .Height(65)
                                        .Image(signatureBytes)
                                        .FitArea();
                                }
                                else
                                {
                                    left.Item().Height(65);
                                }

                                // Text below image (same visual column)
                                left.Item()
                                    .Width(120)
                                    .AlignCenter()
                                    .PaddingTop(4)
                                    .Text("Authorized Signatory");
                            });


                            // ---------- RIGHT SIDE ----------
                            r.ConstantItem(220).Column(right =>
                            {
                                right.Item()
                                    .AlignCenter()
                                    .Text("Accepted By");

                                right.Item()
                                    .PaddingTop(65)
                                    .AlignCenter()
                                    .Text("Signature of Employee");
                            });
                        });


                    });
                });

                static IContainer CellStyle(IContainer c) => c.BorderBottom(1).Padding(6);
            })
            .GeneratePdf(ms);

            return ms.ToArray();
        }


      // ---------------------------------------------------------
        // PAYSLIP (AUTO CALCULATION + NA DISPLAY) ✅ FIXED
        // ---------------------------------------------------------
        public byte[] GeneratePayslip(Employee emp, DateTime payslipMonth, SalaryBreakup? overrides = null, string? logoFile = null)
        {
            // ---------------- DEFAULT VALUES ----------------
            decimal defaultPF = 3600;
            decimal defaultPT = 200;
            decimal defaultPFAdmin = 150;

            // ---------------- CALCULATE EARNINGS ONLY ----------------
            // ❌ DO NOT PASS overrides here
            var sb = _calculator.Calculate(emp.MonthlyCTC, null);

            // ---------------- APPLY DEDUCTIONS (UI → PDF) ----------------
            sb.PF = overrides?.PF ?? defaultPF;
            sb.PT = overrides?.PT ?? defaultPT;
            sb.PFAdmin = overrides?.PFAdmin ?? defaultPFAdmin;

            sb.MobileDeduction = overrides?.MobileDeduction ?? 0;
            sb.HealthInsurance = overrides?.HealthInsurance ?? 0;
            sb.TravelAllowance = overrides?.TravelAllowance ?? 0;

            // ---------------- LOAD LOGO ----------------
            // ---------------- LOAD LOGO ----------------
            byte[]? logoBytes = null;

            if (!string.IsNullOrWhiteSpace(logoFile))
            {
                try
                {
                    var path = Path.Combine(_env.WebRootPath!, "images", logoFile);

                    if (File.Exists(path))
                        logoBytes = File.ReadAllBytes(path);
                }
                catch
                {
                    logoBytes = null;
                }
            }

            // Precompute total deductions and net pay so they're available
            // outside the rendering lambdas (avoids scope issues).
            decimal totalDeductions =
                sb.PF + sb.PT + sb.PFAdmin + sb.MobileDeduction + sb.HealthInsurance;
            decimal net = sb.MonthlyCTC - totalDeductions;

            using var ms = new MemoryStream();

            Document.Create(doc =>
            {
                doc.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));

                    // ---------------- HEADER ----------------
                    page.Header().Row(r =>
                    {
                        r.RelativeItem().AlignLeft().Column(left =>
                        {
                            if (logoBytes != null)
                                        left.Item().Width(160).Image(logoBytes);

                            left.Item().PaddingTop(6).Text(t =>
                            {
                                // Show company name ONLY for old logo
                                if (!string.IsNullOrWhiteSpace(logoFile) &&
                                    logoFile.Equals("Metrolabs_old.png", StringComparison.OrdinalIgnoreCase))
                                {
                                    t.Line("METROLABS SERVICES PVT.LTD")
                                    .SemiBold()
                                    .FontSize(12);
                                }

                                t.Line("1-90/2/46/1, 4th floor, Sriram Plaza,");
                                t.Line("Vial Rao Nagar, Madhapur, Hyderabad,");
                                t.Line("Telangana, India, 500081");
                            });

                        });
                    });

                    // ---------------- FOOTER ----------------
                    page.Footer().AlignCenter().PaddingTop(10).Text(t =>
                    {
                        t.Span("Phone : +91-9951222468,   ");
                        t.Span("Email : hr@metrolabsservices.com,   ");
                        t.Span("Web : www.metrolabsservices.com");
                    });

                    // Helper: 0 => NA
                    string Amt(decimal v) => v == 0 ? "NA" : v.ToString("N2");

                    // ---------------- CONTENT ----------------
                    page.Content().PaddingTop(10).Column(col =>
                    {
                        // ---------------- TITLE ----------------
                        // ---------------- TITLE ----------------
                        col.Item()
                            .AlignCenter()
                            .Text($"Pay-slip for the month of {payslipMonth:MMMM yyyy}")
                            .Bold()
                            .FontSize(14);

                        // ---------------- DETAILS BOX ----------------
                        col.Item()
                        .PaddingTop(10)
                        .Border(1)
                        .Row(row =>
                        {
                            // -------- LEFT COLUMN (50%) --------
                            row.RelativeItem(1)
                            .Padding(10)
                            .Column(left =>
                            {
                                void LeftLine(string label, string value)
                                {
                                    left.Item().Text(t =>
                                    {
                                        t.Span(label).Bold();
                                        t.Span(value);
                                    });
                                    left.Item().PaddingBottom(6);
                                }

                                LeftLine("Name: ", emp.Name);
                                LeftLine("Joining Date: ", emp.JoiningDate?.ToString("dd MMM yyyy") ?? "-");
                                LeftLine("Designation: ", emp.Designation);
                                LeftLine("Department: ", emp.Department);
                                LeftLine("Location: ", emp.Location);
                                LeftLine("Effective Working Days: ", "25");
                                LeftLine("LOP: ", "0");
                            });

                            // -------- CENTER LINE --------
                            row.ConstantItem(1)
                            .Background(Colors.Grey.Lighten1);

                            // -------- RIGHT COLUMN (50%) --------
                            row.RelativeItem(1)
                            .Padding(10)
                            .Column(right =>
                            {
                                void RightLine(string label, string value)
                                {
                                    right.Item().Text(t =>
                                    {
                                        t.Span(label).Bold();
                                        t.Span(value);
                                    });
                                    right.Item().PaddingBottom(12);
                                }

                                RightLine("Employee No: ", emp.EmployeeNo);
                                RightLine("PAN Number: ", emp.PAN);
                                RightLine("UAN Number: ", emp.UAN ?? "-");
                            });
                        });


                        col.Item().PaddingTop(12).Table(main =>
                        {
                            main.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.RelativeColumn();
                            });

                            // ================= LEFT : EARNINGS =================
                            main.Cell().Border(1).Column(left =>
                            {
                                left.Item()
                                    .BorderBottom(1)
                                    .AlignCenter()
                                    .PaddingVertical(6)
                                    .Text("EARNINGS")
                                    .Bold();

                                left.Item()
                                    .BorderBottom(1)
                                    .PaddingVertical(4)
                                    .PaddingHorizontal(8)   // ✅ safe inner spacing
                                    .Row(r =>
                                    {
                                        r.RelativeItem().Text("ITEM").Bold();
                                        r.ConstantItem(110).AlignRight().Text("AMOUNT").Bold();
                                    });

                                void E(string label, decimal value)
                                {
                                    left.Item()
                                        .BorderBottom(1)
                                        .PaddingVertical(4)
                                        .PaddingHorizontal(8)   // ✅ safe inner spacing
                                        .Row(r =>
                                        {
                                            r.RelativeItem().Text(label);
                                            r.ConstantItem(110).AlignRight().Text(Amt(value));
                                        });
                                }

                                E("BASIC ADVANCE", sb.Basic);
                                E("HRA", sb.HRA);
                                E("CONVEYANCE", sb.Conveyance);
                                E("SPECIAL ALLOWANCE", sb.SpecialAllowance);
                                E("TRAVEL ALLOWANCE", sb.TravelAllowance);

                                left.Item()
                                    .PaddingVertical(6)
                                    .PaddingHorizontal(8)   // ✅ safe inner spacing
                                    .Row(r =>
                                    {
                                        r.RelativeItem().Text("TOTAL EARNINGS.INR").Bold();
                                        r.ConstantItem(110)
                                            .AlignRight()
                                            .Text(sb.MonthlyCTC.ToString("N2"))
                                            .Bold();
                                    });
                            });

                            // ================= RIGHT : DEDUCTIONS =================
                            main.Cell().Border(1).Column(right =>
                            {
                                right.Item()
                                    .BorderBottom(1)
                                    .AlignCenter()
                                    .PaddingVertical(6)
                                    .Text("DEDUCTIONS")
                                    .Bold();

                                right.Item()
                                    .BorderBottom(1)
                                    .PaddingVertical(4)
                                    .PaddingHorizontal(8)   // ✅ safe inner spacing
                                    .Row(r =>
                                    {
                                        r.RelativeItem().Text("ITEM").Bold();
                                        r.ConstantItem(110).AlignRight().Text("AMOUNT").Bold();
                                    });

                                void D(string label, decimal value)
                                {
                                    right.Item()
                                        .BorderBottom(1)
                                        .PaddingVertical(4)
                                        .PaddingHorizontal(8)   // ✅ safe inner spacing
                                        .Row(r =>
                                        {
                                            r.RelativeItem().Text(label);
                                            r.ConstantItem(110).AlignRight().Text(Amt(value));
                                        });
                                }

                                D("PF", sb.PF);
                                D("PT", sb.PT);
                                D("PF ADMIN", sb.PFAdmin);
                                D("MOBILE DEDUCTION", sb.MobileDeduction);
                                D("HEALTH INSURANCE", sb.HealthInsurance);

                                // use precomputed `totalDeductions` and `net`
                                right.Item()
                                    .PaddingVertical(6)
                                    .PaddingHorizontal(8)   // ✅ safe inner spacing
                                    .Row(r =>
                                    {
                                        r.RelativeItem().Text("TOTAL DEDUCTIONS.INR").Bold();
                                        r.ConstantItem(110)
                                            .AlignRight()
                                            .Text(totalDeductions.ToString("N2"))
                                            .Bold();
                                    });
                            });
                        });

                        // -------- 2 line gap after table --------
                        col.Item().PaddingTop(24);

                        // -------- Net Pay numeric --------
                        col.Item()
                            .Text($"Net Pay for the month: {net:N2}/-")
                            .Bold();

                        // -------- 1 line gap --------
                        col.Item().PaddingTop(10);

                        // -------- Net Pay in words --------
                        col.Item().Text(t =>
                        {
                            t.Span("Net Pay for the month: ").Bold();
                            t.Span($"{NumberToWords((long)net)} Rupees Only.");
                        });

                        // -------- Rupees in words note --------
                        col.Item()
                            .PaddingTop(2)
                            .Text("(Rupees In words only)")
                            .Italic();

                        // -------- Footer note --------
                        col.Item()
                            .PaddingTop(20)
                            .Text("*This is a computer generated pay-slip, hence does not require signature or stamp.")
                            .Italic();


                    });
                });

                static IContainer CellInner(IContainer c) => c.BorderBottom(1).Padding(6);
            })
            .GeneratePdf(ms);

            return ms.ToArray();
        }



        // ---------------------------------------------------------
        // EXPERIENCE LETTER
        // ---------------------------------------------------------
        public byte[] GenerateExperienceLetter(Employee emp, DateTime fromDate, DateTime toDate, DateTime issuedOn)
        {
            using var ms = new MemoryStream();

            Document.Create(doc =>
            {
                doc.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Content().Column(col =>
                    {
                        col.Item().AlignCenter().Text("TO WHOMSOEVER IT MAY CONCERN").Bold();
                        col.Item().PaddingTop(8).Text($"Date: {issuedOn:dd MMM yyyy}");
                        col.Item().PaddingTop(15).Text(
                            $"This is to certify that {emp.Name} worked with Metrolabs Services Pvt.Ltd from {fromDate:dd MMM yyyy} to {toDate:dd MMM yyyy}. His designation was {emp.Designation}. His conduct was good.");
                        col.Item().PaddingTop(15).Text("We wish him all success in future endeavors.");
                    });
                });
            })
            .GeneratePdf(ms);

            return ms.ToArray();
        }

        // ---------------------------------------------------------
        // RELIEVING LETTER
        // ---------------------------------------------------------
        public byte[] GenerateRelievingLetter(Employee emp, DateTime relievingDate, DateTime issuedOn)
        {
            using var ms = new MemoryStream();

            Document.Create(doc =>
            {
                doc.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Content().Column(col =>
                    {
                        col.Item().Text($"{issuedOn:dd MMM yyyy}");
                        col.Item().PaddingTop(10).Text("RELIEVING LETTER").SemiBold();
                        col.Item().PaddingTop(10)
                            .Text($"This is to confirm that {emp.Name} (Emp No: {emp.EmployeeNo}) has been relieved from Metrolabs Services Pvt.Ltd effective {relievingDate:dd MMM yyyy}.");
                        col.Item().PaddingTop(25).Text("Authorized Signatory");
                    });
                });
            })
            .GeneratePdf(ms);

            return ms.ToArray();
        }

        // common cell padding helper used by multiple tables in this service
        private static IContainer CellPadding(IContainer c) => c.PaddingVertical(3);
    }
}
