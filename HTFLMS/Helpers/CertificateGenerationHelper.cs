namespace HTFLMS.Helper
{
    public static class CertificateGenerationHelper
    {
        public static string NormalizeDeliveryMode(string? deliveryMode)
        {
            return string.Equals(deliveryMode?.Trim(), "Online", StringComparison.OrdinalIgnoreCase)
                ? "Online"
                : "Onsite";
        }

        public static bool HasValidTitlePrefix(string? titlePrefix)
        {
            return string.Equals(titlePrefix?.Trim(), "Mr.", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(titlePrefix?.Trim(), "Ms.", StringComparison.OrdinalIgnoreCase);
        }

        public static string? BuildSuffixFromExistingCertificateCount(int existingCertificateCount)
        {
            if (existingCertificateCount <= 0)
                return null;

            return ToAlphabetSuffix(existingCertificateCount);
        }

        public static string BuildCertificateNumber(
            int year,
            int baseNumber,
            string? suffix)
        {
            var baseText = $"HTF-{year}-{baseNumber:D4}";

            return string.IsNullOrWhiteSpace(suffix)
                ? baseText
                : $"{baseText}-{suffix}";
        }

        public static string BuildStudentDisplayName(
            string? titlePrefix,
            string studentName)
        {
            var cleanName = studentName?.Trim() ?? "";
            var cleanPrefix = titlePrefix?.Trim();

            if (string.IsNullOrWhiteSpace(cleanPrefix))
                return cleanName;

            return $"{cleanPrefix} {cleanName}";
        }

        private static string ToAlphabetSuffix(int index)
        {
            var value = index;
            var suffix = "";

            while (value > 0)
            {
                value--;
                suffix = (char)('A' + (value % 26)) + suffix;
                value /= 26;
            }

            return suffix;
        }
    }
}