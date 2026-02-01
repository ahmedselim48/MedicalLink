using Domain.ErrorHandling;
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

        public async Task<Result<MessageDto>> SendMessageAsync(int appointmentId, string senderId, string content)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                    return Result.Failure<MessageDto>(Error.Validation("Message content is empty"));

                var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(appointmentId);
                if (appointment == null)
                    return Result.Failure<MessageDto>(Error.NotFound("Appointment not found"));

             
                var sender = await _userManager.FindByIdAsync(senderId); 
                if (sender == null)
                    return Result.Failure<MessageDto>(Error.NotFound("Sender not found"));

                if (!await _chatRoomService.CanUserAccessAsync(appointmentId, senderId))
                    return Result.Failure<MessageDto>(Error.Forbidden("Access denied"));

                var chatRoomResult = await _chatRoomService.GetOrCreateChatRoomAsync(appointmentId);
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

                var messageDto = new MessageDto
                {
                    Id = message.Id,
                    ChatRoomId = message.ChatRoomId,
                    SenderId = message.SenderId,
                    SenderName = sender.FullName ?? sender.UserName ?? "User",
                    Content = message.Content,
                    CreatedAt = message.CreatedAt,
                    IsDeleted = message.IsDeleted
                };

                return Result.Success(messageDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendMessageAsync");
                return Result.Failure<MessageDto>(Error.InternalServer("Failed to send message"));
            }
        }
        public async Task<Result<List<MessageDto>>> GetMessagesAsync(int appointmentId, int page, int pageSize)
        {
            try
            {
                if (page <= 0) page = 1;
                if (pageSize <= 0 || pageSize > 100) pageSize = 50;

                var chatRoomResult = await _chatRoomService.GetOrCreateChatRoomAsync(appointmentId);
                if (chatRoomResult.IsFailure)
                    return Result.Failure<List<MessageDto>>(chatRoomResult.Error);

                var spec = new MessageWithSenderSpec(chatRoomResult.Value.Id, page, pageSize);
                var messages = await _unitOfWork.Repository<Message>().GetAllWithSpecAsync(spec);
                var senderIds = messages.Select(m => m.SenderId).Distinct().ToList();
                var senders = await _userManager.Users
                        .Where(u => senderIds.Contains(u.Id))
                        .ToDictionaryAsync(u => u.Id);

                var dtos = messages.OrderBy(m => m.CreatedAt)
                    .Select(m =>
                    {
                        senders.TryGetValue(m.SenderId, out var user);

                        return new MessageDto
                        {
                            Id = m.Id,
                            ChatRoomId = m.ChatRoomId,
                            SenderId = m.SenderId,
                            SenderName = user?.FullName ?? user?.UserName ?? "User",
                            Content = m.Content,
                            CreatedAt = m.CreatedAt,
                            IsDeleted = m.IsDeleted
                        };
                    })
                    .ToList();

                return Result.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetMessagesAsync");
                return Result.Failure<List<MessageDto>>(Error.InternalServer("Failed to get messages"));
            }

        }




        public async Task<Result> DeleteMessageAsync(int messageId, string currentUserId, bool isAdmin)
        {
            try
            {
                var message = await _unitOfWork.Repository<Message>()
                    .GetByIdAsync(messageId);

                if (message == null)
                    return Result.Failure(Error.NotFound("Message not found"));

         
                if (!isAdmin && message.SenderId != currentUserId)
                    return Result.Failure(Error.Forbidden("You cannot delete this message"));

                message.IsDeleted = true;
       

                _unitOfWork.Repository<Message>().Update(message);
                await _unitOfWork.Complete();

                _logger.LogInformation($"Message {messageId} deleted by user {currentUserId}");
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteMessageAsync");
                return Result.Failure(Error.InternalServer("Failed to delete message"));
            }
        }

        public async Task<Result<MessageDto>> EditMessageAsync(int messageId, string currentUserId, string newContent)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newContent))
                    return Result.Failure<MessageDto>(Error.Validation("Message content is empty"));

                var message = await _unitOfWork.Repository<Message>()
                    .GetByIdAsync(messageId);

                if (message == null)
                    return Result.Failure<MessageDto>(Error.NotFound("Message not found"));

                if (message.SenderId != currentUserId)
                    return Result.Failure<MessageDto>(Error.Forbidden("You cannot edit this message"));


                message.Content = newContent.Trim();
                message.IsEdited = true;
               

                _unitOfWork.Repository<Message>().Update(message);
                await _unitOfWork.Complete();

                var sender = await _userManager.FindByIdAsync(currentUserId);

                var messageDto = new MessageDto
                {
                    Id = message.Id,
                    ChatRoomId = message.ChatRoomId,
                    SenderId = message.SenderId,
                    SenderName = sender?.FullName ?? sender?.UserName ?? "User",
                    Content = message.Content,
                    CreatedAt = message.CreatedAt,
                    IsDeleted = message.IsDeleted
                };

                return Result.Success(messageDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EditMessageAsync");
                return Result.Failure<MessageDto>(Error.InternalServer("Failed to edit message"));
            }
        }

        public async Task<Result> MarkAsReadAsync(int messageId, string userId)
        {
            try
            {
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MarkAsReadAsync");
                return Result.Failure(Error.InternalServer("Failed to mark message as read"));
            }
        }
    }
}


