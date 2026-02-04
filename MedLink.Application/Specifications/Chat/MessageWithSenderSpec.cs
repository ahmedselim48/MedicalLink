using MedLink.Domain.Entities.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedLink.Application.Specifications.Chat
{
   
        public class MessageWithSenderSpec : BaseSpecification<Message>
        {
            public MessageWithSenderSpec(int chatRoomId, int page, int pageSize)
                : base(m => m.ChatRoomId == chatRoomId)
            {
                AddIncludes(m => m.Sender);        // Include User
                AddOrderBy(m => m.CreatedAt);      // sort by time
                ApplyPagination((page - 1) * pageSize, pageSize);
            }
        }


    
}
