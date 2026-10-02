using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PKValves.Models;
using PKValves.Services;

namespace PKValves.Tests
{
    public class FakeContactEmailService : IContactEmailService
    {
        public List<ContactViewModel> SentEnquiries { get; } = new();

        public bool ShouldFail { get; set; }

        public Task SendEnquiryAsync(ContactViewModel enquiry)
        {
            if (ShouldFail)
            {
                throw new InvalidOperationException(
                    "Simulated email service failure.");
            }

            SentEnquiries.Add(enquiry);

            return Task.CompletedTask;
        }
    }
}