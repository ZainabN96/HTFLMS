namespace HTFLMS.Dtos.CertificateGeneration
{
    public class CertificateGenerationResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";

        public int GeneratedCount { get; set; }
        public int PdfGeneratedCount { get; set; }
        public int SkippedCount { get; set; }

        public List<CertificateGenerationItemDto> GeneratedCertificates { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }

    public class CertificateGenerationItemDto
    {
        public int CertificateRecordId { get; set; }
        public int CertificateRequestId { get; set; }

        public int StudentId { get; set; }
        public string StudentName { get; set; } = "";

        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = "";

        public string DeliveryMode { get; set; } = "";
        public string CertificateNumber { get; set; } = "";
        public string CertificateFilePath { get; set; } = "";
    }

    public class CertificatePdfDataDto
    {
        public int StudentId { get; set; }
        public int CourseId { get; set; }

        public string CertificateNumber { get; set; } = "";
        public string StudentFullName { get; set; } = "";
        public string CourseTitle { get; set; } = "";
        public string IssueDateText { get; set; } = "";
        public string PeriodText { get; set; } = "";
        public string DeliveryMode { get; set; } = "";
    }

    public class CertificatePdfOutputDto
    {
        public string RelativePath { get; set; } = "";
        public string PhysicalPath { get; set; } = "";
    }
}