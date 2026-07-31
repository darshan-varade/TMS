using System;
namespace TMS.DataAccess.ViewModels
{
    public class AttachmentViewModel
    {
        public int AttachmentId { get; set; }
        public string StoredFileName { get; set; }
        public string OriginalFileName { get; set; }
        public string FileExtension { get; set; }
        public string ContentType { get; set; }
        public int FileSize { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }
    }
}
