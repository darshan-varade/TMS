using System;
namespace TMS.DataAccess.ViewModels
{
    public class ActivityViewModel
    {
        public int ActivityId { get; set; }
        public string ActivityTypeName { get; set; }
        public string Remarks { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }
    }
}
