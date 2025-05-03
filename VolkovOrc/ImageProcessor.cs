using ImageMagick;
using OpenCvSharp;
using System.IO;
using Tesseract;

public class ImageProcessor
{
    public byte[] PreprocessImage(Stream imageStream)
    {
        // Читаем изображение из потока
        using var memoryStream = new MemoryStream();
        imageStream.CopyTo(memoryStream);
        var inputArray = memoryStream.ToArray();

        // Загружаем изображение в OpenCV
        using var src = Cv2.ImDecode(inputArray, ImreadModes.Grayscale);

        if (src.Empty())
            throw new InvalidDataException("Failed to load image.");

        // Увеличиваем контрастность и четкость
        using var enhanced = new Mat();
        Cv2.EqualizeHist(src, enhanced);

        // Убираем шум
        using var denoised = new Mat();
        Cv2.GaussianBlur(enhanced, denoised, new Size(3, 3), 0);

        // Преобразуем в черно-белое изображение
        using var binary = new Mat();
        Cv2.Threshold(denoised, binary, 0, 255, ThresholdTypes.Otsu);

        // Кодируем обработанное изображение в TIFF
        return binary.ToBytes(".tiff");
    }

    //это Magick.NET-Q8-AnyCPU
    public byte[] ProcessImage(byte[] inputImage)
    {
        using (var image = new MagickImage(inputImage))
        {
            // Установите плотность для PDF (если требуется)
            image.Density = new Density(300);

            // Обрезка изображения (по желанию)
            //image.Crop(new MagickGeometry(50, 50, image.Width - 100, image.Height - 100));
            image.AutoGamma();
            // Преобразование в оттенки серого
            image.ColorType = ColorType.Grayscale;

            // Улучшение контрастности
            image.Normalize();

            // Очистка фона
            image.Level(new Percentage(50), new Percentage(10));

            // Резкость
            //image.Sharpen(0, 1.0);

            // Удаление лишних границ (trim)
            image.Trim();
            // Сохранение результата в формате TIFF
            using (var memoryStream = new MemoryStream())
            {
                image.Write(memoryStream, MagickFormat.Tiff);
                return memoryStream.ToArray();
            }
            // Сохранение результата
           // return image.ToByteArray();
        }
    }

    public byte[] EnhanceImage(byte[] imageBytes)
    {
        using var src = Cv2.ImDecode(imageBytes, ImreadModes.Grayscale);
        if (src.Empty())
            throw new InvalidDataException("Failed to load image.");

        // Увеличиваем контрастность
        using var contrastEnhanced = new Mat();
        Cv2.EqualizeHist(src, contrastEnhanced);

        // Убираем шум
        using var denoised = new Mat();
        Cv2.GaussianBlur(contrastEnhanced, denoised, new Size(3, 3), 0);

        // Преобразуем в черно-белое изображение
        using var binary = new Mat();
        Cv2.Threshold(denoised, binary, 0, 255, ThresholdTypes.Otsu);

        // Конвертируем обработанное изображение в TIFF
        return binary.ToBytes(".tiff");
    }

    // Конвертация Pix в массив байтов
    public byte[] PixToByteArray(Pix pix)
    {
        // Временный файл для сохранения Pix
        var tempFile = Path.GetTempFileName();

        try
        {
            // Сохраняем Pix во временный файл
            pix.Save(tempFile, ImageFormat.TiffG4);

            // Читаем данные из файла в массив байтов
            return File.ReadAllBytes(tempFile);
        }
        finally
        {
            // Удаляем временный файл
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    // Конвертация массива байтов обратно в Pix
    public Pix ByteArrayToPix(byte[] imageBytes)
    {
        return Pix.LoadTiffFromMemory(imageBytes);
    }
}
