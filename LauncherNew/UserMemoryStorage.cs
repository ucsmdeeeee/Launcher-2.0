using LauncherNew.Models;
using System.Collections.Generic;
namespace LauncherNew;



public class UserMemoryStorage
{
    private readonly Dictionary<long, User> _userStorage = new();

    // Сохранить данные пользователя
    public void SaveUser(long telegramId, User user)
    {
        _userStorage[telegramId] = user;
    }

    // Получить данные пользователя
    public User GetUser(long telegramId)
    {
        _userStorage.TryGetValue(telegramId, out var user);
        return user;
    }
}

