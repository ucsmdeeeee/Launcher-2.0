using System.IO;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Media;
using System.Reflection;
using LauncherNew.ViewModels;

namespace LauncherNew.Views.Pages
{
    public partial class SettingsPage : Page
    {
        private readonly Storyboard _cursorStoryboard;
        private readonly TranslateTransform _cursorTransform;
        private readonly DashboardViewModel _dashboardViewModel;
        public SettingsPage()
        {
            _dashboardViewModel = new DashboardViewModel();
            long telegramId = GetTelegramIdFromFile();
            // Если telegramId равен 0, читаем его из файла
            if (telegramId == 0)
            {
                 telegramId = GetTelegramIdFromFile();
            }
            Console.WriteLine($"Начало вызова конструктора ProfilePage с Telegram ID: {telegramId}.");

            try
            {
                InitializeComponent();
                Console.WriteLine("InitializeComponent выполнен успешно.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при вызове InitializeComponent: {ex.Message}");
                throw;
            }
            DataContext = new SettingsViewModel();
            try
            {
                Console.WriteLine($"Перед установкой DataContext с Telegram ID: {telegramId}");
                // Создаем ProfileViewModel вручную и передаем telegramId

                Console.WriteLine("DataContext успешно установлен.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при установке DataContext: {ex.Message}");
                throw;
            }

            CompositionTarget.Rendering += UpdateMousePointerPosition;
            _cursorTransform = new TranslateTransform();
            MousePointer.RenderTransform = _cursorTransform;

            // Создание анимации для плавного перемещения курсора
            _cursorStoryboard = new Storyboard();
            DoubleAnimation xAnimation = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(50),
                AccelerationRatio = 0.5,
                DecelerationRatio = 0.5
            };
            Storyboard.SetTarget(xAnimation, MousePointer);
            Storyboard.SetTargetProperty(xAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));

            DoubleAnimation yAnimation = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(50),
                AccelerationRatio = 0.5,
                DecelerationRatio = 0.5
            };
            Storyboard.SetTarget(yAnimation, MousePointer);
            Storyboard.SetTargetProperty(yAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));

            _cursorStoryboard.Children.Add(xAnimation);
            _cursorStoryboard.Children.Add(yAnimation);
        }
        


        private void SetRam4096_Click(object sender, RoutedEventArgs e)
        {
            _dashboardViewModel.MaximumRamMb = 4096;
            MessageBox.Show("Выставлено 4096 MB оперативной памяти.", "Подтверждение", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SetRam8192_Click(object sender, RoutedEventArgs e)
        {
            _dashboardViewModel.MaximumRamMb = 8192;
            MessageBox.Show("Выставлено 8192 MB оперативной памяти.", "Подтверждение", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SetRam12288_Click(object sender, RoutedEventArgs e)
        {
            _dashboardViewModel.MaximumRamMb = 12288;
            MessageBox.Show("Выставлено 12288 MB оперативной памяти.", "Подтверждение", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SetRam16384_Click(object sender, RoutedEventArgs e)
        {
            _dashboardViewModel.MaximumRamMb = 16384;
            MessageBox.Show("Выставлено 16384 MB оперативной памяти.", "Подтверждение", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private long GetTelegramIdFromFile()
        {
            try
            {
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LauncherNew");
                string filePath = Path.Combine(directoryPath, "tgid.txt");

                // Проверяем, существует ли папка, если нет — создаем
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Если файла нет, создаем пустой файл (или записываем 0)
                if (!File.Exists(filePath))
                {
                    File.WriteAllText(filePath, "0");
                    return 0;
                }

                // Читаем содержимое файла
                string content = File.ReadAllText(filePath).Trim();

                // Если файл пустой, считаем, что там 0
                if (string.IsNullOrEmpty(content))
                {
                    return 0;
                }

                // Парсим содержимое в long
                if (long.TryParse(content, out long telegramId))
                {
                    Console.WriteLine($"Прочитан Telegram ID из файла: {telegramId}");
                    return telegramId;
                }
                else
                {
                    throw new FormatException("Неверный формат Telegram ID в файле.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при чтении Telegram ID: {ex.Message}");
                return 0; // Возвращаем 0 в случае ошибки
            }
        }



        // Обработчик движения мыши
        private void UpdateMousePointerPosition(object sender, EventArgs e)
        {
            var position = Mouse.GetPosition(this);
            _cursorTransform.X = position.X - 16;
            _cursorTransform.Y = position.Y - 16;
        }

        private MediaPlayer _mediaPlayer = new MediaPlayer();

        private void PlayHoverSound(object sender, MouseEventArgs e)
        {
            try
            {
                string resourceName = "LauncherNew.Views.Resources.hover.mp3"; // Namespace + путь
                var assembly = Assembly.GetExecutingAssembly();

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        MessageBox.Show("Ресурс не найден!");
                        return;
                    }

                    string tempFile = Path.Combine(Path.GetTempPath(), "hover.mp3");

                    // Если файл уже существует, пытаемся удалить его
                    if (File.Exists(tempFile))
                    {
                        try
                        {
                            File.Delete(tempFile);
                        }
                        catch (IOException)
                        {
                            MessageBox.Show("Файл используется другим процессом!");
                            return;
                        }
                    }

                    // Записываем во временный файл
                    using (FileStream fs = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        stream.CopyTo(fs);
                    }

                    // Загружаем новый файл
                    _mediaPlayer.Open(new Uri(tempFile, UriKind.Absolute));
                    _mediaPlayer.Play();

            
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка воспроизведения звука: {ex.Message}");
            }
        }



        private void StopHoverSound(object sender, MouseEventArgs e)
        {
            try
            {
                _mediaPlayer.Stop();
                _mediaPlayer.Close(); // Освобождаем файл
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка остановки звука: {ex.Message}");
            }
        }

        private void UpperPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Window.GetWindow(this)?.DragMove();
            }
        }

        private void LauncherHideButton_Click(object sender, RoutedEventArgs e)
        {
            ((SettingsViewModel)this.DataContext).HideLauncher();
        }
        
        private void LauncherCloseButton_Click(object sender, RoutedEventArgs e)
        {
            ((SettingsViewModel)this.DataContext).CloseLauncher();
        }


        private void LauncherShowButton_Click(object sender, RoutedEventArgs e)
        {
            long telegramId = GetTelegramIdFromFile();
            ((MainViewModel)Application.Current.MainWindow.DataContext).ShowLauncher();
        }

        private void ProfileOpenButton_Click(object sender, RoutedEventArgs e)
        {
            ((MainViewModel)Application.Current.MainWindow.DataContext).ShowProfile();
        }
    }
}
