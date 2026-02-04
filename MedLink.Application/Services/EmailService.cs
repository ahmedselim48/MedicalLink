using MailKit.Net.Smtp;
using MedLink.Application.Common.Email;
using MedLink.Application.DTOs.Identity;
using MedLink.Application.Interfaces.Services;
using MimeKit;


namespace MedLink.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailConfigurations _emailConfig;

        public EmailService(EmailConfigurations emailConfig)
        {
            _emailConfig = emailConfig;
        }

        public async Task SendEmailAsync(EmailMessage message)
        {
            var emailMessage = CreateEmailMessage(message);
            SendAsync(emailMessage);

        }


        private MimeMessage CreateEmailMessage(EmailMessage message)
        {

            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("", _emailConfig.From));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = GetEmailTemplate(message.Content, message.Subject)
            };
            return emailMessage;
        }

        private string GetEmailTemplate(string content, string subject)
        {
            var title = subject;
            var buttonText = subject.Contains("Reset", StringComparison.OrdinalIgnoreCase) ? "Reset Password" : "Confirm Email";

            return $@"
            <html>
                <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;'>
                    <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 8px; box-shadow: 0 0 10px rgba(0,0,0,0.1);'>
                        <h2 style='color: #333333; text-align: center;'>{title}</h2>
                        <p style='color: #666666; font-size: 16px; line-height: 1.5;'>
                            Please click the button below to proceed:
                        </p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{content}' style='background-color: #007bff; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block;'>
                                {buttonText}
                            </a>
                        </div>
                        <p style='color: #999999; font-size: 12px; text-align: center; margin-top: 20px;'>
                            If you didn't request this, please ignore this email.
                        </p>
                    </div>
                </body>
            </html>";

        }


        private async Task SendAsync(MimeMessage emailMessage)
        {

            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, true);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    await client.AuthenticateAsync(_emailConfig.UserName, _emailConfig.Password);
                    await client.SendAsync(emailMessage);
                }
                catch (Exception)
                {

                    throw;
                }
                finally
                {
                    await client.DisconnectAsync(true);
                    client.Dispose();
                }
            }

        }

    }
}
