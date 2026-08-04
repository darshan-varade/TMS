USE Training_DB_Darshan_Varade
GO

CREATE OR ALTER PROCEDURE tmsSequenceGetNextValue
	@SequenceName VARCHAR(50)
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Get next sequence value
***********************************************************************************************
tmsSequenceGetNextValue

*/
BEGIN
	SET NOCOUNT ON;
	DECLARE @CurrentMonth CHAR(6) = CONVERT(CHAR(6), GETDATE(), 112);

	UPDATE tmsSequence
	SET currValue = CASE WHEN monthYear = @CurrentMonth THEN currValue + 1 ELSE 1 END,
		monthYear = @CurrentMonth,
		ModifiedOn = GETDATE()
	WHERE sequenceName = @SequenceName;

	IF @@ROWCOUNT = 0
		INSERT INTO tmsSequence (sequenceName, currValue, monthYear, IsActive, CreatedOn, CreatedBy)
		VALUES (@SequenceName, 1, @CurrentMonth, 1, GETDATE(), 1);

	SELECT currValue AS CurrValue, monthYear AS MonthYear
	FROM tmsSequence
	WHERE sequenceName = @SequenceName;
END;
GO

CREATE OR ALTER PROCEDURE tmsUserCheckEmail
	@Email VARCHAR(100)
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Check if email is already registered
***********************************************************************************************
tmsUserCheckEmail

*/
BEGIN
	SET NOCOUNT ON;
	SELECT CAST(CASE WHEN EXISTS (SELECT 1 FROM tmsCredential WHERE emailId = @Email) THEN 1 ELSE 0 END AS BIT) AS EmailExists;
END;
GO

CREATE OR ALTER PROCEDURE tmsUserRegister
	@FullName VARCHAR(100),
	@MobileNumber VARCHAR(15),
	@Email VARCHAR(100),
	@PasswordHash VARCHAR(255),
	@DepartmentId INT
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Register a new employee user
***********************************************************************************************
tmsUserRegister

*/
BEGIN
	SET NOCOUNT ON;
	DECLARE @UserId INT;
	DECLARE @EmployeeRoleId INT = (SELECT roleId FROM tmsRole WHERE roleName = 'Employee');
	BEGIN TRY
		INSERT INTO tmsUser (fullName, mobileNumber, departmentId, IsActive, CreatedOn, CreatedBy)
		VALUES (@FullName, @MobileNumber, @DepartmentId, 1, GETDATE(), 1);
		SET @UserId = SCOPE_IDENTITY();
		INSERT INTO tmsCredential (userId, emailId, passwordHash, roleId, isApproved, IsActive, CreatedOn, CreatedBy)
		VALUES (@UserId, @Email, @PasswordHash, @EmployeeRoleId, NULL, 1, GETDATE(), 1);
		SELECT @UserId AS UserId;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsUserLogin
	@Email VARCHAR(100)
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Get user login details by email
***********************************************************************************************
tmsUserLogin

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		SELECT u.userId, u.fullName, u.mobileNumber, u.departmentId, d.departmentName,
			c.credentialId, c.emailId, c.passwordHash, c.roleId, r.roleName, c.lastLogin, c.isApproved
		FROM tmsUser u
		INNER JOIN tmsCredential c ON u.userId = c.userId
		INNER JOIN tmsRole r ON c.roleId = r.roleId
		INNER JOIN tmsDepartment d ON u.departmentId = d.departmentId
		WHERE c.emailId = @Email AND u.IsActive = 1;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsUserUpdateLastLogin
	@UserId INT
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Update user last login time
***********************************************************************************************
tmsUserUpdateLastLogin

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		UPDATE tmsCredential SET lastLogin = GETDATE() WHERE userId = @UserId;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsOtpCreateByEmail
	@OtpEmail VARCHAR(100), @OtpCode CHAR(6), @ExpiresAt DATETIME
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Create OTP for password reset
***********************************************************************************************
tmsOtpCreateByEmail

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		INSERT INTO tmsOtp (emailId, otpCode, expiresOn, CreatedOn) VALUES (@OtpEmail, @OtpCode, @ExpiresAt, GETDATE());
		SELECT SCOPE_IDENTITY() AS OtpId;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsOtpValidateByEmail
	@OtpEmail VARCHAR(100), @OtpCode CHAR(6)
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Validate OTP code
***********************************************************************************************
tmsOtpValidateByEmail

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		SELECT TOP 1 otpId AS OtpId FROM tmsOtp
		WHERE emailId = @OtpEmail AND otpCode = @OtpCode AND isUsed = 0 AND expiresOn > GETDATE()
		ORDER BY CreatedOn DESC;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsOtpMarkUsed
	@OtpId INT
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Mark OTP as used
***********************************************************************************************
tmsOtpMarkUsed

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		UPDATE tmsOtp SET isUsed = 1 WHERE otpId = @OtpId;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsOtpGetLatestTimeByEmail
	@Email VARCHAR(100)
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Get latest OTP creation time
***********************************************************************************************
tmsOtpGetLatestTimeByEmail

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		SELECT MAX(CreatedOn) AS LatestOtpTime FROM tmsOtp WHERE emailId = @Email;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsRefreshTokenCreate
	@UserId INT, @RefreshTokenHash VARCHAR(255), @ExpiresAt DATETIME
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Create refresh token
***********************************************************************************************
tmsRefreshTokenCreate

*/
BEGIN
	SET NOCOUNT ON;
	DECLARE @CredentialId INT;
	BEGIN TRY
		SELECT @CredentialId = credentialId FROM tmsCredential WHERE userId = @UserId;
		INSERT INTO tmsRefreshToken (credentialId, refreshTokenHash, expiresAt, CreatedOn)
		VALUES (@CredentialId, @RefreshTokenHash, @ExpiresAt, GETDATE());
		SELECT SCOPE_IDENTITY() AS RefreshTokenId;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsRefreshTokenGetByHash
	@Hash VARCHAR(255)
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Get refresh token by hash
***********************************************************************************************
tmsRefreshTokenGetByHash

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		SELECT rt.refreshTokenId, rt.credentialId, u.userId, u.fullName, u.mobileNumber,
			c.emailId, r.roleName, rt.expiresAt
		FROM tmsRefreshToken rt
		INNER JOIN tmsCredential c ON rt.credentialId = c.credentialId
		INNER JOIN tmsUser u ON c.userId = u.userId
		INNER JOIN tmsRole r ON c.roleId = r.roleId
		WHERE rt.refreshTokenHash = @Hash AND rt.revokedAt IS NULL AND rt.expiresAt > GETDATE();
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsRefreshTokenRotate
	@OldRefreshTokenId INT, @NewRefreshTokenHash VARCHAR(255), @NewExpiresAt DATETIME, @UserId INT
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Rotate refresh token
***********************************************************************************************
tmsRefreshTokenRotate

*/
BEGIN
	SET NOCOUNT ON;
	DECLARE @CredentialId INT;
	BEGIN TRY
		SELECT @CredentialId = credentialId FROM tmsCredential WHERE userId = @UserId;
		UPDATE tmsRefreshToken SET revokedAt = GETDATE(), replacedByTokenHash = @NewRefreshTokenHash
		WHERE refreshTokenId = @OldRefreshTokenId;
		INSERT INTO tmsRefreshToken (credentialId, refreshTokenHash, expiresAt, CreatedOn)
		VALUES (@CredentialId, @NewRefreshTokenHash, @NewExpiresAt, GETDATE());
		SELECT SCOPE_IDENTITY() AS RefreshTokenId;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsRefreshTokenRevoke
	@RefreshTokenId INT
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Revoke refresh token by id
***********************************************************************************************
tmsRefreshTokenRevoke

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		UPDATE tmsRefreshToken SET revokedAt = GETDATE() WHERE refreshTokenId = @RefreshTokenId;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsRefreshTokenRevokeByUserId
	@UserId INT
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Revoke refresh tokens by user
***********************************************************************************************
tmsRefreshTokenRevokeByUserId

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		UPDATE rt SET rt.revokedAt = GETDATE()
		FROM tmsRefreshToken rt
		INNER JOIN tmsCredential c ON rt.credentialId = c.credentialId
		WHERE c.userId = @UserId AND rt.revokedAt IS NULL;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsDepartmentGetAll
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Get all departments
***********************************************************************************************
tmsDepartmentGetAll

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		SELECT departmentId, departmentName FROM tmsDepartment WHERE IsActive = 1 ORDER BY departmentName;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsRoleGetAll
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Get all roles
***********************************************************************************************
tmsRoleGetAll

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		SELECT roleId, roleName, roleDescription FROM tmsRole WHERE IsActive = 1 ORDER BY roleName;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsCategoryGetAll
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Get all categories
***********************************************************************************************
tmsCategoryGetAll

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		SELECT categoryId, categoryName FROM tmsCategory WHERE IsActive = 1 ORDER BY categoryName;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsPriorityGetAll
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Get all priorities
***********************************************************************************************
tmsPriorityGetAll

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		SELECT p.priorityId, p.priorityName, s.resolutionHours AS slaHours
		FROM tmsPriority p
		LEFT JOIN tmsSLA s ON p.priorityId = s.priorityId
		WHERE p.IsActive = 1
		ORDER BY p.priorityId;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsStatusGetAll
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Get all statuses
***********************************************************************************************
tmsStatusGetAll

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		SELECT statusId, statusName FROM tmsStatus WHERE IsActive = 1 ORDER BY statusId;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsTicketCreate
	@CreatedBy INT, @Title VARCHAR(200), @Description VARCHAR(MAX), @CategoryId INT, @PriorityId INT
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Create a new ticket
***********************************************************************************************
tmsTicketCreate

*/
BEGIN
	SET NOCOUNT ON;
	DECLARE @TicketNumber VARCHAR(30), @CurrValue INT, @MonthYear CHAR(6), @StatusId INT, @DueDate DATETIME, @ResolutionHours INT;
	BEGIN TRY
		CREATE TABLE #SeqResult (CurrValue INT, MonthYear CHAR(6));
		INSERT INTO #SeqResult EXEC tmsSequenceGetNextValue 'TicketNumber';
		SELECT @CurrValue = CurrValue, @MonthYear = MonthYear FROM #SeqResult;
		DROP TABLE #SeqResult;
		SET @TicketNumber = 'TKT-' + @MonthYear + '-' + RIGHT('0000' + CAST(@CurrValue AS VARCHAR(4)), 4);
		SELECT @StatusId = statusId FROM tmsStatus WHERE statusName = 'New';
		SELECT @ResolutionHours = resolutionHours FROM tmsSLA WHERE priorityId = @PriorityId;
		SET @DueDate = DATEADD(HOUR, ISNULL(@ResolutionHours, 24), GETDATE());
		INSERT INTO tmsTicket (ticketNumber, title, description, categoryId, priorityId, statusId, dueDate, IsActive, CreatedOn, CreatedBy)
		VALUES (@TicketNumber, @Title, @Description, @CategoryId, @PriorityId, @StatusId, @DueDate, 1, GETDATE(), @CreatedBy);
		DECLARE @NewTicketId INT = SCOPE_IDENTITY();
		EXEC tmsTicketActivityCreate @NewTicketId, @CreatedBy, 'Ticket Created', 'Ticket created';
		SELECT @NewTicketId AS TicketId, @TicketNumber AS TicketNumber;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsTicketGetByUserId
	@UserId INT
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Get tickets created by a user
***********************************************************************************************
tmsTicketGetByUserId

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		SELECT t.ticketId, t.ticketNumber, t.title, t.description,
			t.categoryId, c.categoryName, t.priorityId, p.priorityName,
			t.statusId, s.statusName, t.dueDate, t.CreatedOn
		FROM tmsTicket t
		INNER JOIN tmsCategory c ON t.categoryId = c.categoryId
		INNER JOIN tmsPriority p ON t.priorityId = p.priorityId
		INNER JOIN tmsStatus s ON t.statusId = s.statusId
		WHERE t.CreatedBy = @UserId AND t.IsActive = 1
		ORDER BY t.CreatedOn DESC;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsTicketGetById
	@TicketId INT
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Get ticket details by id
***********************************************************************************************
tmsTicketGetById

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		SELECT t.ticketId, t.ticketNumber, t.title, t.description,
			t.categoryId, c.categoryName, t.priorityId, p.priorityName,
			t.statusId, s.statusName, t.assignedToUserId, assignee.fullName AS assignedToName,
			t.dueDate, t.resolvedOn, t.CreatedOn, u.fullName AS createdByName, u.userId AS createdByUserId
		FROM tmsTicket t
		INNER JOIN tmsCategory c ON t.categoryId = c.categoryId
		INNER JOIN tmsPriority p ON t.priorityId = p.priorityId
		INNER JOIN tmsStatus s ON t.statusId = s.statusId
		INNER JOIN tmsUser u ON t.CreatedBy = u.userId
		LEFT JOIN tmsUser assignee ON t.assignedToUserId = assignee.userId
		WHERE t.ticketId = @TicketId AND t.IsActive = 1;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsTicketGetList
	@SearchTerm VARCHAR(200) = NULL, @StatusId INT = NULL, @PriorityId INT = NULL,
	@CategoryId INT = NULL, @AssignedToUserId INT = NULL,
	@DateFrom DATETIME = NULL, @DateTo DATETIME = NULL,
	@UserId INT = NULL, @UserRole VARCHAR(50) = NULL,
	@SortColumn VARCHAR(50) = 'CreatedOn', @SortDirection VARCHAR(4) = 'DESC',
	@PageNumber INT = 1, @PageSize INT = 10, @TotalRows INT OUTPUT
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Get filtered, paginated ticket list
2	3Aug2026		Darshan Varade		Add unassigned ticket filter (@AssignedToUserId = -1)
***********************************************************************************************
tmsTicketGetList

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		SELECT t.ticketId, t.ticketNumber, t.title, t.assignedToUserId, t.CreatedBy AS createdByUserId,
			c.categoryName, p.priorityName, s.statusName,
			u.fullName AS createdByName, assignee.fullName AS assignedToName,
			t.CreatedOn,
			(SELECT COUNT(*) FROM tmsTicketComment tc WHERE tc.ticketId = t.ticketId AND tc.IsActive = 1) AS ConversationCount
		INTO #ticketList
		FROM tmsTicket t
		INNER JOIN tmsCategory c ON t.categoryId = c.categoryId
		INNER JOIN tmsPriority p ON t.priorityId = p.priorityId
		INNER JOIN tmsStatus s ON t.statusId = s.statusId
		INNER JOIN tmsUser u ON t.CreatedBy = u.userId
		LEFT JOIN tmsUser assignee ON t.assignedToUserId = assignee.userId
		WHERE t.IsActive = 1
			AND (@SearchTerm IS NULL OR t.title LIKE '%' + @SearchTerm + '%' OR t.ticketNumber LIKE '%' + @SearchTerm + '%')
			AND (@StatusId IS NULL OR t.statusId = @StatusId)
			AND (@PriorityId IS NULL OR t.priorityId = @PriorityId)
			AND (@CategoryId IS NULL OR t.categoryId = @CategoryId)
			AND (
				(@AssignedToUserId = -1 AND t.assignedToUserId IS NULL)
				OR (@AssignedToUserId IS NULL)
				OR (@AssignedToUserId > 0 AND t.assignedToUserId = @AssignedToUserId)
			)
			AND (@DateFrom IS NULL OR t.CreatedOn >= @DateFrom)
			AND (@DateTo IS NULL OR t.CreatedOn <= @DateTo)
			AND (@UserRole IN ('Admin', 'Support') OR @UserId IS NULL OR t.CreatedBy = @UserId OR t.assignedToUserId = @UserId);

		SELECT @TotalRows = COUNT(1) FROM #ticketList;

		DECLARE @sql NVARCHAR(MAX), @orderCol NVARCHAR(50);
		SET @orderCol = CASE @SortColumn
			WHEN 'TicketNumber' THEN 'ticketNumber'
			WHEN 'Title' THEN 'title'
			WHEN 'StatusName' THEN 'statusName'
			WHEN 'PriorityName' THEN 'priorityName'
			ELSE 'CreatedOn'
		END;

		SET @sql = 'SELECT * FROM #ticketList
			ORDER BY ' + QUOTENAME(@orderCol) + ' '
			+ CASE WHEN @SortDirection = 'DESC' THEN 'DESC' ELSE 'ASC' END + '
			OFFSET (@PageNumber - 1) * @PageSize ROWS
			FETCH NEXT @PageSize ROWS ONLY';

		EXEC sp_executesql @sql,
			N'@PageNumber INT, @PageSize INT',
			@PageNumber, @PageSize;

		DROP TABLE #ticketList;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsTicketUpdate
	@TicketId INT, @Title VARCHAR(200), @Description VARCHAR(MAX), @CategoryId INT = NULL,
	@PriorityId INT, @StatusId INT, @AssignedToUserId INT = NULL, @ModifiedBy INT
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Update ticket details
***********************************************************************************************
tmsTicketUpdate

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		DECLARE @OldStatusId INT, @OldAssignedToUserId INT, @OldPriorityId INT, @OldCategoryId INT;
		SELECT @OldStatusId = statusId, @OldAssignedToUserId = assignedToUserId,
			@OldPriorityId = priorityId, @OldCategoryId = categoryId
		FROM tmsTicket WHERE ticketId = @TicketId;

		UPDATE tmsTicket
		SET title = @Title, description = @Description, categoryId = ISNULL(@CategoryId, categoryId),
			priorityId = @PriorityId, statusId = @StatusId, assignedToUserId = @AssignedToUserId,
			modifiedOn = GETDATE(), ModifiedBy = @ModifiedBy
		WHERE ticketId = @TicketId;

		IF @OldStatusId != @StatusId
		BEGIN
			DECLARE @OldStatusName VARCHAR(50), @NewStatusName VARCHAR(50);
			SELECT @OldStatusName = statusName FROM tmsStatus WHERE statusId = @OldStatusId;
			SELECT @NewStatusName = statusName FROM tmsStatus WHERE statusId = @StatusId;
			EXEC tmsTicketActivityCreate @TicketId, @ModifiedBy, 'Status Changed', NULL, @OldStatusName, @NewStatusName;
		END;

		IF ISNULL(@OldPriorityId, 0) != @PriorityId
		BEGIN
			DECLARE @OldPriorityName VARCHAR(20), @NewPriorityName VARCHAR(20);
			SELECT @OldPriorityName = priorityName FROM tmsPriority WHERE priorityId = @OldPriorityId;
			SELECT @NewPriorityName = priorityName FROM tmsPriority WHERE priorityId = @PriorityId;
			EXEC tmsTicketActivityCreate @TicketId, @ModifiedBy, 'Priority Changed', NULL, @OldPriorityName, @NewPriorityName;
		END;

		IF ISNULL(@OldCategoryId, 0) != ISNULL(@CategoryId, 0)
		BEGIN
			DECLARE @OldCategoryName VARCHAR(50), @NewCategoryName VARCHAR(50);
			SELECT @OldCategoryName = categoryName FROM tmsCategory WHERE categoryId = @OldCategoryId;
			SELECT @NewCategoryName = categoryName FROM tmsCategory WHERE categoryId = @CategoryId;
			EXEC tmsTicketActivityCreate @TicketId, @ModifiedBy, 'Category Changed', NULL, @OldCategoryName, @NewCategoryName;
		END;

		IF ISNULL(@OldAssignedToUserId, 0) != ISNULL(@AssignedToUserId, 0)
		BEGIN
			DECLARE @OldAssigneeName VARCHAR(100), @NewAssigneeName VARCHAR(100);
			SELECT @OldAssigneeName = fullName FROM tmsUser WHERE userId = @OldAssignedToUserId;
			SELECT @NewAssigneeName = fullName FROM tmsUser WHERE userId = @AssignedToUserId;
			EXEC tmsTicketActivityCreate @TicketId, @ModifiedBy, 'Assignee Changed', NULL, @OldAssigneeName, @NewAssigneeName;
		END;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsTicketAssign
	@TicketId INT, @AssignedToUserId INT, @ModifiedBy INT
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Assign ticket to a support executive
***********************************************************************************************
tmsTicketAssign

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		DECLARE @AssignedStatusId INT;
		DECLARE @OldAssignedToUserId INT, @OldStatusName VARCHAR(30);
		DECLARE @NewAssigneeName VARCHAR(100);

		SELECT @OldAssignedToUserId = assignedToUserId,
			@OldStatusName = s.statusName
		FROM tmsTicket t
		INNER JOIN tmsStatus s ON t.statusId = s.statusId
		WHERE t.ticketId = @TicketId;

		SELECT @AssignedStatusId = statusId FROM tmsStatus WHERE statusName = 'Assigned';
		SELECT @NewAssigneeName = fullName FROM tmsUser WHERE userId = @AssignedToUserId;
		DECLARE @OldAssigneeName VARCHAR(100);
		SELECT @OldAssigneeName = fullName FROM tmsUser WHERE userId = @OldAssignedToUserId;

		UPDATE tmsTicket
		SET assignedToUserId = @AssignedToUserId, statusId = @AssignedStatusId,
			modifiedOn = GETDATE(), ModifiedBy = @ModifiedBy
		WHERE ticketId = @TicketId;

		EXEC tmsTicketActivityCreate @TicketId, @ModifiedBy, 'Ticket Assigned',
			NULL, @OldAssigneeName, @NewAssigneeName;

		IF @OldStatusName IS NOT NULL AND @OldStatusName != 'Assigned'
		BEGIN
			EXEC tmsTicketActivityCreate @TicketId, @ModifiedBy, 'Status Changed',
				NULL, @OldStatusName, 'Assigned';
		END;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsTicketUpdateStatus
	@TicketId INT, @StatusId INT, @PriorityId INT, @ModifiedBy INT
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Update ticket status and priority
***********************************************************************************************
tmsTicketUpdateStatus

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		DECLARE @OldStatusName VARCHAR(30), @NewStatusName VARCHAR(30);
		DECLARE @OldPriorityName VARCHAR(20), @NewPriorityName VARCHAR(20);
		DECLARE @OldStatusId INT, @OldPriorityId INT;

		SELECT @OldStatusId = statusId, @OldPriorityId = priorityId
		FROM tmsTicket WHERE ticketId = @TicketId;

		SELECT @OldStatusName = statusName FROM tmsStatus WHERE statusId = @OldStatusId;
		SELECT @NewStatusName = statusName FROM tmsStatus WHERE statusId = @StatusId;
		SELECT @OldPriorityName = priorityName FROM tmsPriority WHERE priorityId = @OldPriorityId;
		SELECT @NewPriorityName = priorityName FROM tmsPriority WHERE priorityId = @PriorityId;

		UPDATE tmsTicket
		SET statusId = @StatusId, priorityId = @PriorityId,
			resolvedOn = CASE WHEN @NewStatusName = 'Resolved' THEN GETDATE()
							WHEN @NewStatusName = 'Reopened' THEN NULL
							ELSE resolvedOn END,
			modifiedOn = GETDATE(), ModifiedBy = @ModifiedBy
		WHERE ticketId = @TicketId;

		IF @OldStatusId != @StatusId
		BEGIN
			EXEC tmsTicketActivityCreate @TicketId, @ModifiedBy, 'Status Changed', NULL, @OldStatusName, @NewStatusName;
		END;

		IF @OldPriorityId != @PriorityId
		BEGIN
			EXEC tmsTicketActivityCreate @TicketId, @ModifiedBy, 'Priority Changed', NULL, @OldPriorityName, @NewPriorityName;
		END;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsTicketDelete
	@TicketId INT, @ModifiedBy INT
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Soft delete a ticket
***********************************************************************************************
tmsTicketDelete

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		UPDATE tmsTicket SET IsActive = 0, modifiedOn = GETDATE(), ModifiedBy = @ModifiedBy
		WHERE ticketId = @TicketId;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsTicketCommentCreate
	@TicketId INT, @CreatedBy INT, @Comment VARCHAR(MAX), @IsInternal BIT = 0
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Add a ticket comment
***********************************************************************************************
tmsTicketCommentCreate

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		INSERT INTO tmsTicketComment (ticketId, comment, isInternal, IsActive, CreatedOn, CreatedBy)
		VALUES (@TicketId, @Comment, @IsInternal, 1, GETDATE(), @CreatedBy);
		DECLARE @NewCommentId INT = SCOPE_IDENTITY();
		DECLARE @ActivityRemarks VARCHAR(MAX);
		SET @ActivityRemarks = CASE WHEN LEN(@Comment) > 100 THEN LEFT(@Comment, 100) + '...' ELSE @Comment END;
		EXEC tmsTicketActivityCreate @TicketId, @CreatedBy, 'Comment Added', NULL, NULL, @ActivityRemarks;
		SELECT @NewCommentId AS CommentId;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsTicketAttachmentCreate
	@TicketId INT, @CommentId INT = NULL, @CreatedBy INT, @StoredFileName VARCHAR(255), @OriginalFileName VARCHAR(255),
	@FileExtension VARCHAR(10), @ContentType VARCHAR(100), @FileSize INT
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Add a ticket attachment
2	1Aug2026		Darshan Varade		Support attachment preview
***********************************************************************************************
tmsTicketAttachmentCreate

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		INSERT INTO tmsTicketAttachment (ticketId, commentId, storedFileName, originalFileName, fileExtension, contentType, fileSize, IsActive, CreatedOn, CreatedBy)
		VALUES (@TicketId, @CommentId, @StoredFileName, @OriginalFileName, @FileExtension, @ContentType, @FileSize, 1, GETDATE(), @CreatedBy);
		DECLARE @NewAttachmentId INT = SCOPE_IDENTITY();
		EXEC tmsTicketActivityCreate @TicketId, @CreatedBy, 'Attachment Added', NULL, NULL, @OriginalFileName;
		SELECT @NewAttachmentId AS AttachmentId;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsTicketActivityCreate
	@TicketId INT, @CreatedBy INT, @ActivityTypeName VARCHAR(50),
	@Remarks VARCHAR(MAX) = NULL, @OldValue VARCHAR(200) = NULL, @NewValue VARCHAR(200) = NULL
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Log ticket activity
***********************************************************************************************
tmsTicketActivityCreate

*/
BEGIN
	SET NOCOUNT ON;
	DECLARE @ActivityTypeId INT;
	BEGIN TRY
		SELECT @ActivityTypeId = activityTypeId FROM tmsActivityType WHERE activityTypeName = @ActivityTypeName;
		INSERT INTO tmsTicketActivity (ticketId, activityTypeId, remarks, oldValue, newValue, IsActive, CreatedOn, CreatedBy)
		VALUES (@TicketId, @ActivityTypeId, @Remarks, @OldValue, @NewValue, 1, GETDATE(), @CreatedBy);
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsTicketGetComments
	@TicketId INT
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Get ticket comments
***********************************************************************************************
tmsTicketGetComments

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		SELECT tc.commentId, tc.ticketId, tc.comment, tc.isInternal,
			tc.CreatedOn, tc.CreatedBy, u.fullName AS createdByName
		FROM tmsTicketComment tc
		INNER JOIN tmsUser u ON tc.CreatedBy = u.userId
		WHERE tc.ticketId = @TicketId AND tc.IsActive = 1
		ORDER BY tc.CreatedOn ASC;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsTicketGetActivity
	@TicketId INT
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Get ticket activity feed
***********************************************************************************************
tmsTicketGetActivity

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		SELECT ta.activityId, ta.ticketId, ta.activityTypeId, at.activityTypeName,
			ta.remarks, ta.oldValue, ta.newValue, ta.referenceId,
			ta.CreatedOn, ta.CreatedBy, u.fullName AS createdByName
		FROM tmsTicketActivity ta
		INNER JOIN tmsActivityType at ON ta.activityTypeId = at.activityTypeId
		INNER JOIN tmsUser u ON ta.CreatedBy = u.userId
		WHERE ta.ticketId = @TicketId AND ta.IsActive = 1
		ORDER BY ta.CreatedOn ASC;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsTicketGetAttachments
	@TicketId INT
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Get ticket attachments
2	1Aug2026		Darshan Varade		Support attachment preview
***********************************************************************************************
tmsTicketGetAttachments

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		SELECT ta.attachmentId, ta.ticketId, ta.commentId, ta.storedFileName, ta.originalFileName,
			ta.fileExtension, ta.contentType, ta.fileSize,
			ta.CreatedOn, ta.CreatedBy, u.fullName AS createdByName
		FROM tmsTicketAttachment ta
		INNER JOIN tmsUser u ON ta.CreatedBy = u.userId
		WHERE ta.ticketId = @TicketId AND ta.IsActive = 1
		ORDER BY ta.CreatedOn DESC;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsTicketGetAttachmentById
	@AttachmentId INT
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Get attachment file by id
2	1Aug2026		Darshan Varade		Support attachment preview
***********************************************************************************************
tmsTicketGetAttachmentById

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		SELECT ta.attachmentId, ta.ticketId, ta.commentId, ta.storedFileName, ta.originalFileName,
			ta.fileExtension, ta.contentType, ta.fileSize,
			ta.CreatedOn, ta.CreatedBy, u.fullName AS createdByName
		FROM tmsTicketAttachment ta
		INNER JOIN tmsUser u ON ta.CreatedBy = u.userId
		WHERE ta.attachmentId = @AttachmentId AND ta.IsActive = 1;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsUserGetList
	@SearchTerm VARCHAR(200) = NULL, @RoleId INT = NULL,
	@SortColumn VARCHAR(50) = 'CreatedOn', @SortDirection VARCHAR(4) = 'DESC',
	@PageNumber INT = 1, @PageSize INT = 10, @TotalRows INT OUTPUT
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Get filtered, paginated user list
2	1Aug2026		Darshan Varade		Add filters and search
***********************************************************************************************
tmsUserGetList

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		SELECT u.userId, u.fullName, c.emailId, u.mobileNumber, c.roleId,
			r.roleName, d.departmentName, u.IsActive, c.isApproved, u.CreatedOn,
			(SELECT COUNT(*) FROM tmsTicket t WHERE t.CreatedBy = u.userId AND t.IsActive = 1) AS TotalTickets
		INTO #userList
		FROM tmsUser u
		INNER JOIN tmsCredential c ON u.userId = c.userId
		INNER JOIN tmsRole r ON c.roleId = r.roleId
		INNER JOIN tmsDepartment d ON u.departmentId = d.departmentId
		WHERE (@SearchTerm IS NULL OR u.fullName LIKE '%' + @SearchTerm + '%')
			AND (@RoleId IS NULL OR c.roleId = @RoleId);

		SELECT @TotalRows = COUNT(1) FROM #userList;

		DECLARE @sql NVARCHAR(MAX), @orderCol NVARCHAR(50);
		SET @orderCol = CASE @SortColumn
			WHEN 'FullName' THEN 'fullName'
			WHEN 'EmailId' THEN 'emailId'
			WHEN 'RoleName' THEN 'roleName'
			WHEN 'DepartmentName' THEN 'departmentName'
			ELSE 'CreatedOn'
		END;

		SET @sql = 'SELECT * FROM #userList
			ORDER BY ' + QUOTENAME(@orderCol) + ' '
			+ CASE WHEN @SortDirection = 'DESC' THEN 'DESC' ELSE 'ASC' END + '
			OFFSET (@PageNumber - 1) * @PageSize ROWS
			FETCH NEXT @PageSize ROWS ONLY';

		EXEC sp_executesql @sql,
			N'@PageNumber INT, @PageSize INT',
			@PageNumber, @PageSize;

		DROP TABLE #userList;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsUserAdd
	@FullName VARCHAR(100), @MobileNumber VARCHAR(15), @Email VARCHAR(100),
	@PasswordHash VARCHAR(255), @RoleId INT, @DepartmentId INT, @CreatedBy INT,
	@UserId INT OUTPUT
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Add a new user
***********************************************************************************************
tmsUserAdd

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		INSERT INTO tmsUser (fullName, mobileNumber, departmentId, IsActive, CreatedOn, CreatedBy)
		VALUES (@FullName, @MobileNumber, @DepartmentId, 1, GETDATE(), @CreatedBy);
		SET @UserId = SCOPE_IDENTITY();
		INSERT INTO tmsCredential (userId, emailId, passwordHash, roleId, isApproved, IsActive, CreatedOn, CreatedBy)
		VALUES (@UserId, @Email, @PasswordHash, @RoleId, 1, 1, GETDATE(), @CreatedBy);
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsUserUpdate
	@UserId INT, @FullName VARCHAR(100), @MobileNumber VARCHAR(15),
	@RoleId INT, @DepartmentId INT, @IsActive BIT, @ModifiedBy INT
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Update user details
***********************************************************************************************
tmsUserUpdate

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		IF @IsActive = 0
			AND EXISTS (
				SELECT 1 FROM tmsCredential c
				INNER JOIN tmsRole r ON c.roleId = r.roleId
				WHERE c.userId = @UserId AND r.roleName = 'Administrator'
					AND (SELECT COUNT(*) FROM tmsUser u
						 INNER JOIN tmsCredential c2 ON u.userId = c2.userId
						 WHERE c2.roleId = c.roleId AND u.IsActive = 1) <= 1
			)
			THROW 51004, 'Cannot deactivate the last active Administrator.', 1;

		UPDATE tmsUser SET fullName = @FullName, mobileNumber = @MobileNumber,
			departmentId = @DepartmentId, IsActive = @IsActive, modifiedOn = GETDATE(), ModifiedBy = @ModifiedBy
		WHERE userId = @UserId;

		UPDATE tmsCredential SET roleId = @RoleId, ModifiedOn = GETDATE(), ModifiedBy = @ModifiedBy
		WHERE userId = @UserId;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsUserChangeRole
	@UserId INT, @RoleId INT, @ModifiedBy INT
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Change user role
***********************************************************************************************
tmsUserChangeRole

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		DECLARE @CurrentRoleId INT;
		SELECT @CurrentRoleId = c.roleId FROM tmsCredential c WHERE c.userId = @UserId;

		IF @CurrentRoleId IS NULL
			THROW 51001, 'User not found.', 1;

		IF @CurrentRoleId != @RoleId
		BEGIN
			IF @CurrentRoleId = (SELECT roleId FROM tmsRole WHERE roleName = 'Administrator')
				AND @RoleId != (SELECT roleId FROM tmsRole WHERE roleName = 'Administrator')
				AND (SELECT COUNT(*) FROM tmsUser u
					 INNER JOIN tmsCredential c ON u.userId = c.userId
					 WHERE c.roleId = @CurrentRoleId AND u.IsActive = 1) <= 1
				THROW 51002, 'Cannot change the role of the last active Administrator.', 1;

			UPDATE tmsCredential SET roleId = @RoleId, ModifiedOn = GETDATE(), ModifiedBy = @ModifiedBy
			WHERE userId = @UserId;
		END;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsUserToggleStatus
	@UserId INT, @IsActive BIT, @ModifiedBy INT
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Activate or deactivate a user
***********************************************************************************************
tmsUserToggleStatus

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		IF @IsActive = 0
			AND EXISTS (
				SELECT 1 FROM tmsCredential c
				INNER JOIN tmsRole r ON c.roleId = r.roleId
				WHERE c.userId = @UserId AND r.roleName = 'Administrator'
					AND (SELECT COUNT(*) FROM tmsUser u
						 INNER JOIN tmsCredential c2 ON u.userId = c2.userId
						 WHERE c2.roleId = c.roleId AND u.IsActive = 1) <= 1
			)
			THROW 51005, 'Cannot deactivate the last active Administrator.', 1;

		UPDATE tmsUser SET IsActive = @IsActive, modifiedOn = GETDATE(), ModifiedBy = @ModifiedBy
		WHERE userId = @UserId;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsUserSetApproval
	@UserId INT, @IsApproved TINYINT = NULL, @ModifiedBy INT
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Approve or reject self-registered user
***********************************************************************************************
tmsUserSetApproval

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		UPDATE tmsCredential SET isApproved = @IsApproved, ModifiedOn = GETDATE(), ModifiedBy = @ModifiedBy
		WHERE userId = @UserId;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsUserDelete
	@UserId INT, @ModifiedBy INT
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Soft delete a user
***********************************************************************************************
tmsUserDelete

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		IF @UserId = @ModifiedBy
			THROW 51006, 'You cannot delete your own account.', 1;

		IF EXISTS (
			SELECT 1 FROM tmsCredential c
			INNER JOIN tmsRole r ON c.roleId = r.roleId
			WHERE c.userId = @UserId AND r.roleName = 'Administrator'
				AND (SELECT COUNT(*) FROM tmsUser u
					 INNER JOIN tmsCredential c2 ON u.userId = c2.userId
					 WHERE c2.roleId = c.roleId AND u.IsActive = 1) <= 1
		)
			THROW 51007, 'Cannot delete the last active Administrator.', 1;

		UPDATE tmsTicket SET assignedToUserId = NULL, modifiedOn = GETDATE(), ModifiedBy = @ModifiedBy
		WHERE assignedToUserId = @UserId;

		UPDATE tmsCredential SET IsActive = 0, ModifiedOn = GETDATE(), ModifiedBy = @ModifiedBy
		WHERE userId = @UserId;

		UPDATE tmsUser SET IsActive = 0, modifiedOn = GETDATE(), ModifiedBy = @ModifiedBy
		WHERE userId = @UserId;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsUserUpdateProfile
	@UserId INT, @FullName VARCHAR(100), @MobileNumber VARCHAR(15)
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Update own profile
***********************************************************************************************
tmsUserUpdateProfile

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		UPDATE tmsUser SET fullName = @FullName, mobileNumber = @MobileNumber WHERE userId = @UserId;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsUserChangePassword
	@CredentialId INT, @PasswordHash VARCHAR(255)
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Change user password
***********************************************************************************************
tmsUserChangePassword

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		UPDATE tmsCredential SET passwordHash = @PasswordHash WHERE credentialId = @CredentialId;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsUserGetById
	@UserId INT
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Get user by id
***********************************************************************************************
tmsUserGetById

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		SELECT u.userId, u.fullName, u.mobileNumber, u.departmentId, d.departmentName,
			c.credentialId, c.emailId, c.passwordHash, c.roleId, r.roleName, u.IsActive,
			c.isApproved, u.CreatedOn
		FROM tmsUser u
		INNER JOIN tmsCredential c ON u.userId = c.userId
		INNER JOIN tmsRole r ON c.roleId = r.roleId
		INNER JOIN tmsDepartment d ON u.departmentId = d.departmentId
		WHERE u.userId = @UserId;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsUserGetSupportList
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Get support executive users
***********************************************************************************************
tmsUserGetSupportList

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		SELECT u.userId, u.fullName
		FROM tmsUser u
		INNER JOIN tmsCredential c ON u.userId = c.userId
		INNER JOIN tmsRole r ON c.roleId = r.roleId
		WHERE r.roleName IN ('Support Executive', 'Administrator') AND u.IsActive = 1
		ORDER BY u.fullName;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsDashboardGetData
	@UserId INT, @RoleName VARCHAR(50)
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Get dashboard stat cards
2	3Aug2026		Darshan Varade		Fix empty-data stats (ISNULL on status counts)
***********************************************************************************************
tmsDashboardGetData

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		DECLARE @TotalTickets INT, @OpenTickets INT, @InProgressTickets INT,
			@ResolvedTickets INT, @ClosedTickets INT, @MyAssignedTickets INT, @MyCreatedTickets INT;

		SELECT @TotalTickets = COUNT(*),
			@OpenTickets = ISNULL(SUM(CASE WHEN s.statusName = 'New' THEN 1 ELSE 0 END), 0),
			@InProgressTickets = ISNULL(SUM(CASE WHEN s.statusName = 'In Progress' THEN 1 ELSE 0 END), 0),
			@ResolvedTickets = ISNULL(SUM(CASE WHEN s.statusName = 'Resolved' THEN 1 ELSE 0 END), 0),
			@ClosedTickets = ISNULL(SUM(CASE WHEN s.statusName = 'Closed' THEN 1 ELSE 0 END), 0)
		FROM tmsTicket t
		LEFT JOIN tmsStatus s ON t.statusId = s.statusId
		WHERE t.IsActive = 1
			AND (@RoleName = 'Admin'
				OR (@RoleName = 'Support' AND (t.assignedToUserId = @UserId OR t.CreatedBy = @UserId))
				OR (ISNULL(@RoleName, '') NOT IN ('Admin', 'Support') AND t.CreatedBy = @UserId));

		SELECT @MyCreatedTickets = COUNT(*) FROM tmsTicket WHERE IsActive = 1 AND CreatedBy = @UserId;
		SELECT @MyAssignedTickets = COUNT(*) FROM tmsTicket WHERE IsActive = 1 AND assignedToUserId = @UserId;

		SELECT @TotalTickets AS TotalTickets, @OpenTickets AS OpenTickets,
			@InProgressTickets AS InProgressTickets, @ResolvedTickets AS ResolvedTickets,
			@ClosedTickets AS ClosedTickets, @MyAssignedTickets AS MyAssignedTickets,
			@MyCreatedTickets AS MyCreatedTickets;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsDashboardGetStatusChart
	@UserId INT, @RoleName VARCHAR(50)
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Get dashboard status chart data
***********************************************************************************************
tmsDashboardGetStatusChart

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		SELECT s.statusName AS [Label], COUNT(t.ticketId) AS [Value]
		FROM tmsStatus s
		LEFT JOIN tmsTicket t ON s.statusId = t.statusId AND t.IsActive = 1
			AND (@RoleName = 'Admin'
				OR (@RoleName = 'Support' AND (t.assignedToUserId = @UserId OR t.CreatedBy = @UserId))
				OR (ISNULL(@RoleName, '') NOT IN ('Admin', 'Support') AND t.CreatedBy = @UserId))
		GROUP BY s.statusName, s.statusId ORDER BY s.statusId;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsDashboardGetPriorityChart
	@UserId INT, @RoleName VARCHAR(50)
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Get dashboard priority chart data
***********************************************************************************************
tmsDashboardGetPriorityChart

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		SELECT p.priorityName AS [Label], COUNT(t.ticketId) AS [Value]
		FROM tmsPriority p
		LEFT JOIN tmsTicket t ON p.priorityId = t.priorityId AND t.IsActive = 1
			AND (@RoleName = 'Admin'
				OR (@RoleName = 'Support' AND (t.assignedToUserId = @UserId OR t.CreatedBy = @UserId))
				OR (ISNULL(@RoleName, '') NOT IN ('Admin', 'Support') AND t.CreatedBy = @UserId))
		GROUP BY p.priorityName, p.priorityId ORDER BY p.priorityId;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE tmsDashboardGetRecentTickets
	@UserId INT, @RoleName VARCHAR(50), @Count INT = 5
AS
/*
***********************************************************************************************
	Date   			Modified By   		Purpose of Modification
1	31Jul2026		Darshan Varade		Get dashboard recent tickets
***********************************************************************************************
tmsDashboardGetRecentTickets

*/
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		SELECT TOP (@Count) t.ticketId, t.ticketNumber, t.title, s.statusName,
			p.priorityName, u.fullName AS createdByName,
			CASE WHEN @RoleName IN ('Admin', 'Support') THEN assignee.fullName ELSE NULL END AS assignedToName,
			t.CreatedOn
		FROM tmsTicket t
		INNER JOIN tmsStatus s ON t.statusId = s.statusId
		INNER JOIN tmsPriority p ON t.priorityId = p.priorityId
		INNER JOIN tmsUser u ON t.CreatedBy = u.userId
		LEFT JOIN tmsUser assignee ON t.assignedToUserId = assignee.userId
		WHERE t.IsActive = 1
			AND (@RoleName = 'Admin'
				OR (@RoleName = 'Support' AND (t.assignedToUserId = @UserId OR t.CreatedBy = @UserId))
				OR (ISNULL(@RoleName, '') NOT IN ('Admin', 'Support') AND t.CreatedBy = @UserId))
			AND (@RoleName <> 'Admin' OR t.assignedToUserId IS NULL)
		ORDER BY t.CreatedOn DESC;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO
