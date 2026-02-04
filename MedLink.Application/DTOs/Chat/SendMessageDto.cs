using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedLink.Application.DTOs.Chat
{
    public class SendMessageDto
    {
        [Required(ErrorMessage = "محتوى الرسالة مطلوب")]
        [StringLength(2000, ErrorMessage = "الرسالة لا يمكن أن تتجاوز 2000 حرف")]
        public string Content { get; set; } = string.Empty;
    }
}
