using Agent;
using Agent.util;
using NLog;
using org.wso2.carbon.identity.agent.saml;
using org.wso2.carbon.identity.agent.util;
using System;
using System.Web;
using System.Web.UI;

namespace org.wso2.carbon.identity.agent
{
    public class FilteringHttpModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            // Registering event handlers
            /* context.BeginRequest += OnApplicationBeginRequest;
               context.PostAcquireRequestState += PostAcquireRequestState;
            */
            context.PostAcquireRequestState += OnApplicationBeginRequest;
        }

        private void OnApplicationBeginRequest(object sender, EventArgs e)
        {
            Logger logger = LogManager.GetCurrentClassLogger();
            logger.Info("application begin request");

            HttpApplication application = (HttpApplication)sender;
            HttpContext context = application.Context;
       
            SSOAgentConfig ssoAgentConfig =  (SSOAgentConfig)HttpContext.Current.Application[SSOAgentConstants.CONFIG_BEAN_NAME];
            SSOAgentRequestResolver requestResolver = new SSOAgentRequestResolver(context.Request,ssoAgentConfig);

            if (requestResolver.isSLOURL()) {
                SAML2SSOManager samlSSOManager = new SAML2SSOManager(ssoAgentConfig);

                if (ssoAgentConfig.Saml2.HttpBinding == SSOAgentConstants.SAML2SSO.SAML2_REDIRECT_BINDING_URI)
                {
                    context.Response.Redirect(samlSSOManager.BuildLogoutRequest());
                }
                else
                {
                    samlSSOManager.SendPOSTLogoutRequest(context);
                }                   
            }

            //Incomplete block
            else if (requestResolver.isSAML2SSOResponse(context.Request))
            {
                SAML2SSOManager samlSSOManager = new SAML2SSOManager(ssoAgentConfig);
                samlSSOManager.ProcessSAMLResponse(context.Request, context.Response);
            }

            else if (requestResolver.isSAML2SSOURL())
            {
                SAML2SSOManager samlSSOManager = new SAML2SSOManager(ssoAgentConfig);
                
                if (ssoAgentConfig.Saml2.HttpBinding == SSOAgentConstants.SAML2SSO.SAML2_REDIRECT_BINDING_URI)
                {
                    context.Response.Redirect(samlSSOManager.BuildRedirectRequest());
                }
                else
                {
                    samlSSOManager.SendPOSTRequest(context);
                }
            }    
        }

        public void PostAcquireRequestState(object sender, EventArgs e)
        {
            HttpApplication application = ((HttpApplication)sender);
            HttpContext context = application.Context;

            context.Session.Add("preHandler","valueHere");
        }

        public void Dispose()
        {
        }
    }
}