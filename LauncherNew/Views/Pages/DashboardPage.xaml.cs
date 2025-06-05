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
    public partial class DashboardPage : Page
    {
        private readonly Storyboard _cursorStoryboard;
        private readonly TranslateTransform _cursorTransform;

        // В DashboardPage.xaml.cs
        // В DashboardPage.xaml.cs
         // Убираем telegramId из параметров конструктора
        public DashboardPage()
        {
            InitializeComponent();
            DataContext = new DashboardViewModel();
            // Читаем Telegram ID из файла
            long telegramId = GetTelegramIdFromFile();
            DataContext = new DashboardViewModel();
            Console.WriteLine($"Начало вызова конструктора DashboardPage с Telegram ID: {telegramId}.");

            try
            {
                Console.WriteLine($"Перед установкой DataContext с Telegram ID: {telegramId}");
                Console.WriteLine("DataContext успешно установлен.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при установке DataContext: {ex.Message}");
                throw;
            }

            if (DataContext == null)
            {
                Console.WriteLine("Ошибка: DataContext не установлен!");
            }
            else
            {
                Console.WriteLine("DataContext установлен успешно.");
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



        // Обработчик MouseMove
        private void UpdateMousePointerPosition(object sender, EventArgs e)
        {
            Point position = Mouse.GetPosition(this);

            // Обновление позиции TranslateTransform напрямую
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

        private void PlayPortalSound(object sender, MouseEventArgs e)
        {
            try
            {
                string resourceName = "LauncherNew.Views.Resources.portal.mp3"; // Namespace + путь
                var assembly = Assembly.GetExecutingAssembly();

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        MessageBox.Show("Ресурс не найден!");
                        return;
                    }

                    string tempFile = Path.Combine(Path.GetTempPath(), "portal.mp3");

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

        private void StopPortalSound(object sender, MouseEventArgs e)
        {
            try
            {
                _mediaPlayer.Stop(); // Остановка воспроизведения
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
                // Получаем главное окно и вызываем DragMove
                Window.GetWindow(this)?.DragMove();
            }
        }

        private void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            ((DashboardViewModel)this.DataContext).LaunchMinecraft();
        }

        private void LauncherHideButton_Click(object sender, RoutedEventArgs e)
        {
            ((DashboardViewModel)this.DataContext).HideLauncher();
        }
        // В DashboardPage.xaml.cs
        


        private void LauncherCloseButton_Click(object sender, RoutedEventArgs e)
        {
            ((DashboardViewModel)this.DataContext).CloseLauncher();
        }
        private void SettingsOpenButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsPage = new SettingsPage();
            ((MainViewModel)Application.Current.MainWindow.DataContext).CurrentPage = settingsPage;
            
        }
        private void ProfileOpenButton_Click(object sender, RoutedEventArgs e)
        {
            var profilePage = new ProfilePage();
            ((MainViewModel)Application.Current.MainWindow.DataContext).CurrentPage = profilePage;
            
        }
        private void ShopShowButton_Click(object sender, RoutedEventArgs e)
        {
                var shopPage = new ShopPage();
                ((MainViewModel)Application.Current.MainWindow.DataContext).CurrentPage = shopPage;
        }
    }
}