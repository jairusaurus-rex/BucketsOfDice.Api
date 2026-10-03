using BucketsOfDice.Api.Common.Enums;

namespace BucketsOfDice.Api.Middleware;

public class ChatRoomConnectionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.Equals("/hubs/dice", StringComparison.OrdinalIgnoreCase)
            && !HttpMethods.IsOptions(context.Request.Method))
        {
            var roomValue = context.Request.Query["room"].ToString();
            var userName = context.Request.Query["userName"].ToString();

            var isValidRoom = Enum.TryParse<ChatRoom>(roomValue, out var room)
                && Enum.IsDefined(room)
                && string.Equals(room.ToString(), roomValue, StringComparison.Ordinal);

            if (!isValidRoom || string.IsNullOrWhiteSpace(userName))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "A valid room and userName are required."
                });
                return;
            }
        }

        await next(context);
    }
}