﻿using System.Threading.Tasks;

namespace HomeWorld.Tracker.Web.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
