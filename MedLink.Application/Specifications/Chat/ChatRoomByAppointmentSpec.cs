using MedLink.Domain.Entities.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedLink.Application.Specifications.Chat
{
    public class ChatRoomByAppointmentSpec : BaseSpecification<ChatRoom>
    {
        public ChatRoomByAppointmentSpec(int appointmentId)
            : base(cr => cr.AppointmentId == appointmentId)
        {
            // مش محتاج includes هنا، المهم تجيب الشات
        }
    }
}
