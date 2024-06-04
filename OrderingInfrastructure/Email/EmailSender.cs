using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderingDomain.Optionals;

namespace OrderingInfrastructure.Email;

internal sealed class EmailSender : InfrastructureService<EmailSender>, IEmailSender, IDisposable
{
    public EmailSender( IConfiguration configuration, ILogger<EmailSender> logger ) : base( logger )
    {
        _config = GetSmtpConfiguration( configuration );
        _smtp = new SmtpClient( _config.Host, _config.Port );
        _smtp.Credentials = new NetworkCredential( _config.Email, _config.Password );
        _smtp.EnableSsl = true;
    }
    public void Dispose()
    {
        _smtp.Dispose();
    }

    readonly SmtpConfiguration _config;
    readonly SmtpClient _smtp;

    public Reply<bool> SendBasicEmail( string to, string header, string body ) =>
        SendEmail( to, header, body, false );
    public Reply<bool> SendHtmlEmail( string to, string header, string body ) =>
        SendEmail( to, header, body, true );

    Reply<bool> SendEmail( string toEmail, string subject, string messageBody, bool isHtml )
    {
        try {
            MailMessage mailMessage = new() {
                From = new MailAddress( _config.Email ),
                Subject = subject,
                Body = messageBody,
                IsBodyHtml = isHtml
            };
            mailMessage.To.Add( toEmail );
            _smtp.Send( mailMessage );
            return IReply.Okay();
        }
        catch ( Exception e ) {
            Logger.LogError( e, e.Message );
            return IReply.None( "Failed to send an email with smtp client." );
        }
    }
    static SmtpConfiguration GetSmtpConfiguration( IConfiguration config )
    {
        SmtpConfiguration c = config.GetSection( "SmtpClient" ).Get<SmtpConfiguration>() ?? throw new Exception( $"Failed to get {nameof( SmtpConfiguration )} from IConfiguration." );

        if (string.IsNullOrEmpty( c.Email ))
            throw new Exception( $"Failed to get {nameof( SmtpConfiguration )} from IConfiguration." );

        return c;
    }

    sealed class SmtpConfiguration
    {
        public string Host { get; init; } = string.Empty;
        public int Port { get; init; }
        public string Email { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }
}