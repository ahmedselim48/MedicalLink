using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace MedicalSystem.API.Hubs;

// ⭐⭐⭐ إزالة الـ Authorize تماماً ⭐⭐⭐
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
        _logger.LogInformation($"👋 User disconnected: {userId}");
        await base.OnDisconnectedAsync(exception);
    }

    // ⭐⭐⭐ دالة محسنة للحصول على UserId ⭐⭐⭐
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

        // 3. استخدام ConnectionId إذا لم يكن هناك userId
        _logger.LogDebug($"No userId provided, using ConnectionId: {Context.ConnectionId}");
        return $"Anonymous_{Context.ConnectionId}";
    }

    // ⭐⭐⭐ دالة بسيطة للانضمام للمحادثة ⭐⭐⭐
    public async Task JoinChat(int appointmentId, string? customUserId = null)
    {
        var userId = string.IsNullOrEmpty(customUserId) ? GetUserId() : customUserId;

        _logger.LogInformation($"🚪 JoinChat called - UserId: {userId}, AppointmentId: {appointmentId}");

        // السماح للجميع بالانضمام
        await Groups.AddToGroupAsync(Context.ConnectionId, appointmentId.ToString());

        // إرسال تأكيد للمستخدم
        await Clients.Caller.SendAsync("JoinedChat", new
        {
            AppointmentId = appointmentId,
            UserId = userId,
            Timestamp = DateTime.UtcNow
        });

        // إعلام المجموعة بمستخدم جديد
        await Clients.OthersInGroup(appointmentId.ToString()).SendAsync("UserJoined", new
        {
            UserId = userId,
            ConnectionId = Context.ConnectionId,
            Timestamp = DateTime.UtcNow
        });

        _logger.LogInformation($"✅ User {userId} joined chat room {appointmentId}");
    }

    // ⭐⭐⭐ دالة بسيطة لمغادرة المحادثة ⭐⭐⭐
    public async Task LeaveChat(int appointmentId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, appointmentId.ToString());

        var userId = GetUserId();
        await Clients.Caller.SendAsync("LeftChat", new
        {
            AppointmentId = appointmentId,
            UserId = userId,
            Timestamp = DateTime.UtcNow
        });

        _logger.LogInformation($"👋 User {userId} left chat room {appointmentId}");
    }

    // ⭐⭐⭐ دالة بسيطة لإرسال الرسائل ⭐⭐⭐
    public async Task SendMessage(int appointmentId, string content)
    {
        var userId = GetUserId();

        if (string.IsNullOrWhiteSpace(content))
        {
            await Clients.Caller.SendAsync("Error", "Message content cannot be empty");
            return;
        }

        var message = new
        {
            Id = Guid.NewGuid(),
            AppointmentId = appointmentId,
            SenderId = userId,
            Content = content,
            Timestamp = DateTime.UtcNow,
            SenderName = userId.Contains("Anonymous") ? "مستخدم مجهول" : userId
        };

        // إرسال الرسالة لكل أعضاء المجموعة
        await Clients.Group(appointmentId.ToString()).SendAsync("ReceiveMessage", message);

        _logger.LogInformation($"📤 Message sent by {userId} in appointment {appointmentId}: {content}");
    }

    // ⭐⭐⭐ دالة للتحقق من حالة الخادم ⭐⭐⭐
    public async Task Ping()
    {
        await Clients.Caller.SendAsync("Pong", new
        {
            Message = "الخادم يعمل",
            Timestamp = DateTime.UtcNow,
            Server = "ChatHub",
            Version = "1.0"
        });
    }

    // ⭐⭐⭐ دالة للحصول على معلومات الاتصال ⭐⭐⭐
    public async Task GetConnectionInfo()
    {
        var userId = GetUserId();
        var info = new
        {
            ConnectionId = Context.ConnectionId,
            UserId = userId,
            ConnectedAt = DateTime.UtcNow,
            Transport = Context.Features.Get<Microsoft.AspNetCore.Http.Connections.Features.IHttpTransportFeature>()?.TransportType.ToString()
        };

        await Clients.Caller.SendAsync("ConnectionInfo", info);
    }

    // ⭐⭐⭐ دالة لمؤشر الكتابة ⭐⭐⭐
    public async Task TypingIndicator(int appointmentId)
    {
        var userId = GetUserId();
        await Clients.OthersInGroup(appointmentId.ToString())
                     .SendAsync("UserTyping", new
                     {
                         UserId = userId,
                         Timestamp = DateTime.UtcNow
                     });
    }

    // ⭐⭐⭐ دالة لتوقف الكتابة ⭐⭐⭐
    public async Task StopTyping(int appointmentId)
    {
        var userId = GetUserId();
        await Clients.OthersInGroup(appointmentId.ToString())
                     .SendAsync("UserStoppedTyping", new
                     {
                         UserId = userId,
                         Timestamp = DateTime.UtcNow
                     });
    }

    // ⭐⭐⭐ دالة للحصول على المستخدمين في الغرفة ⭐⭐⭐
    public async Task GetUsersInRoom(int appointmentId)
    {
        // هذه دالة تجريبية، في الإنتاج ستحتاج لتخزين المستخدمين
        await Clients.Caller.SendAsync("UsersInRoom", new
        {
            AppointmentId = appointmentId,
            UserCount = 1, // قيم تجريبية
            Timestamp = DateTime.UtcNow
        });
    }

    // ⭐⭐⭐ دالة لإرسال رسالة مباشرة لمستخدم ⭐⭐⭐
    public async Task SendPrivateMessage(string targetUserId, string content)
    {
        var senderId = GetUserId();

        if (string.IsNullOrWhiteSpace(content))
        {
            await Clients.Caller.SendAsync("Error", "Message content cannot be empty");
            return;
        }

        var message = new
        {
            Id = Guid.NewGuid(),
            SenderId = senderId,
            ReceiverId = targetUserId,
            Content = content,
            Timestamp = DateTime.UtcNow,
            IsPrivate = true
        };

        // في الإنتاج، تحتاج لتتبع اتصالات المستخدمين
        await Clients.Caller.SendAsync("PrivateMessageSent", message);
        _logger.LogInformation($"🔒 Private message from {senderId} to {targetUserId}: {content}");
    }
}