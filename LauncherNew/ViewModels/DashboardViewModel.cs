using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;
using MvvmCross.ViewModels;
using LauncherNew.Models;
using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.Installer.Forge;
using CmlLib.Core.VersionLoader;
using LauncherNew.ViewModels;
using LauncherNew.Views.Pages;
using System.Timers;
using CmlLib.Core.ProcessBuilder;
using Timer = System.Timers.Timer;

namespace LauncherNew.ViewModels;

public class DashboardViewModel : MvxViewModel, INotifyPropertyChanged
{
    private readonly MinecraftLauncher _launcher;
    private readonly MinecraftPath _minecraftPath = new MinecraftPath();
    private readonly Database _database;
    private readonly Timer _updateTimer;
    private readonly MinecraftRconService _rconService;
    private int _progressPercentage;
    private string _loadingStatus = string.Empty; // Устанавливаем начальное значение
    private bool _isLoadingVisible;

    public event PropertyChangedEventHandler? PropertyChanged; // Разрешаем `null`

    public int ProgressPercentage
    {
        get => _progressPercentage;
        set
        {
            _progressPercentage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ProgressBarWidth)); // Обновляем ProgressBarWidth
        }
    }

    public string LoadingStatus
    {
        get => _loadingStatus;
        set
        {
            _loadingStatus = value;
            OnPropertyChanged();
        }
    }

    public bool IsLoadingVisible
    {
        get => _isLoadingVisible;
        set
        {
            _isLoadingVisible = value;
            OnPropertyChanged();
        }
    }
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public double ProgressBarWidth => (ProgressPercentage / 100.0) * 200.0;
    public DashboardViewModel()
    {
        _launcher = new MinecraftLauncher (_minecraftPath);
        _database = new Database("Host=93.158.195.13;Port=5432;Username=postgres;Password=1548;Database=students");
        OnlinePlayers = new ObservableCollection<string>();
        Teammates = new ObservableCollection<Teammate>();
        _rconService = new MinecraftRconService("93.158.194.51", 25575, "1548");
        OnlinePlayers = new ObservableCollection<string>();
        Teammates = new ObservableCollection<Teammate>();

        // Таймер для периодического обновления данных
        _updateTimer = new Timer(5000); // Интервал: 5000 мс (5 секунд)
        _updateTimer.Elapsed += (s, e) => UpdateData();
        _updateTimer.Start();

        // Инициализация данных
        UpdateData();
        LoadOnlinePlayers();
        LoadTeammatesAsync();
        long telegramId = GetTelegramIdFromFile();
        InitializeUserDataAsync();
    }
    private async void UpdateData()
    {
        await LoadOnlinePlayers();
        await LoadTeammatesAsync();
    }
    private async void InitializeUserDataAsync()
    {
        long telegramId = GetTelegramIdFromFile();
        var user = await _database.GetUserByTelegramIdAsync(telegramId); 
        if (user == null)
        {
            Console.WriteLine($"Пользователь с telegramId: {telegramId} не найден.");
            return;
        }

        // Устанавливаем дополнительные параметры
        Console.WriteLine($"Пользователь найден: {user.FullName}");
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


    private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
    {
        // Получаем список всех файлов в исходной директории
        DirectoryInfo dir = new DirectoryInfo(sourceDirName);

        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException($"Исходная папка не найдена: {sourceDirName}");
        }

        DirectoryInfo[] dirs = dir.GetDirectories();
        Directory.CreateDirectory(destDirName);

        // Копируем все файлы из текущей директории
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            string tempPath = Path.Combine(destDirName, file.Name);
            file.CopyTo(tempPath, true); // Копируем с перезаписью
        }

        // Если необходимо, рекурсивно копируем поддиректории
        if (copySubDirs)
        {
            foreach (DirectoryInfo subdir in dirs)
            {
                string tempPath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
            }
        }
    }
    private void CopyMods(string sourceDirectory, string destinationDirectory)
    {
        try
        {
            // Проверяем, существует ли папка исходников
            if (!Directory.Exists(sourceDirectory))
            {
                throw new DirectoryNotFoundException($"Папка с модами не найдена: {sourceDirectory}");
            }

            // Путь к папке "mods" в папке назначения
            string modsDestination = Path.Combine(destinationDirectory, "mods");

            // Удаляем папку "mods" в папке назначения, если она существует
            if (Directory.Exists(modsDestination))
            {
                Directory.Delete(modsDestination, recursive: true); // Удаляем папку "mods" со всем содержимым
            }

            // Копируем всю папку "mods" целиком
            DirectoryCopy(sourceDirectory, modsDestination, true);

            Console.WriteLine($"Папка модов успешно скопирована из {sourceDirectory} в {modsDestination}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка копирования модов: {ex.Message}");
            throw;
        }
    }

private ObservableCollection<string> _onlinePlayers;
    public ObservableCollection<string> OnlinePlayers
    {
        get => _onlinePlayers;
        set
        {
            _onlinePlayers = value;
            OnPropertyChanged(nameof(OnlinePlayers));
        }
    }

    private ObservableCollection<Teammate> _teammates;
    public ObservableCollection<Teammate> Teammates
    {
        get => _teammates;
        set
        {
            _teammates = value;
            OnPropertyChanged(nameof(Teammates));
        }
    }

    private int _maximumRamMb = 4096; // Значение по умолчанию 4096 MB

    public int MaximumRamMb
    {
        get => _maximumRamMb;
        set
        {
            _maximumRamMb = value;
            OnPropertyChanged(nameof(MaximumRamMb));
        }
    }


    private int _onlinePlayerCount;
    public int OnlinePlayerCount
    {
        get => _onlinePlayerCount;
        set
        {
            _onlinePlayerCount = value;
            OnPropertyChanged(nameof(OnlinePlayerCount));
        }
    }

    private async Task LoadOnlinePlayers()
    {
        long telegramId = GetTelegramIdFromFile();
        try
        {
            var response = await _rconService.ExecuteCommandAsync("list");
    
            if (!string.IsNullOrEmpty(response))
            {
                // Разбираем строку "list"
                var parts = response.Split(':');
                if (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]))
                {
                    var players = parts[1]
                        ?.Split(',')
                        .Select(p => p.Trim())
                        .Where(p => !string.IsNullOrEmpty(p))
                        .ToList();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        OnlinePlayerCount = players?.Count ?? 0; // Устанавливаем количество игроков
                    });
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        OnlinePlayerCount = 0; // Если игроков нет
                    });
                }
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OnlinePlayerCount = 0; // Если сервер не вернул список
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при загрузке онлайн-игроков: {ex.Message}");
            Application.Current.Dispatcher.Invoke(() =>
            {
                OnlinePlayerCount = 0; // Обнуляем, если произошла ошибка
            });
        }
    }






    private async Task LoadTeammatesAsync()
    {
        try
        {
            // Получаем Telegram ID текущего пользователя
            long telegramId = GetTelegramIdFromFile();
            var user = await _database.GetUserByTelegramIdAsync(telegramId);

            if (user == null || string.IsNullOrEmpty(user.Team))
            {
                Console.WriteLine("Пользователь не найден или у него нет команды.");
                return;
            }

            Console.WriteLine($"Пользователь найден: {user.FullName}, Команда: {user.Team}");

            // Получаем список всех сокомандников
            var teammates = await _database.GetTeammatesAsync(user.Team);
            Console.WriteLine($"Найдено сокомандников: {teammates.Count()}");

            // Попробуем получить список онлайн игроков через RCON
            List<string> onlinePlayers = new List<string>();
            try
            {
                var response = await _rconService.ExecuteCommandAsync("list");
                onlinePlayers = response.Split(':')[1]?.Split(',').Select(p => p.Trim()).ToList() ?? new List<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении списка онлайн-игроков: {ex.Message}");
            }

            // Обновляем список сокомандников
            Application.Current.Dispatcher.Invoke(() =>
            {
                Teammates.Clear();

                foreach (var teammate in teammates)
                {
                    Teammates.Add(new Teammate
                    {
                        Nickname = teammate.Nickname,
                        IsOnline = onlinePlayers.Contains(teammate.Nickname) // Проверяем, онлайн ли сокомандник
                    });
                }

                Console.WriteLine($"Список сокомандников обновлен. Всего: {Teammates.Count}");
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при загрузке сокомандников: {ex.Message}");
        }
    }



    


    // Уничтожаем таймер при закрытии
    ~DashboardViewModel()
    {
        _updateTimer?.Dispose();
    }






    private void ExtractModsFolder(string targetDirectory)
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourcePrefix = "LauncherNew.Views.Resources.mods."; // Укажите правильный Namespace

            // Создаем целевую папку, если её нет
            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            // Получаем все файлы, вшитые в ресурсы (из `mods/`)
            var resourceNames = assembly.GetManifestResourceNames()
                .Where(r => r.StartsWith(resourcePrefix));

            foreach (var resourceName in resourceNames)
            {
                // Получаем относительный путь внутри `mods/`
                string relativePath = resourceName.Substring(resourcePrefix.Length);
                string destinationPath = Path.Combine(targetDirectory, relativePath);

                // Если это папка — создаем её
                string directoryPath = Path.GetDirectoryName(destinationPath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Если файл уже существует, пропускаем (чтобы не перезаписывать конфиги)
                if (File.Exists(destinationPath))
                {
                    continue;
                }

                // Копируем ресурс в файл
                using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
                using (FileStream fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
                {
                    resourceStream.CopyTo(fileStream);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка извлечения модов и конфигураций: {ex.Message}");
        }
    }








    public async Task LaunchMinecraft()
    {
        try
        {
            IsLoadingVisible = true;
            System.Net.ServicePointManager.DefaultConnectionLimit = 256;

            LoadingStatus = "Инициализация лаунчера...";
            ProgressPercentage = 10;
            await Task.Delay(2000);

            // Версии Minecraft и Forge
            string minecraftVersion = "1.20.1";
            string forgeVersion = "47.4.2"; // Только номер версии Forge

            // Создаем кастомный путь
            string customMinecraftPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "ItHubMineraft",
                $"{minecraftVersion}_Forge"
            );
            Directory.CreateDirectory(customMinecraftPath);

            // Инициализация пути и лаунчера
            var path = new MinecraftPath(customMinecraftPath);
            var launcher = new MinecraftLauncher(path);

            LoadingStatus = "Установка Forge...";
            ProgressPercentage = 20;

            // Создаем Forge Installer
            var forge = new ForgeInstaller(launcher);

            // Устанавливаем Forge
            string versionName = await forge.Install(minecraftVersion, forgeVersion, new ForgeInstallOptions
            {

            });

            LoadingStatus = "Копирование модов...";
            ProgressPercentage = 60;

            // Копируем моды
            string modsDestinationPath = Path.Combine(customMinecraftPath, "mods");
            ExtractModsFolder(modsDestinationPath);

            LoadingStatus = "Подготовка к запуску...";
            ProgressPercentage = 70;

            // Получаем данные пользователя
            long telegramId = GetTelegramIdFromFile();
            var user = await _database.GetUserByTelegramIdAsync(telegramId)
                ?? throw new Exception("Пользователь не найден в базе данных.");

            string nickname = user.Nickname;

            LoadingStatus = "Авторизация на сервере...";
            ProgressPercentage = 75;

            try
            {
                // Фиксированный токен с бесконечным сроком действия
                string fixedToken = "qwe123asd456zxc7890";
                string command = $"launcher-auth add {nickname} {fixedToken} 2099-12-31_23:59:59";

                // Используем существующий RCON сервис
                string response = await _rconService.ExecuteCommandAsync(command);
                Debug.WriteLine($"RCON ответ: {response}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка отправки RCON команды: {ex.Message}");
                // Продолжаем выполнение несмотря на ошибку RCON
            }

            // Настройки запуска
            var launchOption = new MLaunchOption
            {
                MaximumRamMb = MaximumRamMb,
                Session = MSession.CreateOfflineSession(nickname),
            };

            LoadingStatus = "Запуск Minecraft Forge...";
            ProgressPercentage = 80;
            await Task.Delay(5000);

            // Запуск игры
            var process = await launcher.InstallAndBuildProcessAsync(versionName, launchOption);

            LoadingStatus = "Запуск, приятной игры!";
            ProgressPercentage = 100;

            // Запускаем процесс с логированием
            var processWrapper = new ProcessWrapper(process);
            processWrapper.OutputReceived += (s, e) => Debug.WriteLine($"Minecraft: {e}");
            processWrapper.StartWithEvents();

            await Task.Delay(15000);
            IsLoadingVisible = false;
        }
        catch (Exception ex)
        {
            LoadingStatus = $"Ошибка: {ex.Message}";
            ProgressPercentage = 0;
            IsLoadingVisible = true;
            Debug.WriteLine($"Ошибка запуска Minecraft: {ex.Message}");
        }
    }

    // Вспомогательный метод для копирования директорий
    private void CopyDirectory(string sourceDir, string destinationDir)
    {
        if (!Directory.Exists(sourceDir))
            return;

        Directory.CreateDirectory(destinationDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var destFile = Path.Combine(destinationDir, Path.GetFileName(file));
            File.Copy(file, destFile, true);
        }

        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            var destSubDir = Path.Combine(destinationDir, Path.GetFileName(dir));
            CopyDirectory(dir, destSubDir);
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

    
}
