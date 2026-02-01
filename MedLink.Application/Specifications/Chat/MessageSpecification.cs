using MedLink.Domain.Entities.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedLink.Application.Specifications.Chat
{
    public class MessageSpecification : BaseSpecification<Message>
    {
        public MessageSpecification(int chatRoomId, int page, int pageSize)
            : base(m => m.ChatRoomId == chatRoomId && !m.IsDeleted)
        {
            Includes.Add(m => m.Sender); // هات السيندر مع الرسالة
            AddOrderByDesc(m => m.CreatedAt);

            if (page > 0 && pageSize > 0)
            {
                Skip = (page - 1) * pageSize;
                Take = pageSize;
                IsPaginationEnabled = true;
            }
        }
    }
}
