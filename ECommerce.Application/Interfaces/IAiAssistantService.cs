using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Interfaces
{
    public interface IAiAssistantService
    {
        Task<string> GetResponseAsync(string userMessage);
    }
}
