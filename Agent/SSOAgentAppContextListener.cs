using Agent.util;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Agent
{
    public class SSOAgentAppContextListener
    {
        public void InitializePropertySet(SettingsPropertyCollection proeprtiesInSettingsFile) {

            Dictionary<string, string> ssoProperties = new Dictionary<string, string>();

            //assign default values for properties
            loadDefaultValuesForProperties(ssoProperties);

            //load values from Settings.settings file
            loadSettingsFileValuesForProperties(ssoProperties, proeprtiesInSettingsFile);
           
            SSOAgentConfig ssoAgentConfig = new SSOAgentConfig(ssoProperties);
           
            HttpContext.Current.Application[SSOAgentConstants.CONFIG_BEAN_NAME] = ssoAgentConfig;
        }

        private void loadSettingsFileValuesForProperties(Dictionary<string, string> ssoProperties, SettingsPropertyCollection proeprtiesInSettingsFile)
        {
            //overriding default values with proeprties defined in Settings.settings file
            foreach (SettingsProperty property in proeprtiesInSettingsFile) {
                if (ssoProperties.Keys.Contains(property.Name)) {
                    ssoProperties[property.Name] = property.DefaultValue.ToString();
                }
            }
        }

        private void loadDefaultValuesForProperties(Dictionary<string, string> ssoProperties)
        {
            ssoProperties.Add(SSOAgentConstants.SSOAgentConfig.SAML2_SSO_URL,"samlsso");
            ssoProperties.Add(SSOAgentConstants.SSOAgentConfig.SAML2.HTTP_BINDING,SSOAgentConstants.SAML2SSO.SAML2_POST_BINDING_URI);
            ssoProperties.Add(SSOAgentConstants.SSOAgentConfig.SAML2.SP_ENTITY_ID,null);
            ssoProperties.Add(SSOAgentConstants.SSOAgentConfig.SAML2.ACS_URL,null);
            ssoProperties.Add(SSOAgentConstants.SSOAgentConfig.SAML2.IDP_ENTITY_ID,"localhost");
            ssoProperties.Add(SSOAgentConstants.SSOAgentConfig.SAML2.IDP_URL, "https://localhost:9443/samlsso");
            ssoProperties.Add(SSOAgentConstants.SSOAgentConfig.SAML2.ATTRIBUTE_CONSUMING_SERVICE_INDEX,null);
            ssoProperties.Add(SSOAgentConstants.SSOAgentConfig.SAML2.ENABLE_SLO,"false");
            ssoProperties.Add(SSOAgentConstants.SSOAgentConfig.SAML2.SLO_URL, "samllogout");
            ssoProperties.Add(SSOAgentConstants.SSOAgentConfig.SAML2.ENABLE_ASSERTION_SIGNING,"false");
            ssoProperties.Add(SSOAgentConstants.SSOAgentConfig.SAML2.ENABLE_ASSERTION_ENCRYPTION, "false");
            ssoProperties.Add(SSOAgentConstants.SSOAgentConfig.SAML2.ENABLE_RESPONSE_SIGNING, "false");
            ssoProperties.Add(SSOAgentConstants.SSOAgentConfig.SAML2.ENABLE_REQUEST_SIGNING, "false");
            ssoProperties.Add(SSOAgentConstants.SSOAgentConfig.SAML2.IS_PASSIVE_AUTHN, "false");
            ssoProperties.Add(SSOAgentConstants.SSOAgentConfig.SAML2.IS_FORCE_AUTHN, "false");
            ssoProperties.Add(SSOAgentConstants.SSOAgentConfig.SAML2.RELAY_STATE,null);
            ssoProperties.Add(SSOAgentConstants.SSOAgentConfig.SAML2.POST_BINDING_REQUEST_HTML_PAYLOAD,null);
            ssoProperties.Add(SSOAgentConstants.SSOAgentConfig.SAML2.TIME_STAMP_SKEW,"300");

        }
    }
}
