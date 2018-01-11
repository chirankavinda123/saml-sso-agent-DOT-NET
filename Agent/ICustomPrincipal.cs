using System.Security.Principal;

namespace Agent
{
    internal interface ICustomPrincipal : IPrincipal
    {
        string FirstName { get; set; }

        string LastName { get; set; }
    }
}