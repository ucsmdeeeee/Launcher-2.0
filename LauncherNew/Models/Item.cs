using System;
using System.Windows.Media.Imaging;

namespace LauncherNew.Models
{
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImagePath { get; set; } // Путь к изображению
        public BitmapImage Image
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ImagePath))
                    return null;

                try
                {
                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.UriSource = new Uri(ImagePath, UriKind.RelativeOrAbsolute);
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    return bitmapImage;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка загрузки изображения: {ex.Message}");
                    return null;
                }
            }
        }
        public int Price { get; set; }
        public string MineName { get; set; }
        public string Category { get; set; }
        public int Quantity { get; set; } = 1; // Количество предметов
    }
}