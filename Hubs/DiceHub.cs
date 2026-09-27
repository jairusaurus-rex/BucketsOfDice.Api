using Microsoft.AspNetCore.SignalR;
using BucketsOfDice.Api.Common.Enums;
using BucketsOfDice.Api.Services;

namespace BucketsOfDice.Api.Hubs;

public class DiceHub : Hub
{
    private readonly MessageHistory _messageHistory;

    public DiceHub(MessageHistory messageHistory)
    {
        _messageHistory = messageHistory;
    }

    public override async Task OnConnectedAsync()
    {
        var query = Context.GetHttpContext()!.Request.Query;
        var room = Enum.Parse<ChatRoom>(query["room"].ToString());
        Context.Items[nameof(ChatRoom)] = room;
        Context.Items["UserName"] = query["userName"].ToString();

        await Groups.AddToGroupAsync(Context.ConnectionId, room.ToString());
        await Clients.Caller.SendAsync("ReceiveHistory", _messageHistory.GetRecent(room));
        await base.OnConnectedAsync();
    }

    public async Task SendMessage(string message)
    {
        var room = (ChatRoom)Context.Items[nameof(ChatRoom)]!;
        _messageHistory.Add(room, message);
        await Clients.Group(room.ToString()).SendAsync("ReceiveMessage", message);
    }
}