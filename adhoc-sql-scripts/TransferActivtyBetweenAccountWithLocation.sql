--Transaction (BillerAccountId)
--ActivityPayment (AccountId)
--Registration - Should have the AccountId of the school/provider
--Registrant - The Registrant that is linked to the school/Provider Account Id
--Activity table (accountid)
--ActivityMessage
--Ensure Activity.LocationId is set to the location owned by the school so that it shows up on the school dashboard
--Tables to double check (just verify not sure if this directly links to account id)
--RegistrantActivity (no accountId on table)
--Attendance (no accountId on table)

--Purchase (only the purchasing account)
--Don’t worry about
--FormQuestionAnswer - since that has links to the formid, etc and there’s a lot there to try to migrate over.




begin tran tran1;

declare @old_account_id as int
set @old_account_id = 0 -- !!!need to change to school ID!!!

select [Name] from Account
where ID = @old_account_id

declare @new_account_id as int
set @new_account_id = 0 -- !!!need to change to provider ID!!!

select [Name] from Account
where ID = @new_account_id

declare @old_account_location_id as int
select @old_account_location_id = DefaultLocationId
	from Account where ID = @old_account_id

declare @tmp_activity_id as int
set @tmp_activity_id = 0 -- !!!need to change to activity ID!!!

-- Get all transactions from the old account, but only the ones associated with the right activities
declare @tmp_activity_schedule_ids table (id int)
insert @tmp_activity_schedule_ids (id)
	select ActivityScheduleId from Activity 
		where ID = @tmp_activity_id
		and ActivityScheduleId is not null

declare @tmp_activity_message_ids table (id int)
insert @tmp_activity_message_ids (id)
	select ID from ActivityMessage 
		where ActivityId = @tmp_activity_id
		and AccountId = @old_account_id

declare @tmp_registration_ids table (id int)
insert @tmp_registration_ids (id)  
	select RegistrationId from RegistrantActivity 
	where ParticipationObjectId = @tmp_activity_id
	and ParticipationObjectTypeId = 1 


declare @tmp_registrant_ids table (id int)
insert @tmp_registrant_ids (id) 
	 select DISTINCT(RegistrantId) from RegistrantActivity 
		where ParticipationObjectId = @tmp_activity_id
		and ParticipationObjectTypeId = 1

declare @tmp_purchase_ids table (id int)
insert @tmp_purchase_ids (id) 
	 select ID from Purchase 
		where RegistrationId in ( select id from @tmp_registration_ids) 


declare @tmp_transaction_ids table (id int)
insert @tmp_transaction_ids	(id)
  select ID from [Transaction] 
	where PurchaseId in ( select id from @tmp_purchase_ids)  

declare @tmp_activitypayment_ids table (id int)
insert @tmp_activitypayment_ids (id)
	select ID from ActivityPayment where TransactionId in ( select id from @tmp_transaction_ids)

--Transaction (BillerAccountId),  im also adding the PayerAccountId in case there were refunds
select 
	ID as "Transaction_ID", 
	BillerAccountId as "BillerAccountId_Before", 
	PayerAccountId as "PayerAccountId_Before" 
	from [Transaction] 
	where ID in (select id from @tmp_transaction_ids)
	order by ID

-- switch BillerAccountId and PayerAccountId, respectively on Transactions to the new account
update [Transaction] set BillerAccountId = @new_account_id where 
	BillerAccountId = @old_account_id 
	and ID in (select id from @tmp_transaction_ids)
update [Transaction] set PayerAccountId =  @new_account_id where 
	PayerAccountId = @old_account_id 
	and ID in (select id from @tmp_transaction_ids)

select 
	ID as "Transaction_ID", 
	BillerAccountId as "BillerAccountId_After", 
	PayerAccountId as "PayerAccountId_After" 
	from [Transaction] 
	where ID in (select id from @tmp_transaction_ids)
	order by ID

--ActivityPayment (AccountId)
select ID as "ActivityPaymentId", AccountId as "AccountId_Before" from ActivityPayment 
	where ID in ( select id from @tmp_activitypayment_ids)

update ActivityPayment set AccountId = @new_account_id 
	where ID in (select id from @tmp_activitypayment_ids)

select ID as "ActivityPaymentId", AccountId as "AccountId_After" from ActivityPayment 
	where ID in ( select id from @tmp_activitypayment_ids)

--Registration - Should have the AccountId of the school/provider
select ID as "RegistrationId", AccountId as "AccountId_Before" from Registration 
	where ID in  ( select id from @tmp_registration_ids )

update Registration set AccountId = @new_account_id 
	where ID in ( select id from @tmp_registration_ids )

select ID as "RegistrationId", AccountId as "AccountId_After" from Registration 
	where ID in  ( select id from @tmp_registration_ids )

--Registrant - The Registrant that is linked to the school/Provider Account Id
select ID as "RegistrantId", AccountId as "AccountId_Before" from Registrant 
	where ID in  ( select id from @tmp_registrant_ids )

update Registrant set AccountId = @new_account_id where 
	ID in ( select id from @tmp_registrant_ids )
	and AccountId = @old_account_id

select ID as "RegistrantId", AccountId as "AccountId_After" from Registrant 
	where ID in  ( select id from @tmp_registrant_ids )

--Activity table (accountid)
select ID as "ActivityId", AccountId as "AccountId_Before", LocationId as "LocationId_Before" from Activity 
	where ID = @tmp_activity_id

update Activity set AccountId = @new_account_id, 
	LocationId = @old_account_location_id,
	ActivityLocationTypeId = 1
	where ID = @tmp_activity_id

select ID as "ActivityId", AccountId as "AccountId_After", LocationId as "LocationId_After" from Activity 
	where ID = @tmp_activity_id

--Update Location HostAccountId with activity location
select ID as "LocationId", HostAccountId as "HostAccountId_Before" from Location
	where ID = @old_account_location_id

update Location
	set HostAccountId = @old_account_id
	where ID = @old_account_location_id


select ID as "LocationId", HostAccountId as "HostAccountId_After" from Location
	where ID = @old_account_location_id

 --activity Schedule ( accountid)
select ID as "ActivityScheduleId", AccountId as "AccountId_Before" from ActivitySchedule 
	where ID in ( select id from @tmp_activity_schedule_ids )

update ActivitySchedule set AccountId = @new_account_id 
	where ID in ( select id from @tmp_activity_schedule_ids )

select ID as "ActivityScheduleId", AccountId as "AccountId_After" from ActivitySchedule 
	where ID in ( select id from @tmp_activity_schedule_ids )


 --activityMessages
 select ID as "ActivityMessageId", AccountId as "AccountId_Before" from ActivityMessage 
	where ID in ( select id from @tmp_activity_message_ids )

update ActivityMessage set AccountId = @new_account_id 
	where ID in ( select id from @tmp_activity_message_ids )

 select ID as "ActivityMessageId", AccountId as "AccountId_After" from ActivityMessage 
	where ID in ( select id from @tmp_activity_message_ids )

rollback;
--commit;