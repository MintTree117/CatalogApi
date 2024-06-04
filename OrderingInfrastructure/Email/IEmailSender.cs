using OrderingDomain.Optionals;

namespace OrderingInfrastructure.Email;

public interface IEmailSender
{
    public Reply<bool> SendBasicEmail( string to, string header, string body );
    public Reply<bool> SendHtmlEmail( string to, string header, string body );
}