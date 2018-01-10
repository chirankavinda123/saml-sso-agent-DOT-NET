using Agent;
using Agent.util;
using System;
using System.Web;

namespace org.wso2.carbon.identity.agent.util
{
    public class SSOAgentRequestResolver
    {
        HttpRequest request;
        SSOAgentConfig ssoAgentConfig;

        public SSOAgentRequestResolver(HttpRequest request, SSOAgentConfig ssoAgentConfig) {
            this.request = request;
            this.ssoAgentConfig = ssoAgentConfig;
        }

        public Boolean isSAML2SSOURL() {
            return request.RawUrl.EndsWith("/samlsso");
        }

        public bool isSAML2SSOResponse(HttpRequest request)
        {
            return request.Params["SAMLResponse"] != null;                
        }

        public bool isSLOURL()
        {
            //ssoAgentConfig.getSAML2().isSLOEnabled() &&

            return request.RawUrl.EndsWith(ssoAgentConfig.Saml2.SLOURL);

        }
    }
}