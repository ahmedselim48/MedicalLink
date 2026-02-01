using MedLink.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedLink.Application.Services
{
    public class PresenceService : IPresenceService
    {
        private static readonly HashSet<string> OnlineUsers = new(); // ✅ أصبح string

        public void UserConnected(string userId) // ✅ أصبح string
        {
            OnlineUsers.Add(userId);
        }

        public void UserDisconnected(string userId) // ✅ أصبح string
        {
            OnlineUsers.Remove(userId);
        }

        public bool IsOnline(string userId) // ✅ أصبح string
        {
            return OnlineUsers.Contains(userId);
        }

        public IReadOnlyCollection<string> GetOnlineUsers()
        {
            return OnlineUsers.ToList().AsReadOnly();
        }
    }
}
