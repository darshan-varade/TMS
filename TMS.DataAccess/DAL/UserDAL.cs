using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using TMS.DataAccess.Models;
using TMS.DataAccess.ViewModels;
using Serilog;

namespace TMS.DataAccess.DAL
{
    public class UserDAL
    {
        private Database db;

        public UserDAL()
        {
            this.db = DatabaseFactory.CreateDatabase();
        }

        public UserModel GetUserById(int userId)
        {
            UserModel model = null;
            DbCommand cmd = db.GetStoredProcCommand("tmsUserGetById");
            db.AddInParameter(cmd, "@UserId", DbType.Int32, userId);
            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read())
                    {
                        model = new UserModel
                        {
                            UserId = Convert.ToInt32(reader["userId"]),
                            FullName = reader["fullName"].ToString(),
                            MobileNumber = reader["mobileNumber"] != DBNull.Value ? reader["mobileNumber"].ToString() : null,
                            DepartmentId = Convert.ToInt32(reader["departmentId"]),
                            DepartmentName = reader["departmentName"].ToString(),
                            CredentialId = Convert.ToInt32(reader["credentialId"]),
                            EmailId = reader["emailId"].ToString(),
                            PasswordHash = reader["passwordHash"].ToString(),
                            RoleId = Convert.ToInt32(reader["roleId"]),
                            RoleName = reader["roleName"].ToString(),
                            IsActive = Convert.ToBoolean(reader["IsActive"]),
                            IsApproved = reader["isApproved"] != DBNull.Value ? Convert.ToByte(reader["isApproved"]) : (byte?)null,
                            CreatedOn = Convert.ToDateTime(reader["CreatedOn"])
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetUserById");
                throw;
            }
            return model;
        }

        public List<UserRowViewModel> GetUserList(string searchTerm, int? roleId, string sortColumn, string sortDirection, int pageNumber, int pageSize, out int totalRows)
        {
            List<UserRowViewModel> list = new List<UserRowViewModel>();
            DbCommand cmd = db.GetStoredProcCommand("tmsUserGetList");
            db.AddInParameter(cmd, "@SearchTerm", DbType.String, searchTerm ?? (object)DBNull.Value);
            db.AddInParameter(cmd, "@RoleId", DbType.Int32, roleId ?? (object)DBNull.Value);
            db.AddInParameter(cmd, "@SortColumn", DbType.String, sortColumn);
            db.AddInParameter(cmd, "@SortDirection", DbType.String, sortDirection);
            db.AddInParameter(cmd, "@PageNumber", DbType.Int32, pageNumber);
            db.AddInParameter(cmd, "@PageSize", DbType.Int32, pageSize);
            db.AddOutParameter(cmd, "@TotalRows", DbType.Int32, 0);
            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        list.Add(new UserRowViewModel
                        {
                            UserId = Convert.ToInt32(reader["userId"]),
                            FullName = reader["fullName"].ToString(),
                            EmailId = reader["emailId"].ToString(),
                            MobileNumber = reader["mobileNumber"] != DBNull.Value ? reader["mobileNumber"].ToString() : null,
                            RoleId = Convert.ToInt32(reader["roleId"]),
                            RoleName = reader["roleName"].ToString(),
                            DepartmentName = reader["departmentName"].ToString(),
                            IsActive = Convert.ToBoolean(reader["IsActive"]),
                            IsApproved = reader["isApproved"] != DBNull.Value ? Convert.ToByte(reader["isApproved"]) : (byte?)null,
                            CreatedOn = Convert.ToDateTime(reader["CreatedOn"]),
                            TotalTickets = Convert.ToInt32(reader["TotalTickets"])
                        });
                    }
                }
                totalRows = Convert.ToInt32(db.GetParameterValue(cmd, "@TotalRows"));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetUserList");
                throw;
            }
            return list;
        }

        public int AddUser(string fullName, string mobileNumber, string email, string passwordHash, int roleId, int departmentId, int createdBy)
        {
            DbCommand cmd = db.GetStoredProcCommand("tmsUserAdd");
            db.AddInParameter(cmd, "@FullName", DbType.String, fullName);
            db.AddInParameter(cmd, "@MobileNumber", DbType.String, mobileNumber);
            db.AddInParameter(cmd, "@Email", DbType.String, email);
            db.AddInParameter(cmd, "@PasswordHash", DbType.String, passwordHash);
            db.AddInParameter(cmd, "@RoleId", DbType.Int32, roleId);
            db.AddInParameter(cmd, "@DepartmentId", DbType.Int32, departmentId);
            db.AddInParameter(cmd, "@CreatedBy", DbType.Int32, createdBy);
            db.AddOutParameter(cmd, "@UserId", DbType.Int32, 0);
            try
            {
                db.ExecuteNonQuery(cmd);
                return Convert.ToInt32(db.GetParameterValue(cmd, "@UserId"));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in AddUser");
                throw;
            }
        }

        public void UpdateUser(int userId, string fullName, string mobileNumber, int roleId, int departmentId, bool isActive, int modifiedBy)
        {
            DbCommand cmd = db.GetStoredProcCommand("tmsUserUpdate");
            db.AddInParameter(cmd, "@UserId", DbType.Int32, userId);
            db.AddInParameter(cmd, "@FullName", DbType.String, fullName);
            db.AddInParameter(cmd, "@MobileNumber", DbType.String, mobileNumber);
            db.AddInParameter(cmd, "@RoleId", DbType.Int32, roleId);
            db.AddInParameter(cmd, "@DepartmentId", DbType.Int32, departmentId);
            db.AddInParameter(cmd, "@IsActive", DbType.Boolean, isActive);
            db.AddInParameter(cmd, "@ModifiedBy", DbType.Int32, modifiedBy);
            try
            {
                db.ExecuteNonQuery(cmd);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in UpdateUser");
                throw;
            }
        }

        public void ChangeUserRole(int userId, int roleId, int modifiedBy)
        {
            DbCommand cmd = db.GetStoredProcCommand("tmsUserChangeRole");
            db.AddInParameter(cmd, "@UserId", DbType.Int32, userId);
            db.AddInParameter(cmd, "@RoleId", DbType.Int32, roleId);
            db.AddInParameter(cmd, "@ModifiedBy", DbType.Int32, modifiedBy);
            try
            {
                db.ExecuteNonQuery(cmd);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in ChangeUserRole");
                throw;
            }
        }

        public void SetUserApproval(int userId, byte? isApproved, int modifiedBy)
        {
            DbCommand cmd = db.GetStoredProcCommand("tmsUserSetApproval");
            db.AddInParameter(cmd, "@UserId", DbType.Int32, userId);
            db.AddInParameter(cmd, "@IsApproved", DbType.Byte, isApproved ?? (object)DBNull.Value);
            db.AddInParameter(cmd, "@ModifiedBy", DbType.Int32, modifiedBy);
            try
            {
                db.ExecuteNonQuery(cmd);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in SetUserApproval");
                throw;
            }
        }

        public void DeleteUser(int userId, int modifiedBy)
        {
            DbCommand cmd = db.GetStoredProcCommand("tmsUserDelete");
            db.AddInParameter(cmd, "@UserId", DbType.Int32, userId);
            db.AddInParameter(cmd, "@ModifiedBy", DbType.Int32, modifiedBy);
            try
            {
                db.ExecuteNonQuery(cmd);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in DeleteUser");
                throw;
            }
        }

        public void ToggleUserStatus(int userId, bool isActive, int modifiedBy)
        {
            DbCommand cmd = db.GetStoredProcCommand("tmsUserToggleStatus");
            db.AddInParameter(cmd, "@UserId", DbType.Int32, userId);
            db.AddInParameter(cmd, "@IsActive", DbType.Boolean, isActive);
            db.AddInParameter(cmd, "@ModifiedBy", DbType.Int32, modifiedBy);
            try
            {
                db.ExecuteNonQuery(cmd);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in ToggleUserStatus");
                throw;
            }
        }

        public void UpdateProfile(int userId, string fullName, string mobileNumber)
        {
            DbCommand cmd = db.GetStoredProcCommand("tmsUserUpdateProfile");
            db.AddInParameter(cmd, "@UserId", DbType.Int32, userId);
            db.AddInParameter(cmd, "@FullName", DbType.String, fullName);
            db.AddInParameter(cmd, "@MobileNumber", DbType.String, mobileNumber);
            try
            {
                db.ExecuteNonQuery(cmd);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in UpdateProfile");
                throw;
            }
        }

        public void ChangePassword(int credentialId, string passwordHash)
        {
            DbCommand cmd = db.GetStoredProcCommand("tmsUserChangePassword");
            db.AddInParameter(cmd, "@CredentialId", DbType.Int32, credentialId);
            db.AddInParameter(cmd, "@PasswordHash", DbType.String, passwordHash);
            try
            {
                db.ExecuteNonQuery(cmd);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in ChangePassword");
                throw;
            }
        }

        public List<DropdownViewModel> GetSupportUsers()
        {
            List<DropdownViewModel> list = new List<DropdownViewModel>();
            DbCommand cmd = db.GetStoredProcCommand("tmsUserGetSupportList");
            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        list.Add(new DropdownViewModel
                        {
                            Id = Convert.ToInt32(reader["userId"]),
                            Name = reader["fullName"].ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetSupportUsers");
                throw;
            }
            return list;
        }
    }
}
