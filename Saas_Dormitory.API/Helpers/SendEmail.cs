using System.Net;
using System.Net.Mail;

namespace Saas_Dormitory.API.Helpers
{
    public class SendEmail
    {
        public async Task<bool> SendHtmlEmail(string toEmail, string subject, string htmlBody)
        {
            try
            {
                SmtpClient client = new SmtpClient("smtp.hostinger.com", 587)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("smtp@designblocks.in", "SMTP@designblocks123")
                };

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("smtp@designblocks.in");
                mail.To.Add(toEmail);
                mail.Subject = subject;
                mail.Body = htmlBody;
                mail.IsBodyHtml = true;

                await client.SendMailAsync(mail);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Email Error: " + ex.Message);
                return false;
            }
        }

    }
}
