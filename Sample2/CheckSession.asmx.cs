using System.Web.Services;

namespace Sample2
{
    /// <summary>
    /// Summary description for CheckSession
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class CheckSession : System.Web.Services.WebService
    {

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }

        [WebMethod(EnableSession = true)]
        public string GetSessionValue()
        {
            string sessionVal = string.Empty;


            if (Session["claims"] == null)
            {
                sessionVal = "false";
            }
            else
            {
                sessionVal = "true";
            }

            return sessionVal;
        }

        [WebMethod(EnableSession = true)]
        public string SetSessionValue()
        {
            Session["claims"] = "helloValue";

            return "session value set";
        }
    }
}
