using System.Threading.Tasks;

namespace GrpcWheatherService.Email
{
    public interface IEmailService
    {
        Task<bool> SendMessage(MailModel model);
    }
}