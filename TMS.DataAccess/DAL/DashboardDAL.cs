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

        public void PopulateJSMEnhancements(DashboardViewModel vm, int userId, string roleName)
        {
            string roleFilter = "";
            string catRoleFilter = "";
            string activityRoleFilter = "";
            if (roleName == "Support" || roleName == "Support Executive")
            {
                roleFilter = " AND assignedToUserId = " + userId;
                catRoleFilter = " AND t.assignedToUserId = " + userId;
                activityRoleFilter = " AND (t.assignedToUserId = " + userId + " OR t.CreatedBy = " + userId + ")";
            }
            else if (roleName == "Employee")
            {
                roleFilter = " AND CreatedBy = " + userId;
                catRoleFilter = " AND t.CreatedBy = " + userId;
                activityRoleFilter = " AND t.CreatedBy = " + userId;
            }

            try
            {
                // 1. Stat cards
                string statQuery = string.Format(@"
                    SELECT 
                        (SELECT COUNT(*) FROM tmsTicket WHERE statusId IN (4, 6) AND IsActive = 1 AND ModifiedOn >= DATEADD(day, -7, GETDATE()){0}) as CompletedCount,
                        (SELECT COUNT(*) FROM tmsTicket WHERE IsActive = 1 AND ModifiedOn >= DATEADD(day, -7, GETDATE()){0}) as UpdatedCount,
                        (SELECT COUNT(*) FROM tmsTicket WHERE IsActive = 1 AND CreatedOn >= DATEADD(day, -7, GETDATE()){0}) as CreatedCount,
                        (SELECT COUNT(*) FROM tmsTicket WHERE IsActive = 1 AND statusId NOT IN (4, 6) AND dueDate >= GETDATE() AND dueDate <= DATEADD(day, 7, GETDATE()){0}) as DueSoonCount
                ", roleFilter);

                DbCommand cmdStats = db.GetSqlStringCommand(statQuery);
                using (IDataReader reader = db.ExecuteReader(cmdStats))
                {
                    if (reader.Read())
                    {
                        vm.CompletedLast7Days = Convert.ToInt32(reader["CompletedCount"]);
                        vm.UpdatedLast7Days = Convert.ToInt32(reader["UpdatedCount"]);
                        vm.CreatedLast7Days = Convert.ToInt32(reader["CreatedCount"]);
                        vm.DueSoonNext7Days = Convert.ToInt32(reader["DueSoonCount"]);
                    }
                }

                // 2. Category Distribution
                vm.CategoryDistribution = new List<CategoryCountViewModel>();
                string catQuery = string.Format(@"
                    SELECT c.categoryName, COUNT(t.ticketId) as ticketCount 
                    FROM tmsCategory c 
                    LEFT JOIN tmsTicket t ON t.categoryId = c.categoryId AND t.statusId NOT IN (4, 6) AND t.IsActive = 1{0}
                    WHERE c.IsActive = 1 
                    GROUP BY c.categoryName
                ", catRoleFilter);

                DbCommand cmdCat = db.GetSqlStringCommand(catQuery);
                int totalOpenCatTickets = 0;
                using (IDataReader reader = db.ExecuteReader(cmdCat))
                {
                    while (reader.Read())
                    {
                        var count = Convert.ToInt32(reader["ticketCount"]);
                        totalOpenCatTickets += count;
                        vm.CategoryDistribution.Add(new CategoryCountViewModel
                        {
                            CategoryName = reader["categoryName"].ToString(),
                            TicketCount = count
                        });
                    }
                }
                // Calculate category percentages
                if (totalOpenCatTickets > 0)
                {
                    foreach (var c in vm.CategoryDistribution)
                    {
                        c.Percentage = Math.Round(((double)c.TicketCount / totalOpenCatTickets) * 100, 1);
                    }
                }

                // 3. Team Workload (Admins only)
                vm.TeamWorkloads = new List<TeamWorkloadViewModel>();
                if (roleName == "Admin" || roleName == "Administrator")
                {
                    string workloadQuery = @"
                        SELECT 
                            u.userId,
                            u.fullName,
                            COUNT(t.ticketId) as ticketCount
                        FROM tmsUser u
                        INNER JOIN tmsCredential c ON u.userId = c.userId
                        LEFT JOIN tmsTicket t ON t.assignedToUserId = u.userId AND t.statusId NOT IN (4, 6) AND t.IsActive = 1
                        WHERE c.roleId = 2 AND u.IsActive = 1
                        GROUP BY u.userId, u.fullName
                        UNION ALL
                        SELECT 
                            NULL as userId,
                            'Unassigned' as fullName,
                            COUNT(ticketId) as ticketCount
                        FROM tmsTicket
                        WHERE assignedToUserId IS NULL AND statusId NOT IN (4, 6) AND IsActive = 1
                    ";

                    DbCommand cmdWorkload = db.GetSqlStringCommand(workloadQuery);
                    int totalWorkloadTickets = 0;
                    using (IDataReader reader = db.ExecuteReader(cmdWorkload))
                    {
                        while (reader.Read())
                        {
                            var count = Convert.ToInt32(reader["ticketCount"]);
                            totalWorkloadTickets += count;
                            vm.TeamWorkloads.Add(new TeamWorkloadViewModel
                            {
                                UserId = reader["userId"] != DBNull.Value ? Convert.ToInt32(reader["userId"]) : (int?)null,
                                FullName = reader["fullName"].ToString(),
                                TicketCount = count
                            });
                        }
                    }
                    if (totalWorkloadTickets > 0)
                    {
                        foreach (var w in vm.TeamWorkloads)
                        {
                            w.Percentage = Math.Round(((double)w.TicketCount / totalWorkloadTickets) * 100, 1);
                        }
                    }
                }

                // 4. Recent Activity Feed
                vm.RecentActivities = new List<ActivityFeedViewModel>();
                string activityQuery = string.Format(@"
                    SELECT TOP 5 
                        a.activityId,
                        a.ticketId,
                        t.ticketNumber,
                        t.title as ticketTitle,
                        a.activityTypeId,
                        at.activityTypeName,
                        a.remarks,
                        a.oldValue,
                        a.newValue,
                        a.CreatedOn,
                        u.fullName as createdByName
                    FROM tmsTicketActivity a
                    INNER JOIN tmsTicket t ON a.ticketId = t.ticketId
                    INNER JOIN tmsActivityType at ON a.activityTypeId = at.activityTypeId
                    INNER JOIN tmsUser u ON a.CreatedBy = u.userId
                    WHERE a.IsActive = 1 AND t.IsActive = 1{0}
                    ORDER BY a.CreatedOn DESC, a.activityId DESC
                ", activityRoleFilter);

                DbCommand cmdActivity = db.GetSqlStringCommand(activityQuery);
                using (IDataReader reader = db.ExecuteReader(cmdActivity))
                {
                    while (reader.Read())
                    {
                        vm.RecentActivities.Add(new ActivityFeedViewModel
                        {
                            ActivityId = Convert.ToInt32(reader["activityId"]),
                            TicketId = Convert.ToInt32(reader["ticketId"]),
                            TicketNumber = reader["ticketNumber"].ToString(),
                            TicketTitle = reader["ticketTitle"].ToString(),
                            ActivityTypeId = Convert.ToInt32(reader["activityTypeId"]),
                            ActivityTypeName = reader["activityTypeName"].ToString(),
                            Remarks = reader["remarks"] != DBNull.Value ? reader["remarks"].ToString() : "",
                            OldValue = reader["oldValue"] != DBNull.Value ? reader["oldValue"].ToString() : "",
                            NewValue = reader["newValue"] != DBNull.Value ? reader["newValue"].ToString() : "",
                            CreatedByName = reader["createdByName"].ToString(),
                            CreatedOn = Convert.ToDateTime(reader["CreatedOn"])
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in PopulateJSMEnhancements");
                throw;
            }
        }
    }
}
