using System.Collections.Generic;
namespace TMS.DataAccess.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalTickets { get; set; }
        public int OpenTickets { get; set; }
        public int InProgressTickets { get; set; }
        public int ResolvedTickets { get; set; }
        public int ClosedTickets { get; set; }
        public int MyAssignedTickets { get; set; }
        public int MyCreatedTickets { get; set; }

        // JSM Stat Cards
        public int CompletedLast7Days { get; set; }
        public int UpdatedLast7Days { get; set; }
        public int CreatedLast7Days { get; set; }
        public int DueSoonNext7Days { get; set; }

        public List<ChartDataPoint> StatusChart { get; set; }
        public List<ChartDataPoint> PriorityChart { get; set; }
        public List<TicketRowViewModel> RecentTickets { get; set; }
        public List<CategoryCountViewModel> CategoryDistribution { get; set; }
        public List<TeamWorkloadViewModel> TeamWorkloads { get; set; }
        public List<ActivityFeedViewModel> RecentActivities { get; set; }
    }

    public class ChartDataPoint
    {
        public string Label { get; set; }
        public int Value { get; set; }
    }

    public class CategoryCountViewModel
    {
        public string CategoryName { get; set; }
        public int TicketCount { get; set; }
        public double Percentage { get; set; }
    }

    public class TeamWorkloadViewModel
    {
        public int? UserId { get; set; }
        public string FullName { get; set; }
        public int TicketCount { get; set; }
        public double Percentage { get; set; }
    }

    public class ActivityFeedViewModel
    {
        public int ActivityId { get; set; }
        public int TicketId { get; set; }
        public string TicketNumber { get; set; }
        public string TicketTitle { get; set; }
        public int ActivityTypeId { get; set; }
        public string ActivityTypeName { get; set; }
        public string Remarks { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string CreatedByName { get; set; }
        public System.DateTime CreatedOn { get; set; }
    }
}
