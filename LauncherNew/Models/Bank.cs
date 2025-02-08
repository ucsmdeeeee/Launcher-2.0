namespace LauncherNew.Models;

public class Bank
{
    public int Id { get; set; } // Уникальный идентификатор
    public decimal Balance { get; set; } // Баланс
    public DateTime LastModified { get; set; } // Время последнего изменения

    // Связь один-к-одному с User
    public int UserId { get; set; } // Внешний ключ
    public User User { get; set; }
}

