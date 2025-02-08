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
                // Путь к файлу tgid.txt
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Views", "Resources", "tgid.txt");

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

        private void PlayPortalSound(object sender, MouseEventArgs e)
        {
            try
            {
                string soundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Views", "Resources", "portal.mp3");

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