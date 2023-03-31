using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EmailApp.ViewModels;

namespace EmailApp.Data
{
    public class EmailAppContext : DbContext
    {
        public EmailAppContext (DbContextOptions<EmailAppContext> options)
            : base(options)
        {
        }

        public DbSet<EmailApp.DataModels.EmailConfig> EmailConfig { get; set; } = default!;
        public DbSet<EmailApp.DataModels.EmailMessage> EmailMessage { get; set; } = default!;
    }
}
