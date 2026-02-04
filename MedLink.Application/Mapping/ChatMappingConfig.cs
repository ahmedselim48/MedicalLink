using Mapster;
using MedLink.Application.DTOs.Chat;
using MedLink.Domain.Entities.Appointments;
using MedLink.Domain.Entities.Chat;
using MedLink.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedLink.Application.Mapping
{
    public class ChatMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
    
            config.NewConfig<Message, MessageDto>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.ChatRoomId, src => src.ChatRoomId)
                .Map(dest => dest.SenderId, src => src.SenderId!)
                .Map(dest => dest.SenderName,
                    src => src.Sender != null ? src.Sender.FullName : "Unknown")
                .Map(dest => dest.Content,
                    src => src.IsDeleted ? "[deleted]" : src.Content)
                .Map(dest => dest.CreatedAt, src => src.CreatedAt)
                .Map(dest => dest.IsDeleted, src => src.IsDeleted);

            config.NewConfig<Appointment, ChatRoomInfoDto>()
                .Map(dest => dest.AppointmentId, src => src.Id)
                .Map(dest => dest.AppointmentDate,
                    src => src.Schedule != null
                        ? src.Schedule.Date
                        : src.CreatedAt)
                .Ignore(dest => dest.ChatRoomId)
                .Ignore(dest => dest.OtherUserId)
                .Ignore(dest => dest.OtherUserName);
        }
    }

}
