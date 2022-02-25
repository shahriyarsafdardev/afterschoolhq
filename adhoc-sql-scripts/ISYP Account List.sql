/****** Script for SelectTopNRows command from SSMS  ******/
SELECT TOP (1000) ar.[ID]
      ,[HostAccountId]
      ,[VisitorAccountId]
	  , a.Name
	  , a.Email
	  , wu.FirstName
	  , wu.LastName
	  , wu.Email
	  , wu.IsActive
	  , awu.UserTypeId
  FROM [AfterSchool].[dbo].[AccountRelationship] as ar
  join Account as a on VisitorAccountId = a.ID
  join AccountWebUser as awu on a.ID = awu.AccountId
  join WebUser as wu on awu.WebUserId = wu.ID
  where HostAccountId = 3044
  and awu.IsGhostAccount = 0
  --and awu.UserTypeId = 2