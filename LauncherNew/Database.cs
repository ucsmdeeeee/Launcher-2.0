    using System;
    using System.IO;
    using System.Threading.Tasks;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Dapper;
    using LauncherNew.Models;
    using Npgsql;

    namespace LauncherNew
    {
        public class Database
        {
            private readonly string _connectionString;

            public Database(string connectionString)
            {
                _connectionString = connectionString;
            }
            public async Task ExecuteAsync(string query, object parameters)
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    await connection.ExecuteAsync(query, parameters);
                }
            }
            
            public async Task<bool> PurchaseItemAsync(long telegramId, IEnumerable<Item> items, decimal totalPrice)
    {
        const string getUserBankQuery = @"
            SELECT b.id AS BankId, b.balance AS Balance
            FROM bank b
            INNER JOIN ""User"" u ON b.user_id = u.id
            WHERE u.tg_id = @telegramId";

        const string updateBalanceQuery = @"
            UPDATE bank
            SET balance = @newBalance
            WHERE id = @bankId";

        const string createTransactionQuery = @"
            INSERT INTO transaction (amount, final_balance, bank_id, transaction_date)
            VALUES (@amount, @finalBalance, @bankId, @transactionDate)";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            // Получаем текущий баланс и идентификатор банка
            var userBank = await connection.QueryFirstOrDefaultAsync<(int BankId, decimal Balance)>(
                getUserBankQuery, new { telegramId }, transaction);

            if (userBank == default) throw new Exception("Пользователь или баланс не найден.");

            if (userBank.Balance < totalPrice) throw new Exception("Недостаточно средств.");

            // Расчет нового баланса
            var newBalance = userBank.Balance - totalPrice;

            // Обновляем баланс в таблице `bank`
            await connection.ExecuteAsync(
                updateBalanceQuery,
                new { newBalance, bankId = userBank.BankId },
                transaction);

            // Создаем запись о транзакции
            await connection.ExecuteAsync(
                createTransactionQuery,
                new
                {
                    amount = -totalPrice, // Покупка — отрицательное значение
                    finalBalance = newBalance,
                    bankId = userBank.BankId,
                    transactionDate = DateTime.UtcNow
                },
                transaction);

            // Фиксируем изменения
            await transaction.CommitAsync();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during purchase: {ex.Message}");
            await transaction.RollbackAsync();
            return false;
        }
    }

            public async Task<IEnumerable<Teammate>> GetTeammatesAsync(string team)
            {
                const string query = "SELECT nickname FROM \"User\" WHERE team = @team";

                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                try
                {
                    var teammates = await connection.QueryAsync<Teammate>(query, new { team });
                    Console.WriteLine($"Список сокомандников из базы данных: {string.Join(", ", teammates.Select(t => t.Nickname))}");
                    return teammates;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при получении сокомандников: {ex.Message}");
                    return Enumerable.Empty<Teammate>();
                }
            }




            public async Task<IEnumerable<Goods>> GetGoodsAsync()
            {
                const string query = "SELECT id AS Id, name AS Name, price AS Price, image_path AS ImagePath FROM goods";

                Console.WriteLine($"Executing query: {query}");

                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                try
                {
                    var goods = await connection.QueryAsync<Goods>(query);
                    return goods; // Возвращаем список товаров
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error executing query: {ex.Message}");
                    return Enumerable.Empty<Goods>(); // Возвращаем пустой список в случае ошибки
                }
            }

            public async Task<decimal?> GetUserBalanceAsync(long telegramId)
            {
                const string query = @"
            SELECT b.balance
            FROM bank b
            INNER JOIN ""User"" u ON b.user_id = u.id
            WHERE u.tg_id = @telegramId";

                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                try
                {
                    var balance = await connection.QueryFirstOrDefaultAsync<decimal?>(query, new { telegramId });
                    return balance;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при получении баланса: {ex.Message}");
                    return null;
                }
            }

            public async Task<IEnumerable<Item>> GetItemsAsync()
            {
                const string query = "SELECT id, name, image AS ImagePath, price, mine_name, category FROM items";

                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                // Загружаем данные из базы данных
                var items = await connection.QueryAsync<Item>(query);

                return items;
            }













            // Получение данных пользователя по tg_id
            public async Task<User> GetUserByTelegramIdAsync(long telegramId)
            {
                const string query = "SELECT id AS Id, full_name AS FullName, email AS Email, password AS Password, course AS Course, department AS Department, group_name AS GroupName, nickname AS Nickname, team AS Team, tg_id AS TgId FROM \"User\" WHERE \"tg_id\" = @telegramId";



                Console.WriteLine($"Executing query: {query} with telegramId: {telegramId}");

                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                try
                {
                    var user = await connection.QueryFirstOrDefaultAsync<User>(query, new { telegramId });
                    return user; // Возвращаем найденного пользователя
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error executing query: {ex.Message}");
                    return null; // Возвращаем null в случае ошибки
                }
            }
        }
    }