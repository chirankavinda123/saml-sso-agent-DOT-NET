using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace org.wso2.carbon.saml.agent.util
{
    public class SSOAgentRequestResolver
    {
        HttpRequest request = null;

        public SSOAgentRequestResolver(HttpRequest request) {
            this.request = request;
        }

        public Boolean isSAML2SSOURL() {
            return request.RawUrl.EndsWith("/samlsso");
        }

        public bool isSAML2SSOResponse(HttpRequest request)
        {
            return request.Params["SAMLResponse"] != null;                
        }
    }
}