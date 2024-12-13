using BitMiracle.Docotic.Pdf;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Tiff;
using System.Text;
using Tesseract;

//namespace VolkovOrc
//{
    public class PdfConverterBitMiracl
    {
        public String ConverPdfToString(Stream pdfStream)
        {
            var documentText = new StringBuilder();
            using (var pdf = new PdfDocument(pdfStream))
            {
                for (int i = 0; i < pdf.PageCount; ++i)
                {
                    if (documentText.Length > 0)
                        documentText.Append("\r\n\r\n");

                    PdfPage page = pdf.Pages[i];
                    string searchableText = page.GetText();
                    if (!string.IsNullOrEmpty(searchableText.Trim()))
                    {
                        documentText.Append(searchableText);
                        continue;
                    }


                    return null;
                    // TODO: This page is not searchable. Perform OCR here
                }
            }

            return null;
        }

        public Pix ConvertPdfToPix(Stream pdfStream)
        {
            using (var pdf = new PdfDocument(pdfStream))
            {
                for (int i = 0; i < pdf.PageCount; i++)
                {
                    PdfPage page = pdf.Pages[i];
                    PdfDrawOptions options = PdfDrawOptions.Create();
                    options.BackgroundColor = new PdfRgbColor(255, 255, 255); // Ensure white background
                    options.HorizontalResolution = 300;
                    options.VerticalResolution = 300;

                    using (var pageImageStream = new MemoryStream())
                    {
                        page.Save(pageImageStream, options);
                        pageImageStream.Position = 0; // Reset stream position

                        using (var image = Image.Load(pageImageStream))
                        {
                            using (var tiffStream = new MemoryStream())
                            {
                                image.SaveAsTiff(tiffStream, new TiffEncoder()); // Saving as TIFF
                                tiffStream.Position = 0;
                                return Pix.LoadTiffFromMemory(tiffStream.ToArray()); // Load TIFF into Pix for OCR
                            }
                        }
                    }
                }
            }

            return null; // Return null if no pages processed or no images found
        }
    }
    
