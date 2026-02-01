using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace MedicalSystem.API.Hubs;

using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(ILogger<ChatHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        _logger.LogInformation($"📞 User connected: {userId}, ConnectionId: {Context.ConnectionId}");

        // إرسال رسالة ترحيب
        await Clients.Caller.SendAsync("Welcome", new
        {
            Message = "مرحباً بك في نظام المحادثة",
            ConnectionId = Context.ConnectionId,
            UserId = userId,
            Timestamp = DateTime.UtcNow
        });

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        _logger.LogInformation($" User disconnected: {userId}");
        await base.OnDisconnectedAsync(exception);
    }
    private string GetUserId()
    {
        var httpContext = Context.GetHttpContext();

        // 1. من QueryString
        if (httpContext != null && httpContext.Request.Query.TryGetValue("userId", out var userIdFromQuery))
        {
            if (!string.IsNullOrEmpty(userIdFromQuery))
            {
                _logger.LogDebug($"Using userId from query string: {userIdFromQuery}");
                return userIdFromQuery;
            }
        }

        // 2. من Headers
        if (httpContext != null && httpContext.Request.Headers.TryGetValue("X-User-Id", out var userIdFromHeader))
        {
            if (!string.IsNullOrEmpty(userIdFromHeader))
            {
                _logger.LogDebug($"Using userId from header: {userIdFromHeader}");
                return userIdFromHeader;
            }
        }
        _logger.LogDebug($"No userId provided, using ConnectionId: {Context.ConnectionId}");
        return $"Anonymous_{Context.ConnectionId}";
    }
    public async Task JoinRoom(string appointmentId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, appointmentId);
        await Clients.Caller.SendAsync("ReceiveSystemMessage", $"You joined room {appointmentId}");
    }

    public async Task LeaveRoom(string appointmentId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, appointmentId);
        await Clients.Caller.SendAsync("ReceiveSystemMessage", $"You left room {appointmentId}");
    }

    public async Task SendMessage(string appointmentId, string senderId, string receiverId, string content)
    {
        var message = new
        {
            MessageId = System.Guid.NewGuid().ToString(),
            SenderId = senderId,
            ReceiverId = receiverId,
            Content = content,
            AppointmentId = appointmentId
        };

        await Clients.Group(appointmentId).SendAsync("ReceiveMessage", message);
    }

    public async Task UpdateMessage(string appointmentId, string messageId, string newContent, string senderId)
    {
        var updatedMessage = new
        {
            MessageId = messageId,
            SenderId = senderId,
            Content = newContent,
            AppointmentId = appointmentId
        };

        await Clients.Group(appointmentId).SendAsync("MessageUpdated", updatedMessage);
    }

    public async Task DeleteMessage(string appointmentId, string messageId, string senderId)
    {
        var deletedMessage = new
        {
            MessageId = messageId,
            SenderId = senderId,
            AppointmentId = appointmentId
        };

    
        await Clients.Group(appointmentId).SendAsync("MessageDeleted", deletedMessage);
    }

}
