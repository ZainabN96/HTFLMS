using HTFLMS.Dtos.CertificateGeneration;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HTFLMS.Data.Services
{
    public class CertificatePdfService
    {
        private const float PageWidth = 2000;
        private const float PageHeight = 1380;

        private const string BodyFont = "Times New Roman";
        private const string ScriptFont = "Monotype Corsiva";

        private readonly string webRootPath;

        public CertificatePdfService()
        {
            webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }

        public CertificatePdfOutputDto GeneratePdf(CertificatePdfDataDto data)
        {
            var templatePath = Path.Combine(
                webRootPath,
                "templates",
                "certificates",
                "certificate-template.png");

            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException("Certificate template image was not found.", templatePath);
            }

            var outputFolder = Path.Combine(
                webRootPath,
                "uploads",
                "certificates",
                "generated");

            Directory.CreateDirectory(outputFolder);

            var fileName = BuildFileName(data);
            var physicalPath = Path.Combine(outputFolder, fileName);

            var document = new CertificatePdfDocument(data, templatePath);
            document.GeneratePdf(physicalPath);

            return new CertificatePdfOutputDto
            {
                RelativePath = $"/uploads/certificates/generated/{fileName}",
                PhysicalPath = physicalPath
            };
        }

        private static string BuildFileName(CertificatePdfDataDto data)
        {
            var certificateNumber = MakeSafeFileName(data.CertificateNumber);
            var deliveryMode = MakeSafeFileName(data.DeliveryMode);

            return $"{certificateNumber}-{deliveryMode}-S{data.StudentId}-C{data.CourseId}.pdf";
        }

        private static string MakeSafeFileName(string value)
        {
            var safe = string.IsNullOrWhiteSpace(value)
                ? "certificate"
                : value.Trim();

            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                safe = safe.Replace(invalidChar, '-');
            }

            return safe.Replace(" ", "-");
        }

        private class CertificatePdfDocument : IDocument
        {
            private readonly CertificatePdfDataDto data;
            private readonly string templatePath;

            public CertificatePdfDocument(
                CertificatePdfDataDto data,
                string templatePath)
            {
                this.data = data;
                this.templatePath = templatePath;
            }

            public DocumentMetadata GetMetadata()
            {
                return DocumentMetadata.Default;
            }

            public void Compose(IDocumentContainer container)
            {
                container.Page(page =>
                {
                    page.Size(PageWidth, PageHeight);
                    page.Margin(0);

                    page.DefaultTextStyle(x => x
                        .FontFamily(BodyFont)
                        .FontColor(Colors.Black));

                    page.Content().Layers(layers =>
                    {
                        layers.Layer()
                            .Image(templatePath)
                            .FitUnproportionally();

                        layers.PrimaryLayer()
                            .Element(ComposeCertificateFields);
                    });
                });
            }

            private void ComposeCertificateFields(IContainer container)
            {
                var studentNameFontSize = GetStudentNameFontSize(data.StudentFullName);
                var courseTitleFontSize = GetCourseTitleFontSize(data.CourseTitle);
                var periodFontSize = GetPeriodFontSize(data.PeriodText);

                container.Layers(layers =>
                {
                    layers.PrimaryLayer()
                        .Width(PageWidth)
                        .Height(PageHeight);

                    PlaceText(
                        layers,
                        $"Certificate ID: {data.CertificateNumber}",
                        x: 1465,
                        y: 90,
                        width: 500,
                        height: 45,
                        fontFamily: BodyFont,
                        fontSize: 31,
                        isBold: true,
                        alignCenter: false);

                    PlaceText(
                        layers,
                        $"Grant Date: {data.IssueDateText}",
                        x: 1465,
                        y: 136,
                        width: 500,
                        height: 45,
                        fontFamily: BodyFont,
                        fontSize: 31,
                        isBold: true,
                        alignCenter: false);

                    PlaceText(
                        layers,
                        "CERTIFICATE OF TRAINING",
                        x: 395,
                        y: 305,
                        width: 1230,
                        height: 85,
                        fontFamily: BodyFont,
                        fontSize: 68,
                        isBold: true,
                        alignCenter: true);

                    PlaceText(
                        layers,
                        "THIS IS TO CERTIFY THAT",
                        x: 500,
                        y: 415,
                        width: 1000,
                        height: 65,
                        fontFamily: BodyFont,
                        fontSize: 43,
                        isBold: false,
                        alignCenter: true);

                    PlaceText(
                        layers,
                        data.StudentFullName,
                        x: 420,
                        y: 512,
                        width: 1160,
                        height: 100,
                        fontFamily: ScriptFont,
                        fontSize: studentNameFontSize,
                        isBold: false,
                        alignCenter: true);

                    PlaceText(
                        layers,
                        "HAS SUCCESSFULLY COMPLETED TRAINING IN",
                        x: 385,
                        y: 650,
                        width: 1230,
                        height: 60,
                        fontFamily: BodyFont,
                        fontSize: 42,
                        isBold: false,
                        alignCenter: true);

                    PlaceText(
                        layers,
                        data.CourseTitle,
                        x: 520,
                        y: 755,
                        width: 950,
                        height: 60,
                        fontFamily: BodyFont,
                        fontSize: courseTitleFontSize,
                        isBold: true,
                        alignCenter: true);
                    PlacePeriodLine(
                            layers,
                            data.PeriodText,
                            x: 430,
                            y: 845,
                            width: 1140,
                            height: 45,
                            fontSize: 30);

                    //PlaceText(
                    //    layers,
                    //    "FROM THE PERIOD",
                    //    x: 500,
                    //    y: 845,
                    //    width: 360,
                    //    height: 45,
                    //    fontFamily: BodyFont,
                    //    fontSize: 32,
                    //    isBold: false,
                    //    alignCenter: true);

                    //PlaceText(
                    //    layers,
                    //    data.PeriodText,
                    //    x: 825,
                    //    y: 845,
                    //    width: 410,
                    //    height: 45,
                    //    fontFamily: BodyFont,
                    //    fontSize: periodFontSize,
                    //    isBold: true,
                    //    alignCenter: true);

                    //PlaceText(
                    //    layers,
                    //    "AT",
                    //    x: 1225,
                    //    y: 845,
                    //    width: 100,
                    //    height: 45,
                    //    fontFamily: BodyFont,
                    //    fontSize: 32,
                    //    isBold: true,
                    //    alignCenter: true);

                    PlaceText(
                        layers,
                        "HCC TECHNOLOGY FOUNDATION",
                        x: 515,
                        y: 920,
                        width: 1000,
                        height: 55,
                        fontFamily: BodyFont,
                        fontSize: 38,
                        isBold: true,
                        alignCenter: true);
                });
            }
            private static void PlacePeriodLine(
                    LayersDescriptor layers,
                    string periodText,
                    float x,
                    float y,
                    float width,
                    float height,
                    float fontSize)
            {
                layers.Layer().Element(container =>
                {
                    var box = container
                        .Unconstrained()
                        .TranslateX(x)
                        .TranslateY(y)
                        .Width(width)
                        .Height(height)
                        .AlignMiddle()
                        .AlignCenter();

                    var normalStyle = TextStyle.Default
                        .FontFamily(BodyFont)
                        .FontSize(fontSize)
                        .FontColor(Colors.Black);

                    var boldStyle = TextStyle.Default
                        .FontFamily(BodyFont)
                        .FontSize(fontSize)
                        .FontColor(Colors.Black)
                        .Bold();

                    box.Text(text =>
                    {
                        text.Span("FROM THE PERIOD ").Style(normalStyle);
                        text.Span(periodText ?? string.Empty).Style(boldStyle);
                        text.Span(" AT").Style(normalStyle);
                    });
                });
            }
            private static float GetStudentNameFontSize(string? value)
            {
                var length = string.IsNullOrWhiteSpace(value)
                    ? 0
                    : value.Trim().Length;

                if (length > 42)
                    return 46;

                if (length > 36)
                    return 50;

                if (length > 30)
                    return 56;

                if (length > 24)
                    return 62;

                if (length > 18)
                    return 72;

                return 80;
            }

            private static float GetCourseTitleFontSize(string? value)
            {
                var length = string.IsNullOrWhiteSpace(value)
                    ? 0
                    : value.Trim().Length;

                if (length > 45)
                    return 34;

                if (length > 36)
                    return 38;

                if (length > 28)
                    return 42;

                return 48;
            }

            private static float GetPeriodFontSize(string? value)
            {
                var length = string.IsNullOrWhiteSpace(value)
                    ? 0
                    : value.Trim().Length;

                if (length > 25)
                    return 24;

                if (length > 20)
                    return 27;

                return 30;
            }

            private static void PlaceText(
                LayersDescriptor layers,
                string text,
                float x,
                float y,
                float width,
                float height,
                string fontFamily,
                float fontSize,
                bool isBold,
                bool alignCenter)
            {
                layers.Layer().Element(container =>
                {
                    var box = container
                        .Unconstrained()
                        .TranslateX(x)
                        .TranslateY(y)
                        .Width(width)
                        .Height(height)
                        .AlignMiddle();

                    box = alignCenter
                        ? box.AlignCenter()
                        : box.AlignLeft();

                    var style = TextStyle.Default
                        .FontFamily(fontFamily)
                        .FontSize(fontSize)
                        .FontColor(Colors.Black);

                    if (isBold)
                    {
                        style = style.Bold();
                    }

                    box.Text(text ?? string.Empty)
                        .Style(style);
                });
            }
        }
    }
}