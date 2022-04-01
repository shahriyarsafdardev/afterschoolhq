/****** Script for SelectTopNRows command from SSMS  ******/
SELECT TOP (1000) [ID]
      ,[CreateDate]
      ,[CreatedBy]
      ,[SessionId]
      ,[ActorId]
      ,[AccountId]
      ,[ActionTypeId]
      ,[ObjectTypeId]
      ,[ObjectId]
      ,[IndirectObjectId]
      ,[ActionStringFormat]
      ,[ActionString]
      ,[IndirectObjectTypeId]
  FROM [AfterSchool].[dbo].[WebSessionAction]
  where ActionTypeId = 19
  and ActionString like '%has moved to url%'
  and CreateDate >= '2022-03-28'
  and CreateDate < '2022-04-04'