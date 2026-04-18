using System;
using System.IO;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using QRCoder;
using Microsoft.Extensions.Configuration;

namespace CinemaManagement.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendTicketEmailAsync(
            string toEmail,
            string subject,
            string htmlContent,
            string qrContent)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Cinema", _config["Gmail:Username"]));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            // 1. TẠO QR IMAGE (BYTE[])
            var qrBytes = GenerateQRCodeBytes(qrContent);

            var image = new MimePart("image", "png")
            {
                Content = new MimeContent(new MemoryStream(qrBytes)),
                ContentId = "qrcode",
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = "ticket-qr.png"
            };

            // 2. HTML EMAIL (CID)
            var html = new TextPart("html")
            {
                Text = htmlContent.Replace(
                    "{{QR_IMAGE}}",
                    "<img src=\"cid:qrcode\" style=\"width:200px\" />"
                )
            };

            // 3. BODY COMPOSITE
            var body = new Multipart("related");
            body.Add(html);
            body.Add(image);

            message.Body = body;

            using var client = new SmtpClient();
            await client.ConnectAsync(
                _config["Gmail:SmtpServer"],
                int.Parse(_config["Gmail:Port"]),
                SecureSocketOptions.StartTls
            );

            await client.AuthenticateAsync(
                _config["Gmail:Username"],
                _config["Gmail:Password"]
            );

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        private byte[] GenerateQRCodeBytes(string content)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCode(qrData);
            using var bitmap = qrCode.GetGraphic(20);
            using var ms = new MemoryStream();

            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }
    }
}
