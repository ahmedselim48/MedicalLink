
using Medical_Team_B.Extensions;
using MedLink.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace MedicalSystem.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IMessageService _messageService;
    private readonly IChatRoomService _chatRoomService;
    private readonly IPresenceService _presence;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(
        IMessageService messageService,
        IChatRoomService chatRoomService,
        IPresenceService presence,
        ILogger<ChatHub> logger)
    {
        _messageService = messageService;
        _chatRoomService = chatRoomService;
        _presence = presence;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User.GetUserId(); // string
        if (!string.IsNullOrEmpty(userId))
        {
            _presence.UserConnected(userId);
            _logger.LogInformation($"User {userId} connected to ChatHub");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User.GetUserId(); // string
        if (!string.IsNullOrEmpty(userId))
        {
            _presence.UserDisconnected(userId);
            _logger.LogInformation($"User {userId} disconnected from ChatHub");
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinChat(int appointmentId)
    {
        var userId = Context.User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            await Clients.Caller.SendAsync("Error", "User not authenticated");
            return;
        }

        // التحقق من صلاحية المستخدم
        var canAccess = await _chatRoomService.CanUserAccessAsync(appointmentId, userId);
        if (!canAccess)
        {
            await Clients.Caller.SendAsync("Error", "Access denied to this chat");
            return;
        }

        // الانضمام إلى مجموعة الـ Chat Room
        await Groups.AddToGroupAsync(Context.ConnectionId, appointmentId.ToString());
        await Clients.Caller.SendAsync("JoinedChat", appointmentId);

        _logger.LogInformation($"User {userId} joined chat for appointment {appointmentId}");
    }

    public async Task LeaveChat(int appointmentId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, appointmentId.ToString());
        await Clients.Caller.SendAsync("LeftChat", appointmentId);

        var userId = Context.User.GetUserId();
        _logger.LogInformation($"User {userId} left chat for appointment {appointmentId}");
    }

    public async Task SendMessage(int appointmentId, string content)
    {
        var userId = Context.User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            await Clients.Caller.SendAsync("Error", "User not authenticated");
            return;
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            await Clients.Caller.SendAsync("Error", "Message content cannot be empty");
            return;
        }

        try
        {
            // إرسال الرسالة
            var result = await _messageService.SendMessageAsync(appointmentId, userId, content);

            if (result.IsSuccess)
            {
                // إرسال الرسالة لكل أعضاء المجموعة
                await Clients.Group(appointmentId.ToString()).SendAsync("ReceiveMessage", result.Value);
                _logger.LogInformation($"Message sent by {userId} in appointment {appointmentId}");
            }
            else
            {
                await Clients.Caller.SendAsync("Error", result.Error.Description);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message in ChatHub");
            await Clients.Caller.SendAsync("Error", "Failed to send message");
        }
    }

    public async Task DeleteMessage(int appointmentId, int messageId)
    {
        var userId = Context.User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            await Clients.Caller.SendAsync("Error", "User not authenticated");
            return;
        }

        bool isAdmin = Context.User.IsInRole("Admin");

        try
        {
            var result = await _messageService.DeleteMessageAsync(messageId, userId, isAdmin);

            if (result.IsSuccess)
            {
                // إشعار جميع الأعضاء بحذف الرسالة
                await Clients.Group(appointmentId.ToString()).SendAsync("MessageDeleted", messageId);
                _logger.LogInformation($"Message {messageId} deleted by {userId}");
            }
            else
            {
                await Clients.Caller.SendAsync("Error", new
                {
                    Code = result.Error.Code,
                    Message = result.Error.Description
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting message in ChatHub");
            await Clients.Caller.SendAsync("Error", "Failed to delete message");
        }
    }

    public async Task TypingIndicator(int appointmentId)
    {
        var userId = Context.User.GetUserId();
        if (string.IsNullOrEmpty(userId))
            return;

        // إرسال إشارة الكتابة لكل المستخدمين الآخرين في المجموعة
        await Clients.OthersInGroup(appointmentId.ToString())
                     .SendAsync("UserTyping", userId);
    }

    public async Task StopTyping(int appointmentId)
    {
        var userId = Context.User.GetUserId();
        if (string.IsNullOrEmpty(userId))
            return;

        await Clients.OthersInGroup(appointmentId.ToString())
                     .SendAsync("UserStoppedTyping", userId);
    }

    public async Task MarkAsRead(int appointmentId, int messageId)
    {
        var userId = Context.User.GetUserId();
        if (string.IsNullOrEmpty(userId))
            return;

        // يمكنك إضافة منطق لتحديد الرسائل المقروءة
        await Clients.Group(appointmentId.ToString())
                     .SendAsync("MessageRead", new { MessageId = messageId, UserId = userId });
    }

    public async Task GetOnlineUsers(int appointmentId)
    {
        var userId = Context.User.GetUserId();
        if (string.IsNullOrEmpty(userId))
            return;

        // الحصول على معلومات Chat Room
        var chatRoomInfo = await _chatRoomService.GetChatRoomInfoAsync(appointmentId, userId);

        if (chatRoomInfo.IsSuccess)
        {
            var otherUserId = chatRoomInfo.Value.OtherUserId;
            var isOtherUserOnline = _presence.IsOnline(otherUserId);

            await Clients.Caller.SendAsync("OnlineStatus", new
            {
                UserId = otherUserId,
                IsOnline = isOtherUserOnline
            });
        }
    }
}