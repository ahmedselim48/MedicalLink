using Domain.ErrorHandling;
using MedLink.Application.DTOs.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedLink.Application.Interfaces.Services
{
    public interface IMessageService
    {
        Task<Result<MessageDto>> SendMessageAsync(int appointmentId, string senderId, string content);
        Task<Result<List<MessageDto>>> GetMessagesAsync(int appointmentId, int page, int pageSize);
        Task<Result> DeleteMessageAsync(int messageId, string currentUserId, bool isAdmin);
    }

}
