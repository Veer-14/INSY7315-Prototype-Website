using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using PKValves.Models;

namespace PKValves.Services
{
    public class ContactEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ContactEmailService> _logger;

        public ContactEmailService(
            IConfiguration configuration,
            ILogger<ContactEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEnquiryAsync(ContactViewModel enquiry)
        {
            string senderEmail = GetRequiredSetting("SenderEmail");
            string password = GetRequiredSetting("Password");
            string recipientEmail = GetRequiredSetting("RecipientEmail");
            string host = GetRequiredSetting("Host");

            if (!int.TryParse(
                GetRequiredSetting("Port"),
                out int port) || port < 1 || port > 65535)
            {
                throw new InvalidOperationException(
                    "The email port is invalid.");
            }

            var email = new MimeMessage();

            // The website sends through your authorised email account.
            email.From.Add(
                new MailboxAddress("PK Valves Website", senderEmail));

            // During testing, this will be your own inbox.
            email.To.Add(MailboxAddress.Parse(recipientEmail));

            // Clicking Reply will address the customer's email.
            email.ReplyTo.Add(
                new MailboxAddress(
                    enquiry.Name.Trim(),
                    enquiry.Email.Trim()));

            email.Subject =
                $"Website enquiry: {enquiry.Subject}";

            // Plain text keeps the customer's message as text.
            email.Body = new TextPart("plain")
            {
                Text =
                    $"New enquiry from the PK Valves website\n\n" +
                    $"Name: {enquiry.Name.Trim()}\n" +
                    $"Email: {enquiry.Email.Trim()}\n" +
                    $"Phone: {enquiry.Phone?.Trim() ?? "Not provided"}\n" +
                    $"Subject: {enquiry.Subject}\n\n" +
                    $"Message:\n{enquiry.Message.Trim()}"
            };

            using var client = new SmtpClient();

            client.Timeout = 30000;

            await client.ConnectAsync(
                host,
                port,
                SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(senderEmail, password);

            await client.SendAsync(email);

            // Sending has succeeded at this point.
            // A disconnect problem should not report the send as failed.
            try
            {
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    "Email was accepted, but SMTP disconnect failed: {ErrorType}",
                    ex.GetType().Name);
            }
        }

        private string GetRequiredSetting(string name)
        {
            return _configuration[$"EmailSettings:{name}"]
                ?? throw new InvalidOperationException(
                    $"EmailSettings:{name} is missing.");
        }
    }
}