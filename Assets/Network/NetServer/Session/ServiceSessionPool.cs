using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Network.Server
{
    /// <summary>
    /// 会话客户端池
    /// </summary>
    public class ServiceSessionPool
    {
        // 会话池
        private static ConcurrentBag<ServiceSession> Sessions = new ConcurrentBag<ServiceSession>();

        /// <summary>
        /// 设置最大会话数量
        /// </summary>
        public static void SetMaxSessionClient(int count)
        {
            Sessions = new ConcurrentBag<ServiceSession>();
            for (int i = 0; i < count; i++)
            {
                Sessions.Add(new ServiceSession());
            }
        }

        /// <summary>
        /// 获取空会话
        /// </summary>
        public static ServiceSession GetSessionClient()
        {
            foreach (var session in Sessions)
            {
                if (!session.isUse) return session;
            }
            return null;
        }

        /// <summary>
        /// 获取所有在线会话
        /// </summary>
        public static IEnumerable<ServiceSession> GetOnlineSession()
        {
            List<ServiceSession> sessions = new List<ServiceSession>();
            foreach (var session in GetEnumerable())
            {
                if (session.isUse) sessions.Add(session);
            }
            return sessions;
        }

        /// <summary>
        /// 获取会话池迭代器
        /// </summary>
        public static IEnumerable<ServiceSession> GetEnumerable()
        {
            return Sessions;
        }
    }
}
