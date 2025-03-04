using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Collections.Generic;

namespace ExportExcelWorker.Service
{
    public class EmailService
    {
        private readonly string _server;
        private readonly int _port;
        private readonly string _senderName;
        private readonly string _senderEmail;
        private readonly string _username;
        private readonly string _password;
        private readonly bool _useSSL;

        public EmailService(IConfiguration configuration)
        {
            var smtpSettings = configuration.GetSection("SmtpSettings");
            _server = smtpSettings["Server"];
            _port = int.Parse(smtpSettings["Port"]);
            _senderName = smtpSettings["SenderName"];
            _senderEmail = smtpSettings["SenderEmail"];
            _username = smtpSettings["Username"];
            _password = smtpSettings["Password"];
            _useSSL = bool.Parse(smtpSettings["UseSSL"]);
        }

        public void SendEmailWithAttachment(string filePath, List<string> recipientEmails)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_senderName, _senderEmail));

            foreach (var email in recipientEmails)
            {
                message.To.Add(new MailboxAddress("", email));
            }

            message.Subject = "Approved Customers - " + DateTime.Now.ToString("yyyy-MM-dd");

            var body = new TextPart("plain")
            {
                Text = "Attached is the exported Excel file containing today's approved customers."
            };

            var attachment = new MimePart("application", "vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                Content = new MimeContent(File.OpenRead(filePath)),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = Path.GetFileName(filePath)
            };

            var multipart = new Multipart("mixed");
            multipart.Add(body);
            multipart.Add(attachment);
            message.Body = multipart;

            using (var client = new SmtpClient())
            {
                client.Connect(_server, _port, _useSSL);
                client.Authenticate(_username, _password);
                client.Send(message);
                client.Disconnect(true);
            }
        }
    }
}
