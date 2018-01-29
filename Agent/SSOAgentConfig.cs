using System;
using System.Collections.Generic;
using Agent.exceptions;
using Agent.util;
using NLog;

namespace Agent
{
    public class SSOAgentConfig
    {

        private bool isSAML2SSOLoginEnabled = false;

        private string SAML2SSOURL { get; set; } = null;

        private HashSet<string> SkipURIs { get; set; } = new HashSet<string>();

        private Dictionary<string, string[]> QueryParams { get; set; } = new Dictionary<string, string[]>();

        //setting public since getter needs access for outer ref
        public SAML2 Saml2 { get; set; } = new SAML2();
        //private skipURIs ;

        public SSOAgentConfig(Dictionary<string,string> properties)
        {
            InitConfig(properties);            
        }

        private void InitConfig(Dictionary<string,string> ssoProperties)
        {
            SAML2SSOURL = ssoProperties[SSOAgentConstants.SSOAgentConfig.SAML2_SSO_URL];

            Saml2.HttpBinding = ssoProperties[SSOAgentConstants.SSOAgentConfig.SAML2.HTTP_BINDING];
            Saml2.SPEntityId = ssoProperties[SSOAgentConstants.SSOAgentConfig.SAML2.SP_ENTITY_ID];
     
            Saml2.ACSURL = ssoProperties[SSOAgentConstants.SSOAgentConfig.SAML2.ACS_URL];
            Saml2.IdPEntityId = ssoProperties[SSOAgentConstants.SSOAgentConfig.SAML2.IDP_ENTITY_ID];
            Saml2.IdPURL = ssoProperties[SSOAgentConstants.SSOAgentConfig.SAML2.IDP_URL];
            Saml2.AttributeConsumingServiceIndex = ssoProperties[SSOAgentConstants.SSOAgentConfig.SAML2.ATTRIBUTE_CONSUMING_SERVICE_INDEX];

            Saml2.SetSLOEnabled = ssoProperties[SSOAgentConstants.SSOAgentConfig.SAML2.ENABLE_SLO];
            Saml2.SLOURL = ssoProperties[SSOAgentConstants.SSOAgentConfig.SAML2.SLO_URL];

            Saml2.SetAssertionSigned  = ssoProperties[SSOAgentConstants.SSOAgentConfig.SAML2.ENABLE_ASSERTION_SIGNING];
            Saml2.SetAssertionEncrypted = ssoProperties[SSOAgentConstants.SSOAgentConfig.SAML2.ENABLE_ASSERTION_ENCRYPTION];
            Saml2.SetResponseSigned = ssoProperties[SSOAgentConstants.SSOAgentConfig.SAML2.ENABLE_RESPONSE_SIGNING];
            Saml2.SetRequestSigned = ssoProperties[SSOAgentConstants.SSOAgentConfig.SAML2.ENABLE_REQUEST_SIGNING];
            Saml2.SetPassiveAuthn = ssoProperties[SSOAgentConstants.SSOAgentConfig.SAML2.IS_PASSIVE_AUTHN];
            Saml2.SetForceAuthn = ssoProperties[SSOAgentConstants.SSOAgentConfig.SAML2.IS_FORCE_AUTHN];

            Saml2.RelayState = ssoProperties[SSOAgentConstants.SSOAgentConfig.SAML2.RELAY_STATE];
            Saml2.PostBindingRequestHTMLPayload = ssoProperties[SSOAgentConstants.SSOAgentConfig.SAML2.POST_BINDING_REQUEST_HTML_PAYLOAD];
            Saml2.TimeStampSkewInSeconds = ssoProperties[SSOAgentConstants.SSOAgentConfig.SAML2.TIME_STAMP_SKEW];

            Saml2.PostLogoutRedirectURL = ssoProperties[SSOAgentConstants.SSOAgentConfig.SAML2.POST_LOGOUT_REDIRECT_URL];
        }
    }

    public class SAML2
    {
        Logger logger = LogManager.GetCurrentClassLogger();

        private String httpBinding = null;
        private String spEntityId = null;
        private String acsURL = null;
        private String idPEntityId = null;
        private String idPURL = null;

        private String sloURL = null;
        private String attributeConsumingServiceIndex = null;

        private bool isSLOEnabled = false;     
        private bool isAssertionSigned = false;
        private bool isAssertionEncrypted = false;
        private bool isResponseSigned = false;
        private bool isRequestSigned = false;
        private bool isPassiveAuthn = false;
        private bool isForceAuthn = false;
        private String relayState = null;
        private String signatureValidatorImplClass = null;
        private int timeStampSkewInSeconds = 300;
        
        private String postBindingRequestHTMLPayload = null;

        public string HttpBinding
        {
            get { return httpBinding; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    logger.Info(SSOAgentConstants.SSOAgentConfig.SAML2.HTTP_BINDING +" not configured properly. Defaulting to \'" +
                        SSOAgentConstants.SAML2SSO.SAML2_POST_BINDING_URI + "\'");
                    httpBinding = SSOAgentConstants.SAML2SSO.SAML2_POST_BINDING_URI;
                }      
                else
                {
                    httpBinding = value;
                }
            }
        }

        public String SPEntityId
        {
            get{ return spEntityId; }
            set{
                if (!string.IsNullOrWhiteSpace(value))
                {
                    spEntityId = value;
                }
                else
                {
                    throw new SSOAgentException(SSOAgentConstants.SSOAgentConfig.SAML2.SP_ENTITY_ID + 
                        "was not configured. Cannot proceed Further.");
                }
            }       
        }

        public String ACSURL
        {
            get { return acsURL; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    acsURL = value;
                }
                else {
                    throw new SSOAgentException( SSOAgentConstants.SSOAgentConfig.SAML2.ACS_URL + 
                        " is not specified. Cannot proceed Further.");
                }
            }          
        }

        public String IdPEntityId
        {
            get { return idPEntityId; }
            set 
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    idPEntityId = value;
                }
                else
                {
                    throw new SSOAgentException( SSOAgentConstants.SSOAgentConfig.SAML2.IDP_ENTITY_ID + 
                        " context-param is not specified in the web.xml. Cannot proceed Further.");
                }
            }
        }

        public String IdPURL
        {
            get { return idPURL; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    idPURL = value;
                }
                else
                {
                    throw new SSOAgentException(SSOAgentConstants.SSOAgentConfig.SAML2.IDP_URL+" is not configured. Cannot proceed further.");
                }
            }
        }
        
        public String SLOURL
        {
            get{ return sloURL; }
            set { sloURL = value; }
        }

        public String AttributeConsumingServiceIndex
        {
            get { return attributeConsumingServiceIndex; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    attributeConsumingServiceIndex = value;
                }
                else{
                    logger.Info("Attribute Consuming Service index is not specified.IDP configuration is required to get user attributes.");
                }
            }
        }
        
        public bool IsAssertionSigned
        {
            get { return isAssertionSigned; }
        }

        public Boolean IsAssertionEncrypted
        {
            get { return isAssertionEncrypted; }
        }

        public Boolean IsResponseSigned
        {
            get { return isResponseSigned; }
        }

        public Boolean IsRequestSigned
        {
           get { return isRequestSigned; }
        }

        public Boolean IsPassiveAuthn
        {
            get{ return isPassiveAuthn; }
        }

        public Boolean IsForceAuthn
        {
            get { return isForceAuthn; }
        }

        public String RelayState
        {
            get { return relayState; }
            set { relayState = value; }
        }

        public String PostBindingRequestHTMLPayload
        {
            get { return postBindingRequestHTMLPayload; }
            set { postBindingRequestHTMLPayload = value; }
        }

        public bool IsSLOEnabled
        {
            get { return isSLOEnabled; }
        }

        public String SetSLOEnabled
        {        
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    isSLOEnabled = bool.Parse(value);
                }
                else
                {
                    //defaulting to false since not configured
                    isSLOEnabled = false;
                }
            } 
        }

        public string SetAssertionSigned
        {
            set{
                if (string.IsNullOrWhiteSpace(value))
                {
                    isAssertionSigned = bool.Parse(value);
                }
                else
                {
                    logger.Info(SSOAgentConstants.SSOAgentConfig.SAML2.ENABLE_ASSERTION_SIGNING + " not configured properly." +
                        " Defaulting to \'false\'");
                    isAssertionSigned = false;
                }
            }
        }

        public String SetAssertionEncrypted
        {
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    isAssertionEncrypted = bool.Parse(value);       
                }
                else
                {
                    logger.Info(SSOAgentConstants.SSOAgentConfig.SAML2.ENABLE_ASSERTION_ENCRYPTION +" is not configured properly." +
                        " Defaulting to \'false\'");
                    this.isAssertionEncrypted = false;
                }
            }
        }

        public String SetResponseSigned
        {
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    isResponseSigned = bool.Parse(value);
                }
                else
                {
                    logger.Info(SSOAgentConstants.SSOAgentConfig.SAML2.ENABLE_RESPONSE_SIGNING +"is not configured properly." +
                        " Defaulting to \'false\'");
                    isResponseSigned = false;
                }
            }
        }

        public String SetRequestSigned
        {
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    isRequestSigned = bool.Parse(value);
                }
                else     
                {
                    logger.Info(SSOAgentConstants.SSOAgentConfig.SAML2.ENABLE_REQUEST_SIGNING + "is not configured properly." +
                        " Defaulting to \'false\'");
                    isRequestSigned = false;
                }
            }
        }

        public String SetPassiveAuthn
        {
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    isPassiveAuthn = bool.Parse(value);
                }
                else{
                    //defaulting to false
                    logger.Info(SSOAgentConstants.SSOAgentConfig.SAML2.IS_PASSIVE_AUTHN + "is not configured properly." +
                        " Defaulting to \'false\'");
                    isPassiveAuthn = false;
                }
            }
        }

        public bool PassiveAuthn
        {
             set { isPassiveAuthn = value; }
        }

        public String SetForceAuthn
        {
            set {
                if (string.IsNullOrWhiteSpace(value))
                {
                    isForceAuthn = bool.Parse(value);
                }
                else
                {
                    logger.Info(SSOAgentConstants.SSOAgentConfig.SAML2.IS_FORCE_AUTHN + 
                        "is not configured properly. Defaulting to \'false\'");
                    this.isForceAuthn = false;
                }
            }
        }

        public String GetSignatureValidatorImplClass()
        {
            return signatureValidatorImplClass;
        }

        public void SetSignatureValidatorImplClass(String signatureValidatorImplClass)
        {
            if (this.isResponseSigned || this.isAssertionSigned)
            {
                if (signatureValidatorImplClass != null)
                {
                    this.signatureValidatorImplClass = signatureValidatorImplClass;
                }
                else
                {
                    //LOGGER.log(Level.FINE, SSOAgentConstants.SSOAgentConfig.SAML2.SIGNATURE_VALIDATOR +
                      //      " not configured.");
                }
            }
        }

        public int GetTimeStampSkewInSeconds()
        {
            return timeStampSkewInSeconds;
        }

        public string TimeStampSkewInSeconds
        {
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    timeStampSkewInSeconds = int.Parse(value);
                }
                else
                {
                    logger.Info(SSOAgentConstants.SSOAgentConfig.SAML2.TIME_STAMP_SKEW + 
                        "is not configured properly. Defaulting to 300 seconds.");
                    timeStampSkewInSeconds = 300;
                }
            }
        }

        public string PostLogoutRedirectURL
        {
            get;set;
        }
    }
}
