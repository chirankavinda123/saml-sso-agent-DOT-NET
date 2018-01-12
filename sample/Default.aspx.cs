using Agent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace sample
{
    public partial class _Default : Page
    {
        public Dictionary<String, String> Claims { get; set; }

        public IPrincipal Principal { get; set; } = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            Claims = (Dictionary<String, String>)HttpContext.Current.Session["claims"];
        }
    }
}