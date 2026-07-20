namespace HTFLMS.Dtos.StudentCertificate
{
    public class StudentCertificateDto
    {
        public int CourseId { get; set; }
        public int? CertificateRequestId { get; set; }

        public string CourseTitle { get; set; } = "";
        public string StudentName { get; set; } = "";
        public string? CourseImagePath { get; set; }

        public string? BatchNumber { get; set; }
        public string? DurationText { get; set; }

        public string? BatchStartDateText { get; set; }
        public string? BatchEndDateText { get; set; }

        public string Status { get; set; } = "";
        public string StatusText { get; set; } = "";
        public string StatusCssClass { get; set; } = "";

        public bool CanApply { get; set; }
        public bool CanView { get; set; }
        public bool CanDownload { get; set; }

        public string ButtonText { get; set; } = "";
        public string? Message { get; set; }

        public string? RequestedAtText { get; set; }
        public string? ApprovedAtText { get; set; }

        public string? ViewUrl { get; set; }
        public string? DownloadUrl { get; set; }
    }
}