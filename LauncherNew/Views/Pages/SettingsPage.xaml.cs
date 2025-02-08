using System.IO;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Media;
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
                // Путь к файлу tgid.txt
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"..", "..", "..", "Views", "Resources", "tgid.txt");

                // Проверяем, существует ли файл
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Файл {filePath} не найден.");
                }

                // Читаем содержимое файла
                string content = File.ReadAllText(filePath);

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
                string soundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Views", "Resources", "hover.mp3");

                if (!System.IO.File.Exists(soundPath))
                {
                    MessageBox.Show($"Файл звука не найден: {soundPath}");
                    return;
                }

                // Установка пути к файлу и включение повтора
                _mediaPlayer.Open(new Uri(soundPath, UriKind.Absolute));
                _mediaPlayer.MediaEnded += (s, ev) =>
                {
                    _mediaPlayer.Position = TimeSpan.Zero; // Сброс к началу
                    _mediaPlayer.Play(); // Повтор воспроизведения
                };

                _mediaPlayer.Play();
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
                _mediaPlayer.Stop(); // Остановка воспроизведения
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка остановки звука: {ex.Message}");
            }
        }

        private void PlaySound(string soundPath)
        {
            try
            {
                if (!System.IO.File.Exists(soundPath))
                {
                    MessageBox.Show($"Файл звука не найден: {soundPath}");
                    return;
                }

                _mediaPlayer.Open(new Uri(soundPath, UriKind.Absolute));
                _mediaPlayer.MediaEnded += (s, ev) =>
                {
                    _mediaPlayer.Position = TimeSpan.Zero;
                    _mediaPlayer.Play();
                };
                _mediaPlayer.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка воспроизведения звука: {ex.Message}");
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
