using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcWheatherService.Email
{
    public class EmailSettings
    {
        public int MailPort { get; set; }
        public string MailServer { get; set; }
        public string AdminEmail { get; set; }
        public string AdminPassword { get; set; }
        public bool UseSsl { get; set; }
    }
}
