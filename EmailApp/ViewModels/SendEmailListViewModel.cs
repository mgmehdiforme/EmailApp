using EmailApp.DataModels;
using System.ComponentModel.DataAnnotations;

namespace EmailApp.ViewModels
{
    public class SendEmailListViewModel
    {
        public int ConfigId { get; set; }
        public List<SendEmail> Emails { get; set; }
        public int CurrentPageNumber { get; set; }
        public int TotalPages { get; set; }

    }
}
