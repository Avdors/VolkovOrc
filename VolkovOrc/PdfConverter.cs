
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Tesseract;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.XObjects;



public class PdfConverter
{
    public Pix ConvertPdfToPix(Stream pdfStream)
    {
        using (var document = PdfDocument.Open(pdfStream))
        {
            var page = document.GetPage(1);
            var images = page.GetImages();

            foreach (var image in images)
            {
                if (image.TryGetPng(out var bytes))
                {
                    // Конвертируем PNG bytes в ImageSharp Image
                    using (var imageStream = new MemoryStream(bytes))
                    {
                        var img = Image.Load<Rgba32>(imageStream);

                        // Сохраняем изображение в поток как TIFF
                        using (var stream = new MemoryStream())
                        {
                            img.Save(stream, new TiffEncoder());
                            stream.Position = 0;
                            // Загружаем изображение в формате Tiff в Pix
                            return Pix.LoadTiffFromMemory(stream.ToArray());
                        }
                    }
                }
            }
        }

        return null; // Возвращаем null, если изображений нет
    }

    public String ConverPdfToString(Stream pdfStream)
    {
        StringBuilder documentText = new StringBuilder();
        using (PdfDocument document = PdfDocument.Open(pdfStream))
        {
            foreach (UglyToad.PdfPig.Content.Page page in document.GetPages())
            {
                string pageText = page.Text;
                documentText.AppendLine(pageText); // добавляю текст в стрингбилдер

                foreach (Word word in page.GetWords())
                {
                    documentText.Append(word.Text + " ");
                }
                documentText.AppendLine(); // пвзделитель между страницами
            }



        }

            return documentText.ToString();
    }

}
