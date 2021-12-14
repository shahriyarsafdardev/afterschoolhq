using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Data;
using System.Collections;
using System.Configuration;

namespace AfterSchoolLibrary.Models
{
    public class Database
    {
		#region "ClassGenerator"
		public static DatabaseCredentials ProductionCredentials { get { return new DatabaseCredentials(ConfigurationManager.AppSettings["DB_SERVER"], ConfigurationManager.AppSettings["DB_NAME"], ConfigurationManager.AppSettings["DB_USER"], ConfigurationManager.AppSettings["DB_PASSWORD"]); } }
		public static DatabaseCredentials LocalCredentials { get { return new DatabaseCredentials("localhost", "AfterSchool", "AfterSchoolUser", "Nj8Ed$wpLfJ"); } }
		public static DatabaseCredentials CICredentials { get { return new DatabaseCredentials(ConfigurationManager.AppSettings["DB_SERVER"], ConfigurationManager.AppSettings["DB_NAME"], ConfigurationManager.AppSettings["DB_USER"], ConfigurationManager.AppSettings["DB_PASSWORD"]); } }
		public static DatabaseCredentials QACredentials { get { return new DatabaseCredentials(ConfigurationManager.AppSettings["DB_SERVER"], ConfigurationManager.AppSettings["DB_NAME"], ConfigurationManager.AppSettings["DB_USER"], ConfigurationManager.AppSettings["DB_PASSWORD"]); } }
		public static DatabaseCredentials TestCredentials { get { return new DatabaseCredentials(ConfigurationManager.AppSettings["DB_SERVER"], ConfigurationManager.AppSettings["DB_NAME"], ConfigurationManager.AppSettings["DB_USER"], ConfigurationManager.AppSettings["DB_PASSWORD"]); } }
		public static DatabaseCredentials StagingCredentials { get { return new DatabaseCredentials(ConfigurationManager.AppSettings["DB_SERVER"], ConfigurationManager.AppSettings["DB_NAME"], ConfigurationManager.AppSettings["DB_USER"], ConfigurationManager.AppSettings["DB_PASSWORD"]); } }
		public static DatabaseCredentials LocalTestCredentials { get { return new DatabaseCredentials("localhost", "AfterSchoolTest", "AfterSchoolUser", "Nj8Ed$wpLfJ"); } }
		public static DatabaseCredentials Credentials { get { if(Helpers.Utilities.IsProduction){return ProductionCredentials;} else if(Helpers.Utilities.IsCI){return CICredentials;} else if(Helpers.Utilities.IsStaging){return StagingCredentials;} else if(Helpers.Utilities.IsQA){return QACredentials;} else{return LocalCredentials;} } }
		

		public static SystemMessages CreateTables(DatabaseConnection dc = null)
		{
			SystemMessages msgs = new SystemMessages();

			WebUser.Create(dc);
			Activity.Create(dc);
			Location.Create(dc);
			ActivityDay.Create(dc);
			Tag.Create(dc);
			ObjectTag.Create(dc);
			ActivitySchedule.Create(dc);
			ActivityInstance.Create(dc);
			ActivityInstanceSeats.Create(dc);
			Match.Create(dc);
			ActivitySection.Create(dc);
			ActivityGradeLevel.Create(dc);
			ActivityMessage.Create(dc);
			ActivityMessageRegistrant.Create(dc);
			Account.Create(dc);
			AccountProfile.Create(dc);
			CheckPaymentAccount.Create(dc);
			BankPaymentAccount.Create(dc);
			StripeAccount.Create(dc);
			AccountRelationship.Create(dc);
			AccountRelationshipLocation.Create(dc);
			AccountProperty.Create(dc);
			AccountWebUser.Create(dc);
			GradeLevel.Create(dc);
			RegistrationPeriod.Create(dc);
			ActivityRegistrationPeriod.Create(dc);
			ActivityGroup.Create(dc);
			AccountActivityGroup.Create(dc);
			Registrant.Create(dc);
			RegistrantWebUser.Create(dc);
			ContactList.Create(dc);
			ContactListRegistrant.Create(dc);
			ActivityPayment.Create(dc);
			RegistrantActivityGroup.Create(dc);
			RegistrantActivity.Create(dc);
			ActivityForm.Create(dc);
			Form.Create(dc);
			FormQuestion.Create(dc);
			FormQuestionGroup.Create(dc);
			FormQuestionAnswer.Create(dc);
			Registration.Create(dc);
			ActivityWebUser.Create(dc);
			Attendance.Create(dc);
			AttendanceRegistrant.Create(dc);
			Package.Create(dc);
			AccountPackage.Create(dc);
			PackageFeature.Create(dc);
			AccountPackageFeature.Create(dc);
			File.Create(dc);
			ActivityFile.Create(dc);
			Session.Create(dc);
			SessionAction.Create(dc);
			Cart.Create(dc);
			CartItem.Create(dc);
			PasswordResetToken.Create(dc);
			SubscriptionPlan.Create(dc);
			SubscriptionPlanPrePayment.Create(dc);
			RegistrantSubscriptionPlan.Create(dc);
			Transaction.Create(dc);
			Payout.Create(dc);
			Product.Create(dc);
			ProductOffering.Create(dc);
			ProductPurchase.Create(dc);
			Coupon.Create(dc);
			AttachedCoupon.Create(dc);
			PurchaseableObjectCoupon.Create(dc);
			Purchase.Create(dc);
			TwilioSMS.Create(dc);
			TwilioBlockedPhone.Create(dc);
			Migration.Create(dc);
			RefundRequest.Create(dc);
			Subscriber.Create(dc);
			WebSession.Create(dc);
			WebSessionAction.Create(dc);
			WebSessionObjectSnapshot.Create(dc);
			SurveyRequest.Create(dc);
			SurveyResponse.Create(dc);
			RegistrantInviteCode.Create(dc);
			RefreshToken.Create(dc);
			

			return msgs;
		}

		public static SystemMessages AlterTables(DatabaseConnection dc = null)
		{
			SystemMessages msgs = new SystemMessages();

			WebUser.Alter(dc);
			Activity.Alter(dc);
			Location.Alter(dc);
			ActivityDay.Alter(dc);
			Tag.Alter(dc);
			ObjectTag.Alter(dc);
			ActivitySchedule.Alter(dc);
			ActivityInstance.Alter(dc);
			ActivityInstanceSeats.Alter(dc);
			Match.Alter(dc);
			ActivitySection.Alter(dc);
			ActivityGradeLevel.Alter(dc);
			ActivityMessage.Alter(dc);
			ActivityMessageRegistrant.Alter(dc);
			Account.Alter(dc);
			AccountProfile.Alter(dc);
			CheckPaymentAccount.Alter(dc);
			BankPaymentAccount.Alter(dc);
			StripeAccount.Alter(dc);
			AccountRelationship.Alter(dc);
			AccountRelationshipLocation.Alter(dc);
			AccountProperty.Alter(dc);
			AccountWebUser.Alter(dc);
			GradeLevel.Alter(dc);
			RegistrationPeriod.Alter(dc);
			ActivityRegistrationPeriod.Alter(dc);
			ActivityGroup.Alter(dc);
			AccountActivityGroup.Alter(dc);
			Registrant.Alter(dc);
			RegistrantWebUser.Alter(dc);
			ContactList.Alter(dc);
			ContactListRegistrant.Alter(dc);
			ActivityPayment.Alter(dc);
			RegistrantActivityGroup.Alter(dc);
			RegistrantActivity.Alter(dc);
			ActivityForm.Alter(dc);
			Form.Alter(dc);
			FormQuestion.Alter(dc);
			FormQuestionGroup.Alter(dc);
			FormQuestionAnswer.Alter(dc);
			Registration.Alter(dc);
			ActivityWebUser.Alter(dc);
			Attendance.Alter(dc);
			AttendanceRegistrant.Alter(dc);
			Package.Alter(dc);
			AccountPackage.Alter(dc);
			PackageFeature.Alter(dc);
			AccountPackageFeature.Alter(dc);
			File.Alter(dc);
			ActivityFile.Alter(dc);
			Session.Alter(dc);
			SessionAction.Alter(dc);
			Cart.Alter(dc);
			CartItem.Alter(dc);
			PasswordResetToken.Alter(dc);
			SubscriptionPlan.Alter(dc);
			SubscriptionPlanPrePayment.Alter(dc);
			RegistrantSubscriptionPlan.Alter(dc);
			Transaction.Alter(dc);
			Payout.Alter(dc);
			Product.Alter(dc);
			ProductOffering.Alter(dc);
			ProductPurchase.Alter(dc);
			Coupon.Alter(dc);
			AttachedCoupon.Alter(dc);
			PurchaseableObjectCoupon.Alter(dc);
			Purchase.Alter(dc);
			TwilioSMS.Alter(dc);
			TwilioBlockedPhone.Alter(dc);
			Migration.Alter(dc);
			RefundRequest.Alter(dc);
			Subscriber.Alter(dc);
			WebSession.Alter(dc);
			WebSessionAction.Alter(dc);
			WebSessionObjectSnapshot.Alter(dc);
			SurveyRequest.Alter(dc);
			SurveyResponse.Alter(dc);
			RegistrantInviteCode.Alter(dc);
			RefreshToken.Alter(dc);
			

			return msgs;
		}
		
		public static SystemMessages UpdateData(DatabaseConnection dc = null)
		{
			SystemMessages msgs = new SystemMessages();

			//**UPDATES**//
			WebUser.Update(new Hashtable(){{"IsActive",false}}, new Hashtable(){{"IsActive", DBNull.Value}}, WebUser.ObjectMap, dc);
			WebUser.Update(new Hashtable(){{"VerifiedAccount",false}}, new Hashtable(){{"VerifiedAccount", DBNull.Value}}, WebUser.ObjectMap, dc);
			WebUser.Update(new Hashtable(){{"IsRegistered",false}}, new Hashtable(){{"IsRegistered", DBNull.Value}}, WebUser.ObjectMap, dc);
			WebUser.Update(new Hashtable(){{"AllowTextMessages",false}}, new Hashtable(){{"AllowTextMessages", DBNull.Value}}, WebUser.ObjectMap, dc);
			Activity.Update(new Hashtable(){{"AllowToPayLater",false}}, new Hashtable(){{"AllowToPayLater", DBNull.Value}}, Activity.ObjectMap, dc);
			Activity.Update(new Hashtable(){{"GenderRestriction",0}}, new Hashtable(){{"GenderRestriction", DBNull.Value}}, Activity.ObjectMap, dc);
			Activity.Update(new Hashtable(){{"RegistrationModeId",0}}, new Hashtable(){{"RegistrationModeId", DBNull.Value}}, Activity.ObjectMap, dc);
			Activity.Update(new Hashtable(){{"IsDeleted",false}}, new Hashtable(){{"IsDeleted", DBNull.Value}}, Activity.ObjectMap, dc);
			Activity.Update(new Hashtable(){{"IsPublished",false}}, new Hashtable(){{"IsPublished", DBNull.Value}}, Activity.ObjectMap, dc);
			Activity.Update(new Hashtable(){{"IsArchived",false}}, new Hashtable(){{"IsArchived", DBNull.Value}}, Activity.ObjectMap, dc);
			Activity.Update(new Hashtable(){{"IsInviteOnly",false}}, new Hashtable(){{"IsInviteOnly", DBNull.Value}}, Activity.ObjectMap, dc);
			Activity.Update(new Hashtable(){{"ActivityTypeId",0}}, new Hashtable(){{"ActivityTypeId", DBNull.Value}}, Activity.ObjectMap, dc);
			Activity.Update(new Hashtable(){{"LeaderTypeId",0}}, new Hashtable(){{"LeaderTypeId", DBNull.Value}}, Activity.ObjectMap, dc);
			Activity.Update(new Hashtable(){{"RequiresParentalConsent",false}}, new Hashtable(){{"RequiresParentalConsent", DBNull.Value}}, Activity.ObjectMap, dc);
			Activity.Update(new Hashtable(){{"ActivityLocationTypeId",0}}, new Hashtable(){{"ActivityLocationTypeId", DBNull.Value}}, Activity.ObjectMap, dc);
			Activity.Update(new Hashtable(){{"HidePreciseLocation",false}}, new Hashtable(){{"HidePreciseLocation", DBNull.Value}}, Activity.ObjectMap, dc);
			Activity.Update(new Hashtable(){{"RegistrationFinalized",false}}, new Hashtable(){{"RegistrationFinalized", DBNull.Value}}, Activity.ObjectMap, dc);
			Activity.Update(new Hashtable(){{"IsConvenienceFeeTakenOutOfCost",false}}, new Hashtable(){{"IsConvenienceFeeTakenOutOfCost", DBNull.Value}}, Activity.ObjectMap, dc);
			Activity.Update(new Hashtable(){{"CostFrequencyTypeId",0}}, new Hashtable(){{"CostFrequencyTypeId", DBNull.Value}}, Activity.ObjectMap, dc);
			Location.Update(new Hashtable(){{"IsDefault",false}}, new Hashtable(){{"IsDefault", DBNull.Value}}, Location.ObjectMap, dc);
			Location.Update(new Hashtable(){{"LocationTypeId",0}}, new Hashtable(){{"LocationTypeId", DBNull.Value}}, Location.ObjectMap, dc);
			Location.Update(new Hashtable(){{"IsTest",false}}, new Hashtable(){{"IsTest", DBNull.Value}}, Location.ObjectMap, dc);
			ActivityDay.Update(new Hashtable(){{"Day",0}}, new Hashtable(){{"Day", DBNull.Value}}, ActivityDay.ObjectMap, dc);
			ObjectTag.Update(new Hashtable(){{"ParticipationObjectTypeId",0}}, new Hashtable(){{"ParticipationObjectTypeId", DBNull.Value}}, ObjectTag.ObjectMap, dc);
			ActivitySchedule.Update(new Hashtable(){{"RepeatsWeekly",false}}, new Hashtable(){{"RepeatsWeekly", DBNull.Value}}, ActivitySchedule.ObjectMap, dc);
			ActivitySchedule.Update(new Hashtable(){{"IsOngoing",false}}, new Hashtable(){{"IsOngoing", DBNull.Value}}, ActivitySchedule.ObjectMap, dc);
			ActivityInstance.Update(new Hashtable(){{"Day",0}}, new Hashtable(){{"Day", DBNull.Value}}, ActivityInstance.ObjectMap, dc);
			ActivityInstance.Update(new Hashtable(){{"InstanceTypeId",0}}, new Hashtable(){{"InstanceTypeId", DBNull.Value}}, ActivityInstance.ObjectMap, dc);
			ActivityMessage.Update(new Hashtable(){{"SendToStaff",false}}, new Hashtable(){{"SendToStaff", DBNull.Value}}, ActivityMessage.ObjectMap, dc);
			ActivityMessageRegistrant.Update(new Hashtable(){{"SentText",false}}, new Hashtable(){{"SentText", DBNull.Value}}, ActivityMessageRegistrant.ObjectMap, dc);
			ActivityMessageRegistrant.Update(new Hashtable(){{"SentEmail",false}}, new Hashtable(){{"SentEmail", DBNull.Value}}, ActivityMessageRegistrant.ObjectMap, dc);
			Account.Update(new Hashtable(){{"IsAdminAccount",false}}, new Hashtable(){{"IsAdminAccount", DBNull.Value}}, Account.ObjectMap, dc);
			Account.Update(new Hashtable(){{"IsActive",false}}, new Hashtable(){{"IsActive", DBNull.Value}}, Account.ObjectMap, dc);
			Account.Update(new Hashtable(){{"IsAfterSchool",false}}, new Hashtable(){{"IsAfterSchool", DBNull.Value}}, Account.ObjectMap, dc);
			Account.Update(new Hashtable(){{"AccountTypeId",0}}, new Hashtable(){{"AccountTypeId", DBNull.Value}}, Account.ObjectMap, dc);
			Account.Update(new Hashtable(){{"CreditCardFeesPaidBy",0}}, new Hashtable(){{"CreditCardFeesPaidBy", DBNull.Value}}, Account.ObjectMap, dc);
			Account.Update(new Hashtable(){{"ConvenienceFeesPaidBy",0}}, new Hashtable(){{"ConvenienceFeesPaidBy", DBNull.Value}}, Account.ObjectMap, dc);
			Account.Update(new Hashtable(){{"StripeChargesEnabled",false}}, new Hashtable(){{"StripeChargesEnabled", DBNull.Value}}, Account.ObjectMap, dc);
			Account.Update(new Hashtable(){{"StripePayoutsEnabled",false}}, new Hashtable(){{"StripePayoutsEnabled", DBNull.Value}}, Account.ObjectMap, dc);
			Account.Update(new Hashtable(){{"IsTest",false}}, new Hashtable(){{"IsTest", DBNull.Value}}, Account.ObjectMap, dc);
			Account.Update(new Hashtable(){{"IsDeleted",false}}, new Hashtable(){{"IsDeleted", DBNull.Value}}, Account.ObjectMap, dc);
			Account.Update(new Hashtable(){{"IsClaimed",false}}, new Hashtable(){{"IsClaimed", DBNull.Value}}, Account.ObjectMap, dc);
			Account.Update(new Hashtable(){{"OnboardingStepDone",0}}, new Hashtable(){{"OnboardingStepDone", DBNull.Value}}, Account.ObjectMap, dc);
			AccountProfile.Update(new Hashtable(){{"GenderRestriction",0}}, new Hashtable(){{"GenderRestriction", DBNull.Value}}, AccountProfile.ObjectMap, dc);
			AccountProfile.Update(new Hashtable(){{"IsOpenDuringCovid",false}}, new Hashtable(){{"IsOpenDuringCovid", DBNull.Value}}, AccountProfile.ObjectMap, dc);
			AccountProfile.Update(new Hashtable(){{"IsELearningCenter",false}}, new Hashtable(){{"IsELearningCenter", DBNull.Value}}, AccountProfile.ObjectMap, dc);
			AccountProfile.Update(new Hashtable(){{"IsAHQApproved",false}}, new Hashtable(){{"IsAHQApproved", DBNull.Value}}, AccountProfile.ObjectMap, dc);
			AccountProfile.Update(new Hashtable(){{"IsVirtualProgram",false}}, new Hashtable(){{"IsVirtualProgram", DBNull.Value}}, AccountProfile.ObjectMap, dc);
			CheckPaymentAccount.Update(new Hashtable(){{"IsVerified",false}}, new Hashtable(){{"IsVerified", DBNull.Value}}, CheckPaymentAccount.ObjectMap, dc);
			CheckPaymentAccount.Update(new Hashtable(){{"LegalEntityTypeId",0}}, new Hashtable(){{"LegalEntityTypeId", DBNull.Value}}, CheckPaymentAccount.ObjectMap, dc);
			BankPaymentAccount.Update(new Hashtable(){{"IsVerified",false}}, new Hashtable(){{"IsVerified", DBNull.Value}}, BankPaymentAccount.ObjectMap, dc);
			BankPaymentAccount.Update(new Hashtable(){{"BankAccountTypeId",0}}, new Hashtable(){{"BankAccountTypeId", DBNull.Value}}, BankPaymentAccount.ObjectMap, dc);
			StripeAccount.Update(new Hashtable(){{"LegalEntityTypeId",0}}, new Hashtable(){{"LegalEntityTypeId", DBNull.Value}}, StripeAccount.ObjectMap, dc);
			StripeAccount.Update(new Hashtable(){{"RequiresFullSSN",false}}, new Hashtable(){{"RequiresFullSSN", DBNull.Value}}, StripeAccount.ObjectMap, dc);
			StripeAccount.Update(new Hashtable(){{"VerificationStatusId",0}}, new Hashtable(){{"VerificationStatusId", DBNull.Value}}, StripeAccount.ObjectMap, dc);
			StripeAccount.Update(new Hashtable(){{"ChargesEnabled",false}}, new Hashtable(){{"ChargesEnabled", DBNull.Value}}, StripeAccount.ObjectMap, dc);
			StripeAccount.Update(new Hashtable(){{"PayoutsEnabled",false}}, new Hashtable(){{"PayoutsEnabled", DBNull.Value}}, StripeAccount.ObjectMap, dc);
			StripeAccount.Update(new Hashtable(){{"RequiresDocument",false}}, new Hashtable(){{"RequiresDocument", DBNull.Value}}, StripeAccount.ObjectMap, dc);
			StripeAccount.Update(new Hashtable(){{"RequiresAdditionalOwners",false}}, new Hashtable(){{"RequiresAdditionalOwners", DBNull.Value}}, StripeAccount.ObjectMap, dc);
			StripeAccount.Update(new Hashtable(){{"FailedDetailsCode",0}}, new Hashtable(){{"FailedDetailsCode", DBNull.Value}}, StripeAccount.ObjectMap, dc);
			AccountRelationship.Update(new Hashtable(){{"HostPermissionTypeId",0}}, new Hashtable(){{"HostPermissionTypeId", DBNull.Value}}, AccountRelationship.ObjectMap, dc);
			AccountRelationship.Update(new Hashtable(){{"ApprovalStatusId",0}}, new Hashtable(){{"ApprovalStatusId", DBNull.Value}}, AccountRelationship.ObjectMap, dc);
			AccountRelationship.Update(new Hashtable(){{"RequiresPublishPermission",false}}, new Hashtable(){{"RequiresPublishPermission", DBNull.Value}}, AccountRelationship.ObjectMap, dc);
			AccountRelationship.Update(new Hashtable(){{"ShowAllPrograms",false}}, new Hashtable(){{"ShowAllPrograms", DBNull.Value}}, AccountRelationship.ObjectMap, dc);
			AccountProperty.Update(new Hashtable(){{"AccountPropertyTypeId",0}}, new Hashtable(){{"AccountPropertyTypeId", DBNull.Value}}, AccountProperty.ObjectMap, dc);
			AccountWebUser.Update(new Hashtable(){{"IsActive",false}}, new Hashtable(){{"IsActive", DBNull.Value}}, AccountWebUser.ObjectMap, dc);
			AccountWebUser.Update(new Hashtable(){{"IsDefault",false}}, new Hashtable(){{"IsDefault", DBNull.Value}}, AccountWebUser.ObjectMap, dc);
			AccountWebUser.Update(new Hashtable(){{"UserTypeId",0}}, new Hashtable(){{"UserTypeId", DBNull.Value}}, AccountWebUser.ObjectMap, dc);
			AccountWebUser.Update(new Hashtable(){{"IsDeleted",false}}, new Hashtable(){{"IsDeleted", DBNull.Value}}, AccountWebUser.ObjectMap, dc);
			AccountWebUser.Update(new Hashtable(){{"IsGhostAccount",false}}, new Hashtable(){{"IsGhostAccount", DBNull.Value}}, AccountWebUser.ObjectMap, dc);
			RegistrationPeriod.Update(new Hashtable(){{"RegistrationModeId",0}}, new Hashtable(){{"RegistrationModeId", DBNull.Value}}, RegistrationPeriod.ObjectMap, dc);
			ActivityRegistrationPeriod.Update(new Hashtable(){{"IsActive",false}}, new Hashtable(){{"IsActive", DBNull.Value}}, ActivityRegistrationPeriod.ObjectMap, dc);
			ActivityGroup.Update(new Hashtable(){{"RegistrationModeId",0}}, new Hashtable(){{"RegistrationModeId", DBNull.Value}}, ActivityGroup.ObjectMap, dc);
			ActivityGroup.Update(new Hashtable(){{"IsPublished",false}}, new Hashtable(){{"IsPublished", DBNull.Value}}, ActivityGroup.ObjectMap, dc);
			ActivityGroup.Update(new Hashtable(){{"IsArchived",false}}, new Hashtable(){{"IsArchived", DBNull.Value}}, ActivityGroup.ObjectMap, dc);
			ActivityGroup.Update(new Hashtable(){{"IsInviteOnly",false}}, new Hashtable(){{"IsInviteOnly", DBNull.Value}}, ActivityGroup.ObjectMap, dc);
			ActivityGroup.Update(new Hashtable(){{"HasLotteryRun",false}}, new Hashtable(){{"HasLotteryRun", DBNull.Value}}, ActivityGroup.ObjectMap, dc);
			ActivityGroup.Update(new Hashtable(){{"ActivityGroupTypeId",0}}, new Hashtable(){{"ActivityGroupTypeId", DBNull.Value}}, ActivityGroup.ObjectMap, dc);
			ActivityGroup.Update(new Hashtable(){{"GenderRestriction",0}}, new Hashtable(){{"GenderRestriction", DBNull.Value}}, ActivityGroup.ObjectMap, dc);
			ActivityGroup.Update(new Hashtable(){{"CostUnitTypeId",0}}, new Hashtable(){{"CostUnitTypeId", DBNull.Value}}, ActivityGroup.ObjectMap, dc);
			ActivityGroup.Update(new Hashtable(){{"RequiresParentalConsent",false}}, new Hashtable(){{"RequiresParentalConsent", DBNull.Value}}, ActivityGroup.ObjectMap, dc);
			ActivityGroup.Update(new Hashtable(){{"AllowToPayLater",false}}, new Hashtable(){{"AllowToPayLater", DBNull.Value}}, ActivityGroup.ObjectMap, dc);
			ActivityGroup.Update(new Hashtable(){{"IsDeleted",false}}, new Hashtable(){{"IsDeleted", DBNull.Value}}, ActivityGroup.ObjectMap, dc);
			AccountActivityGroup.Update(new Hashtable(){{"InviteStatusId",0}}, new Hashtable(){{"InviteStatusId", DBNull.Value}}, AccountActivityGroup.ObjectMap, dc);
			AccountActivityGroup.Update(new Hashtable(){{"IsPublished",false}}, new Hashtable(){{"IsPublished", DBNull.Value}}, AccountActivityGroup.ObjectMap, dc);
			AccountActivityGroup.Update(new Hashtable(){{"CostUnitTypeId",0}}, new Hashtable(){{"CostUnitTypeId", DBNull.Value}}, AccountActivityGroup.ObjectMap, dc);
			Registrant.Update(new Hashtable(){{"RegistrantTypeId",0}}, new Hashtable(){{"RegistrantTypeId", DBNull.Value}}, Registrant.ObjectMap, dc);
			Registrant.Update(new Hashtable(){{"IsMaster",false}}, new Hashtable(){{"IsMaster", DBNull.Value}}, Registrant.ObjectMap, dc);
			Registrant.Update(new Hashtable(){{"IsContactOnly",false}}, new Hashtable(){{"IsContactOnly", DBNull.Value}}, Registrant.ObjectMap, dc);
			Registrant.Update(new Hashtable(){{"IsParentOrGuardian",false}}, new Hashtable(){{"IsParentOrGuardian", DBNull.Value}}, Registrant.ObjectMap, dc);
			Registrant.Update(new Hashtable(){{"AllowTextMessages",false}}, new Hashtable(){{"AllowTextMessages", DBNull.Value}}, Registrant.ObjectMap, dc);
			Registrant.Update(new Hashtable(){{"ProfileStatusId",0}}, new Hashtable(){{"ProfileStatusId", DBNull.Value}}, Registrant.ObjectMap, dc);
			Registrant.Update(new Hashtable(){{"IsDeleted",false}}, new Hashtable(){{"IsDeleted", DBNull.Value}}, Registrant.ObjectMap, dc);
			RegistrantWebUser.Update(new Hashtable(){{"IsDeleted",false}}, new Hashtable(){{"IsDeleted", DBNull.Value}}, RegistrantWebUser.ObjectMap, dc);
			ActivityPayment.Update(new Hashtable(){{"IsProRated",false}}, new Hashtable(){{"IsProRated", DBNull.Value}}, ActivityPayment.ObjectMap, dc);
			ActivityPayment.Update(new Hashtable(){{"ParticipationObjectTypeId",0}}, new Hashtable(){{"ParticipationObjectTypeId", DBNull.Value}}, ActivityPayment.ObjectMap, dc);
			ActivityPayment.Update(new Hashtable(){{"IsDeleted",false}}, new Hashtable(){{"IsDeleted", DBNull.Value}}, ActivityPayment.ObjectMap, dc);
			RegistrantActivity.Update(new Hashtable(){{"ParticipationObjectTypeId",0}}, new Hashtable(){{"ParticipationObjectTypeId", DBNull.Value}}, RegistrantActivity.ObjectMap, dc);
			RegistrantActivity.Update(new Hashtable(){{"AssignmentTypeId",0}}, new Hashtable(){{"AssignmentTypeId", DBNull.Value}}, RegistrantActivity.ObjectMap, dc);
			ActivityForm.Update(new Hashtable(){{"IsActive",false}}, new Hashtable(){{"IsActive", DBNull.Value}}, ActivityForm.ObjectMap, dc);
			ActivityForm.Update(new Hashtable(){{"IsPublished",false}}, new Hashtable(){{"IsPublished", DBNull.Value}}, ActivityForm.ObjectMap, dc);
			Form.Update(new Hashtable(){{"IsActive",false}}, new Hashtable(){{"IsActive", DBNull.Value}}, Form.ObjectMap, dc);
			Form.Update(new Hashtable(){{"FormTypeId",0}}, new Hashtable(){{"FormTypeId", DBNull.Value}}, Form.ObjectMap, dc);
			FormQuestion.Update(new Hashtable(){{"IsTemplate",false}}, new Hashtable(){{"IsTemplate", DBNull.Value}}, FormQuestion.ObjectMap, dc);
			FormQuestion.Update(new Hashtable(){{"QuestionTypeId",0}}, new Hashtable(){{"QuestionTypeId", DBNull.Value}}, FormQuestion.ObjectMap, dc);
			FormQuestion.Update(new Hashtable(){{"QuestionDataTypeId",0}}, new Hashtable(){{"QuestionDataTypeId", DBNull.Value}}, FormQuestion.ObjectMap, dc);
			FormQuestion.Update(new Hashtable(){{"IsActive",false}}, new Hashtable(){{"IsActive", DBNull.Value}}, FormQuestion.ObjectMap, dc);
			FormQuestion.Update(new Hashtable(){{"IsDeleted",false}}, new Hashtable(){{"IsDeleted", DBNull.Value}}, FormQuestion.ObjectMap, dc);
			FormQuestion.Update(new Hashtable(){{"IsRequired",false}}, new Hashtable(){{"IsRequired", DBNull.Value}}, FormQuestion.ObjectMap, dc);
			FormQuestion.Update(new Hashtable(){{"IsLocked",false}}, new Hashtable(){{"IsLocked", DBNull.Value}}, FormQuestion.ObjectMap, dc);
			FormQuestion.Update(new Hashtable(){{"IsAnswerDescriptionEnabled",false}}, new Hashtable(){{"IsAnswerDescriptionEnabled", DBNull.Value}}, FormQuestion.ObjectMap, dc);
			FormQuestionGroup.Update(new Hashtable(){{"RegistrantTypeId",0}}, new Hashtable(){{"RegistrantTypeId", DBNull.Value}}, FormQuestionGroup.ObjectMap, dc);
			FormQuestionGroup.Update(new Hashtable(){{"QuestionGroupDataTypeId",0}}, new Hashtable(){{"QuestionGroupDataTypeId", DBNull.Value}}, FormQuestionGroup.ObjectMap, dc);
			FormQuestionGroup.Update(new Hashtable(){{"IsActive",false}}, new Hashtable(){{"IsActive", DBNull.Value}}, FormQuestionGroup.ObjectMap, dc);
			FormQuestionGroup.Update(new Hashtable(){{"IsDeleted",false}}, new Hashtable(){{"IsDeleted", DBNull.Value}}, FormQuestionGroup.ObjectMap, dc);
			FormQuestionAnswer.Update(new Hashtable(){{"QuestionDataTypeId",0}}, new Hashtable(){{"QuestionDataTypeId", DBNull.Value}}, FormQuestionAnswer.ObjectMap, dc);
			Registration.Update(new Hashtable(){{"PaymentTypeId",0}}, new Hashtable(){{"PaymentTypeId", DBNull.Value}}, Registration.ObjectMap, dc);
			Registration.Update(new Hashtable(){{"CompletionStatusId",0}}, new Hashtable(){{"CompletionStatusId", DBNull.Value}}, Registration.ObjectMap, dc);
			ActivityWebUser.Update(new Hashtable(){{"IsLeader",false}}, new Hashtable(){{"IsLeader", DBNull.Value}}, ActivityWebUser.ObjectMap, dc);
			Attendance.Update(new Hashtable(){{"IsCompleted",false}}, new Hashtable(){{"IsCompleted", DBNull.Value}}, Attendance.ObjectMap, dc);
			Attendance.Update(new Hashtable(){{"IsCharged",false}}, new Hashtable(){{"IsCharged", DBNull.Value}}, Attendance.ObjectMap, dc);
			AttendanceRegistrant.Update(new Hashtable(){{"IsPresent",false}}, new Hashtable(){{"IsPresent", DBNull.Value}}, AttendanceRegistrant.ObjectMap, dc);
			AttendanceRegistrant.Update(new Hashtable(){{"IsLate",false}}, new Hashtable(){{"IsLate", DBNull.Value}}, AttendanceRegistrant.ObjectMap, dc);
			Package.Update(new Hashtable(){{"IsActive",false}}, new Hashtable(){{"IsActive", DBNull.Value}}, Package.ObjectMap, dc);
			Package.Update(new Hashtable(){{"IsPopular",false}}, new Hashtable(){{"IsPopular", DBNull.Value}}, Package.ObjectMap, dc);
			Package.Update(new Hashtable(){{"IsAfterSchool",false}}, new Hashtable(){{"IsAfterSchool", DBNull.Value}}, Package.ObjectMap, dc);
			AccountPackage.Update(new Hashtable(){{"BillingPeriodTypeId",0}}, new Hashtable(){{"BillingPeriodTypeId", DBNull.Value}}, AccountPackage.ObjectMap, dc);
			AccountPackage.Update(new Hashtable(){{"IsActive",false}}, new Hashtable(){{"IsActive", DBNull.Value}}, AccountPackage.ObjectMap, dc);
			PackageFeature.Update(new Hashtable(){{"FeatureTypeId",0}}, new Hashtable(){{"FeatureTypeId", DBNull.Value}}, PackageFeature.ObjectMap, dc);
			AccountPackageFeature.Update(new Hashtable(){{"FeatureTypeId",0}}, new Hashtable(){{"FeatureTypeId", DBNull.Value}}, AccountPackageFeature.ObjectMap, dc);
			File.Update(new Hashtable(){{"FileTypeId",0}}, new Hashtable(){{"FileTypeId", DBNull.Value}}, File.ObjectMap, dc);
			SessionAction.Update(new Hashtable(){{"SessionActionTypeId",0}}, new Hashtable(){{"SessionActionTypeId", DBNull.Value}}, SessionAction.ObjectMap, dc);
			CartItem.Update(new Hashtable(){{"PurchaseableObjectTypeId",0}}, new Hashtable(){{"PurchaseableObjectTypeId", DBNull.Value}}, CartItem.ObjectMap, dc);
			CartItem.Update(new Hashtable(){{"ParticipationObjectTypeId",0}}, new Hashtable(){{"ParticipationObjectTypeId", DBNull.Value}}, CartItem.ObjectMap, dc);
			CartItem.Update(new Hashtable(){{"Day",0}}, new Hashtable(){{"Day", DBNull.Value}}, CartItem.ObjectMap, dc);
			PasswordResetToken.Update(new Hashtable(){{"Used",false}}, new Hashtable(){{"Used", DBNull.Value}}, PasswordResetToken.ObjectMap, dc);
			SubscriptionPlan.Update(new Hashtable(){{"ParticipationObjectTypeId",0}}, new Hashtable(){{"ParticipationObjectTypeId", DBNull.Value}}, SubscriptionPlan.ObjectMap, dc);
			SubscriptionPlan.Update(new Hashtable(){{"BillingPeriodTypeId",0}}, new Hashtable(){{"BillingPeriodTypeId", DBNull.Value}}, SubscriptionPlan.ObjectMap, dc);
			SubscriptionPlan.Update(new Hashtable(){{"DoesAutoRenew",false}}, new Hashtable(){{"DoesAutoRenew", DBNull.Value}}, SubscriptionPlan.ObjectMap, dc);
			SubscriptionPlan.Update(new Hashtable(){{"IsActive",false}}, new Hashtable(){{"IsActive", DBNull.Value}}, SubscriptionPlan.ObjectMap, dc);
			SubscriptionPlan.Update(new Hashtable(){{"PlanTypeId",0}}, new Hashtable(){{"PlanTypeId", DBNull.Value}}, SubscriptionPlan.ObjectMap, dc);
			SubscriptionPlan.Update(new Hashtable(){{"PaymentStartTypeId",0}}, new Hashtable(){{"PaymentStartTypeId", DBNull.Value}}, SubscriptionPlan.ObjectMap, dc);
			SubscriptionPlan.Update(new Hashtable(){{"SplitCostEvenly",false}}, new Hashtable(){{"SplitCostEvenly", DBNull.Value}}, SubscriptionPlan.ObjectMap, dc);
			SubscriptionPlan.Update(new Hashtable(){{"RegFeeAsDownPayment",false}}, new Hashtable(){{"RegFeeAsDownPayment", DBNull.Value}}, SubscriptionPlan.ObjectMap, dc);
			SubscriptionPlan.Update(new Hashtable(){{"IsConvenienceFeeTakenOutOfCost",false}}, new Hashtable(){{"IsConvenienceFeeTakenOutOfCost", DBNull.Value}}, SubscriptionPlan.ObjectMap, dc);
			SubscriptionPlan.Update(new Hashtable(){{"IsOngoingSubscription",false}}, new Hashtable(){{"IsOngoingSubscription", DBNull.Value}}, SubscriptionPlan.ObjectMap, dc);
			SubscriptionPlanPrePayment.Update(new Hashtable(){{"ParticipationObjectTypeId",0}}, new Hashtable(){{"ParticipationObjectTypeId", DBNull.Value}}, SubscriptionPlanPrePayment.ObjectMap, dc);
			RegistrantSubscriptionPlan.Update(new Hashtable(){{"PaymentTypeFeesPaidBy",0}}, new Hashtable(){{"PaymentTypeFeesPaidBy", DBNull.Value}}, RegistrantSubscriptionPlan.ObjectMap, dc);
			RegistrantSubscriptionPlan.Update(new Hashtable(){{"ConvenienceFeesPaidBy",0}}, new Hashtable(){{"ConvenienceFeesPaidBy", DBNull.Value}}, RegistrantSubscriptionPlan.ObjectMap, dc);
			RegistrantSubscriptionPlan.Update(new Hashtable(){{"PaymentTypeId",0}}, new Hashtable(){{"PaymentTypeId", DBNull.Value}}, RegistrantSubscriptionPlan.ObjectMap, dc);
			RegistrantSubscriptionPlan.Update(new Hashtable(){{"BillingPeriodTypeId",0}}, new Hashtable(){{"BillingPeriodTypeId", DBNull.Value}}, RegistrantSubscriptionPlan.ObjectMap, dc);
			Transaction.Update(new Hashtable(){{"PaymentTypeId",0}}, new Hashtable(){{"PaymentTypeId", DBNull.Value}}, Transaction.ObjectMap, dc);
			Transaction.Update(new Hashtable(){{"TransactionTypeId",0}}, new Hashtable(){{"TransactionTypeId", DBNull.Value}}, Transaction.ObjectMap, dc);
			Transaction.Update(new Hashtable(){{"PaymentSourceTypeId",0}}, new Hashtable(){{"PaymentSourceTypeId", DBNull.Value}}, Transaction.ObjectMap, dc);
			Transaction.Update(new Hashtable(){{"IsDeleted",false}}, new Hashtable(){{"IsDeleted", DBNull.Value}}, Transaction.ObjectMap, dc);
			Transaction.Update(new Hashtable(){{"UsedStripeConnect",false}}, new Hashtable(){{"UsedStripeConnect", DBNull.Value}}, Transaction.ObjectMap, dc);
			Payout.Update(new Hashtable(){{"PayoutStatusId",0}}, new Hashtable(){{"PayoutStatusId", DBNull.Value}}, Payout.ObjectMap, dc);
			Payout.Update(new Hashtable(){{"PaymentTypeId",0}}, new Hashtable(){{"PaymentTypeId", DBNull.Value}}, Payout.ObjectMap, dc);
			Product.Update(new Hashtable(){{"IsActive",false}}, new Hashtable(){{"IsActive", DBNull.Value}}, Product.ObjectMap, dc);
			ProductOffering.Update(new Hashtable(){{"ParticipationObjectTypeId",0}}, new Hashtable(){{"ParticipationObjectTypeId", DBNull.Value}}, ProductOffering.ObjectMap, dc);
			ProductPurchase.Update(new Hashtable(){{"ParticipationObjectTypeId",0}}, new Hashtable(){{"ParticipationObjectTypeId", DBNull.Value}}, ProductPurchase.ObjectMap, dc);
			Coupon.Update(new Hashtable(){{"AmountTypeId",0}}, new Hashtable(){{"AmountTypeId", DBNull.Value}}, Coupon.ObjectMap, dc);
			Coupon.Update(new Hashtable(){{"AppliesTo",0}}, new Hashtable(){{"AppliesTo", DBNull.Value}}, Coupon.ObjectMap, dc);
			Coupon.Update(new Hashtable(){{"IsDisabled",false}}, new Hashtable(){{"IsDisabled", DBNull.Value}}, Coupon.ObjectMap, dc);
			AttachedCoupon.Update(new Hashtable(){{"IsUsed",false}}, new Hashtable(){{"IsUsed", DBNull.Value}}, AttachedCoupon.ObjectMap, dc);
			PurchaseableObjectCoupon.Update(new Hashtable(){{"PurchaseableObjectTypeId",0}}, new Hashtable(){{"PurchaseableObjectTypeId", DBNull.Value}}, PurchaseableObjectCoupon.ObjectMap, dc);
			RefundRequest.Update(new Hashtable(){{"RequestFulfilled",false}}, new Hashtable(){{"RequestFulfilled", DBNull.Value}}, RefundRequest.ObjectMap, dc);
			RefundRequest.Update(new Hashtable(){{"RefundRequestStatusId",0}}, new Hashtable(){{"RefundRequestStatusId", DBNull.Value}}, RefundRequest.ObjectMap, dc);
			RefundRequest.Update(new Hashtable(){{"RequestedRefundTypeId",0}}, new Hashtable(){{"RequestedRefundTypeId", DBNull.Value}}, RefundRequest.ObjectMap, dc);
			RefundRequest.Update(new Hashtable(){{"ApprovedRefundTypeId",0}}, new Hashtable(){{"ApprovedRefundTypeId", DBNull.Value}}, RefundRequest.ObjectMap, dc);
			Subscriber.Update(new Hashtable(){{"ObjectTypeId",0}}, new Hashtable(){{"ObjectTypeId", DBNull.Value}}, Subscriber.ObjectMap, dc);
			Subscriber.Update(new Hashtable(){{"SubscriberStatusId",0}}, new Hashtable(){{"SubscriberStatusId", DBNull.Value}}, Subscriber.ObjectMap, dc);
			WebSessionAction.Update(new Hashtable(){{"ActionTypeId",0}}, new Hashtable(){{"ActionTypeId", DBNull.Value}}, WebSessionAction.ObjectMap, dc);
			WebSessionAction.Update(new Hashtable(){{"ObjectTypeId",0}}, new Hashtable(){{"ObjectTypeId", DBNull.Value}}, WebSessionAction.ObjectMap, dc);
			WebSessionAction.Update(new Hashtable(){{"IndirectObjectTypeId",0}}, new Hashtable(){{"IndirectObjectTypeId", DBNull.Value}}, WebSessionAction.ObjectMap, dc);
			SurveyRequest.Update(new Hashtable(){{"MultipleResponsesEnabled",false}}, new Hashtable(){{"MultipleResponsesEnabled", DBNull.Value}}, SurveyRequest.ObjectMap, dc);
			SurveyRequest.Update(new Hashtable(){{"IsDeleted",false}}, new Hashtable(){{"IsDeleted", DBNull.Value}}, SurveyRequest.ObjectMap, dc);
			SurveyResponse.Update(new Hashtable(){{"SurveyFeedbackStatusTypeId",0}}, new Hashtable(){{"SurveyFeedbackStatusTypeId", DBNull.Value}}, SurveyResponse.ObjectMap, dc);
			SurveyResponse.Update(new Hashtable(){{"IsDeleted",false}}, new Hashtable(){{"IsDeleted", DBNull.Value}}, SurveyResponse.ObjectMap, dc);
			RegistrantInviteCode.Update(new Hashtable(){{"IsDeleted",false}}, new Hashtable(){{"IsDeleted", DBNull.Value}}, RegistrantInviteCode.ObjectMap, dc);
			RefreshToken.Update(new Hashtable(){{"IsActive",false}}, new Hashtable(){{"IsActive", DBNull.Value}}, RefreshToken.ObjectMap, dc);
			
			//**ENDUPDATES**//

			return msgs;
		}

		public static SystemMessages CreateIndexes(DatabaseConnection dc = null)
		{
			SystemMessages msgs = new SystemMessages();
			BasicLibrary.DataAccess.DatabaseQuery dq = new BasicLibrary.DataAccess.DatabaseQuery();
            dq.Connection = dc;

			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_WebUser_Uid ON [WebUser] (Uid)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_WebUser_IsActive_Uid ON [WebUser] (Uid,IsActive)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_WebUser_IsActive ON [WebUser] (IsActive)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_WebUser_FacebookId ON [WebUser] (FacebookId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Activity_AccountId ON [Activity] (AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Activity_AccountId_IsPublished ON [Activity] (AccountId,IsPublished)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Activity_LocationId ON [Activity] (LocationId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Activity_Uid ON [Activity] (Uid)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Activity_AccountId_Uid ON [Activity] (Uid,AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Activity_ProviderAccountId_Uid ON [Activity] (Uid,ProviderAccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Activity_ActivityGroupId ON [Activity] (ActivityGroupId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Activity_AccountId_ActivityGroupId ON [Activity] (ActivityGroupId,AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Activity_AccountActivityGroupId ON [Activity] (AccountActivityGroupId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Activity_ActivityScheduleId ON [Activity] (ActivityScheduleId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Activity_ProviderAccountId ON [Activity] (ProviderAccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Activity_AccountId_UniqueName ON [Activity] (UniqueName,AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Location_HostAccountId ON [Location] (HostAccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Location_HostAccountId_IsDefault ON [Location] (HostAccountId,IsDefault)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Location_LocationTypeId ON [Location] (LocationTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Location_GooglePlaceId ON [Location] (GooglePlaceId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Location_City_Country_State ON [Location] (City,State,Country)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Location_Country_County_State ON [Location] (County,State,Country)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Location_Country_State ON [Location] (State,Country)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Location_Country ON [Location] (Country)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityDay_ActivityId ON [ActivityDay] (ActivityId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityDay_ActivityRecurrenceId ON [ActivityDay] (ActivityRecurrenceId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Tag_Name ON [Tag] (Name)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Tag_AccountId_Name ON [Tag] (Name,AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Tag_ParentTagId ON [Tag] (ParentTagId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Tag_AccountId ON [Tag] (AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Tag_AccountId_ParentTagId ON [Tag] (AccountId,ParentTagId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ObjectTag_ParticipationObjectId_ParticipationObjectTypeId ON [ObjectTag] (ParticipationObjectId,ParticipationObjectTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ObjectTag_TagId ON [ObjectTag] (TagId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ObjectTag_ParticipationObjectId_ParticipationObjectTypeId_TagId ON [ObjectTag] (ParticipationObjectId,ParticipationObjectTypeId,TagId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivitySchedule_AccountId ON [ActivitySchedule] (AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivitySchedule_ActivityGroupId ON [ActivitySchedule] (ActivityGroupId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivitySchedule_AccountId_IsOngoing ON [ActivitySchedule] (AccountId,IsOngoing)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivitySchedule_ActivityGroupId_IsOngoing ON [ActivitySchedule] (ActivityGroupId,IsOngoing)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityInstance_ActivityId ON [ActivityInstance] (ActivityId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityInstance_ActivityGroupId ON [ActivityInstance] (ActivityGroupId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityInstance_ActivityScheduleId ON [ActivityInstance] (ActivityScheduleId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityInstance_ActivityScheduleId_InstanceTypeId ON [ActivityInstance] (ActivityScheduleId,InstanceTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityInstanceSeats_RegistrantActivityId ON [ActivityInstanceSeats] (RegistrantActivityId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityInstanceSeats_ActivityInstanceId ON [ActivityInstanceSeats] (ActivityInstanceId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityInstanceSeats_ActivityInstanceId_RegistrantActivityId ON [ActivityInstanceSeats] (RegistrantActivityId,ActivityInstanceId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Match_TeamId ON [Match] (TeamId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Match_ActivityInstanceId ON [Match] (ActivityInstanceId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Match_ActivityInstanceId_TeamId ON [Match] (TeamId,ActivityInstanceId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityGradeLevel_ActivityId ON [ActivityGradeLevel] (ActivityId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityGradeLevel_ActivityId_GradeLevelId ON [ActivityGradeLevel] (ActivityId,GradeLevelId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityGradeLevel_GradeLevelId ON [ActivityGradeLevel] (GradeLevelId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityMessage_ActivityId ON [ActivityMessage] (ActivityId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityMessage_ActivityGroupId ON [ActivityMessage] (ActivityGroupId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityMessage_Uid ON [ActivityMessage] (Uid)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityMessage_AccountId_Uid ON [ActivityMessage] (Uid,AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityMessage_AccountId ON [ActivityMessage] (AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityMessageRegistrant_ActivityMessageId ON [ActivityMessageRegistrant] (ActivityMessageId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityMessageRegistrant_RegistrantId ON [ActivityMessageRegistrant] (RegistrantId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityMessageRegistrant_CreatedBy ON [ActivityMessageRegistrant] (CreatedBy)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityMessageRegistrant_ActivityMessageId_RegistrantId ON [ActivityMessageRegistrant] (ActivityMessageId,RegistrantId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Account_OwnerId ON [Account] (OwnerId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Account_UniqueName ON [Account] (UniqueName)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Account_IsAdminAccount ON [Account] (IsAdminAccount)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Account_Uid ON [Account] (Uid)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Account_AccountTypeId ON [Account] (AccountTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Account_StripeUserId ON [Account] (StripeUserId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Account_StripeCustomerId ON [Account] (StripeCustomerId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Account_InviteCodeToShare ON [Account] (InviteCodeToShare)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Account_Subdomain ON [Account] (Subdomain)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountProfile_AccountId ON [AccountProfile] (AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_CheckPaymentAccount_AccountId ON [CheckPaymentAccount] (AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_BankPaymentAccount_AccountId ON [BankPaymentAccount] (AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_StripeAccount_AccountId ON [StripeAccount] (AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountRelationship_HostAccountId ON [AccountRelationship] (HostAccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountRelationship_ApprovalStatusId_HostAccountId ON [AccountRelationship] (HostAccountId,ApprovalStatusId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountRelationship_VisitorAccountId ON [AccountRelationship] (VisitorAccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountRelationship_ApprovalStatusId_VisitorAccountId ON [AccountRelationship] (VisitorAccountId,ApprovalStatusId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountRelationship_HostAccountId_VisitorAccountId ON [AccountRelationship] (HostAccountId,VisitorAccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountRelationship_HostAccountId_HostPermissionTypeId ON [AccountRelationship] (HostAccountId,HostPermissionTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountRelationship_HostPermissionTypeId_VisitorAccountId ON [AccountRelationship] (VisitorAccountId,HostPermissionTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountRelationshipLocation_AccountRelationshipId ON [AccountRelationshipLocation] (AccountRelationshipId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountRelationshipLocation_LocationId ON [AccountRelationshipLocation] (LocationId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountRelationshipLocation_HostAccountId ON [AccountRelationshipLocation] (HostAccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountRelationshipLocation_VisitorAccountId ON [AccountRelationshipLocation] (VisitorAccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountRelationshipLocation_HostAccountId_VisitorAccountId ON [AccountRelationshipLocation] (HostAccountId,VisitorAccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountRelationshipLocation_AccountRelationshipId_LocationId ON [AccountRelationshipLocation] (AccountRelationshipId,LocationId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountRelationshipLocation_HostAccountId_LocationId_VisitorAccountId ON [AccountRelationshipLocation] (HostAccountId,VisitorAccountId,LocationId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountProperty_AccountId ON [AccountProperty] (AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountProperty_ActivityGroupId ON [AccountProperty] (ActivityGroupId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountProperty_ActivityId ON [AccountProperty] (ActivityId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountProperty_AccountId_AccountPropertyTypeId ON [AccountProperty] (AccountId,AccountPropertyTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountProperty_AccountId_AccountPropertyTypeId_Value ON [AccountProperty] (AccountId,AccountPropertyTypeId,Value)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountProperty_AccountPropertyTypeId_ActivityGroupId ON [AccountProperty] (ActivityGroupId,AccountPropertyTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountProperty_AccountPropertyTypeId_ActivityId ON [AccountProperty] (ActivityId,AccountPropertyTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountWebUser_AccountId_IsDeleted ON [AccountWebUser] (AccountId,IsDeleted)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountWebUser_IsDeleted_WebUserId ON [AccountWebUser] (WebUserId,IsDeleted)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountWebUser_AccountId_WebUserId ON [AccountWebUser] (AccountId,WebUserId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountWebUser_IsDefault_WebUserId ON [AccountWebUser] (WebUserId,IsDefault)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountWebUser_IsActive_IsDeleted_WebUserId ON [AccountWebUser] (WebUserId,IsActive,IsDeleted)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountWebUser_AccountId_IsActive_IsDeleted ON [AccountWebUser] (AccountId,IsActive,IsDeleted)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountWebUser_AccountId_UserTypeId ON [AccountWebUser] (AccountId,UserTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_RegistrationPeriod_AccountId ON [RegistrationPeriod] (AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_RegistrationPeriod_Uid ON [RegistrationPeriod] (Uid)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_RegistrationPeriod_ActivityGroupId ON [RegistrationPeriod] (ActivityGroupId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_RegistrationPeriod_AccountId_ActivityGroupId ON [RegistrationPeriod] (ActivityGroupId,AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_RegistrationPeriod_AccountActivityGroupId ON [RegistrationPeriod] (AccountActivityGroupId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityRegistrationPeriod_RegistrationPeriodId ON [ActivityRegistrationPeriod] (RegistrationPeriodId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityRegistrationPeriod_ActivityId ON [ActivityRegistrationPeriod] (ActivityId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityRegistrationPeriod_ActivityId_IsActive ON [ActivityRegistrationPeriod] (ActivityId,IsActive)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityRegistrationPeriod_ActivityId_RegistrationPeriodId ON [ActivityRegistrationPeriod] (RegistrationPeriodId,ActivityId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityGroup_AccountId ON [ActivityGroup] (AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityGroup_AccountId_ActivityGroupTypeId ON [ActivityGroup] (AccountId,ActivityGroupTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityGroup_AccountId_IsPublished ON [ActivityGroup] (AccountId,IsPublished)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityGroup_AccountId_IsArchived_IsDeleted_IsPublished ON [ActivityGroup] (AccountId,IsPublished,IsArchived,IsDeleted)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityGroup_AccountId_IsDeleted_IsInviteOnly_IsPublished ON [ActivityGroup] (AccountId,IsPublished,IsInviteOnly,IsDeleted)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityGroup_AccountId_UniqueName ON [ActivityGroup] (AccountId,UniqueName)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityGroup_Uid ON [ActivityGroup] (Uid)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityGroup_AccountId_Uid ON [ActivityGroup] (Uid,AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityGroup_IsPublished_Uid ON [ActivityGroup] (Uid,IsPublished)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityGroup_IsArchived_IsPublished_Uid ON [ActivityGroup] (Uid,IsPublished,IsArchived)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityGroup_IsInviteOnly_IsPublished_Uid ON [ActivityGroup] (Uid,IsPublished,IsInviteOnly)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityGroup_AdministratorAccountId ON [ActivityGroup] (AdministratorAccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountActivityGroup_AccountId ON [AccountActivityGroup] (AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountActivityGroup_ActivityGroupId ON [AccountActivityGroup] (ActivityGroupId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountActivityGroup_AccountId_IsPublished ON [AccountActivityGroup] (AccountId,IsPublished)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountActivityGroup_AccountId_InviteStatusId ON [AccountActivityGroup] (AccountId,InviteStatusId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountActivityGroup_AccountId_ActivityGroupId ON [AccountActivityGroup] (AccountId,ActivityGroupId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Registrant_AccountId_IsDeleted ON [Registrant] (AccountId,IsDeleted)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Registrant_ParentRegistrantId ON [Registrant] (ParentRegistrantId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Registrant_AccountId_IsDeleted_RegistrantTypeId ON [Registrant] (AccountId,RegistrantTypeId,IsDeleted)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Registrant_AccountId_IsContactOnly_IsDeleted ON [Registrant] (AccountId,IsContactOnly,IsDeleted)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Registrant_Uid ON [Registrant] (Uid)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Registrant_AccountId_Uid ON [Registrant] (Uid,AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Registrant_IsDeleted_WebUserId ON [Registrant] (WebUserId,IsDeleted)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Registrant_IsDeleted_IsMaster_WebUserId ON [Registrant] (WebUserId,IsMaster,IsDeleted)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Registrant_FirstRegistrantId ON [Registrant] (FirstRegistrantId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Registrant_AccountId_ParentRegistrantId ON [Registrant] (ParentRegistrantId,AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_RegistrantWebUser_IsDeleted_RegistrantId ON [RegistrantWebUser] (RegistrantId,IsDeleted)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_RegistrantWebUser_IsDeleted_WebUserId ON [RegistrantWebUser] (WebUserId,IsDeleted)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_RegistrantWebUser_RegistrantId_WebUserId ON [RegistrantWebUser] (RegistrantId,WebUserId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ContactList_AccountId ON [ContactList] (AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ContactListRegistrant_ContactListId_RegistrantId ON [ContactListRegistrant] (ContactListId,RegistrantId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ContactListRegistrant_ContactListId ON [ContactListRegistrant] (ContactListId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ContactListRegistrant_RegistrantId ON [ContactListRegistrant] (RegistrantId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityPayment_ActivityId_IsDeleted ON [ActivityPayment] (ActivityId,IsDeleted)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityPayment_AccountId_IsDeleted ON [ActivityPayment] (AccountId,IsDeleted)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityPayment_IsDeleted_RegistrantId ON [ActivityPayment] (RegistrantId,IsDeleted)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityPayment_IsDeleted_RegistrantSubscriptionPlanId ON [ActivityPayment] (RegistrantSubscriptionPlanId,IsDeleted)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityPayment_Uid ON [ActivityPayment] (Uid)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityPayment_IsDeleted_ParticipationObjectId_ParticipationObjectTypeId ON [ActivityPayment] (ParticipationObjectId,ParticipationObjectTypeId,IsDeleted)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityPayment_IsDeleted_ParticipationObjectId_ParticipationObjectTypeId_RegistrantId ON [ActivityPayment] (ParticipationObjectId,ParticipationObjectTypeId,RegistrantId,IsDeleted)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityPayment_IsDeleted_TransactionId ON [ActivityPayment] (TransactionId,IsDeleted)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_RegistrantActivityGroup_ActivityGroupId_RegistrantId ON [RegistrantActivityGroup] (ActivityGroupId,RegistrantId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_RegistrantActivityGroup_RegistrantId ON [RegistrantActivityGroup] (RegistrantId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_RegistrantActivityGroup_ActivityGroupId ON [RegistrantActivityGroup] (ActivityGroupId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_RegistrantActivityGroup_Uid ON [RegistrantActivityGroup] (Uid)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_RegistrantActivity_RegistrantId ON [RegistrantActivity] (RegistrantId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_RegistrantActivity_RegistrationId ON [RegistrantActivity] (RegistrationId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_RegistrantActivity_Uid ON [RegistrantActivity] (Uid)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_RegistrantActivity_ParticipationObjectId_ParticipationObjectTypeId ON [RegistrantActivity] (ParticipationObjectId,ParticipationObjectTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_RegistrantActivity_ParticipationObjectId_ParticipationObjectTypeId_RegistrantId ON [RegistrantActivity] (ParticipationObjectId,ParticipationObjectTypeId,RegistrantId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityForm_FormId ON [ActivityForm] (FormId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityForm_ActivityId ON [ActivityForm] (ActivityId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityForm_ActivityId_FormId ON [ActivityForm] (ActivityId,FormId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityForm_ActivityId_IsActive ON [ActivityForm] (ActivityId,IsActive)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityForm_AccountId ON [ActivityForm] (AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityForm_Uid ON [ActivityForm] (Uid)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityForm_IsPublished_Uid ON [ActivityForm] (Uid,IsPublished)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Form_AccountId ON [Form] (AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Form_Uid ON [Form] (Uid)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_FormQuestion_FormId ON [FormQuestion] (FormId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_FormQuestion_FormId_IsActive ON [FormQuestion] (FormId,IsActive)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_FormQuestion_FormQuestionGroupId ON [FormQuestion] (FormQuestionGroupId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_FormQuestion_FormQuestionGroupId_IsActive ON [FormQuestion] (FormQuestionGroupId,IsActive)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_FormQuestion_FormQuestionGroupId_IsDeleted ON [FormQuestion] (FormQuestionGroupId,IsDeleted)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_FormQuestion_DependsOnQuestionId ON [FormQuestion] (DependsOnQuestionId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_FormQuestion_FormId_QuestionDataTypeId ON [FormQuestion] (FormId,QuestionDataTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_FormQuestion_FormQuestionGroupId_QuestionDataTypeId ON [FormQuestion] (FormQuestionGroupId,QuestionDataTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_FormQuestionGroup_FormId ON [FormQuestionGroup] (FormId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_FormQuestionGroup_FormId_RegistrantTypeId ON [FormQuestionGroup] (FormId,RegistrantTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_FormQuestionGroup_FormId_IsDeleted ON [FormQuestionGroup] (FormId,IsDeleted)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_FormQuestionAnswer_RegistrantId ON [FormQuestionAnswer] (RegistrantId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_FormQuestionAnswer_FormQuestionId_RegistrantId_RegistrationId ON [FormQuestionAnswer] (RegistrantId,FormQuestionId,RegistrationId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_FormQuestionAnswer_ActivityId_RegistrantId ON [FormQuestionAnswer] (RegistrantId,ActivityId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_FormQuestionAnswer_FormQuestionId ON [FormQuestionAnswer] (FormQuestionId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_FormQuestionAnswer_FormQuestionId_RegistrantId ON [FormQuestionAnswer] (FormQuestionId,RegistrantId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_FormQuestionAnswer_RegistrationId ON [FormQuestionAnswer] (RegistrationId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_FormQuestionAnswer_SurveyResponseId ON [FormQuestionAnswer] (SurveyResponseId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Registration_Uid ON [Registration] (Uid)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Registration_AccountId_Uid ON [Registration] (Uid,AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Registration_RegistrantId ON [Registration] (RegistrantId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Registration_AccountId ON [Registration] (AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Registration_RegistrationPeriodId ON [Registration] (RegistrationPeriodId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Registration_AccountId_RegistrantId ON [Registration] (AccountId,RegistrantId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Registration_ActivityGroupId ON [Registration] (ActivityGroupId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Registration_AccountActivityGroupId ON [Registration] (AccountActivityGroupId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Registration_FirstRegistrationId ON [Registration] (FirstRegistrationId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Registration_CouponId_IsDeleted ON [Registration] (CouponId,IsDeleted)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityWebUser_ActivityId ON [ActivityWebUser] (ActivityId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityWebUser_WebUserId ON [ActivityWebUser] (WebUserId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityWebUser_IsLeader_WebUserId ON [ActivityWebUser] (WebUserId,IsLeader)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityWebUser_ActivityId_IsLeader ON [ActivityWebUser] (ActivityId,IsLeader)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityWebUser_ActivityId_WebUserId ON [ActivityWebUser] (ActivityId,WebUserId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Attendance_ActivityId ON [Attendance] (ActivityId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Attendance_ActivityId_Date ON [Attendance] (ActivityId,Date)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Attendance_ActivityId_IsCompleted ON [Attendance] (ActivityId,IsCompleted)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Attendance_ActivityInstanceId ON [Attendance] (ActivityInstanceId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AttendanceRegistrant_RegistrantId ON [AttendanceRegistrant] (RegistrantId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AttendanceRegistrant_AttendanceId ON [AttendanceRegistrant] (AttendanceId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AttendanceRegistrant_AttendanceId_RegistrantId ON [AttendanceRegistrant] (AttendanceId,RegistrantId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Package_IsActive ON [Package] (IsActive)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Package_IsActive_IsAfterSchool ON [Package] (IsActive,IsAfterSchool)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Package_IsPopular ON [Package] (IsPopular)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Package_IsAfterSchool_IsPopular ON [Package] (IsPopular,IsAfterSchool)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Package_Uid ON [Package] (Uid)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountPackage_PackageId ON [AccountPackage] (PackageId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountPackage_AccountId ON [AccountPackage] (AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountPackage_AccountId_IsActive ON [AccountPackage] (AccountId,IsActive)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountPackage_Uid ON [AccountPackage] (Uid)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountPackage_AccountId_Uid ON [AccountPackage] (Uid,AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_PackageFeature_PackageId ON [PackageFeature] (PackageId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_PackageFeature_FeatureTypeId_PackageId ON [PackageFeature] (PackageId,FeatureTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountPackageFeature_AccountPackageId ON [AccountPackageFeature] (AccountPackageId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AccountPackageFeature_AccountPackageId_FeatureTypeId ON [AccountPackageFeature] (AccountPackageId,FeatureTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_File_AccountId ON [File] (AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityFile_ActivityId ON [ActivityFile] (ActivityId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityFile_FileId ON [ActivityFile] (FileId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ActivityFile_ActivityId_FileId ON [ActivityFile] (ActivityId,FileId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Session_RegistrantId ON [Session] (RegistrantId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_SessionAction_SessionId ON [SessionAction] (SessionId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_SessionAction_WebUserId ON [SessionAction] (WebUserId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_SessionAction_SessionActionTypeId ON [SessionAction] (SessionActionTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_SessionAction_ActivityId ON [SessionAction] (ActivityId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_SessionAction_AccountId ON [SessionAction] (AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_SessionAction_RegistrantId ON [SessionAction] (RegistrantId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_SessionAction_RegistrationId ON [SessionAction] (RegistrationId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_SessionAction_RegistrationPeriodId ON [SessionAction] (RegistrationPeriodId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_SessionAction_ActivityGroupId ON [SessionAction] (ActivityGroupId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_SessionAction_ActivityGroupId_SessionActionTypeId ON [SessionAction] (ActivityGroupId,SessionActionTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_SessionAction_ActivityId_SessionActionTypeId ON [SessionAction] (ActivityId,SessionActionTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_SessionAction_RegistrantId_SessionActionTypeId ON [SessionAction] (RegistrantId,SessionActionTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_SessionAction_RegistrationId_SessionActionTypeId ON [SessionAction] (RegistrationId,SessionActionTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_SessionAction_RegistrationPeriodId_SessionActionTypeId ON [SessionAction] (RegistrationPeriodId,SessionActionTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Cart_Uid ON [Cart] (Uid)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_CartItem_Uid ON [CartItem] (Uid)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_CartItem_CartId ON [CartItem] (CartId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_PasswordResetToken_Uid ON [PasswordResetToken] (Uid)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_SubscriptionPlan_ParticipationObjectId_ParticipationObjectTypeId_PlanTypeId ON [SubscriptionPlan] (ParticipationObjectId,ParticipationObjectTypeId,PlanTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_SubscriptionPlan_PlanOwnerAccountId ON [SubscriptionPlan] (PlanOwnerAccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_SubscriptionPlanPrePayment_ParticipationObjectId_ParticipationObjectTypeId ON [SubscriptionPlanPrePayment] (ParticipationObjectId,ParticipationObjectTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_SubscriptionPlanPrePayment_ParticipationObjectId_ParticipationObjectTypeId_RegistrationId ON [SubscriptionPlanPrePayment] (RegistrationId,ParticipationObjectId,ParticipationObjectTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_SubscriptionPlanPrePayment_TransactionId ON [SubscriptionPlanPrePayment] (TransactionId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_SubscriptionPlanPrePayment_RegistrationId ON [SubscriptionPlanPrePayment] (RegistrationId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_SubscriptionPlanPrePayment_PayerAccountId ON [SubscriptionPlanPrePayment] (PayerAccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_SubscriptionPlanPrePayment_AttendanceRegistrantId ON [SubscriptionPlanPrePayment] (AttendanceRegistrantId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_RegistrantSubscriptionPlan_PurchaseId ON [RegistrantSubscriptionPlan] (PurchaseId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_RegistrantSubscriptionPlan_RegistrantId ON [RegistrantSubscriptionPlan] (RegistrantId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_RegistrantSubscriptionPlan_SubscriptionPlanId ON [RegistrantSubscriptionPlan] (SubscriptionPlanId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_RegistrantSubscriptionPlan_RegistrantId_SubscriptionPlanId ON [RegistrantSubscriptionPlan] (RegistrantId,SubscriptionPlanId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_RegistrantSubscriptionPlan_StripeSubscriptionId ON [RegistrantSubscriptionPlan] (StripeSubscriptionId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Transaction_BillerAccountId_IsDeleted ON [Transaction] (BillerAccountId,IsDeleted)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Transaction_IsDeleted_PayerAccountId ON [Transaction] (PayerAccountId,IsDeleted)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Transaction_Uid ON [Transaction] (Uid)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Transaction_BillerAccountId_Uid ON [Transaction] (Uid,BillerAccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Transaction_PayerAccountId_Uid ON [Transaction] (Uid,PayerAccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Transaction_BillerAccountId_IsDeleted_TransactionTypeId ON [Transaction] (BillerAccountId,TransactionTypeId,IsDeleted)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Transaction_IsDeleted_PayerAccountId_TransactionTypeId ON [Transaction] (PayerAccountId,TransactionTypeId,IsDeleted)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Transaction_StripeChargeOrRefundId ON [Transaction] (StripeChargeOrRefundId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Transaction_IsDeleted_OriginalTransactionId ON [Transaction] (OriginalTransactionId,IsDeleted)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Transaction_IsDeleted_PayoutId ON [Transaction] (PayoutId,IsDeleted)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Transaction_CouponId_IsDeleted ON [Transaction] (CouponId,IsDeleted)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Transaction_CouponId_IsDeleted_PayerAccountId ON [Transaction] (CouponId,PayerAccountId,IsDeleted)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Transaction_BillerAccountId_CouponId_IsDeleted ON [Transaction] (CouponId,BillerAccountId,IsDeleted)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Payout_AccountId ON [Payout] (AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Payout_AccountId_PayoutStatusId ON [Payout] (AccountId,PayoutStatusId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Payout_PayoutStatusId ON [Payout] (PayoutStatusId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Payout_Uid ON [Payout] (Uid)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Payout_AccountId_Uid ON [Payout] (Uid,AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Product_AccountId ON [Product] (AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Product_Uid ON [Product] (Uid)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ProductOffering_ParticipationObjectId_ParticipationObjectTypeId ON [ProductOffering] (ParticipationObjectId,ParticipationObjectTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ProductOffering_ProductId ON [ProductOffering] (ProductId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ProductOffering_ParticipationObjectId_ParticipationObjectTypeId_ProductId ON [ProductOffering] (ParticipationObjectId,ParticipationObjectTypeId,ProductId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ProductPurchase_RegistrantId ON [ProductPurchase] (RegistrantId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ProductPurchase_ProductId ON [ProductPurchase] (ProductId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ProductPurchase_ParticipationObjectId_ParticipationObjectTypeId ON [ProductPurchase] (ParticipationObjectId,ParticipationObjectTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_ProductPurchase_ParticipationObjectId_ParticipationObjectTypeId_RegistrantId ON [ProductPurchase] (RegistrantId,ParticipationObjectId,ParticipationObjectTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Coupon_AccountId ON [Coupon] (AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Coupon_AccountId_CouponCode ON [Coupon] (AccountId,CouponCode)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Coupon_Uid ON [Coupon] (Uid)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Coupon_AccountId_Uid ON [Coupon] (Uid,AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AttachedCoupon_CouponId ON [AttachedCoupon] (CouponId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AttachedCoupon_AccountId ON [AttachedCoupon] (AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AttachedCoupon_AccountId_IsUsed ON [AttachedCoupon] (AccountId,IsUsed)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_AttachedCoupon_AccountId_CouponId ON [AttachedCoupon] (CouponId,AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_PurchaseableObjectCoupon_CouponId_PurchaseableObjectId_PurchaseableObjectTypeId ON [PurchaseableObjectCoupon] (CouponId,PurchaseableObjectId,PurchaseableObjectTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_PurchaseableObjectCoupon_CouponId ON [PurchaseableObjectCoupon] (CouponId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_PurchaseableObjectCoupon_PurchaseableObjectId_PurchaseableObjectTypeId ON [PurchaseableObjectCoupon] (PurchaseableObjectId,PurchaseableObjectTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Purchase_RegistrationId ON [Purchase] (RegistrationId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Purchase_RegistrantId ON [Purchase] (RegistrantId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Purchase_CouponId ON [Purchase] (CouponId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Purchase_CouponId_PurchaserAccountId ON [Purchase] (CouponId,PurchaserAccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_TwilioSMS_TwilioResourceId ON [TwilioSMS] (TwilioResourceId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_TwilioSMS_ActivityMessageId_RegistrantId ON [TwilioSMS] (RegistrantId,ActivityMessageId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_TwilioSMS_RegistrantId ON [TwilioSMS] (RegistrantId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_TwilioBlockedPhone_Phone ON [TwilioBlockedPhone] (Phone)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Migration_UniqueIdentifier ON [Migration] (UniqueIdentifier)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_RefundRequest_Uid ON [RefundRequest] (Uid)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_RefundRequest_AccountId_Uid ON [RefundRequest] (Uid,AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_RefundRequest_TransactionId ON [RefundRequest] (TransactionId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_RefundRequest_AccountId ON [RefundRequest] (AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_RefundRequest_AccountId_RefundRequestStatusId ON [RefundRequest] (AccountId,RefundRequestStatusId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_RefundRequest_RefundRequestStatusId ON [RefundRequest] (RefundRequestStatusId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Subscriber_Uid ON [Subscriber] (Uid)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Subscriber_AccountId_WebUserId ON [Subscriber] (AccountId,WebUserId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Subscriber_ObjectId_ObjectTypeId_WebUserId ON [Subscriber] (ObjectId,ObjectTypeId,WebUserId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Subscriber_WebUserId ON [Subscriber] (WebUserId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Subscriber_AccountId ON [Subscriber] (AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Subscriber_ObjectId_ObjectTypeId ON [Subscriber] (ObjectId,ObjectTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Subscriber_SubscriberStatusId_WebUserId ON [Subscriber] (WebUserId,SubscriberStatusId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_Subscriber_ObjectId_ObjectTypeId_SubscriberStatusId ON [Subscriber] (ObjectId,ObjectTypeId,SubscriberStatusId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_WebSession_WebUserId ON [WebSession] (WebUserId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_WebSession_AccountId ON [WebSession] (AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_WebSessionAction_Id ON [WebSessionAction] (Id)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_WebSessionAction_WebUserId ON [WebSessionAction] (WebUserId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_WebSessionAction_Id_WebUserId ON [WebSessionAction] (Id,WebUserId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_WebSessionAction_AccountId_Id ON [WebSessionAction] (Id,AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_WebSessionAction_AccountId_Id_WebUserId ON [WebSessionAction] (Id,WebUserId,AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_WebSessionAction_ActionTypeId ON [WebSessionAction] (ActionTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_WebSessionAction_ActionTypeId_ObjectTypeId ON [WebSessionAction] (ActionTypeId,ObjectTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_WebSessionAction_ObjectId_ObjectTypeId ON [WebSessionAction] (ObjectId,ObjectTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_WebSessionAction_ActionId_ObjectId ON [WebSessionAction] (ObjectId,ActionId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_WebSessionAction_IndirectObjectId_ObjectId ON [WebSessionAction] (ObjectId,IndirectObjectId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_WebSessionAction_IndirectObjectId_IndirectObjectTypeId ON [WebSessionAction] (IndirectObjectId,IndirectObjectTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_WebSessionAction_ObjectId_ObjectTypeId_WebUserId ON [WebSessionAction] (WebUserId,ObjectId,ObjectTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_WebSessionAction_IndirectObjectId_IndirectObjectTypeId_ObjectId_ObjectTypeId_WebUserId ON [WebSessionAction] (WebUserId,ObjectId,ObjectTypeId,IndirectObjectId,IndirectObjectTypeId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_WebSessionObjectSnapshot_WebUserId ON [WebSessionObjectSnapshot] (WebUserId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_WebSessionObjectSnapshot_AccountId ON [WebSessionObjectSnapshot] (AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_SurveyRequest_AccountId ON [SurveyRequest] (AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_SurveyRequest_ActivityId ON [SurveyRequest] (ActivityId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_SurveyRequest_FormId ON [SurveyRequest] (FormId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_SurveyRequest_Uid ON [SurveyRequest] (Uid)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_SurveyRequest_AccountId_Uid ON [SurveyRequest] (Uid,AccountId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_SurveyRequest_ActivityId_Uid ON [SurveyRequest] (Uid,ActivityId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_SurveyRequest_FormId_Uid ON [SurveyRequest] (Uid,FormId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_SurveyResponse_RegistrantId ON [SurveyResponse] (RegistrantId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_SurveyResponse_SurveyRequestId ON [SurveyResponse] (SurveyRequestId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_SurveyResponse_Uid ON [SurveyResponse] (Uid)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_SurveyResponse_SurveyRequestId_Uid ON [SurveyResponse] (Uid,SurveyRequestId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_SurveyResponse_RegistrantId_Uid ON [SurveyResponse] (Uid,RegistrantId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_RegistrantInviteCode_RegistrantId ON [RegistrantInviteCode] (RegistrantId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_RegistrantInviteCode_SurveyResponseId ON [RegistrantInviteCode] (SurveyResponseId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_RegistrantInviteCode_IsDeleted_RegistrantId_SurveyResponseId ON [RegistrantInviteCode] (RegistrantId,SurveyResponseId,IsDeleted)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_RegistrantInviteCode_Uid ON [RegistrantInviteCode] (Uid)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_RegistrantInviteCode_SurveyResponseId_Uid ON [RegistrantInviteCode] (Uid,SurveyResponseId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_RegistrantInviteCode_RegistrantId_Uid ON [RegistrantInviteCode] (Uid,RegistrantId)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_RegistrantInviteCode_Code_IsDeleted ON [RegistrantInviteCode] (IsDeleted,Code)"));
			msgs.AddRange(dq.ExecuteSql("CREATE INDEX IX_RegistrantInviteCode_Code ON [RegistrantInviteCode] (Code)"));
			

			return msgs;
		}

		public static DatabaseConnection StartTransaction()
        {
            var dc = new DatabaseConnection(Database.Credentials.ConnectionString);
            dc.Open();
            dc.BeginTransaction();
            return dc;
        }

        public static bool EndTransaction(DatabaseConnection dc, SystemMessages msgs, SuccessAction onSuccess = null, FailureAction onFailure = null)
        {
            return EndTransaction(dc, msgs.HasNoErrors(), msgs, null, null, true);
        }

        public static bool EndTransaction(DatabaseConnection dc, bool commit, SystemMessages msgs = null, SuccessAction onSuccess = null, FailureAction onFailure = null, bool closeTransaction = true)
        {
            if (commit)
            {
                if (closeTransaction)
                {
                    dc.CommitTransaction();
                }
                if (onSuccess.IsNotEmpty())
                {                    
                    onSuccess(msgs);
                }
            }
            else
            {
                if (closeTransaction)
                {
                    dc.RollbackTransaction();
                }
                if (onFailure.IsNotEmpty())
                {
                    onFailure(msgs);
                }
            }

			if (closeTransaction)
            {
				dc.Close();
				dc.Dispose();
				dc = null;
			}

            return true;
        }

		public delegate void TransactionAction(SystemMessages msgs, DatabaseConnection dc);
        public delegate void FailureAction(SystemMessages msgs = null);
        public delegate void SuccessAction(SystemMessages msgs = null);
		public delegate void ExceptionAction(SystemMessages msgs = null);

        public static void ExecuteTransaction(TransactionAction action, SuccessAction onSuccess = null, FailureAction onFailure = null, ExceptionAction onException = null, DatabaseConnection dc = null, bool forceRollback = false)
        {
            var msgs = new SystemMessages();
            var closeTransaction = false;

            if (dc.IsEmpty())
            {
                dc = Database.StartTransaction();
                closeTransaction = true;
            }

            try
            {
                action(msgs, dc);
            }
            catch(Exception ex)
            {
                msgs.Add(ex.Message, MessageType.Error, ex);

				if (onException.IsNotEmpty())
					onException(msgs);
            }
            finally
            {
                Database.EndTransaction(
                    dc, 
                    !forceRollback && msgs.HasNoErrors(),
                    msgs,
                    onSuccess: onSuccess,
                    onFailure: onFailure,
                    closeTransaction: closeTransaction
                    );
            }            
        }

		#endregion

		#region "UserDefined"

        #endregion
	}

	public class DatabaseCredentials
    {
        public string Server { get; set; }
        public string DatabaseName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

		public string ConnectionString { get { return string.Format("Data Source={0};User Id={1};Password={2};Initial Catalog={3};", Server, Username, Password, DatabaseName); } }

        public DatabaseCredentials()
        {
        }
        public DatabaseCredentials(string server, string databaseName, string username, string password)
        {
            this.Server = server;
            this.DatabaseName = databaseName;
            this.Username = username;
            this.Password = password;
        }
    }
}