using FluentEmail.Core;
using FluentEmail.Core.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcWheatherService.Email
{
    public class EmailService : IEmailService
    {
        private readonly IFluentEmail _email;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IFluentEmail email,
            ILogger<EmailService> logger)
        {
            _email = email;
            _logger = logger;
        }

        public async Task<bool> SendMessage(MailModel model)
        {
            try
            {
                var result = await _email
                    .To(model.ToEmail)
                    .Subject(model.Subject)
                    .Body(model.Body)
                    .Attach(model.Attachments.Select(file =>
                    {
                        return MapToAttachment(file);
                    }))
                    .SendAsync();
               
              
                if(!result.Successful)
                {
                    _logger.LogError("Failed to send an email.\n{Errors}",
                        string.Join(Environment.NewLine, result.ErrorMessages));
                }

                return result.Successful;
            }
            catch(Exception ex)
            {

                _logger.LogError($"{DateTime.Now}: Failed to send email notification ❌! ({ex.Message})");
                return false;
            }

        }

        private static Attachment MapToAttachment(IFormFile file)
        {
            if(file.Length > 0)
            {
                using(var ms = new MemoryStream())
                {
                    file.CopyTo(ms);

                    return new Attachment
                    {
                        Filename = file.FileName,
                        Data = ms,
                        ContentType = file.ContentType,
                    };
                }
            }
            return new Attachment();
        }
    }
}
