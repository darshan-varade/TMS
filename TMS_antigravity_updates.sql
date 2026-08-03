-- ====================================================================================
-- TMS Database Updates (Antigravity modifications not present in TMS_SP or TMS_Tables)
-- ====================================================================================

-- ====================================================================================
-- 1. Modified Stored Procedure: tmsTicketGetList
-- Added support for filtering unassigned tickets by passing @AssignedToUserId = -1.
-- ====================================================================================
CREATE OR ALTER PROCEDURE tmsTicketGetList
	@SearchTerm VARCHAR(200) = NULL, 
	@StatusId INT = NULL, 
	@PriorityId INT = NULL,
	@CategoryId INT = NULL, 
	@AssignedToUserId INT = NULL,
	@DateFrom DATETIME = NULL, 
	@DateTo DATETIME = NULL,
	@UserId INT = NULL, 
	@UserRole VARCHAR(50) = NULL,
	@SortColumn VARCHAR(50) = 'CreatedOn', 
	@SortDirection VARCHAR(4) = 'DESC',
	@PageNumber INT = 1, 
	@PageSize INT = 10, 
	@TotalRows INT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

		SELECT @TotalRows = COUNT(*)
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
			-- Modified to handle unassigned check (-1 parameter value filter for NULL assignee)
			AND (@AssignedToUserId IS NULL OR (@AssignedToUserId = -1 AND t.assignedToUserId IS NULL) OR (@AssignedToUserId <> -1 AND t.assignedToUserId = @AssignedToUserId))
			AND (@DateFrom IS NULL OR t.CreatedOn >= @DateFrom)
			AND (@DateTo IS NULL OR t.CreatedOn <= @DateTo)
			AND (@UserRole IN ('Admin', 'Support') OR @UserId IS NULL OR t.CreatedBy = @UserId OR t.assignedToUserId = @UserId);

		SELECT t.ticketId, t.ticketNumber, t.title, t.assignedToUserId, t.CreatedBy AS createdByUserId,
			c.categoryName, p.priorityName, s.statusName,
			u.fullName AS createdByName, assignee.fullName AS assignedToName,
			t.CreatedOn,
			(SELECT COUNT(*) FROM tmsTicketComment tc WHERE tc.ticketId = t.ticketId AND tc.IsActive = 1) AS ConversationCount
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
			-- Modified to handle unassigned check (-1 parameter value filter for NULL assignee)
			AND (@AssignedToUserId IS NULL OR (@AssignedToUserId = -1 AND t.assignedToUserId IS NULL) OR (@AssignedToUserId <> -1 AND t.assignedToUserId = @AssignedToUserId))
			AND (@DateFrom IS NULL OR t.CreatedOn >= @DateFrom)
			AND (@DateTo IS NULL OR t.CreatedOn <= @DateTo)
			AND (@UserRole IN ('Admin', 'Support') OR @UserId IS NULL OR t.CreatedBy = @UserId OR t.assignedToUserId = @UserId)
		ORDER BY
			CASE WHEN @SortColumn = 'TicketNumber' AND @SortDirection = 'ASC' THEN t.ticketNumber END ASC,
			CASE WHEN @SortColumn = 'TicketNumber' AND @SortDirection = 'DESC' THEN t.ticketNumber END DESC,
			CASE WHEN @SortColumn = 'Title' AND @SortDirection = 'ASC' THEN t.title END ASC,
			CASE WHEN @SortColumn = 'Title' AND @SortDirection = 'DESC' THEN t.title END DESC,
			CASE WHEN @SortColumn = 'StatusName' AND @SortDirection = 'ASC' THEN s.statusName END ASC,
			CASE WHEN @SortColumn = 'StatusName' AND @SortDirection = 'DESC' THEN s.statusName END DESC,
			CASE WHEN @SortColumn = 'PriorityName' AND @SortDirection = 'ASC' THEN p.priorityName END ASC,
			CASE WHEN @SortColumn = 'PriorityName' AND @SortDirection = 'DESC' THEN p.priorityName END DESC,
			CASE WHEN @SortColumn != 'CreatedOn' OR @SortDirection = 'ASC' THEN t.CreatedOn END ASC,
			t.CreatedOn DESC
		OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH;
END;
GO
