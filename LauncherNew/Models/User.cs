namespace LauncherNew.Models;

public class User
{
    public int Id { get; set; } // Уникальный идентификатор
    public string FullName { get; set; } // ФИО
    public string Email { get; set; } // Почта корп.
    public string Password { get; set; } // Пароль
    public int Course { get; set; } // Курс
    public string Department { get; set; } // Кафедра
    public string GroupName { get; set; } // Группа
    public string Nickname { get; set; } // Ник
    public string? Team { get; set; } // Команда
    public long? TgId { get; set; } // Telegram ID

    // Связь один-к-одному с Bank
    public Bank Bank { get; set; }
}

