using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;
using MvvmCross.ViewModels;
using LauncherNew.Models;
using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.Version;
using CmlLib.Core.VersionLoader;
using LauncherNew.ViewModels;
using LauncherNew.Views.Pages;

namespace LauncherNew.ViewModels
{
    public class ProfileViewModel :MvxViewModel,INotifyPropertyChanged
    {
        private readonly Database _database; // Класс для работы с базой данных

        private string _fullName;
        private string _email;
        private string _groupName;
        private string _team;
        private string _department;
        private string _nickname;

        public string Nickname
        {
            get => _nickname;
            set
            {
                _nickname = value;
                OnPropertyChanged();
            }
        }
        public string FullName
        {
            get => _fullName;
            set
            {
                _fullName = value;
                OnPropertyChanged();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
            }
        }

        public string GroupName
        {
            get => _groupName;
            set
            {
                _groupName = value;
                OnPropertyChanged();
            }
        }

        public string Team
        {
            get => _team;
            set
            {
                _team = value;
                OnPropertyChanged();
            }
        }

        public string Department
        {
            get => _department;
            set
            {
                _department = value;
                OnPropertyChanged();
            }
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


        public ProfileViewModel()
        {
            try
            {
                _database = new Database("Host=93.158.195.13;Port=5432;Username=postgres;Password=1548;Database=students");
                long telegramId = GetTelegramIdFromFile();
                LoadUserData(telegramId);
                Console.WriteLine($"ProfileViewModel успешно инициализирован.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в ProfileViewModel: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }


        private async void LoadUserData(long telegramId)
        {
            try
            {
                Console.WriteLine($"Загрузка данных пользователя с Telegram ID: {telegramId}");
                var user = await _database.GetUserByTelegramIdAsync(telegramId);

                if (user != null)
                {
                    Nickname = user.Nickname;
                    FullName = user.FullName;
                    Email = user.Email;
                    GroupName = user.GroupName;
                    Team = user.Team;
                    Department = user.Department;
                }
                else
                {
                    Console.WriteLine($"Пользователь с Telegram ID {telegramId} не найден.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке данных пользователя: {ex.Message}");
                throw;
            }
        }

        public async Task HideLauncher()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Application.Current.MainWindow.WindowState = WindowState.Minimized;
            });
        }

        public async Task CloseLauncher()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Application.Current.MainWindow.Close(); 
                
            });
        }

        private readonly MainViewModel _mainViewModel;

        public ProfileViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
