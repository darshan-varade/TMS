IF OBJECT_ID(N'dbo.tmsSequence', N'U') IS NOT NULL DROP TABLE dbo.tmsSequence;
IF OBJECT_ID(N'dbo.tmsOtp', N'U') IS NOT NULL DROP TABLE dbo.tmsOtp;
IF OBJECT_ID(N'dbo.tmsNotification', N'U') IS NOT NULL DROP TABLE dbo.tmsNotification;
IF OBJECT_ID(N'dbo.tmsNotificationType', N'U') IS NOT NULL DROP TABLE dbo.tmsNotificationType;
IF OBJECT_ID(N'dbo.tmsTicketActivity', N'U') IS NOT NULL DROP TABLE dbo.tmsTicketActivity;
IF OBJECT_ID(N'dbo.tmsTicketAttachment', N'U') IS NOT NULL DROP TABLE dbo.tmsTicketAttachment;
IF OBJECT_ID(N'dbo.tmsTicketComment', N'U') IS NOT NULL DROP TABLE dbo.tmsTicketComment;
IF OBJECT_ID(N'dbo.tmsTicket', N'U') IS NOT NULL DROP TABLE dbo.tmsTicket;
IF OBJECT_ID(N'dbo.tmsRefreshToken', N'U') IS NOT NULL DROP TABLE dbo.tmsRefreshToken;
IF OBJECT_ID(N'dbo.tmsCredential', N'U') IS NOT NULL DROP TABLE dbo.tmsCredential;
IF OBJECT_ID(N'dbo.tmsUser', N'U') IS NOT NULL DROP TABLE dbo.tmsUser;
IF OBJECT_ID(N'dbo.tmsActivityType', N'U') IS NOT NULL DROP TABLE dbo.tmsActivityType;
IF OBJECT_ID(N'dbo.tmsStatus', N'U') IS NOT NULL DROP TABLE dbo.tmsStatus;
IF OBJECT_ID(N'dbo.tmsSLA', N'U') IS NOT NULL DROP TABLE dbo.tmsSLA;
IF OBJECT_ID(N'dbo.tmsPriority', N'U') IS NOT NULL DROP TABLE dbo.tmsPriority;
IF OBJECT_ID(N'dbo.tmsCategory', N'U') IS NOT NULL DROP TABLE dbo.tmsCategory;
IF OBJECT_ID(N'dbo.tmsDepartment', N'U') IS NOT NULL DROP TABLE dbo.tmsDepartment;
IF OBJECT_ID(N'dbo.tmsRole', N'U') IS NOT NULL DROP TABLE dbo.tmsRole;
GO

CREATE TABLE tmsRole
(
	roleId INT IDENTITY(1,1) PRIMARY KEY,
	roleName VARCHAR(50) NOT NULL UNIQUE,
	roleDescription VARCHAR(200) NULL,

	IsActive BIT DEFAULT 1,
	CreatedOn DATETIME DEFAULT GETDATE(),
	CreatedBy INT NOT NULL,
	ModifiedOn DATETIME DEFAULT NULL,
	ModifiedBy INT DEFAULT NULL
);

INSERT INTO tmsRole
(
	roleName,
	roleDescription,
	IsActive,
	CreatedOn,
	CreatedBy,
	ModifiedOn,
	ModifiedBy
)
VALUES
('Administrator', 'System Administrator', 1, GETDATE(), 1, NULL, NULL),
('Support Executive', 'Handles assigned support tickets', 1, GETDATE(), 1, NULL, NULL),
('Employee', 'Creates and tracks support tickets', 1, GETDATE(), 1, NULL, NULL);

CREATE TABLE tmsDepartment
(
	departmentId INT IDENTITY(1,1) PRIMARY KEY,
	departmentName VARCHAR(100) NOT NULL UNIQUE,

	IsActive BIT DEFAULT 1,
	CreatedOn DATETIME DEFAULT GETDATE(),
	CreatedBy INT NOT NULL,
	ModifiedOn DATETIME DEFAULT NULL,
	ModifiedBy INT DEFAULT NULL
);

INSERT INTO tmsDepartment
(
	departmentName,
	IsActive,
	CreatedOn,
	CreatedBy,
	ModifiedOn,
	ModifiedBy
)
VALUES
('Information Technology', 1, GETDATE(), 1, NULL, NULL),
('Human Resources', 1, GETDATE(), 1, NULL, NULL),
('Finance', 1, GETDATE(), 1, NULL, NULL),
('Sales', 1, GETDATE(), 1, NULL, NULL),
('Marketing', 1, GETDATE(), 1, NULL, NULL),
('Operations', 1, GETDATE(), 1, NULL, NULL),
('Administration', 1, GETDATE(), 1, NULL, NULL),
('Customer Support', 1, GETDATE(), 1, NULL, NULL);

CREATE TABLE tmsCategory
(
	categoryId INT IDENTITY(1,1) PRIMARY KEY,
	categoryName VARCHAR(100) NOT NULL UNIQUE,

	IsActive BIT DEFAULT 1,
	CreatedOn DATETIME DEFAULT GETDATE(),
	CreatedBy INT NOT NULL,
	ModifiedOn DATETIME DEFAULT NULL,
	ModifiedBy INT DEFAULT NULL
);

INSERT INTO tmsCategory
(
	categoryName,
	IsActive,
	CreatedOn,
	CreatedBy,
	ModifiedOn,
	ModifiedBy
)
VALUES
('Hardware', 1, GETDATE(), 1, NULL, NULL),
('Software', 1, GETDATE(), 1, NULL, NULL),
('Network', 1, GETDATE(), 1, NULL, NULL),
('Email', 1, GETDATE(), 1, NULL, NULL),
('Printer', 1, GETDATE(), 1, NULL, NULL),
('Account Access', 1, GETDATE(), 1, NULL, NULL),
('Application Support', 1, GETDATE(), 1, NULL, NULL),
('Other', 1, GETDATE(), 1, NULL, NULL);

CREATE TABLE tmsPriority
(
	priorityId INT IDENTITY(1,1) PRIMARY KEY,
	priorityName VARCHAR(20) NOT NULL UNIQUE,

	IsActive BIT DEFAULT 1,
	CreatedOn DATETIME DEFAULT GETDATE(),
	CreatedBy INT NOT NULL,
	ModifiedOn DATETIME DEFAULT NULL,
	ModifiedBy INT DEFAULT NULL
);

INSERT INTO tmsPriority
(
	priorityName,
	IsActive,
	CreatedOn,
	CreatedBy,
	ModifiedOn,
	ModifiedBy
)
VALUES
('Low', 1, GETDATE(), 1, NULL, NULL),
('Medium', 1, GETDATE(), 1, NULL, NULL),
('High', 1, GETDATE(), 1, NULL, NULL),
('Critical', 1, GETDATE(), 1, NULL, NULL);

CREATE TABLE tmsSLA
(
	slaId INT IDENTITY(1,1) PRIMARY KEY,
	priorityId INT NOT NULL,
	resolutionHours INT NOT NULL,

	IsActive BIT DEFAULT 1,
	CreatedOn DATETIME DEFAULT GETDATE(),
	CreatedBy INT NOT NULL,
	ModifiedOn DATETIME DEFAULT NULL,
	ModifiedBy INT DEFAULT NULL,

	FOREIGN KEY (priorityId) REFERENCES tmsPriority(priorityId)
);

INSERT INTO tmsSLA
(
	priorityId,
	resolutionHours,
	IsActive,
	CreatedOn,
	CreatedBy,
	ModifiedOn,
	ModifiedBy
)
SELECT priorityId, 24, 1, GETDATE(), 1, NULL, NULL
FROM tmsPriority
WHERE priorityName = 'Low';

INSERT INTO tmsSLA
(
	priorityId,
	resolutionHours,
	IsActive,
	CreatedOn,
	CreatedBy,
	ModifiedOn,
	ModifiedBy
)
SELECT priorityId, 8, 1, GETDATE(), 1, NULL, NULL
FROM tmsPriority
WHERE priorityName = 'Medium';

INSERT INTO tmsSLA
(
	priorityId,
	resolutionHours,
	IsActive,
	CreatedOn,
	CreatedBy,
	ModifiedOn,
	ModifiedBy
)
SELECT priorityId, 4, 1, GETDATE(), 1, NULL, NULL
FROM tmsPriority
WHERE priorityName = 'High';

INSERT INTO tmsSLA
(
	priorityId,
	resolutionHours,
	IsActive,
	CreatedOn,
	CreatedBy,
	ModifiedOn,
	ModifiedBy
)
SELECT priorityId, 2, 1, GETDATE(), 1, NULL, NULL
FROM tmsPriority
WHERE priorityName = 'Critical';


CREATE TABLE tmsStatus
(
	statusId INT IDENTITY(1,1) PRIMARY KEY,
	statusName VARCHAR(30) NOT NULL UNIQUE,

	IsActive BIT DEFAULT 1,
	CreatedOn DATETIME DEFAULT GETDATE(),
	CreatedBy INT NOT NULL,
	ModifiedOn DATETIME DEFAULT NULL,
	ModifiedBy INT DEFAULT NULL
);

INSERT INTO tmsStatus
(
	statusName,
	IsActive,
	CreatedOn,
	CreatedBy,
	ModifiedOn,
	ModifiedBy
)
VALUES
('New', 1, GETDATE(), 1, NULL, NULL),
('Assigned', 1, GETDATE(), 1, NULL, NULL),
('In Progress', 1, GETDATE(), 1, NULL, NULL),
('Resolved', 1, GETDATE(), 1, NULL, NULL),
('Reopened', 1, GETDATE(), 1, NULL, NULL),
('Closed', 1, GETDATE(), 1, NULL, NULL);


CREATE TABLE tmsActivityType
(
	activityTypeId INT IDENTITY(1,1) PRIMARY KEY,
	activityTypeName VARCHAR(50) NOT NULL UNIQUE,

	IsActive BIT DEFAULT 1,
	CreatedOn DATETIME DEFAULT GETDATE(),
	CreatedBy INT NOT NULL,
	ModifiedOn DATETIME DEFAULT NULL,
	ModifiedBy INT DEFAULT NULL
);

INSERT INTO tmsActivityType
(
	activityTypeName,
	IsActive,
	CreatedOn,
	CreatedBy,
	ModifiedOn,
	ModifiedBy
)
VALUES
('Ticket Created', 1, GETDATE(), 1, NULL, NULL),
('Ticket Assigned', 1, GETDATE(), 1, NULL, NULL),
('Assignee Changed', 1, GETDATE(), 1, NULL, NULL),
('Status Changed', 1, GETDATE(), 1, NULL, NULL),
('Priority Changed', 1, GETDATE(), 1, NULL, NULL),
('Category Changed', 1, GETDATE(), 1, NULL, NULL),
('Comment Added', 1, GETDATE(), 1, NULL, NULL),
('Attachment Added', 1, GETDATE(), 1, NULL, NULL),
('Ticket Closed', 1, GETDATE(), 1, NULL, NULL);

CREATE TABLE tmsUser
(
	userId INT IDENTITY(1,1) PRIMARY KEY,
	fullName VARCHAR(100) NOT NULL,
	mobileNumber VARCHAR(15) NULL,
	departmentId INT NOT NULL,

	IsActive BIT DEFAULT 1,
	CreatedOn DATETIME DEFAULT GETDATE(),
	CreatedBy INT NOT NULL,
	ModifiedOn DATETIME DEFAULT NULL,
	ModifiedBy INT DEFAULT NULL,

	FOREIGN KEY (departmentId) REFERENCES tmsDepartment(departmentId)
);

CREATE TABLE tmsCredential
(
	credentialId INT IDENTITY(1,1) PRIMARY KEY,
	userId INT NOT NULL UNIQUE,
	emailId VARCHAR(100) NOT NULL UNIQUE,
	passwordHash VARCHAR(255) NOT NULL,
	roleId INT NOT NULL,
	lastLogin DATETIME NULL,
	isApproved TINYINT DEFAULT NULL,

	IsActive BIT DEFAULT 1,
	CreatedOn DATETIME DEFAULT GETDATE(),
	CreatedBy INT NOT NULL,
	ModifiedOn DATETIME DEFAULT NULL,
	ModifiedBy INT DEFAULT NULL,

	FOREIGN KEY (userId) REFERENCES tmsUser(userId),
	FOREIGN KEY (roleId) REFERENCES tmsRole(roleId)
);

DECLARE @AdminUserId INT, @SupportUserId INT, @EmployeeUserId INT;

INSERT INTO tmsUser (fullName, mobileNumber, departmentId, IsActive, CreatedOn, CreatedBy)
VALUES ('System Administrator', '0000000000', 1, 1, GETDATE(), 1);
SET @AdminUserId = SCOPE_IDENTITY();

INSERT INTO tmsUser (fullName, mobileNumber, departmentId, IsActive, CreatedOn, CreatedBy)
VALUES ('Support Executive', '0000000000', 1, 1, GETDATE(), 1);
SET @SupportUserId = SCOPE_IDENTITY();

INSERT INTO tmsUser (fullName, mobileNumber, departmentId, IsActive, CreatedOn, CreatedBy)
VALUES ('Employee', '0000000000', 2, 1, GETDATE(), 1);
SET @EmployeeUserId = SCOPE_IDENTITY();

INSERT INTO tmsCredential (userId, emailId, passwordHash, roleId, isApproved, IsActive, CreatedOn, CreatedBy)
VALUES
(@AdminUserId,   'admin@tms.com',   '$2a$11$1D6x/2GRJ8bfBvxwGp2GxOgzRV2PQzxp/qynDQ6g8xI7UIiLdbyZ.', 1, 1, 1, GETDATE(), 1),
(@SupportUserId, 'support@tms.com', '$2a$11$x3gl5ROO4.dLp0puHSXvoe2QRmn/CbtcR3I9yJ6ka/rhojFrzNClK', 2, 1, 1, GETDATE(), 1),
(@EmployeeUserId,'employee@tms.com','$2a$11$2.kLwOmrxmHFuytqsQIOdeW8fXXuD7VhTqzgIWrQTv2rYbNHJTB4G', 3, 1, 1, GETDATE(), 1);

CREATE TABLE tmsRefreshToken
(
	refreshTokenId INT IDENTITY(1,1) PRIMARY KEY,
	credentialId INT NOT NULL,
	refreshTokenHash VARCHAR(255) NOT NULL,
	expiresAt DATETIME NOT NULL,
	revokedAt DATETIME NULL,
	replacedByTokenHash VARCHAR(255) NULL,

	CreatedOn DATETIME DEFAULT GETDATE(),

	FOREIGN KEY (credentialId) REFERENCES tmsCredential(credentialId)
);

CREATE TABLE tmsTicket
(
	ticketId INT IDENTITY(1,1) PRIMARY KEY,
	ticketNumber VARCHAR(30) NOT NULL UNIQUE,
	title VARCHAR(200) NOT NULL,
	description VARCHAR(MAX) NOT NULL,

	categoryId INT NOT NULL,
	priorityId INT NOT NULL,
	statusId INT NOT NULL,

	assignedToUserId INT NULL,

	dueDate DATETIME NOT NULL,
	resolvedOn DATETIME NULL,

	IsActive BIT DEFAULT 1,
	CreatedOn DATETIME DEFAULT GETDATE(),
	CreatedBy INT NOT NULL,
	ModifiedOn DATETIME DEFAULT NULL,
	ModifiedBy INT DEFAULT NULL,

	FOREIGN KEY (categoryId) REFERENCES tmsCategory(categoryId),
	FOREIGN KEY (priorityId) REFERENCES tmsPriority(priorityId),
	FOREIGN KEY (statusId) REFERENCES tmsStatus(statusId),
	FOREIGN KEY (assignedToUserId) REFERENCES tmsUser(userId)
);

CREATE TABLE tmsTicketComment
(
	commentId INT IDENTITY(1,1) PRIMARY KEY,
	ticketId INT NOT NULL,
	isInternal BIT DEFAULT 0,
	comment VARCHAR(MAX) NOT NULL,

	IsActive BIT DEFAULT 1,
	CreatedOn DATETIME DEFAULT GETDATE(),
	CreatedBy INT NOT NULL,
	ModifiedOn DATETIME DEFAULT NULL,
	ModifiedBy INT DEFAULT NULL,

	FOREIGN KEY (ticketId) REFERENCES tmsTicket(ticketId)
);

CREATE TABLE tmsTicketAttachment
(
	attachmentId INT IDENTITY(1,1) PRIMARY KEY,
	ticketId INT NULL,
	commentId INT NULL,

	storedFileName VARCHAR(255) NOT NULL,
	originalFileName VARCHAR(255) NOT NULL,
	fileExtension VARCHAR(10) NOT NULL,
	contentType VARCHAR(100) NOT NULL,
	fileSize INT NOT NULL,

	IsActive BIT DEFAULT 1,
	CreatedOn DATETIME DEFAULT GETDATE(),
	CreatedBy INT NOT NULL,
	ModifiedOn DATETIME DEFAULT NULL,
	ModifiedBy INT DEFAULT NULL,

	CHECK (ticketId IS NOT NULL OR commentId IS NOT NULL),

	FOREIGN KEY (ticketId) REFERENCES tmsTicket(ticketId),
	FOREIGN KEY (commentId) REFERENCES tmsTicketComment(commentId)
);

CREATE TABLE tmsTicketActivity
(
	activityId INT IDENTITY(1,1) PRIMARY KEY,
	ticketId INT NOT NULL,
	activityTypeId INT NOT NULL,
	remarks VARCHAR(MAX) NULL,
	referenceId INT NULL,

	oldValue VARCHAR(200) NULL,
	newValue VARCHAR(200) NULL,

	IsActive BIT DEFAULT 1,
	CreatedOn DATETIME DEFAULT GETDATE(),
	CreatedBy INT NOT NULL,
	ModifiedOn DATETIME DEFAULT NULL,
	ModifiedBy INT DEFAULT NULL,

	FOREIGN KEY (ticketId) REFERENCES tmsTicket(ticketId),
	FOREIGN KEY (activityTypeId) REFERENCES tmsActivityType(activityTypeId)
);

CREATE TABLE tmsNotificationType
(
	notificationTypeId INT IDENTITY(1,1) PRIMARY KEY,
	notificationTypeName VARCHAR(100) NOT NULL UNIQUE,

	IsActive BIT DEFAULT 1,
	CreatedOn DATETIME DEFAULT GETDATE(),
	CreatedBy INT NOT NULL,
	ModifiedOn DATETIME DEFAULT NULL,
	ModifiedBy INT DEFAULT NULL
);

INSERT INTO tmsNotificationType
(
	notificationTypeName,
	IsActive,
	CreatedOn,
	CreatedBy,
	ModifiedOn,
	ModifiedBy
)
VALUES
('Ticket Created',1,GETDATE(),1,NULL,NULL),
('Ticket Assigned',1,GETDATE(),1,NULL,NULL),
('Ticket Comment Added',1,GETDATE(),1,NULL,NULL),
('Attachment Added',1,GETDATE(),1,NULL,NULL),
('Status Changed',1,GETDATE(),1,NULL,NULL),
('Priority Changed',1,GETDATE(),1,NULL,NULL),
('Ticket Resolved',1,GETDATE(),1,NULL,NULL),
('Ticket Closed',1,GETDATE(),1,NULL,NULL),
('Ticket Reopened',1,GETDATE(),1,NULL,NULL),
('SLA Warning',1,GETDATE(),1,NULL,NULL);

CREATE TABLE tmsNotification
(
	notificationId INT IDENTITY(1,1) PRIMARY KEY,
	userId INT NOT NULL,
	ticketId INT NULL,
	notificationTypeId INT NOT NULL,

	title VARCHAR(200) NOT NULL,
	message VARCHAR(MAX) NOT NULL,

	isRead BIT DEFAULT 0,

	IsActive BIT DEFAULT 1,
	CreatedOn DATETIME DEFAULT GETDATE(),
	CreatedBy INT NOT NULL,
	ModifiedOn DATETIME DEFAULT NULL,
	ModifiedBy INT DEFAULT NULL,

	FOREIGN KEY (userId) REFERENCES tmsUser(userId),
	FOREIGN KEY (ticketId) REFERENCES tmsTicket(ticketId),
	FOREIGN KEY (notificationTypeId) REFERENCES tmsNotificationType(notificationTypeId)
);

CREATE TABLE tmsOtp
(
	otpId INT IDENTITY(1,1) PRIMARY KEY,
	emailId VARCHAR(100) NOT NULL,
	otpCode CHAR(6) NOT NULL,
	expiresOn DATETIME NOT NULL,
	isUsed BIT DEFAULT 0,
	CreatedOn DATETIME DEFAULT GETDATE()
);

CREATE TABLE tmsSequence
(
    sequenceName VARCHAR(50) NOT NULL PRIMARY KEY,
    currValue INT NOT NULL DEFAULT 0,
    monthYear CHAR(6) NOT NULL,

    IsActive BIT DEFAULT 1,
    CreatedOn DATETIME DEFAULT GETDATE(),
    CreatedBy INT NOT NULL,
    ModifiedOn DATETIME DEFAULT NULL,
    ModifiedBy INT DEFAULT NULL
);

INSERT INTO tmsSequence (sequenceName, currValue, monthYear, IsActive, CreatedOn, CreatedBy)
VALUES ('TicketNumber', 0, FORMAT(GETDATE(), 'yyyyMM'), 1, GETDATE(), 1);

select * from tmsPriority