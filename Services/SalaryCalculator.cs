using System;

namespace GeneratingDocs.Services
{
    public class SalaryBreakup
    {
        public decimal Basic { get; set; }
        public decimal HRA { get; set; }
        public decimal Conveyance { get; set; }
        public decimal SpecialAllowance { get; set; }
        public decimal MonthlyCTC { get; set; }
        public decimal AnnualCTC { get; set; }
        public decimal PT { get; set; }
        public decimal PF { get; set; }
        public decimal PFAdmin { get; set; }
        public decimal MobileDeduction { get; set; }
        public decimal HealthInsurance { get; set; }
        public decimal TravelAllowance { get; set; } // -1 = NA
    }

    public class SalaryCalculator
    {
        private const decimal BasicPercent = 0.50m;
        private const decimal HraPercent = 0.25m;
        private const decimal ConveyanceFixed = 1600m;
        private const decimal PTFixed = 200m;
        private const decimal PfPercentOfBasic = 0.12m;

        public SalaryBreakup Calculate(decimal monthlyCtc, SalaryBreakup? overrides = null)
        {
            var basic = Math.Round(monthlyCtc * BasicPercent, 2);
            var hra = Math.Round(monthlyCtc * HraPercent, 2);
            var conveyance = ConveyanceFixed;
            var special = Math.Round(monthlyCtc - (basic + hra + conveyance), 2);
            var pt = PTFixed;
            var pf = Math.Round(basic * PfPercentOfBasic, 2);

            var sb = new SalaryBreakup
            {
                Basic = basic,
                HRA = hra,
                Conveyance = conveyance,
                SpecialAllowance = special,
                MonthlyCTC = monthlyCtc,
                AnnualCTC = Math.Round(monthlyCtc * 12, 2),
                PT = pt,
                PF = pf,
                PFAdmin = 0m,
                MobileDeduction = 0m,
                HealthInsurance = 0m,
                TravelAllowance = -1m
            };

            if (overrides != null)
            {
                if (overrides.Basic > 0) sb.Basic = overrides.Basic;
                if (overrides.HRA > 0) sb.HRA = overrides.HRA;
                if (overrides.Conveyance > 0) sb.Conveyance = overrides.Conveyance;
                if (overrides.SpecialAllowance > 0) sb.SpecialAllowance = overrides.SpecialAllowance;
                if (overrides.PT > 0) sb.PT = overrides.PT;
                if (overrides.PF > 0) sb.PF = overrides.PF;
                if (overrides.PFAdmin > 0) sb.PFAdmin = overrides.PFAdmin;
                if (overrides.MobileDeduction > 0) sb.MobileDeduction = overrides.MobileDeduction;
                if (overrides.HealthInsurance > 0) sb.HealthInsurance = overrides.HealthInsurance;
                if (overrides.TravelAllowance >= 0) sb.TravelAllowance = overrides.TravelAllowance;
            }

            // Recompute special if needed
            sb.SpecialAllowance = Math.Round(sb.MonthlyCTC - (sb.Basic + sb.HRA + (sb.Conveyance > 0 ? sb.Conveyance : 0)), 2);

            return sb;
        }
    }
}
