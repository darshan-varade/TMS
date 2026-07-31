using System.Web.Mvc;
using TMS.DataAccess.DAL;

namespace TMS.WebApp.Controllers
{
    public class DashboardController : BaseController
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Dashboard";
            DashboardDAL dal = new DashboardDAL();
            string role = GetNormalizedRoleName();
            var vm = dal.GetDashboardData(CurrentUserId, role);
            vm.StatusChart = dal.GetStatusChartData(CurrentUserId, role);
            vm.PriorityChart = dal.GetPriorityChartData(CurrentUserId, role);
            if (role == "Admin")
            {
                vm.RecentTickets = dal.GetRecentTickets(CurrentUserId, role, 5);
            }
            return View(vm);
        }
    }
}
