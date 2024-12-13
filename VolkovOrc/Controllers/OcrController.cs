using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using Tesseract;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Processing;
using static System.Net.Mime.MediaTypeNames;





namespace VolkovOrc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OcrController : ControllerBase
    {
        private readonly TesseractEngine _ocrEngine;
        private readonly PdfConverterBitMiracl _pdfConverter;

        public OcrController()
        {
            _ocrEngine = new TesseractEngine(@"./tessdata", "eng+rus", EngineMode.Default);
            _pdfConverter = new PdfConverterBitMiracl();
        }

        [HttpPost("upload")]
        public IActionResult Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            try
            {
                Pix image = null;
                String textPdf = "не удалось прочесть";

                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);
                    stream.Position = 0; // Reset stream position before reading

                    switch (fileExtension)
                    {
                        case ".pdf":
                            // Специальная обработка PDF
                            textPdf = _pdfConverter.ConverPdfToString(stream);
                            if(textPdf != null)
                            {
                                break;
                            }
                            image = _pdfConverter.ConvertPdfToPix(stream);
                            break;
                        case ".jpeg":
                        case ".jpg":
                        case ".png":
                        case ".tiff":
                        case ".bmp":
                            // Обработка изображений через ImageSharp
                            using (var img = SixLabors.ImageSharp.Image.Load(stream))
                            {
                                img.Mutate(x => x.BackgroundColor(Color.White));
                                using (var ms = new MemoryStream())
                                {
                                    img.SaveAsTiff(ms, new TiffEncoder());
                                    ms.Position = 0;
                                    image = Pix.LoadTiffFromMemory(ms.ToArray());
                                }
                            }
                            break;
                        default:
                            return BadRequest("Unsupported file type.");
                    }


                    if (image != null)
                    {
                        using (var page = _ocrEngine.Process(image))
                        {
                            var text = page.GetText();
                            return Ok(text);
                        }
                    }
                    else { return Ok(textPdf); }

                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}