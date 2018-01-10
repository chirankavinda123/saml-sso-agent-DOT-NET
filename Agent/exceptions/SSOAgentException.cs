using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Agent.exceptions
{
    class SSOAgentException : HttpException
    {
        public SSOAgentException(String message) :base(message){
        }

        public SSOAgentException(String message, Exception innerException):base(message,innerException) {
        }
    }
}
