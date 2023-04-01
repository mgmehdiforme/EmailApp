namespace EmailApp.ViewModels
{
    public class AttachmentViewModel
    {
        public string FileName { get; set; }
        public Stream? FileStream { get; set; }
        public string ContentType { get; set; }
    }
}
