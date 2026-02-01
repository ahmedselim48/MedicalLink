using Domain.ErrorHandling;
using MedLink.Application.DTOs.Chat;
using MedLink.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;
using System.Security.Claims;

namespace Medical_Team_B.Controllers
{
    [ApiController]
    [Route("api/chat")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatRoomService _chatRoomService;
        private readonly IMessageService _messageService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(
            IChatRoomService chatRoomService,
            IMessageService messageService,
            ILogger<ChatController> logger)
        {
            _chatRoomService = chatRoomService;
            _messageService = messageService;
            _logger = logger;
        }
        [HttpPost("send/{appointmentId}")]
        public async Task<IActionResult> SendMessage(
     int appointmentId,
     [FromBody] SendMessageDto request)
        {
            try
            {
                _logger.LogInformation("SendMessage endpoint called - AppointmentId: {AppointmentId}", appointmentId);

       
                string userId = null;

       
                userId = User.FindFirstValue("uid") ??
                         User.FindFirstValue("sub") ??
                         User.FindFirstValue("userId") ??
                         User.FindFirstValue(ClaimTypes.NameIdentifier);

                _logger.LogInformation("User claims - All claims:");
                foreach (var claim in User.Claims)
                {
                    _logger.LogInformation("  {Type}: {Value}", claim.Type, claim.Value);
                }

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in claims");
                    return Unauthorized(new { error = "User not authenticated" });
                }

                _logger.LogInformation("User ID found: {UserId}", userId);
                _logger.LogInformation("Request content: {Content}", request?.Content);

                if (request == null || string.IsNullOrWhiteSpace(request.Content))
                {
                    return BadRequest(new { error = "Message content is required" });
                }

    
                if (userId.Contains("@"))
                {
                    _logger.LogWarning("User ID appears to be an email: {UserId}", userId);
                   
                }
                else
                {
                    _logger.LogInformation("User ID looks like a proper ID: {UserId}", userId);
                }

                var result = await _messageService.SendMessageAsync(
                    appointmentId,
                    userId,
                    request.Content);

                if (result.IsFailure)
                {
                    _logger.LogWarning("SendMessage failed: {Error}", result.Error?.Description);
                    return StatusCode(result.Error?.StatusCode ?? 500,
                        new { error = result.Error?.Description ?? "Unknown error" });
                }

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in SendMessage endpoint");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet("messages/{appointmentId}")]
        public async Task<IActionResult> GetMessages(int appointmentId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                string userId = null;

               
                userId = User.FindFirstValue("uid") ??
                         User.FindFirstValue("sub") ??
                         User.FindFirstValue("userId") ??
                         User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(Result.Failure(Error.Unauthorized("User not authenticated")));

                // Validate pagination
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100)
                    return BadRequest(Result.Failure(Error.Validation("Page size must be between 1 and 100")));

                // Check access
                var canAccess = await _chatRoomService.CanUserAccessAsync(appointmentId, userId);
                if (!canAccess)
                    return Forbid();

                var result = await _messageService.GetMessagesAsync(appointmentId, page, pageSize);

                return result.IsSuccess
                    ? Ok(result)
                    : result.ToProblem();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting messages for appointment {AppointmentId}", appointmentId);
                return StatusCode(500, Result.Failure(Error.InternalServer("An unexpected error occurred")));
            }
        }

        [HttpDelete("message/{messageId}")]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            try
            {
                string userId = null;

                // الطريقة 1: جرب الـ UID أولاً (هو الأصح في معظم أنظمة JWT)
                userId = User.FindFirstValue("uid") ??
                         User.FindFirstValue("sub") ??
                         User.FindFirstValue("userId") ??
                         User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(Result.Failure(Error.Unauthorized("User not authenticated")));

                if (messageId <= 0)
                    return BadRequest(Result.Failure(Error.Validation("Invalid message ID")));

                var result = await _messageService.DeleteMessageAsync(messageId, userId, isAdmin);

                return result.IsSuccess
                    ? NoContent()
                    : result.ToProblem();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message {MessageId}", messageId);
                return StatusCode(500, Result.Failure(Error.InternalServer("An unexpected error occurred")));
            }
        }

        [HttpGet("info/{appointmentId}")]
        public async Task<IActionResult> GetChatInfo(int appointmentId)
        {
            try
            {
                string userId = null;

             
                userId = User.FindFirstValue("uid") ??
                         User.FindFirstValue("sub") ??
                         User.FindFirstValue("userId") ??
                         User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(Result.Failure(Error.Unauthorized("User not authenticated")));

                var result = await _chatRoomService.GetChatRoomInfoAsync(appointmentId, userId);

                return result.IsSuccess
                    ? Ok(result)
                    : result.ToProblem();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat info for appointment {AppointmentId}", appointmentId);
                return StatusCode(500, Result.Failure(Error.InternalServer("An unexpected error occurred")));
            }
        }

        [HttpGet("check-access/{appointmentId}")]
        public async Task<IActionResult> CheckAccess(int appointmentId)
        {
            try
            {
                string userId = null;

                userId = User.FindFirstValue("uid") ??
                         User.FindFirstValue("sub") ??
                         User.FindFirstValue("userId") ??
                         User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(Result.Failure(Error.Unauthorized("User not authenticated")));

                var canAccess = await _chatRoomService.CanUserAccessAsync(appointmentId, userId);

                return Ok(Result.Success(new { CanAccess = canAccess, UserId = userId, AppointmentId = appointmentId }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking access for appointment {AppointmentId}", appointmentId);
                return StatusCode(500, Result.Failure(Error.InternalServer("An unexpected error occurred")));
            }
        }
    } }



