using Microsoft.AspNetCore.SignalR;
using BucketsOfDice.Api.Common.Enums;

namespace BucketsOfDice.Api.Hubs;

public class DiceHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var query = Context.GetHttpContext()!.Request.Query;
        var room = Enum.Parse<ChatRoom>(query["room"].ToString());
        Context.Items[nameof(ChatRoom)] = room;
        Context.Items["UserName"] = query["userName"].ToString();

        await Groups.AddToGroupAsync(Context.ConnectionId, room.ToString());
        await base.OnConnectedAsync();
    }

    public async Task SendMessage(string message)
    {
        var room = (ChatRoom)Context.Items[nameof(ChatRoom)]!;
        await Clients.Group(room.ToString()).SendAsync("ReceiveMessage", message);
    }
}