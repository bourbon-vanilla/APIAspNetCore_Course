using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.API.Services
{
    public interface IMailService
    {
        void Send(string subject, string message);
    }

    internal class LocalMailService : IMailService
    {
        private readonly IConfiguration _configuration;


        public LocalMailService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }


        public void Send(string subject, string message)
        {
            Debug.WriteLine($"Sending email from {_configuration["MailSettings:FromAddress"]} to {_configuration["MailSettings:ToAddress"]} with local Mail-Service:");
            Debug.WriteLine($"Subject: {subject}");
            Debug.WriteLine($"Message: {message}");
        }
    }
}
