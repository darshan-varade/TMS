using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using TMS.DataAccess.ViewModels;
using Serilog;

namespace TMS.DataAccess.DAL
{
    public class DashboardDAL
    {
        private Database db;

        public DashboardDAL()
        {
            this.db = DatabaseFactory.CreateDatabase();
        }

        public DashboardViewModel GetDashboardData(int userId, string roleName)
        {
            DashboardViewModel vm = new DashboardViewModel();
            DbCommand cmd = db.GetStoredProcCommand("tmsDashboardGetData");
            db.AddInParameter(cmd, "@UserId", DbType.Int32, userId);
            db.AddInParameter(cmd, "@RoleName", DbType.String, roleName);
            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read())
                    {
                        vm.TotalTickets = Convert.ToInt32(reader["TotalTickets"]);
                        vm.OpenTickets = Convert.ToInt32(reader["OpenTickets"]);
                        vm.InProgressTickets = Convert.ToInt32(reader["InProgressTickets"]);
                        vm.ResolvedTickets = Convert.ToInt32(reader["ResolvedTickets"]);
                        vm.ClosedTickets = Convert.ToInt32(reader["ClosedTickets"]);
                        vm.MyAssignedTickets = Convert.ToInt32(reader["MyAssignedTickets"]);
                        vm.MyCreatedTickets = Convert.ToInt32(reader["MyCreatedTickets"]);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetDashboardData");
                throw;
            }
            return vm;
        }

        public List<ChartDataPoint> GetStatusChartData(int userId, string roleName)
        {
            List<ChartDataPoint> list = new List<ChartDataPoint>();
            DbCommand cmd = db.GetStoredProcCommand("tmsDashboardGetStatusChart");
            db.AddInParameter(cmd, "@UserId", DbType.Int32, userId);
            db.AddInParameter(cmd, "@RoleName", DbType.String, roleName);
            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        list.Add(new ChartDataPoint
                        {
                            Label = reader["Label"].ToString(),
                            Value = Convert.ToInt32(reader["Value"])
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetStatusChartData");
                throw;
            }
            return list;
        }

        public List<ChartDataPoint> GetPriorityChartData(int userId, string roleName)
        {
            List<ChartDataPoint> list = new List<ChartDataPoint>();
            DbCommand cmd = db.GetStoredProcCommand("tmsDashboardGetPriorityChart");
            db.AddInParameter(cmd, "@UserId", DbType.Int32, userId);
            db.AddInParameter(cmd, "@RoleName", DbType.String, roleName);
            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        list.Add(new ChartDataPoint
                        {
                            Label = reader["Label"].ToString(),
                            Value = Convert.ToInt32(reader["Value"])
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetPriorityChartData");
                throw;
            }
            return list;
        }

        public List<TicketRowViewModel> GetRecentTickets(int userId, string roleName, int count = 5)
        {
            List<TicketRowViewModel> list = new List<TicketRowViewModel>();
            DbCommand cmd = db.GetStoredProcCommand("tmsDashboardGetRecentTickets");
            db.AddInParameter(cmd, "@UserId", DbType.Int32, userId);
            db.AddInParameter(cmd, "@RoleName", DbType.String, roleName);
            db.AddInParameter(cmd, "@Count", DbType.Int32, count);
            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        list.Add(new TicketRowViewModel
                        {
                            TicketId = Convert.ToInt32(reader["ticketId"]),
                            TicketNumber = reader["ticketNumber"].ToString(),
                            Title = reader["title"].ToString(),
                            StatusName = reader["statusName"].ToString(),
                            PriorityName = reader["priorityName"].ToString(),
                            CreatedByName = reader["createdByName"].ToString(),
                            AssignedToName = reader["assignedToName"] != DBNull.Value ? reader["assignedToName"].ToString() : null,
                            CreatedOn = Convert.ToDateTime(reader["CreatedOn"])
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetRecentTickets");
                throw;
            }
            return list;
        }
    }
}
