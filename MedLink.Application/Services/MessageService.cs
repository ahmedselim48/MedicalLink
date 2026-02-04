using Domain.ErrorHandling;
using Mapster;
using MapsterMapper;
using MedLink.Application.DTOs.Chat;
using MedLink.Application.Interfaces.Persistence;
using MedLink.Application.Interfaces.Services;
using MedLink.Application.Services;
using MedLink.Application.Specifications.Chat;
using MedLink.Domain.Entities.Appointments;
using MedLink.Domain.Entities.Chat;
using MedLink.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedLink.Application.Services
{
    public class MessageService : IMessageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IChatRoomService _chatRoomService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<MessageService> _logger;

        public MessageService(
            IUnitOfWork unitOfWork,
            IChatRoomService chatRoomService,
            UserManager<ApplicationUser> userManager,
            ILogger<MessageService> logger)
        {
            _unitOfWork = unitOfWork;
            _chatRoomService = chatRoomService;
            _userManager = userManager;
            _logger = logger;
        }

        #region Send Message

        public async Task<Result<MessageDto>> SendMessageAsync(
            int appointmentId,
            string senderId,
            string content)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                    return Result.Failure<MessageDto>(
                        Error.Validation("Message content is empty"));

                var appointment = await _unitOfWork.Repository<Appointment>()
                    .GetByIdAsync(appointmentId);

                if (appointment == null)
                    return Result.Failure<MessageDto>(
                        Error.NotFound("Appointment not found"));

                if (!await _chatRoomService.CanUserAccessAsync(appointmentId, senderId))
                    return Result.Failure<MessageDto>(
                        Error.Forbidden("Access denied"));

                var sender = await _userManager.FindByIdAsync(senderId);
                if (sender == null)
                    return Result.Failure<MessageDto>(
                        Error.NotFound("Sender not found"));

                var chatRoomResult =
                    await _chatRoomService.GetOrCreateChatRoomAsync(appointmentId);

                if (chatRoomResult.IsFailure)
                    return Result.Failure<MessageDto>(chatRoomResult.Error);

                var message = new Message
                {
                    ChatRoomId = chatRoomResult.Value.Id,
                    SenderId = sender.Id,
                    Content = content.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false,
                    IsEdited = false
                };

                await _unitOfWork.Repository<Message>().AddAsync(message);
                await _unitOfWork.Complete();

                // 👇 ربط الـ Sender عشان Mapster يجيب الاسم
                message.Sender = sender;

                return Result.Success(message.Adapt<MessageDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendMessageAsync");
                return Result.Failure<MessageDto>(
                    Error.InternalServer("Failed to send message"));
            }
        }

        #endregion

        #region Get Messages (Pagination)

        public async Task<Result<List<MessageDto>>> GetMessagesAsync(
            int appointmentId,
            int page,
            int pageSize)
        {
            try
            {
                if (page <= 0) page = 1;
                if (pageSize <= 0 || pageSize > 100) pageSize = 50;

                var chatRoomResult =
                    await _chatRoomService.GetOrCreateChatRoomAsync(appointmentId);

                if (chatRoomResult.IsFailure)
                    return Result.Failure<List<MessageDto>>(chatRoomResult.Error);

                var spec = new MessageWithSenderSpec(
                    chatRoomResult.Value.Id, page, pageSize);

                var messages =
                    await _unitOfWork.Repository<Message>()
                        .GetAllWithSpecAsync(spec);

              
                var dtos = messages
                    .OrderBy(m => m.CreatedAt)
                    .Adapt<List<MessageDto>>();

                return Result.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetMessagesAsync");
                return Result.Failure<List<MessageDto>>(
                    Error.InternalServer("Failed to get messages"));
            }
        }

        #endregion

        #region Delete Message

        public async Task<Result> DeleteMessageAsync(
            int messageId,
            string currentUserId,
            bool isAdmin)
        {
            try
            {
                var message = await _unitOfWork.Repository<Message>()
                    .GetByIdAsync(messageId);

                if (message == null)
                    return Result.Failure(
                        Error.NotFound("Message not found"));

                if (!isAdmin && message.SenderId != currentUserId)
                    return Result.Failure(
                        Error.Forbidden("You cannot delete this message"));

                message.IsDeleted = true;

                _unitOfWork.Repository<Message>().Update(message);
                await _unitOfWork.Complete();

                _logger.LogInformation(
                    "Message {MessageId} deleted by user {UserId}",
                    messageId, currentUserId);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteMessageAsync");
                return Result.Failure(
                    Error.InternalServer("Failed to delete message"));
            }
        }

        #endregion

        #region Edit Message

        public async Task<Result<MessageDto>> EditMessageAsync(
            int messageId,
            string currentUserId,
            string newContent)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newContent))
                    return Result.Failure<MessageDto>(
                        Error.Validation("Message content is empty"));

                var message = await _unitOfWork.Repository<Message>()
                    .GetByIdAsync(messageId);

                if (message == null)
                    return Result.Failure<MessageDto>(
                        Error.NotFound("Message not found"));

                if (message.SenderId != currentUserId)
                    return Result.Failure<MessageDto>(
                        Error.Forbidden("You cannot edit this message"));

                message.Content = newContent.Trim();
                message.IsEdited = true;

                _unitOfWork.Repository<Message>().Update(message);
                await _unitOfWork.Complete();

                message.Sender =
                    await _userManager.FindByIdAsync(currentUserId);

                return Result.Success(message.Adapt<MessageDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EditMessageAsync");
                return Result.Failure<MessageDto>(
                    Error.InternalServer("Failed to edit message"));
            }
        }

        #endregion

        #region Mark As Read (Placeholder)

        public async Task<Result> MarkAsReadAsync(int messageId, string userId)
        {
            try
            {
                // TODO: MessageStatus / Seen logic
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MarkAsReadAsync");
                return Result.Failure(
                    Error.InternalServer("Failed to mark message as read"));
            }
        }

        #endregion
    }

}


