select * from Registrant
where uid in ('')

declare @duplicateRegistrant as int
set @duplicateRegistrant = 0

select * from FormQuestionAnswer as fqa
where fqa.RegistrantId = @duplicateRegistrant

select * from ActivityPayment as fqa
where fqa.RegistrantId = @duplicateRegistrant
select * from AttendanceRegistrant as fqa
where fqa.RegistrantId = @duplicateRegistrant
select * from ContactListRegistrant as fqa
where fqa.RegistrantId = @duplicateRegistrant
select * from Purchase as fqa
where fqa.RegistrantId = @duplicateRegistrant
select * from RegistrantActivity as fqa
where fqa.RegistrantId = @duplicateRegistrant
select * from RegistrantActivityGroup as fqa
where fqa.RegistrantId = @duplicateRegistrant
select * from RegistrantInviteCode as fqa
where fqa.RegistrantId = @duplicateRegistrant
select * from RegistrantSubscriptionPlan as fqa
where fqa.RegistrantId = @duplicateRegistrant
select * from RegistrantWebUser as fqa
where fqa.RegistrantId = @duplicateRegistrant
select * from Registration as fqa
where fqa.RegistrantId = @duplicateRegistrant
select * from SurveyResponse as fqa
where fqa.RegistrantId = @duplicateRegistrant