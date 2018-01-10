using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MyModule
{
    public class SyncModule : IHttpModule
    {
        private MyEventHandler _eventHandler = null;

        public void Init(HttpApplication app)
        {
            app.BeginRequest += new EventHandler(OnBeginRequest);
        }

        public void Dispose() { }
    }

    public delegate void MyEventHandler(Object s, EventArgs e);
}
