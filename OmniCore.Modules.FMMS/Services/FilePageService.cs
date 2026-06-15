using iText.Kernel.Pdf;

namespace OmniCore.Modules.FMMS.Services
{
    internal static class FilePageService
    {
        public static int GetPagesCountInPdf(string filePath)
        {
            using FileStream stream = new(filePath, FileMode.Open, FileAccess.Read);

            using PdfReader pdfReader = new(stream);
            using PdfDocument pdf = new(pdfReader);
            return pdf.GetNumberOfPages();
        }

        public static int GetPagesCountInPdf(Stream stream)
        {
            using PdfReader pdfReader = new(stream);
            using PdfDocument pdf = new(pdfReader);
            return pdf.GetNumberOfPages();
        }
    }
}
