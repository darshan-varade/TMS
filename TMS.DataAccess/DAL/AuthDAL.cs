using System;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using TMS.DataAccess.Models;
using Serilog;

namespace TMS.DataAccess.DAL
{
    public class AuthDAL
    {
        private Database db;

        public AuthDAL()
        {
            this.db = DatabaseFactory.CreateDatabase();
        }

        public int UserRegister(string fullName, string mobileNumber, string email, string passwordHash, int departmentId)
        {
            DbCommand cmd = db.GetStoredProcCommand("tmsUserRegister");
            db.AddInParameter(cmd, "@FullName", DbType.String, fullName);
            db.AddInParameter(cmd, "@MobileNumber", DbType.String, mobileNumber);
            db.AddInParameter(cmd, "@Email", DbType.String, email);
            db.AddInParameter(cmd, "@PasswordHash", DbType.String, passwordHash);
            db.AddInParameter(cmd, "@DepartmentId", DbType.Int32, departmentId);
            try
            {
                object result = db.ExecuteScalar(cmd);
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in UserRegister");
                throw;
            }
        }

        public UserModel UserLogin(string email)
        {
            UserModel model = null;
            DbCommand cmd = db.GetStoredProcCommand("tmsUserLogin");
            db.AddInParameter(cmd, "@Email", DbType.String, email);
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
                            LastLogin = reader["lastLogin"] != DBNull.Value ? Convert.ToDateTime(reader["lastLogin"]) : (DateTime?)null,
                            IsApproved = reader["isApproved"] != DBNull.Value ? Convert.ToByte(reader["isApproved"]) : (byte?)null
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in UserLogin");
                throw;
            }
            return model;
        }

        public bool UserCheckEmail(string email)
        {
            DbCommand cmd = db.GetStoredProcCommand("tmsUserCheckEmail");
            db.AddInParameter(cmd, "@Email", DbType.String, email);
            try
            {
                object result = db.ExecuteScalar(cmd);
                return result != null && Convert.ToBoolean(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in UserCheckEmail");
                throw;
            }
        }

        public int CreateOtpByEmail(string email, string otpCode, DateTime expiresAt)
        {
            DbCommand cmd = db.GetStoredProcCommand("tmsOtpCreateByEmail");
            db.AddInParameter(cmd, "@OtpEmail", DbType.String, email);
            db.AddInParameter(cmd, "@OtpCode", DbType.String, otpCode);
            db.AddInParameter(cmd, "@ExpiresAt", DbType.DateTime, expiresAt);
            try
            {
                object result = db.ExecuteScalar(cmd);
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in CreateOtpByEmail");
                throw;
            }
        }

        public int? ValidateOtpByEmail(string email, string otpCode)
        {
            DbCommand cmd = db.GetStoredProcCommand("tmsOtpValidateByEmail");
            db.AddInParameter(cmd, "@OtpEmail", DbType.String, email);
            db.AddInParameter(cmd, "@OtpCode", DbType.String, otpCode);
            try
            {
                object result = db.ExecuteScalar(cmd);
                return result != null ? Convert.ToInt32(result) : (int?)null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in ValidateOtpByEmail");
                throw;
            }
        }

        public void MarkOtpUsed(int otpId)
        {
            DbCommand cmd = db.GetStoredProcCommand("tmsOtpMarkUsed");
            db.AddInParameter(cmd, "@OtpId", DbType.Int32, otpId);
            try
            {
                db.ExecuteNonQuery(cmd);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in MarkOtpUsed");
                throw;
            }
        }

        public int CreateRefreshToken(int userId, string hash, DateTime expiresAt)
        {
            DbCommand cmd = db.GetStoredProcCommand("tmsRefreshTokenCreate");
            db.AddInParameter(cmd, "@UserId", DbType.Int32, userId);
            db.AddInParameter(cmd, "@RefreshTokenHash", DbType.String, hash);
            db.AddInParameter(cmd, "@ExpiresAt", DbType.DateTime, expiresAt);
            try
            {
                object result = db.ExecuteScalar(cmd);
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in CreateRefreshToken");
                throw;
            }
        }

        public RefreshTokenModel GetRefreshTokenByHash(string hash)
        {
            DbCommand cmd = db.GetStoredProcCommand("tmsRefreshTokenGetByHash");
            db.AddInParameter(cmd, "@Hash", DbType.String, hash);
            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read())
                    {
                        return new RefreshTokenModel
                        {
                            RefreshTokenId = Convert.ToInt32(reader["refreshTokenId"]),
                            UserId = Convert.ToInt32(reader["userId"]),
                            FullName = reader["fullName"].ToString(),
                            MobileNumber = reader["mobileNumber"] != DBNull.Value ? reader["mobileNumber"].ToString() : null,
                            EmailId = reader["emailId"].ToString(),
                            RoleName = reader["roleName"].ToString(),
                            ExpiresAt = Convert.ToDateTime(reader["expiresAt"])
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetRefreshTokenByHash");
                throw;
            }
            return null;
        }

        public int RotateRefreshToken(int oldTokenId, string newHash, DateTime newExpiresAt, int userId)
        {
            DbCommand cmd = db.GetStoredProcCommand("tmsRefreshTokenRotate");
            db.AddInParameter(cmd, "@OldRefreshTokenId", DbType.Int32, oldTokenId);
            db.AddInParameter(cmd, "@NewRefreshTokenHash", DbType.String, newHash);
            db.AddInParameter(cmd, "@NewExpiresAt", DbType.DateTime, newExpiresAt);
            db.AddInParameter(cmd, "@UserId", DbType.Int32, userId);
            try
            {
                object result = db.ExecuteScalar(cmd);
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in RotateRefreshToken");
                throw;
            }
        }

        public void RevokeRefreshToken(int refreshTokenId)
        {
            DbCommand cmd = db.GetStoredProcCommand("tmsRefreshTokenRevoke");
            db.AddInParameter(cmd, "@RefreshTokenId", DbType.Int32, refreshTokenId);
            try
            {
                db.ExecuteNonQuery(cmd);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in RevokeRefreshToken");
                throw;
            }
        }

        public DateTime? GetLatestOtpTimeByEmail(string email)
        {
            DbCommand cmd = db.GetSqlStringCommand("SELECT MAX(CreatedOn) FROM tmsOtp WHERE emailId = @Email");
            db.AddInParameter(cmd, "@Email", DbType.String, email);
            try
            {
                object result = db.ExecuteScalar(cmd);
                return result != null && result != DBNull.Value ? (DateTime?)Convert.ToDateTime(result) : null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetLatestOtpTimeByEmail");
                throw;
            }
        }
    }
}
