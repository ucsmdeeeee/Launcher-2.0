using System;
using System.Windows;
using Telegram.Bot;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Media;
using LauncherNew.ViewModels;
using System.Net.Http;
using System.Text;
using System.Text.Encodings;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Text;
using System.Text.Json;
using System.Text.Json;

namespace LauncherNew.Views.Pages
{
    public partial class AuthorizationPage : Page
    {
        private string _authCode;
        private string _userId;
        private readonly HttpClient _httpClient;
        private readonly Storyboard _cursorStoryboard;
        private readonly TranslateTransform _cursorTransform;
        public AuthorizationPage()
        {
            InitializeComponent();

            // Инициализация HTTP-клиента
            _httpClient = new HttpClient();
            
            CompositionTarget.Rendering += UpdateMousePointerPosition;

            // Инициализация TranslateTransform для курсора
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
        

        private async void BtnSendAuthCode_Click(object sender, RoutedEventArgs e)
        {
            _userId = txtUserId.Text;

            if (string.IsNullOrEmpty(_userId))
            {
                MessageBox.Show("Введите Telegram ID.");
                return;
            }

            if (!long.TryParse(_userId, out long telegramId))
            {
                MessageBox.Show("Telegram ID должен быть числом.");
                return;
            }

            try
            {
                // Проверяем наличие пользователя в базе данных
                var database = new Database("Host=93.158.195.13;Port=5432;Username=postgres;Password=1548;Database=students");
                var user = await database.GetUserByTelegramIdAsync(telegramId);

                if (user == null)
                {
                    MessageBox.Show("Пользователь с таким Telegram ID не найден!");
                    return;
                }
                // Сохраняем Telegram ID в файл
                SaveTelegramIdToFile(telegramId);
                // Генерируем код авторизации
                _authCode = GenerateAuthCode();

                // Отправка кода на бот-сервер
                var payload = new
                {
                    telegramId = _userId,
                    authCode = _authCode
                };

                var json = JsonSerializer.Serialize(payload);

                var response = await _httpClient.PostAsync(
                    "http://93.158.195.13:8080/send-code",
                    new StringContent(json, Encoding.UTF8, "application/json")
                );

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Код отправлен в Telegram!");
                }
                else
                {
                    MessageBox.Show("Ошибка отправки кода. Проверьте Telegram ID.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }




        private void TxtUserId_TextChanged(object sender, TextChangedEventArgs e)
        {
            PlaceholderText.Visibility = string.IsNullOrEmpty(txtUserId.Text) ? Visibility.Visible : Visibility.Hidden;
        }

        private void TxtAuthCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            PlaceholderText2.Visibility = string.IsNullOrEmpty(txtAuthCode.Text) ? Visibility.Visible : Visibility.Hidden;
        }

        // AuthorizationPage.xaml.cs
        private void BtnVerifyCode_Click(object sender, RoutedEventArgs e)
        {
            if (txtAuthCode.Text == _authCode)
            {
                MessageBox.Show("Авторизация успешна!");

                long telegramId = long.Parse(txtUserId.Text); // Убедитесь, что здесь корректное значение

                // Сохраняем Telegram ID в файл
                SaveTelegramIdToFile(telegramId);

                var mainViewModel = (MainViewModel)Application.Current.MainWindow.DataContext;
                mainViewModel.ShowProfile();
            }
            else
            {
                MessageBox.Show("Неверный код!");
            }
        }

        private void SaveTelegramIdToFile(long telegramId)
        {
            try
            {
                // Используем AppData для хранения файла
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LauncherNew");
                string filePath = Path.Combine(directoryPath, "tgid.txt");

                // Убедимся, что папка существует
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Записываем Telegram ID в файл
                File.WriteAllText(filePath, telegramId.ToString());
                Console.WriteLine($"Telegram ID {telegramId} сохранен в файл {filePath}.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении Telegram ID: {ex.Message}");
            }
        }








        private string GenerateAuthCode()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
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


        
        
        private void UpperPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Получаем главное окно и вызываем DragMove
                Window.GetWindow(this)?.DragMove();
            }
        }



        private void LauncherHideButton_Click(object sender, RoutedEventArgs e)
        {
            ((AuthorizationViewModel)this.DataContext).HideLauncher();
        }



        private void LauncherCloseButton_Click(object sender, RoutedEventArgs e)
        {
            ((AuthorizationViewModel)this.DataContext).CloseLauncher();
        }

    }
        
    
}
