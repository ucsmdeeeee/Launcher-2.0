using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MvvmCross.ViewModels;
using LauncherNew.Models;

namespace LauncherNew.ViewModels;

public class ShopViewModel : MvxViewModel, INotifyPropertyChanged
    {
        
        private ObservableCollection<Item> _displayedItems;
        public ObservableCollection<Item> DisplayedItems
        {
            get => _displayedItems;
            set
            {
                _displayedItems = value;
                OnPropertyChanged(nameof(DisplayedItems));
            }
        }

        

    public ObservableCollection<Item> Items { get; set; }
        public ObservableCollection<Item> SelectedItems  { get; set; }

        private int _totalPrice;
        public int TotalPrice
        {
            get => _totalPrice;
            set
            {
                _totalPrice = value;
                OnPropertyChanged(nameof(TotalPrice));
            }
        }
        private long _telegramId;

        public long TelegramId
        {
            get => _telegramId;
            set
            {
                _telegramId = value;
                OnPropertyChanged(nameof(TelegramId));
            }
        }
        private decimal _balance;
        public decimal Balance
        {
            get => _balance;
            set
            {
                _balance = value;
                OnPropertyChanged(nameof(Balance));
            }
        }


        public ICommand AddToCartCommand { get; }
        public ICommand PurchaseCommand { get; }
        
        

        public ShopViewModel()
        {
            long telegramId = GetTelegramIdFromFile();
            // Логика с использованием _telegramId
            LoadUserBalance();
            Items = new ObservableCollection<Item>();
            DisplayedItems = new ObservableCollection<Item>();
            SelectedItems = new ObservableCollection<Item>();

            AddToCartCommand = new RelayCommand<Item>(AddToCart);
            PurchaseCommand = new RelayCommand<object>(_ => PurchaseItems());
            DisplayedItems.Clear();
            PopulateGridAsync(); // Заполняем Items, но не DisplayedItems
            
        }

        public void FilterItemsByCategory(string category)
        {
            if (string.IsNullOrEmpty(category)) return;

            // Проверяем, что Items и DisplayedItems не null
            if (Items == null || DisplayedItems == null)
            {
                Debug.WriteLine("Items или DisplayedItems не инициализированы.");
                return;
            }

            // Очищаем DisplayedItems и фильтруем
            DisplayedItems.Clear();
            foreach (var item in Items.Where(i => i.Category == category))
            {
                DisplayedItems.Add(item);
            }
        }



        public async Task PopulateGridAsync()
        {
            try
            {
                var database = new Database("Host=93.158.195.13;Port=5432;Username=postgres;Password=1548;Database=students");
                var itemsFromDb = await database.GetItemsAsync();

                Items.Clear();
                foreach (var item in itemsFromDb)
                {
                    Items.Add(item);
                }
                // Обновляем `DisplayedItems`
                DisplayedItems.Clear();
                foreach (var item in Items)
                {
                    DisplayedItems.Add(item);
                }
                DisplayedItems.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке предметов: {ex.Message}");
            }
        }


        

        public void AddToCart(Item item)
        {
            if (item == null) return;
            if (SelectedItems.Count >= 8)
            {
                MessageBox.Show("Вы не можете добавить более 8 предметов в корзину.");
                return;
            }
            var existingItem = SelectedItems.FirstOrDefault(x => x.Name == item.Name);

            if (existingItem != null)
            {
                // Проверяем максимальное количество
                if (existingItem.Category == "Инструменты" || existingItem.Category == "Броня" || existingItem.Category == "Оружие")
                {
                    MessageBox.Show("Максимальное количество для этого предмета: 1");
                    return;
                }

                if (existingItem.Quantity >= 64)
                {
                    MessageBox.Show("Максимальное количество для этого предмета: 64");
                    return;
                }
                
                existingItem.Quantity++;
                var index = SelectedItems.IndexOf(existingItem);
                SelectedItems[index] = null; // Временно очищаем
                SelectedItems[index] = existingItem; // Вставляем обновленный элеме
            }
            else
            {
                SelectedItems.Add(new Item
                {
                    Name = item.Name,
                    Price = item.Price,
                    ImagePath = item.ImagePath,
                    Category = item.Category,
                    MineName = item.MineName,
                    Quantity = 1
                });
            }

            TotalPrice += item.Price;
        }
        
        public ICommand RemoveFromCartCommand => new RelayCommand<Item>(RemoveFromCart);

        private void RemoveFromCart(Item item)
        {
            
            if (item == null) return;

            // Ищем предмет в корзине
            var existingItem = SelectedItems.FirstOrDefault(x => x.Name == item.Name);

            if (existingItem != null)
            {
                // Уменьшаем количество
                existingItem.Quantity--;

                // Если количество стало 0, удаляем предмет
                if (existingItem.Quantity <= 0)
                {
                    SelectedItems.Remove(existingItem);
                }
                else
                {
                    // Обновляем элемент в коллекции
                    var index = SelectedItems.IndexOf(existingItem);
                    SelectedItems[index] = null;
                    SelectedItems[index] = existingItem;
                }

                // Обновляем общую стоимость
                TotalPrice -= item.Price;
                OnPropertyChanged(nameof(TotalPrice));
            }
        }


      private async void PurchaseItems()
{
    try
    {
        // Загружаем баланс пользователя
        await LoadUserBalance();

        // Получение Telegram ID
        long telegramId = GetTelegramIdFromFile();
        if (telegramId == 0)
        {

            return;
        }

        // Инициализация подключения к базе данных
        var database = new Database("Host=93.158.195.13;Port=5432;Username=postgres;Password=1548;Database=students");

        // Получение пользователя
        var user = await database.GetUserByTelegramIdAsync(telegramId);
        if (user == null)
        {

            return;
        }

        string nickname = user.Nickname; // Никнейм
        if (string.IsNullOrEmpty(nickname))
        {
            return;
        }

        // Проверяем баланс пользователя
        if (Balance < TotalPrice)
        {
            MessageBox.Show("Недостаточно средств на балансе!");
            return;
        }

        // Проверяем, находится ли пользователь на сервере
        var rconService = new MinecraftRconService("93.158.194.51", 25575, "1548");
        var isOnline = await rconService.IsPlayerOnlineAsync(nickname);
        if (!isOnline)
        {
            MessageBox.Show("Пользователь не находится на сервере. Покупка отменена.");
            return;
        }

        // Выполнение RCON-команд
        var commands = SelectedItems.Select(item => $"give {nickname} {item.MineName} {item.Quantity}");
        var responses = await Task.Run(async () =>
        {
            var results = new List<string>();
            foreach (var command in commands)
            {
                try
                {
                    Console.WriteLine($"Отправка команды: {command}");
                    var response = await rconService.ExecuteCommandAsync(command);
                    results.Add(response);
                    Console.WriteLine($"Ответ RCON: {response}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка RCON: {ex.Message}");
                    results.Add($"Ошибка: {ex.Message}");
                }
            }
            return results.ToArray();
        });

        // Проверяем, все ли команды выполнены успешно
        if (responses.Any(r => r.Contains("Ошибка")))
        {
            MessageBox.Show("Не удалось выполнить покупку. Попробуйте позже.");
            return;
        }

        // Обновляем баланс и регистрируем транзакцию
        var success = await database.PurchaseItemAsync(telegramId, SelectedItems, TotalPrice);
        if (!success)
        {
            MessageBox.Show("Ошибка при обновлении баланса или создании транзакции.");
            return;
        }

        // Обновление данных UI
        Application.Current.Dispatcher.Invoke(() =>
        {
            Balance -= TotalPrice;
            SelectedItems.Clear();
            TotalPrice = 0;

            OnPropertyChanged(nameof(Balance));
            OnPropertyChanged(nameof(TotalPrice));
            MessageBox.Show("Покупка завершена! Товары отправлены на сервер.");
        });
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Ошибка при выдаче товаров: {ex.Message}");
    }
}





private async Task LoadUserBalance()
{
    try
    {
        // Получение Telegram ID
        long telegramId = GetTelegramIdFromFile();
        if (telegramId == 0)
        {
            return;
        }

        // Инициализация подключения к базе данных
        var database = new Database("Host=93.158.195.13;Port=5432;Username=postgres;Password=1548;Database=students");

        // Получение текущего баланса пользователя
        var balance = await database.GetUserBalanceAsync(telegramId);
        if (balance.HasValue)
        {
            Balance = balance.Value;
        }
        else
        {
            MessageBox.Show("Ошибка: Баланс пользователя не найден.");
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Ошибка при загрузке баланса: {ex.Message}");
    }
}





// Метод для чтения tg_id из файла
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







        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    
        

        public async Task HideLauncher()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Application.Current.MainWindow.WindowState = WindowState.Minimized;
            });
        }

        public async Task CloseLauncher()
        {
            Application.Current.Dispatcher.Invoke(() => { Application.Current.MainWindow.Close(); });
        }

        
    }
public class RelayCommand<T> : ICommand
{
    private readonly Action<T> _execute;
    private readonly Predicate<T> _canExecute;

    public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object parameter) => _canExecute == null || _canExecute((T)parameter);

    public void Execute(object parameter) => _execute((T)parameter);

    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}



