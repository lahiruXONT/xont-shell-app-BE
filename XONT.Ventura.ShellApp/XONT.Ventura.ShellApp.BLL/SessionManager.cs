using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XONT.Ventura.ShellApp.BLL
{
    public class SessionManager
    {
        private readonly ConcurrentDictionary<string, SessionData> _sessions = new();

        public void CreateSession(string sessionId, string userName, string businessUnit, List<string> unAuthorizedTasks)
        {
            var session = new SessionData
            {
                SessionId = sessionId,
                UserName = userName,
                BusinessUnit = businessUnit,
                CreatedAt = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow,
                UnAuthorizedTasks = unAuthorizedTasks
            };

            _sessions[sessionId] = session;
        }

        public void UpdateActivity(string sessionId)
        {
            if (_sessions.TryGetValue(sessionId, out var session))
            {
                session.LastActivity = DateTime.UtcNow;
            }
        }

        public SessionData? GetSession(string sessionId)
        {
            _sessions.TryGetValue(sessionId, out var session);
            return session;
        }

        public void RemoveSession(string sessionId)
        {
            _sessions.TryRemove(sessionId, out _);
        }

        public IEnumerable<SessionData> GetActiveSessions()
        {
            var cutoff = DateTime.UtcNow.AddMinutes(-30);
            return _sessions.Values.Where(s => s.LastActivity > cutoff);
        }
    }

    public class SessionData
    {
        public string SessionId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string BusinessUnit { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime LastActivity { get; set; }
        public List<string> UnAuthorizedTasks { get; set; } = new();
    }
}
