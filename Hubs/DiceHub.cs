using Microsoft.AspNetCore.SignalR;

namespace BucketsOfDice.Api.Hubs;

public class DiceHub : Hub
{
    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", message);
    }
}