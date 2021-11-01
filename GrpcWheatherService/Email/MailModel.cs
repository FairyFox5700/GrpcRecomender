using Microsoft.AspNetCore.Http;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcWheatherService.Email
{
    public class MailModel
    {
        [Required]
        public string ToEmail { get; set; }
        [Required]
        public string Subject { get; set; }
        [Required]
        public string Body { get; set; }
        public List<IFormFile> Attachments { get; set; }
    }
}
