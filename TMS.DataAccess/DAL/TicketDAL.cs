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
    public class TicketDAL
    {
        private Database db;

        public TicketDAL()
        {
            this.db = DatabaseFactory.CreateDatabase();
        }

        public int CreateTicket(TicketAddViewModel vm, int createdBy)
        {
            DbCommand cmd = db.GetStoredProcCommand("tmsTicketCreate");
            db.AddInParameter(cmd, "@CreatedBy", DbType.Int32, createdBy);
            db.AddInParameter(cmd, "@Title", DbType.String, vm.Title);
            db.AddInParameter(cmd, "@Description", DbType.String, vm.Description);
            db.AddInParameter(cmd, "@CategoryId", DbType.Int32, vm.CategoryId);
            db.AddInParameter(cmd, "@PriorityId", DbType.Int32, vm.PriorityId);
            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read())
                    {
                        return Convert.ToInt32(reader["TicketId"]);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in CreateTicket");
                throw;
            }
            return 0;
        }

        public List<TicketRowViewModel> GetTicketList(TicketListViewModel vm, int userId, string roleName)
        {
            vm.PageNumber = vm.PageNumber <= 0 ? 1 : vm.PageNumber;
            vm.PageSize = vm.PageSize <= 0 ? 10 : vm.PageSize;
            if (string.IsNullOrEmpty(vm.SortColumn)) vm.SortColumn = "CreatedOn";
            if (string.IsNullOrEmpty(vm.SortDirection)) vm.SortDirection = "DESC";
            int? assignedToUserId = vm.UnassignedOnly ? -1 : vm.AssignedToUserId;

            List<TicketRowViewModel> list = new List<TicketRowViewModel>();
            DbCommand cmd = db.GetStoredProcCommand("tmsTicketGetList");
            db.AddInParameter(cmd, "@SearchTerm", DbType.String, vm.SearchTerm ?? (object)DBNull.Value);
            db.AddInParameter(cmd, "@StatusId", DbType.Int32, vm.StatusId ?? (object)DBNull.Value);
            db.AddInParameter(cmd, "@PriorityId", DbType.Int32, vm.PriorityId ?? (object)DBNull.Value);
            db.AddInParameter(cmd, "@CategoryId", DbType.Int32, vm.CategoryId ?? (object)DBNull.Value);
            db.AddInParameter(cmd, "@AssignedToUserId", DbType.Int32, assignedToUserId ?? (object)DBNull.Value);
            db.AddInParameter(cmd, "@DateFrom", DbType.DateTime, vm.DateFrom ?? (object)DBNull.Value);
            db.AddInParameter(cmd, "@DateTo", DbType.DateTime, vm.DateTo ?? (object)DBNull.Value);
            db.AddInParameter(cmd, "@UserId", DbType.Int32, userId);
            db.AddInParameter(cmd, "@UserRole", DbType.String, roleName);
            db.AddInParameter(cmd, "@SortColumn", DbType.String, vm.SortColumn);
            db.AddInParameter(cmd, "@SortDirection", DbType.String, vm.SortDirection);
            db.AddInParameter(cmd, "@PageNumber", DbType.Int32, vm.PageNumber);
            db.AddInParameter(cmd, "@PageSize", DbType.Int32, vm.PageSize);
            db.AddOutParameter(cmd, "@TotalRows", DbType.Int32, 0);
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
                            CategoryName = reader["categoryName"].ToString(),
                            PriorityName = reader["priorityName"].ToString(),
                            StatusName = reader["statusName"].ToString(),
                            CreatedByName = reader["createdByName"].ToString(),
                            CreatedBy = Convert.ToInt32(reader["createdByUserId"]),
                            AssignedToUserId = reader["assignedToUserId"] != DBNull.Value ? Convert.ToInt32(reader["assignedToUserId"]) : (int?)null,
                            AssignedToName = reader["assignedToName"] != DBNull.Value ? reader["assignedToName"].ToString() : null,
                            CreatedOn = Convert.ToDateTime(reader["CreatedOn"]),
                            ConversationCount = Convert.ToInt32(reader["ConversationCount"])
                        });
                    }
                }
                vm.TotalRows = Convert.ToInt32(db.GetParameterValue(cmd, "@TotalRows"));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetTicketList");
                throw;
            }
            return list;
        }

        public TicketModel GetTicketById(int ticketId)
        {
            TicketModel model = null;
            DbCommand cmd = db.GetStoredProcCommand("tmsTicketGetById");
            db.AddInParameter(cmd, "@TicketId", DbType.Int32, ticketId);
            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read())
                    {
                        model = new TicketModel
                        {
                            TicketId = Convert.ToInt32(reader["ticketId"]),
                            TicketNumber = reader["ticketNumber"].ToString(),
                            Title = reader["title"].ToString(),
                            Description = reader["description"].ToString(),
                            CategoryId = Convert.ToInt32(reader["categoryId"]),
                            CategoryName = reader["categoryName"].ToString(),
                            PriorityId = Convert.ToInt32(reader["priorityId"]),
                            PriorityName = reader["priorityName"].ToString(),
                            StatusId = Convert.ToInt32(reader["statusId"]),
                            StatusName = reader["statusName"].ToString(),
                            AssignedToUserId = reader["assignedToUserId"] != DBNull.Value ? Convert.ToInt32(reader["assignedToUserId"]) : (int?)null,
                            AssignedToName = reader["assignedToName"] != DBNull.Value ? reader["assignedToName"].ToString() : null,
                            DueDate = Convert.ToDateTime(reader["dueDate"]),
                            ResolvedOn = reader["resolvedOn"] != DBNull.Value ? Convert.ToDateTime(reader["resolvedOn"]) : (DateTime?)null,
                            CreatedOn = Convert.ToDateTime(reader["CreatedOn"]),
                            CreatedByName = reader["createdByName"].ToString(),
                            CreatedBy = Convert.ToInt32(reader["createdByUserId"])
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetTicketById");
                throw;
            }
            return model;
        }

        public void UpdateTicket(TicketEditViewModel vm, int modifiedBy)
        {
            DbCommand cmd = db.GetStoredProcCommand("tmsTicketUpdate");
            db.AddInParameter(cmd, "@TicketId", DbType.Int32, vm.TicketId);
            db.AddInParameter(cmd, "@Title", DbType.String, vm.Title);
            db.AddInParameter(cmd, "@Description", DbType.String, vm.Description);
            db.AddInParameter(cmd, "@CategoryId", DbType.Int32, vm.CategoryId);
            db.AddInParameter(cmd, "@PriorityId", DbType.Int32, vm.PriorityId);
            db.AddInParameter(cmd, "@StatusId", DbType.Int32, vm.StatusId);
            db.AddInParameter(cmd, "@AssignedToUserId", DbType.Int32, vm.AssignedToUserId ?? (object)DBNull.Value);
            db.AddInParameter(cmd, "@ModifiedBy", DbType.Int32, modifiedBy);
            try
            {
                db.ExecuteNonQuery(cmd);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in UpdateTicket");
                throw;
            }
        }

        public void AssignTicket(TicketAssignViewModel vm, int modifiedBy)
        {
            DbCommand cmd = db.GetStoredProcCommand("tmsTicketAssign");
            db.AddInParameter(cmd, "@TicketId", DbType.Int32, vm.TicketId);
            db.AddInParameter(cmd, "@AssignedToUserId", DbType.Int32, vm.AssignedToUserId);
            db.AddInParameter(cmd, "@ModifiedBy", DbType.Int32, modifiedBy);
            try
            {
                db.ExecuteNonQuery(cmd);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in AssignTicket");
                throw;
            }
        }

        public void UpdateTicketStatus(TicketStatusUpdateViewModel vm, int modifiedBy)
        {
            DbCommand cmd = db.GetStoredProcCommand("tmsTicketUpdateStatus");
            db.AddInParameter(cmd, "@TicketId", DbType.Int32, vm.TicketId);
            db.AddInParameter(cmd, "@StatusId", DbType.Int32, vm.StatusId);
            db.AddInParameter(cmd, "@PriorityId", DbType.Int32, vm.PriorityId);
            db.AddInParameter(cmd, "@ModifiedBy", DbType.Int32, modifiedBy);
            try
            {
                db.ExecuteNonQuery(cmd);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in UpdateTicketStatus");
                throw;
            }
        }

        public void DeleteTicket(int ticketId, int modifiedBy)
        {
            DbCommand cmd = db.GetStoredProcCommand("tmsTicketDelete");
            db.AddInParameter(cmd, "@TicketId", DbType.Int32, ticketId);
            db.AddInParameter(cmd, "@ModifiedBy", DbType.Int32, modifiedBy);
            try
            {
                db.ExecuteNonQuery(cmd);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in DeleteTicket");
                throw;
            }
        }

        public int AddComment(int ticketId, int createdBy, string comment, bool isInternal = false)
        {
            DbCommand cmd = db.GetStoredProcCommand("tmsTicketCommentCreate");
            db.AddInParameter(cmd, "@TicketId", DbType.Int32, ticketId);
            db.AddInParameter(cmd, "@CreatedBy", DbType.Int32, createdBy);
            db.AddInParameter(cmd, "@Comment", DbType.String, comment);
            db.AddInParameter(cmd, "@IsInternal", DbType.Boolean, isInternal);
            try
            {
                object result = db.ExecuteScalar(cmd);
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in AddComment");
                throw;
            }
        }

        public int AddAttachment(int ticketId, int createdBy, string storedFileName, string originalFileName, string fileExtension, string contentType, int fileSize, int? commentId = null)
        {
            DbCommand cmd = db.GetStoredProcCommand("tmsTicketAttachmentCreate");
            db.AddInParameter(cmd, "@TicketId", DbType.Int32, ticketId);
            db.AddInParameter(cmd, "@CommentId", DbType.Int32, commentId ?? (object)DBNull.Value);
            db.AddInParameter(cmd, "@CreatedBy", DbType.Int32, createdBy);
            db.AddInParameter(cmd, "@StoredFileName", DbType.String, storedFileName);
            db.AddInParameter(cmd, "@OriginalFileName", DbType.String, originalFileName);
            db.AddInParameter(cmd, "@FileExtension", DbType.String, fileExtension);
            db.AddInParameter(cmd, "@ContentType", DbType.String, contentType);
            db.AddInParameter(cmd, "@FileSize", DbType.Int32, fileSize);
            try
            {
                object result = db.ExecuteScalar(cmd);
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in AddAttachment");
                throw;
            }
        }

        public List<CommentViewModel> GetComments(int ticketId)
        {
            List<CommentViewModel> list = new List<CommentViewModel>();
            DbCommand cmd = db.GetStoredProcCommand("tmsTicketGetComments");
            db.AddInParameter(cmd, "@TicketId", DbType.Int32, ticketId);
            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        list.Add(new CommentViewModel
                        {
                            CommentId = Convert.ToInt32(reader["commentId"]),
                            Comment = reader["comment"].ToString(),
                            IsInternal = Convert.ToBoolean(reader["isInternal"]),
                            CreatedOn = Convert.ToDateTime(reader["CreatedOn"]),
                            CreatedBy = Convert.ToInt32(reader["CreatedBy"]),
                            CreatedByName = reader["createdByName"].ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetComments");
                throw;
            }
            return list;
        }

        public List<ActivityViewModel> GetActivities(int ticketId)
        {
            List<ActivityViewModel> list = new List<ActivityViewModel>();
            DbCommand cmd = db.GetStoredProcCommand("tmsTicketGetActivity");
            db.AddInParameter(cmd, "@TicketId", DbType.Int32, ticketId);
            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        list.Add(new ActivityViewModel
                        {
                            ActivityId = Convert.ToInt32(reader["activityId"]),
                            ActivityTypeName = reader["activityTypeName"].ToString(),
                            Remarks = reader["remarks"] != DBNull.Value ? reader["remarks"].ToString() : null,
                            OldValue = reader["oldValue"] != DBNull.Value ? reader["oldValue"].ToString() : null,
                            NewValue = reader["newValue"] != DBNull.Value ? reader["newValue"].ToString() : null,
                            CreatedOn = Convert.ToDateTime(reader["CreatedOn"]),
                            CreatedBy = Convert.ToInt32(reader["CreatedBy"]),
                            CreatedByName = reader["createdByName"].ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetActivities");
                throw;
            }
            return list;
        }

        public List<AttachmentViewModel> GetAttachments(int ticketId)
        {
            List<AttachmentViewModel> list = new List<AttachmentViewModel>();
            DbCommand cmd = db.GetStoredProcCommand("tmsTicketGetAttachments");
            db.AddInParameter(cmd, "@TicketId", DbType.Int32, ticketId);
            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        list.Add(new AttachmentViewModel
                        {
                            AttachmentId = Convert.ToInt32(reader["attachmentId"]),
                            TicketId = Convert.ToInt32(reader["ticketId"]),
                            CommentId = reader["commentId"] != DBNull.Value ? Convert.ToInt32(reader["commentId"]) : (int?)null,
                            StoredFileName = reader["storedFileName"].ToString(),
                            OriginalFileName = reader["originalFileName"].ToString(),
                            FileExtension = reader["fileExtension"].ToString(),
                            ContentType = reader["contentType"].ToString(),
                            FileSize = Convert.ToInt32(reader["fileSize"]),
                            CreatedOn = Convert.ToDateTime(reader["CreatedOn"]),
                            CreatedBy = Convert.ToInt32(reader["CreatedBy"]),
                            CreatedByName = reader["createdByName"].ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetAttachments");
                throw;
            }
            return list;
        }

        public AttachmentViewModel GetAttachmentById(int attachmentId)
        {
            AttachmentViewModel model = null;
            DbCommand cmd = db.GetStoredProcCommand("tmsTicketGetAttachmentById");
            db.AddInParameter(cmd, "@AttachmentId", DbType.Int32, attachmentId);
            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read())
                    {
                        model = new AttachmentViewModel
                        {
                            AttachmentId = Convert.ToInt32(reader["attachmentId"]),
                            TicketId = Convert.ToInt32(reader["ticketId"]),
                            CommentId = reader["commentId"] != DBNull.Value ? Convert.ToInt32(reader["commentId"]) : (int?)null,
                            StoredFileName = reader["storedFileName"].ToString(),
                            OriginalFileName = reader["originalFileName"].ToString(),
                            FileExtension = reader["fileExtension"].ToString(),
                            ContentType = reader["contentType"].ToString(),
                            FileSize = Convert.ToInt32(reader["fileSize"]),
                            CreatedOn = Convert.ToDateTime(reader["CreatedOn"]),
                            CreatedBy = Convert.ToInt32(reader["CreatedBy"]),
                            CreatedByName = reader["createdByName"].ToString()
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetAttachmentById");
                throw;
            }
            return model;
        }
    }
}
