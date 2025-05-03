
using PdfSharpCore.Pdf.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using Tesseract;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Writer;
using UglyToad.PdfPig.XObjects;
using iText.Kernel.Pdf;



public class PdfConverter
{
    public Pix ConvertPdfToPix(Stream pdfStream)
    {
        if (pdfStream.CanSeek)
        {
            pdfStream.Position = 0; // Сброс позиции потока
        }
        using (var document = UglyToad.PdfPig.PdfDocument.Open(pdfStream))
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
        using (UglyToad.PdfPig.PdfDocument document = UglyToad.PdfPig.PdfDocument.Open(pdfStream))
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


    public Pix ExtractImagesToTiff(Stream pdfStream)
    {
        if (pdfStream.CanSeek)
        {
            pdfStream.Position = 0; // Сброс позиции потока
        }

        using var document = UglyToad.PdfPig.PdfDocument.Open(pdfStream);
        using var outputMemoryStream = new MemoryStream(); // Для записи TIFF результата
        //using var tiffEncoder = new TiffEncoder(); // Настройки для TIFF

        foreach (var page in document.GetPages())
        {
            foreach (var pdfImage in page.GetImages())
            {
                var imageBytes = TryGetImageBytes(pdfImage);
                if (imageBytes == null) continue;

                using var imageStream = new MemoryStream(imageBytes);
                var image = Image.Load<Rgba32>(imageStream);

                using (var stream = new MemoryStream())
                {
                    image.Save(stream, new TiffEncoder());
                    stream.Position = 0;
                    // Загружаем изображение в формате Tiff в Pix
                    return Pix.LoadTiffFromMemory(stream.ToArray());
                }

                // Конвертируем изображение в TIFF и добавляем в результат
               // image.Save(outputMemoryStream, tiffEncoder);
            }
        }

        return null;
    }

    private byte[] TryGetImageBytes(IPdfImage image)
    {
        if (image.TryGetPng(out var pngBytes))
        {
            return pngBytes;
        }

        if (image.RawBytes != null && image.RawBytes.Length > 0)
        {
            return image.RawBytes.ToArray();
        }

        return null; // Если невозможно извлечь изображение
    }


    /// <summary>
    /// Разбивает PDF-файл на страницы и возвращает список файлов в формате байтовых массивов.
    /// </summary>
    public List<byte[]> SplitPdfPages(Stream pdfStream)
    {
        var resultFiles = new List<byte[]>();

        // Загружаем PDF в MemoryStream, чтобы избежать конфликтов
        using var memoryStream = new MemoryStream();
        pdfStream.CopyTo(memoryStream);
        memoryStream.Position = 0; // Сбрасываем позицию в начало

        using var reader = new iText.Kernel.Pdf.PdfReader(memoryStream);
        using var pdfDocument = new iText.Kernel.Pdf.PdfDocument(reader);

        int pageCount = pdfDocument.GetNumberOfPages();

        for (int i = 1; i <= pageCount; i++)
        {
            using var outputStream = new MemoryStream();
            using var writer = new iText.Kernel.Pdf.PdfWriter(outputStream);
            using var newPdf = new iText.Kernel.Pdf.PdfDocument(writer);

            // Копируем страницу
            pdfDocument.CopyPagesTo(i, i, newPdf);

            newPdf.Close();
            resultFiles.Add(outputStream.ToArray());
        }

        return resultFiles;
    }

}
