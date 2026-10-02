using PKValves.Models;

namespace PKValves.Services
{
    public interface IContactEmailService
    {
        Task SendEnquiryAsync(ContactViewModel enquiry);
    }
}