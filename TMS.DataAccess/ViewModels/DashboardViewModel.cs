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

        public List<ChartDataPoint> StatusChart { get; set; }
        public List<ChartDataPoint> PriorityChart { get; set; }
        public List<TicketRowViewModel> RecentTickets { get; set; }
    }

    public class ChartDataPoint
    {
        public string Label { get; set; }
        public int Value { get; set; }
    }
}
