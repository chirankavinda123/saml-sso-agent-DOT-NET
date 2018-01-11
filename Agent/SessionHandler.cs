using System.Web;
using System.Web.SessionState;

namespace Agent
{
    public class SessionHandler : IRequiresSessionState
    {
        public SessionHandler() { }

        public object GetSession(string key)
        {
            object session = HttpContext.Current.Session[key];
            return session;
        }
        public void SetSession(object data, string key)
        {
            HttpContext.Current.Session.Add(key, data);
        }

        public string Test(string sessionKey)
        {

            string ss = GetSession(sessionKey).ToString();
            return ss + " from ClassLibrary1";
        }

    }
}
