using System;

namespace sqa_testing_report.Models
{
    // Mô hình d? li?u cho 1 b??c (1 row) trong test case
    public class TestCaseStep
    {
        // S? th? t? (No.)
        public string No { get; set; }

        // Test Requirement ID
        public string TestRequirementID { get; set; }

        // Test Case ID
        public string TestCaseID { get; set; }

        // M?c tiêu test
        public string TestObjective { get; set; }

        // ?i?u ki?n ti?n ??
        public string PreConditions { get; set; }

        // Step number (Step #)
        public string StepNumber { get; set; }

        // Hành ??ng b??c
        public string StepAction { get; set; }

        // D? li?u test
        public string TestData { get; set; }

        // K?t qu? mong ??i
        public string ExpectedResult { get; set; }

        // K?t qu? th?c t?
        public string ActualResult { get; set; }

        // Tr?ng thái (Status)
        public string Status { get; set; }

        // ???ng d?n/ghi chú ?nh
        public string Screenshots { get; set; }

        // S? hàng trong sheet (dùng ?? ghi tr? l?i file)
        public int RowNumber { get; set; }
    }
}