using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EmailApp.Models;

namespace EmailApp.Data
{
    public class EmailAppContext : DbContext
    {
        public EmailAppContext (DbContextOptions<EmailAppContext> options)
            : base(options)
        {
        }

        public DbSet<EmailApp.Models.EmailConfig> EmailConfig { get; set; } = default!;
    }
}
