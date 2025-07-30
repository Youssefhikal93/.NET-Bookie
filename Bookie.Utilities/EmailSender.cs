//using MimeKit;
//using MailKit.Net.Smtp;
//using MimeKit.Text;
//using Microsoft.AspNetCore.Identity.UI.Services;
//using Microsoft.Extensions.Configuration;
//using System.Net.Mail;
//using System.Net;


//namespace Bookie.Utilities
//{
//    public class EmailSender : IEmailSender
//    {
//        private readonly IConfiguration _configuration;

//        public EmailSender(IConfiguration configuration)
//        {
//            _configuration = configuration;
//        }



//        public Task SendEmailAsync(string email, string subject, string content)
//        {

//            string response = "";

//            var message = new MimeMessage();

//            message.Sender = MailboxAddress.Parse(_configuration["SenderEmail"]);

//            message.Sender.Name = _configuration["SenderName"];

//            message.To.Add(MailboxAddress.Parse(email));

//            message.From.Add(message.Sender);

//            message.Subject = subject;

//            //We will say we are sending HTML. But there are options for plaintext etc.

//            message.Body = new TextPart(TextFormat.Html) { Text = content };

//            //Be careful that the SmtpClient class is the one from Mailkit not the framework!

//            using (var emailClient = new SmtpClient())

//            {
//                try

//                {

//                    //The last parameter here is to use SSL (Which you should!)

//                    emailClient.Connect(_configuration["SmtpServer"], Convert.ToInt32(_configuration["SmtpPort"]), true);

//                }

//                catch (SmtpCommandException ex)

//                {

//                    response = "Error trying to connect:" + ex.Message + " StatusCode: " + ex.StatusCode;

//                    return Task.FromResult(response);

//                }

//                catch (SmtpProtocolException ex)

//                {

//                    response = "Protocol error while trying to connect:" + ex.Message;

//                    return Task.FromResult(response);

//                }

//                //Remove any OAuth functionality as we won't be using it.

//                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

//                emailClient.Authenticate(_configuration["SmtpUsername"], _configuration["SmtpPassword"]);


//                try

//                {
//                    emailClient.Send(message);

//                }

//                catch (SmtpCommandException ex)

//                {

//                    response = "Error sending message: " + ex.Message + " StatusCode: " + ex.StatusCode;


//                    switch (ex.ErrorCode)

//                    {

//                        case SmtpErrorCode.RecipientNotAccepted:

//                            response += " Recipient not accepted: " + ex.Mailbox;

//                            break;

//                        case SmtpErrorCode.SenderNotAccepted:

//                            response += " Sender not accepted: " + ex.Mailbox;

//                            Console.WriteLine("\tSender not accepted: {0}", ex.Mailbox);

//                            break;

//                        case SmtpErrorCode.MessageNotAccepted:

//                            response += " Message not accepted.";

//                            break;

//                    }

//                }

//                emailClient.Disconnect(true);

//            }

//            return Task.CompletedTask;

//        }
//    }
//}

using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;

public class EmailSender : IEmailSender
{
    private readonly string _fromEmail;
    private readonly string _appPassword;

    public EmailSender(IConfiguration config)
    {
        _fromEmail = config["Gmail:Email"];
        _appPassword = config["Gmail:AppPassword"];

    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        using var client = new SmtpClient("smtp.gmail.com", 587)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_fromEmail, _appPassword),
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_fromEmail),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };
        mailMessage.To.Add(email);

        await client.SendMailAsync(mailMessage);
    }
}