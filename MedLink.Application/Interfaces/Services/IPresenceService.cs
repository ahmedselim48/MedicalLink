using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedLink.Application.Interfaces.Services
{
    public interface IPresenceService
    {
        void UserConnected(string userId);
        void UserDisconnected(string userId);
        bool IsOnline(string userId);
    }
}
