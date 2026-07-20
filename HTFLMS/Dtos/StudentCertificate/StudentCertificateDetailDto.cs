namespace HTFLMS.Dtos.StudentCertificate
{
    public class StudentCertificateDetailDto
    {
        public int CertificateRequestId { get; set; }

        public string CertificateId { get; set; } = "";
        public string StudentName { get; set; } = "";
        public string CourseTitle { get; set; } = "";

        public string? BatchNumber { get; set; }
        public string? DurationText { get; set; }

        public string? BatchStartDateText { get; set; }
        public string? BatchEndDateText { get; set; }

        public string IssueDateText { get; set; } = "";
        public string Status { get; set; } = "";
        public string? DownloadUrl { get; set; }
    }
}