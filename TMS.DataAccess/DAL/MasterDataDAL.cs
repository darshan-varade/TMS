using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using TMS.DataAccess.ViewModels;
using Serilog;

namespace TMS.DataAccess.DAL
{
    public class MasterDataDAL
    {
        private Database db;

        public MasterDataDAL()
        {
            this.db = DatabaseFactory.CreateDatabase();
        }

        public List<DropdownViewModel> GetDepartments()
        {
            List<DropdownViewModel> list = new List<DropdownViewModel>();
            DbCommand cmd = db.GetStoredProcCommand("tmsDepartmentGetAll");
            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        list.Add(new DropdownViewModel
                        {
                            Id = Convert.ToInt32(reader["departmentId"]),
                            Name = reader["departmentName"].ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetDepartments");
                throw;
            }
            return list;
        }

        public List<DropdownViewModel> GetRoles()
        {
            List<DropdownViewModel> list = new List<DropdownViewModel>();
            DbCommand cmd = db.GetStoredProcCommand("tmsRoleGetAll");
            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        list.Add(new DropdownViewModel
                        {
                            Id = Convert.ToInt32(reader["roleId"]),
                            Name = reader["roleName"].ToString(),
                            AdditionalInfo = reader["roleDescription"] != DBNull.Value ? reader["roleDescription"].ToString() : null
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetRoles");
                throw;
            }
            return list;
        }

        public List<DropdownViewModel> GetCategories()
        {
            List<DropdownViewModel> list = new List<DropdownViewModel>();
            DbCommand cmd = db.GetStoredProcCommand("tmsCategoryGetAll");
            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        list.Add(new DropdownViewModel
                        {
                            Id = Convert.ToInt32(reader["categoryId"]),
                            Name = reader["categoryName"].ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetCategories");
                throw;
            }
            return list;
        }

        public List<DropdownViewModel> GetPriorities()
        {
            List<DropdownViewModel> list = new List<DropdownViewModel>();
            DbCommand cmd = db.GetStoredProcCommand("tmsPriorityGetAll");
            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        list.Add(new DropdownViewModel
                        {
                            Id = Convert.ToInt32(reader["priorityId"]),
                            Name = reader["priorityName"].ToString(),
                            AdditionalInfo = reader["slaHours"] != DBNull.Value ? reader["slaHours"].ToString() + "h" : null
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetPriorities");
                throw;
            }
            return list;
        }

        public List<DropdownViewModel> GetStatuses()
        {
            List<DropdownViewModel> list = new List<DropdownViewModel>();
            DbCommand cmd = db.GetStoredProcCommand("tmsStatusGetAll");
            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        list.Add(new DropdownViewModel
                        {
                            Id = Convert.ToInt32(reader["statusId"]),
                            Name = reader["statusName"].ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetStatuses");
                throw;
            }
            return list;
        }
    }
}
