using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedLink.Application.DTOs.Chat
{
    public class ChatRoomInfoDto
    {
        public int AppointmentId { get; set; }
        public int? ChatRoomId { get; set; }
        public string OtherUserId { get; set; }
        public string OtherUserName { get; set; }
        public DateTime AppointmentDate { get; set; }
    }
}
