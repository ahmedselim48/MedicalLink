
using Domain.ErrorHandling;
using MedLink.Application.DTOs.Chat;
using MedLink.Domain.Entities.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedLink.Application.Interfaces.Services
{
    public interface IChatRoomService
    {
        Task<Result<ChatRoom>> GetOrCreateChatRoomAsync(int appointmentId);
        Task<bool> CanUserAccessAsync(int appointmentId, string userId);
        Task<Result<ChatRoomInfoDto>> GetChatRoomInfoAsync(int appointmentId, string currentUserId);
    }

}
