using System;
using System.IO;
using System.Text;
using UglyToad.PdfPig;

namespace PdfReader
{
    class Program
    {
        static void Main(string[] args)
        {
            string pdfPath = @"d:\OneDrive - Rheal Software Pvt Ltd\Documents\Visual Studio 2015\Projects\TMS\UI Standards.pdf";
            string outputPath = @"d:\OneDrive - Rheal Software Pvt Ltd\Documents\Visual Studio 2015\Projects\TMS\scratch\ui_standards_text.txt";

            try
            {
                using (var document = PdfDocument.Open(pdfPath))
                {
                    // Explicitly use UTF-8 with encoder fallback to replace invalid chars instead of throwing
                    var encoding = Encoding.GetEncoding("utf-8", new EncoderReplacementFallback("?"), new DecoderReplacementFallback("?"));
                    using (var writer = new StreamWriter(outputPath, false, encoding))
                    {
                        foreach (var page in document.GetPages())
                        {
                            try
                            {
                                writer.WriteLine($"--- Page {page.Number} ---");
                                string text = page.Text;
                                writer.WriteLine(text);
                                writer.WriteLine();
                            }
                            catch (Exception pageEx)
                            {
                                writer.WriteLine($"--- Page {page.Number} (ERROR) ---");
                                writer.WriteLine($"Failed to read page: {pageEx.Message}");
                                writer.WriteLine();
                                Console.WriteLine($"Error on Page {page.Number}: {pageEx.ToString()}");
                            }
                        }
                    }
                }
                Console.WriteLine("PDF text extraction finished.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading PDF: " + ex.ToString());
            }
        }
    }
}
