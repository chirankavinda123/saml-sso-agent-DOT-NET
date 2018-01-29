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

        public bool IsSLORequest()
        {
            return  request.Params[SSOAgentConstants.SAML2SSO.HTTP_POST_PARAM_SAML2_AUTH_REQ] != null;
        }

        public bool IsSAML2SSOURL() {
            return request.RawUrl.EndsWith("/samlsso");
        }

        public bool IsSAML2SSOResponse(HttpRequest request)
        {
            return request.Params["SAMLResponse"] != null;                
        }

        public bool IsSLOURL()
        {
            //ssoAgentConfig.getSAML2().isSLOEnabled() &&

            return request.RawUrl.EndsWith(ssoAgentConfig.Saml2.SLOURL);

        }
    }
}