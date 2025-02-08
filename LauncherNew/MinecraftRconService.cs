using System.Net;
using CoreRCON;

namespace LauncherNew;

public class MinecraftRconService
{
    private readonly string _host;
    private readonly ushort _port;
    private readonly string _password;

    public MinecraftRconService(string host, ushort port, string password)
    {
        _host = host;
        _port = port;
        _password = password;
    }

    public async Task<string> ExecuteCommandAsync(string command)
    {
        using var rcon = new RCON(IPAddress.Parse(_host), _port, _password);
        await rcon.ConnectAsync();
        var response = await rcon.SendCommandAsync(command);
        Console.WriteLine($"RCON Response: {response}");
        return response;
    }

    
    public async Task<bool> IsPlayerOnlineAsync(string playerName)
    {
        try
        {
            var response = await ExecuteCommandAsync($"list");
            var players = response.Split(':')[1]?.Split(',').Select(p => p.Trim());
            return players != null && players.Contains(playerName, StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при проверке статуса игрока: {ex.Message}");
            return false;
        }
    }


}
