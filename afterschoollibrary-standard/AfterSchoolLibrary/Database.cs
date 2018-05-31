using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Data;
using System.Collections;

namespace AfterSchoolLibrary
{
    public class Database
    {
		#region "ClassGenerator"
		public static DatabaseCredentials LiveCredentials { get { return new DatabaseCredentials("enrollhq.cbf5h6cofkb3.us-east-2.rds.amazonaws.com", "AfterSchool", "AfterSchoolUser", "Nj8Ed$wpLfJ"); } }
		public static DatabaseCredentials LocalCredentials { get { return new DatabaseCredentials("localhost", "AfterSchool", "AfterSchoolUser", "Nj8Ed$wpLfJ"); } }
		public static DatabaseCredentials DevCredentials { get { return new DatabaseCredentials("enrollhq.cbf5h6cofkb3.us-east-2.rds.amazonaws.com", "AfterSchoolDev", "AfterSchoolUser", "Nj8Ed$wpLfJ"); } }
		public static DatabaseCredentials TestCredentials { get { return new DatabaseCredentials("enrollhq.cbf5h6cofkb3.us-east-2.rds.amazonaws.com", "AfterSchoolTest", "AfterSchoolUser", "Nj8Ed$wpLfJ"); } }
		public static DatabaseCredentials Credentials { get { if(Utilities.IsLiveServer){return LiveCredentials;} else if(Utilities.IsTestServer){return TestCredentials;} else if(Utilities.IsDevServer){return DevCredentials;} else{return LocalCredentials;} } }
		

		public static SystemMessages CreateTables(DatabaseConnection dc = null)
		{
			SystemMessages msgs = new SystemMessages();

			WebUser.Create(dc);
			Activity.Create(dc);
			Location.Create(dc);
			ActivityRecurrence.Create(dc);
			ActivityDay.Create(dc);
			ActivitySchedule.Create(dc);
			ActivityInstance.Create(dc);
			ActivityGradeLevel.Create(dc);
			ActivityMessage.Create(dc);
			ActivityMessageRegistrant.Create(dc);
			Archive.Create(dc);
			Account.Create(dc);
			AccountProperty.Create(dc);
			AccountWebUser.Create(dc);
			GradeLevel.Create(dc);
			RegistrationPeriod.Create(dc);
			ActivityRegistrationPeriod.Create(dc);
			ActivityGroup.Create(dc);
			Registrant.Create(dc);
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
			RegistrationActivity.Create(dc);
			Leader.Create(dc);
			ActivityLeader.Create(dc);
			ActivityWebUser.Create(dc);
			Attendance.Create(dc);
			AttendanceRegistrant.Create(dc);
			Package.Create(dc);
			AccountPackage.Create(dc);
			PackageFeature.Create(dc);
			AccountPackageFeature.Create(dc);
			AccountInvoice.Create(dc);
			AccountPayment.Create(dc);
			File.Create(dc);
			ActivityFile.Create(dc);
			Session.Create(dc);
			SessionAction.Create(dc);
			Cart.Create(dc);
			CartItem.Create(dc);
			PasswordResetToken.Create(dc);
			

			return msgs;
		}

		public static SystemMessages AlterTables(DatabaseConnection dc = null)
		{
			SystemMessages msgs = new SystemMessages();

			WebUser.Alter(dc);
			Activity.Alter(dc);
			Location.Alter(dc);
			ActivityRecurrence.Alter(dc);
			ActivityDay.Alter(dc);
			ActivitySchedule.Alter(dc);
			ActivityInstance.Alter(dc);
			ActivityGradeLevel.Alter(dc);
			ActivityMessage.Alter(dc);
			ActivityMessageRegistrant.Alter(dc);
			Archive.Alter(dc);
			Account.Alter(dc);
			AccountProperty.Alter(dc);
			AccountWebUser.Alter(dc);
			GradeLevel.Alter(dc);
			RegistrationPeriod.Alter(dc);
			ActivityRegistrationPeriod.Alter(dc);
			ActivityGroup.Alter(dc);
			Registrant.Alter(dc);
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
			RegistrationActivity.Alter(dc);
			Leader.Alter(dc);
			ActivityLeader.Alter(dc);
			ActivityWebUser.Alter(dc);
			Attendance.Alter(dc);
			AttendanceRegistrant.Alter(dc);
			Package.Alter(dc);
			AccountPackage.Alter(dc);
			PackageFeature.Alter(dc);
			AccountPackageFeature.Alter(dc);
			AccountInvoice.Alter(dc);
			AccountPayment.Alter(dc);
			File.Alter(dc);
			ActivityFile.Alter(dc);
			Session.Alter(dc);
			SessionAction.Alter(dc);
			Cart.Alter(dc);
			CartItem.Alter(dc);
			PasswordResetToken.Alter(dc);
			

			return msgs;
		}
		
		public static SystemMessages UpdateData(DatabaseConnection dc = null)
		{
			SystemMessages msgs = new SystemMessages();

			//**UPDATES**//
			WebUser.Update(new Hashtable(){{"IsActive",false}}, new Hashtable(){{"IsActive", DBNull.Value}}, WebUser.ObjectMap);
			WebUser.Update(new Hashtable(){{"VerifiedAccount",false}}, new Hashtable(){{"VerifiedAccount", DBNull.Value}}, WebUser.ObjectMap);
			WebUser.Update(new Hashtable(){{"IsRegistered",false}}, new Hashtable(){{"IsRegistered", DBNull.Value}}, WebUser.ObjectMap);
			Activity.Update(new Hashtable(){{"AllowPreRegistration",false}}, new Hashtable(){{"AllowPreRegistration", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"AcceptPayPal",false}}, new Hashtable(){{"AcceptPayPal", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"AcceptCreditCard",false}}, new Hashtable(){{"AcceptCreditCard", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"AllowToPayLater",false}}, new Hashtable(){{"AllowToPayLater", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"AllowOrganizationRegistration",false}}, new Hashtable(){{"AllowOrganizationRegistration", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"GenderRestriction",0}}, new Hashtable(){{"GenderRestriction", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"RegistrationModeId",0}}, new Hashtable(){{"RegistrationModeId", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"IsDeleted",false}}, new Hashtable(){{"IsDeleted", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"IsPublished",false}}, new Hashtable(){{"IsPublished", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"ScheduleRequiresAllDays",false}}, new Hashtable(){{"ScheduleRequiresAllDays", DBNull.Value}}, Activity.ObjectMap);
			Location.Update(new Hashtable(){{"IsDefault",false}}, new Hashtable(){{"IsDefault", DBNull.Value}}, Location.ObjectMap);
			ActivityDay.Update(new Hashtable(){{"Day",0}}, new Hashtable(){{"Day", DBNull.Value}}, ActivityDay.ObjectMap);
			Account.Update(new Hashtable(){{"IsAdminAccount",false}}, new Hashtable(){{"IsAdminAccount", DBNull.Value}}, Account.ObjectMap);
			Account.Update(new Hashtable(){{"IsActive",false}}, new Hashtable(){{"IsActive", DBNull.Value}}, Account.ObjectMap);
			AccountProperty.Update(new Hashtable(){{"AccountPropertyTypeId",0}}, new Hashtable(){{"AccountPropertyTypeId", DBNull.Value}}, AccountProperty.ObjectMap);
			AccountWebUser.Update(new Hashtable(){{"IsActive",false}}, new Hashtable(){{"IsActive", DBNull.Value}}, AccountWebUser.ObjectMap);
			AccountWebUser.Update(new Hashtable(){{"IsDefault",false}}, new Hashtable(){{"IsDefault", DBNull.Value}}, AccountWebUser.ObjectMap);
			AccountWebUser.Update(new Hashtable(){{"UserTypeId",0}}, new Hashtable(){{"UserTypeId", DBNull.Value}}, AccountWebUser.ObjectMap);
			RegistrationPeriod.Update(new Hashtable(){{"RegistrationModeId",0}}, new Hashtable(){{"RegistrationModeId", DBNull.Value}}, RegistrationPeriod.ObjectMap);
			ActivityRegistrationPeriod.Update(new Hashtable(){{"IsActive",false}}, new Hashtable(){{"IsActive", DBNull.Value}}, ActivityRegistrationPeriod.ObjectMap);
			ActivityGroup.Update(new Hashtable(){{"RegistrationModeId",0}}, new Hashtable(){{"RegistrationModeId", DBNull.Value}}, ActivityGroup.ObjectMap);
			ActivityGroup.Update(new Hashtable(){{"IsPublished",false}}, new Hashtable(){{"IsPublished", DBNull.Value}}, ActivityGroup.ObjectMap);
			Registrant.Update(new Hashtable(){{"RegistrantTypeId",0}}, new Hashtable(){{"RegistrantTypeId", DBNull.Value}}, Registrant.ObjectMap);
			Registrant.Update(new Hashtable(){{"IsMaster",false}}, new Hashtable(){{"IsMaster", DBNull.Value}}, Registrant.ObjectMap);
			RegistrantActivity.Update(new Hashtable(){{"OnWaitingList",false}}, new Hashtable(){{"OnWaitingList", DBNull.Value}}, RegistrantActivity.ObjectMap);
			RegistrantActivity.Update(new Hashtable(){{"PaymentTypeId",0}}, new Hashtable(){{"PaymentTypeId", DBNull.Value}}, RegistrantActivity.ObjectMap);
			RegistrantActivity.Update(new Hashtable(){{"Day",0}}, new Hashtable(){{"Day", DBNull.Value}}, RegistrantActivity.ObjectMap);
			ActivityForm.Update(new Hashtable(){{"IsActive",false}}, new Hashtable(){{"IsActive", DBNull.Value}}, ActivityForm.ObjectMap);
			ActivityForm.Update(new Hashtable(){{"IsPublished",false}}, new Hashtable(){{"IsPublished", DBNull.Value}}, ActivityForm.ObjectMap);
			Form.Update(new Hashtable(){{"IsActive",false}}, new Hashtable(){{"IsActive", DBNull.Value}}, Form.ObjectMap);
			FormQuestion.Update(new Hashtable(){{"IsTemplate",false}}, new Hashtable(){{"IsTemplate", DBNull.Value}}, FormQuestion.ObjectMap);
			FormQuestion.Update(new Hashtable(){{"QuestionTypeId",0}}, new Hashtable(){{"QuestionTypeId", DBNull.Value}}, FormQuestion.ObjectMap);
			FormQuestion.Update(new Hashtable(){{"QuestionDataTypeId",0}}, new Hashtable(){{"QuestionDataTypeId", DBNull.Value}}, FormQuestion.ObjectMap);
			FormQuestion.Update(new Hashtable(){{"IsActive",false}}, new Hashtable(){{"IsActive", DBNull.Value}}, FormQuestion.ObjectMap);
			FormQuestion.Update(new Hashtable(){{"IsRequired",false}}, new Hashtable(){{"IsRequired", DBNull.Value}}, FormQuestion.ObjectMap);
			FormQuestion.Update(new Hashtable(){{"IsLocked",false}}, new Hashtable(){{"IsLocked", DBNull.Value}}, FormQuestion.ObjectMap);
			FormQuestionGroup.Update(new Hashtable(){{"RegistrantTypeId",0}}, new Hashtable(){{"RegistrantTypeId", DBNull.Value}}, FormQuestionGroup.ObjectMap);
			FormQuestionGroup.Update(new Hashtable(){{"QuestionGroupDataTypeId",0}}, new Hashtable(){{"QuestionGroupDataTypeId", DBNull.Value}}, FormQuestionGroup.ObjectMap);
			FormQuestionAnswer.Update(new Hashtable(){{"QuestionDataTypeId",0}}, new Hashtable(){{"QuestionDataTypeId", DBNull.Value}}, FormQuestionAnswer.ObjectMap);
			Registration.Update(new Hashtable(){{"PaymentTypeId",0}}, new Hashtable(){{"PaymentTypeId", DBNull.Value}}, Registration.ObjectMap);
			ActivityWebUser.Update(new Hashtable(){{"IsLeader",false}}, new Hashtable(){{"IsLeader", DBNull.Value}}, ActivityWebUser.ObjectMap);
			Attendance.Update(new Hashtable(){{"IsCompleted",false}}, new Hashtable(){{"IsCompleted", DBNull.Value}}, Attendance.ObjectMap);
			AttendanceRegistrant.Update(new Hashtable(){{"IsPresent",false}}, new Hashtable(){{"IsPresent", DBNull.Value}}, AttendanceRegistrant.ObjectMap);
			AttendanceRegistrant.Update(new Hashtable(){{"IsLate",false}}, new Hashtable(){{"IsLate", DBNull.Value}}, AttendanceRegistrant.ObjectMap);
			Package.Update(new Hashtable(){{"IsActive",false}}, new Hashtable(){{"IsActive", DBNull.Value}}, Package.ObjectMap);
			Package.Update(new Hashtable(){{"IsPopular",false}}, new Hashtable(){{"IsPopular", DBNull.Value}}, Package.ObjectMap);
			AccountPackage.Update(new Hashtable(){{"BillingPeriodTypeId",0}}, new Hashtable(){{"BillingPeriodTypeId", DBNull.Value}}, AccountPackage.ObjectMap);
			AccountPackage.Update(new Hashtable(){{"IsActive",false}}, new Hashtable(){{"IsActive", DBNull.Value}}, AccountPackage.ObjectMap);
			File.Update(new Hashtable(){{"FileTypeId",0}}, new Hashtable(){{"FileTypeId", DBNull.Value}}, File.ObjectMap);
			SessionAction.Update(new Hashtable(){{"SessionActionTypeId",0}}, new Hashtable(){{"SessionActionTypeId", DBNull.Value}}, SessionAction.ObjectMap);
			CartItem.Update(new Hashtable(){{"Day",0}}, new Hashtable(){{"Day", DBNull.Value}}, CartItem.ObjectMap);
			PasswordResetToken.Update(new Hashtable(){{"Used",false}}, new Hashtable(){{"Used", DBNull.Value}}, PasswordResetToken.ObjectMap);
			Activity.Update(new Hashtable(){{"AllowPreRegistration",false}}, new Hashtable(){{"AllowPreRegistration", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"AcceptPayPal",false}}, new Hashtable(){{"AcceptPayPal", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"AcceptCreditCard",false}}, new Hashtable(){{"AcceptCreditCard", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"AllowToPayLater",false}}, new Hashtable(){{"AllowToPayLater", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"AllowOrganizationRegistration",false}}, new Hashtable(){{"AllowOrganizationRegistration", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"GenderRestriction",0}}, new Hashtable(){{"GenderRestriction", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"RegistrationModeId",0}}, new Hashtable(){{"RegistrationModeId", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"IsDeleted",false}}, new Hashtable(){{"IsDeleted", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"IsPublished",false}}, new Hashtable(){{"IsPublished", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"ScheduleRequiresAllDays",false}}, new Hashtable(){{"ScheduleRequiresAllDays", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"AllowPreRegistration",false}}, new Hashtable(){{"AllowPreRegistration", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"AcceptPayPal",false}}, new Hashtable(){{"AcceptPayPal", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"AcceptCreditCard",false}}, new Hashtable(){{"AcceptCreditCard", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"AllowToPayLater",false}}, new Hashtable(){{"AllowToPayLater", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"AllowOrganizationRegistration",false}}, new Hashtable(){{"AllowOrganizationRegistration", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"GenderRestriction",0}}, new Hashtable(){{"GenderRestriction", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"RegistrationModeId",0}}, new Hashtable(){{"RegistrationModeId", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"IsDeleted",false}}, new Hashtable(){{"IsDeleted", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"IsPublished",false}}, new Hashtable(){{"IsPublished", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"ScheduleRequiresAllDays",false}}, new Hashtable(){{"ScheduleRequiresAllDays", DBNull.Value}}, Activity.ObjectMap);
			ActivityInstance.Update(new Hashtable(){{"Day",0}}, new Hashtable(){{"Day", DBNull.Value}}, ActivityInstance.ObjectMap);
			ActivitySchedule.Update(new Hashtable(){{"RepeatsWeekly",false}}, new Hashtable(){{"RepeatsWeekly", DBNull.Value}}, ActivitySchedule.ObjectMap);
			Attendance.Update(new Hashtable(){{"IsCompleted",false}}, new Hashtable(){{"IsCompleted", DBNull.Value}}, Attendance.ObjectMap);
			Activity.Update(new Hashtable(){{"AllowPreRegistration",false}}, new Hashtable(){{"AllowPreRegistration", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"AcceptPayPal",false}}, new Hashtable(){{"AcceptPayPal", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"AcceptCreditCard",false}}, new Hashtable(){{"AcceptCreditCard", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"AllowToPayLater",false}}, new Hashtable(){{"AllowToPayLater", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"AllowOrganizationRegistration",false}}, new Hashtable(){{"AllowOrganizationRegistration", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"GenderRestriction",0}}, new Hashtable(){{"GenderRestriction", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"RegistrationModeId",0}}, new Hashtable(){{"RegistrationModeId", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"IsDeleted",false}}, new Hashtable(){{"IsDeleted", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"IsPublished",false}}, new Hashtable(){{"IsPublished", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"AllowPreRegistration",false}}, new Hashtable(){{"AllowPreRegistration", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"AcceptPayPal",false}}, new Hashtable(){{"AcceptPayPal", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"AcceptCreditCard",false}}, new Hashtable(){{"AcceptCreditCard", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"AllowToPayLater",false}}, new Hashtable(){{"AllowToPayLater", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"AllowOrganizationRegistration",false}}, new Hashtable(){{"AllowOrganizationRegistration", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"GenderRestriction",0}}, new Hashtable(){{"GenderRestriction", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"RegistrationModeId",0}}, new Hashtable(){{"RegistrationModeId", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"IsDeleted",false}}, new Hashtable(){{"IsDeleted", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"IsPublished",false}}, new Hashtable(){{"IsPublished", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"IsArchived",false}}, new Hashtable(){{"IsArchived", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"ScheduleRequiresAllDays",false}}, new Hashtable(){{"ScheduleRequiresAllDays", DBNull.Value}}, Activity.ObjectMap);
			ActivityGroup.Update(new Hashtable(){{"RegistrationModeId",0}}, new Hashtable(){{"RegistrationModeId", DBNull.Value}}, ActivityGroup.ObjectMap);
			ActivityGroup.Update(new Hashtable(){{"IsPublished",false}}, new Hashtable(){{"IsPublished", DBNull.Value}}, ActivityGroup.ObjectMap);
			ActivityGroup.Update(new Hashtable(){{"IsArchived",false}}, new Hashtable(){{"IsArchived", DBNull.Value}}, ActivityGroup.ObjectMap);
			Account.Update(new Hashtable(){{"IsAdminAccount",false}}, new Hashtable(){{"IsAdminAccount", DBNull.Value}}, Account.ObjectMap);
			Account.Update(new Hashtable(){{"IsActive",false}}, new Hashtable(){{"IsActive", DBNull.Value}}, Account.ObjectMap);
			Account.Update(new Hashtable(){{"IsAfterSchool",false}}, new Hashtable(){{"IsAfterSchool", DBNull.Value}}, Account.ObjectMap);
			Package.Update(new Hashtable(){{"IsActive",false}}, new Hashtable(){{"IsActive", DBNull.Value}}, Package.ObjectMap);
			Package.Update(new Hashtable(){{"IsPopular",false}}, new Hashtable(){{"IsPopular", DBNull.Value}}, Package.ObjectMap);
			Package.Update(new Hashtable(){{"IsAfterSchool",false}}, new Hashtable(){{"IsAfterSchool", DBNull.Value}}, Package.ObjectMap);
			WebUser.Update(new Hashtable(){{"IsActive",false}}, new Hashtable(){{"IsActive", DBNull.Value}}, WebUser.ObjectMap);
			WebUser.Update(new Hashtable(){{"VerifiedAccount",false}}, new Hashtable(){{"VerifiedAccount", DBNull.Value}}, WebUser.ObjectMap);
			WebUser.Update(new Hashtable(){{"IsRegistered",false}}, new Hashtable(){{"IsRegistered", DBNull.Value}}, WebUser.ObjectMap);
			Activity.Update(new Hashtable(){{"AllowPreRegistration",false}}, new Hashtable(){{"AllowPreRegistration", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"AcceptPayPal",false}}, new Hashtable(){{"AcceptPayPal", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"AcceptCreditCard",false}}, new Hashtable(){{"AcceptCreditCard", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"AllowToPayLater",false}}, new Hashtable(){{"AllowToPayLater", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"AllowOrganizationRegistration",false}}, new Hashtable(){{"AllowOrganizationRegistration", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"GenderRestriction",0}}, new Hashtable(){{"GenderRestriction", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"RegistrationModeId",0}}, new Hashtable(){{"RegistrationModeId", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"IsDeleted",false}}, new Hashtable(){{"IsDeleted", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"IsPublished",false}}, new Hashtable(){{"IsPublished", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"IsArchived",false}}, new Hashtable(){{"IsArchived", DBNull.Value}}, Activity.ObjectMap);
			Activity.Update(new Hashtable(){{"ScheduleRequiresAllDays",false}}, new Hashtable(){{"ScheduleRequiresAllDays", DBNull.Value}}, Activity.ObjectMap);
			Location.Update(new Hashtable(){{"IsDefault",false}}, new Hashtable(){{"IsDefault", DBNull.Value}}, Location.ObjectMap);
			ActivityDay.Update(new Hashtable(){{"Day",0}}, new Hashtable(){{"Day", DBNull.Value}}, ActivityDay.ObjectMap);
			ActivitySchedule.Update(new Hashtable(){{"RepeatsWeekly",false}}, new Hashtable(){{"RepeatsWeekly", DBNull.Value}}, ActivitySchedule.ObjectMap);
			ActivityInstance.Update(new Hashtable(){{"Day",0}}, new Hashtable(){{"Day", DBNull.Value}}, ActivityInstance.ObjectMap);
			Account.Update(new Hashtable(){{"IsAdminAccount",false}}, new Hashtable(){{"IsAdminAccount", DBNull.Value}}, Account.ObjectMap);
			Account.Update(new Hashtable(){{"IsActive",false}}, new Hashtable(){{"IsActive", DBNull.Value}}, Account.ObjectMap);
			Account.Update(new Hashtable(){{"IsAfterSchool",false}}, new Hashtable(){{"IsAfterSchool", DBNull.Value}}, Account.ObjectMap);
			AccountProperty.Update(new Hashtable(){{"AccountPropertyTypeId",0}}, new Hashtable(){{"AccountPropertyTypeId", DBNull.Value}}, AccountProperty.ObjectMap);
			AccountWebUser.Update(new Hashtable(){{"IsActive",false}}, new Hashtable(){{"IsActive", DBNull.Value}}, AccountWebUser.ObjectMap);
			AccountWebUser.Update(new Hashtable(){{"IsDefault",false}}, new Hashtable(){{"IsDefault", DBNull.Value}}, AccountWebUser.ObjectMap);
			AccountWebUser.Update(new Hashtable(){{"UserTypeId",0}}, new Hashtable(){{"UserTypeId", DBNull.Value}}, AccountWebUser.ObjectMap);
			RegistrationPeriod.Update(new Hashtable(){{"RegistrationModeId",0}}, new Hashtable(){{"RegistrationModeId", DBNull.Value}}, RegistrationPeriod.ObjectMap);
			ActivityRegistrationPeriod.Update(new Hashtable(){{"IsActive",false}}, new Hashtable(){{"IsActive", DBNull.Value}}, ActivityRegistrationPeriod.ObjectMap);
			ActivityGroup.Update(new Hashtable(){{"RegistrationModeId",0}}, new Hashtable(){{"RegistrationModeId", DBNull.Value}}, ActivityGroup.ObjectMap);
			ActivityGroup.Update(new Hashtable(){{"IsPublished",false}}, new Hashtable(){{"IsPublished", DBNull.Value}}, ActivityGroup.ObjectMap);
			ActivityGroup.Update(new Hashtable(){{"IsArchived",false}}, new Hashtable(){{"IsArchived", DBNull.Value}}, ActivityGroup.ObjectMap);
			Registrant.Update(new Hashtable(){{"RegistrantTypeId",0}}, new Hashtable(){{"RegistrantTypeId", DBNull.Value}}, Registrant.ObjectMap);
			Registrant.Update(new Hashtable(){{"IsMaster",false}}, new Hashtable(){{"IsMaster", DBNull.Value}}, Registrant.ObjectMap);
			RegistrantActivity.Update(new Hashtable(){{"OnWaitingList",false}}, new Hashtable(){{"OnWaitingList", DBNull.Value}}, RegistrantActivity.ObjectMap);
			RegistrantActivity.Update(new Hashtable(){{"PaymentTypeId",0}}, new Hashtable(){{"PaymentTypeId", DBNull.Value}}, RegistrantActivity.ObjectMap);
			RegistrantActivity.Update(new Hashtable(){{"Day",0}}, new Hashtable(){{"Day", DBNull.Value}}, RegistrantActivity.ObjectMap);
			ActivityForm.Update(new Hashtable(){{"IsActive",false}}, new Hashtable(){{"IsActive", DBNull.Value}}, ActivityForm.ObjectMap);
			ActivityForm.Update(new Hashtable(){{"IsPublished",false}}, new Hashtable(){{"IsPublished", DBNull.Value}}, ActivityForm.ObjectMap);
			Form.Update(new Hashtable(){{"IsActive",false}}, new Hashtable(){{"IsActive", DBNull.Value}}, Form.ObjectMap);
			FormQuestion.Update(new Hashtable(){{"IsTemplate",false}}, new Hashtable(){{"IsTemplate", DBNull.Value}}, FormQuestion.ObjectMap);
			FormQuestion.Update(new Hashtable(){{"QuestionTypeId",0}}, new Hashtable(){{"QuestionTypeId", DBNull.Value}}, FormQuestion.ObjectMap);
			FormQuestion.Update(new Hashtable(){{"QuestionDataTypeId",0}}, new Hashtable(){{"QuestionDataTypeId", DBNull.Value}}, FormQuestion.ObjectMap);
			FormQuestion.Update(new Hashtable(){{"IsActive",false}}, new Hashtable(){{"IsActive", DBNull.Value}}, FormQuestion.ObjectMap);
			FormQuestion.Update(new Hashtable(){{"IsRequired",false}}, new Hashtable(){{"IsRequired", DBNull.Value}}, FormQuestion.ObjectMap);
			FormQuestion.Update(new Hashtable(){{"IsLocked",false}}, new Hashtable(){{"IsLocked", DBNull.Value}}, FormQuestion.ObjectMap);
			FormQuestionGroup.Update(new Hashtable(){{"RegistrantTypeId",0}}, new Hashtable(){{"RegistrantTypeId", DBNull.Value}}, FormQuestionGroup.ObjectMap);
			FormQuestionGroup.Update(new Hashtable(){{"QuestionGroupDataTypeId",0}}, new Hashtable(){{"QuestionGroupDataTypeId", DBNull.Value}}, FormQuestionGroup.ObjectMap);
			FormQuestionAnswer.Update(new Hashtable(){{"QuestionDataTypeId",0}}, new Hashtable(){{"QuestionDataTypeId", DBNull.Value}}, FormQuestionAnswer.ObjectMap);
			Registration.Update(new Hashtable(){{"PaymentTypeId",0}}, new Hashtable(){{"PaymentTypeId", DBNull.Value}}, Registration.ObjectMap);
			ActivityWebUser.Update(new Hashtable(){{"IsLeader",false}}, new Hashtable(){{"IsLeader", DBNull.Value}}, ActivityWebUser.ObjectMap);
			Attendance.Update(new Hashtable(){{"IsCompleted",false}}, new Hashtable(){{"IsCompleted", DBNull.Value}}, Attendance.ObjectMap);
			AttendanceRegistrant.Update(new Hashtable(){{"IsPresent",false}}, new Hashtable(){{"IsPresent", DBNull.Value}}, AttendanceRegistrant.ObjectMap);
			AttendanceRegistrant.Update(new Hashtable(){{"IsLate",false}}, new Hashtable(){{"IsLate", DBNull.Value}}, AttendanceRegistrant.ObjectMap);
			Package.Update(new Hashtable(){{"IsActive",false}}, new Hashtable(){{"IsActive", DBNull.Value}}, Package.ObjectMap);
			Package.Update(new Hashtable(){{"IsPopular",false}}, new Hashtable(){{"IsPopular", DBNull.Value}}, Package.ObjectMap);
			Package.Update(new Hashtable(){{"IsAfterSchool",false}}, new Hashtable(){{"IsAfterSchool", DBNull.Value}}, Package.ObjectMap);
			AccountPackage.Update(new Hashtable(){{"BillingPeriodTypeId",0}}, new Hashtable(){{"BillingPeriodTypeId", DBNull.Value}}, AccountPackage.ObjectMap);
			AccountPackage.Update(new Hashtable(){{"IsActive",false}}, new Hashtable(){{"IsActive", DBNull.Value}}, AccountPackage.ObjectMap);
			File.Update(new Hashtable(){{"FileTypeId",0}}, new Hashtable(){{"FileTypeId", DBNull.Value}}, File.ObjectMap);
			SessionAction.Update(new Hashtable(){{"SessionActionTypeId",0}}, new Hashtable(){{"SessionActionTypeId", DBNull.Value}}, SessionAction.ObjectMap);
			CartItem.Update(new Hashtable(){{"Day",0}}, new Hashtable(){{"Day", DBNull.Value}}, CartItem.ObjectMap);
			PasswordResetToken.Update(new Hashtable(){{"Used",false}}, new Hashtable(){{"Used", DBNull.Value}}, PasswordResetToken.ObjectMap);
			Registrant.Update(new Hashtable(){{"RegistrantTypeId",0}}, new Hashtable(){{"RegistrantTypeId", DBNull.Value}}, Registrant.ObjectMap);
			Registrant.Update(new Hashtable(){{"IsMaster",false}}, new Hashtable(){{"IsMaster", DBNull.Value}}, Registrant.ObjectMap);
			Registrant.Update(new Hashtable(){{"IsContactOnly",false}}, new Hashtable(){{"IsContactOnly", DBNull.Value}}, Registrant.ObjectMap);
			ActivityGroup.Update(new Hashtable(){{"RegistrationModeId",0}}, new Hashtable(){{"RegistrationModeId", DBNull.Value}}, ActivityGroup.ObjectMap);
			ActivityGroup.Update(new Hashtable(){{"IsPublished",false}}, new Hashtable(){{"IsPublished", DBNull.Value}}, ActivityGroup.ObjectMap);
			ActivityGroup.Update(new Hashtable(){{"IsArchived",false}}, new Hashtable(){{"IsArchived", DBNull.Value}}, ActivityGroup.ObjectMap);
			ActivityGroup.Update(new Hashtable(){{"IsInviteOnly",false}}, new Hashtable(){{"IsInviteOnly", DBNull.Value}}, ActivityGroup.ObjectMap);
			RegistrantActivity.Update(new Hashtable(){{"OnWaitingList",false}}, new Hashtable(){{"OnWaitingList", DBNull.Value}}, RegistrantActivity.ObjectMap);
			RegistrantActivity.Update(new Hashtable(){{"PaymentTypeId",0}}, new Hashtable(){{"PaymentTypeId", DBNull.Value}}, RegistrantActivity.ObjectMap);
			RegistrantActivity.Update(new Hashtable(){{"Day",0}}, new Hashtable(){{"Day", DBNull.Value}}, RegistrantActivity.ObjectMap);
			RegistrantActivity.Update(new Hashtable(){{"OnWaitingList",false}}, new Hashtable(){{"OnWaitingList", DBNull.Value}}, RegistrantActivity.ObjectMap);
			RegistrantActivity.Update(new Hashtable(){{"PaymentTypeId",0}}, new Hashtable(){{"PaymentTypeId", DBNull.Value}}, RegistrantActivity.ObjectMap);
			RegistrantActivity.Update(new Hashtable(){{"Day",0}}, new Hashtable(){{"Day", DBNull.Value}}, RegistrantActivity.ObjectMap);
			Registration.Update(new Hashtable(){{"PaymentTypeId",0}}, new Hashtable(){{"PaymentTypeId", DBNull.Value}}, Registration.ObjectMap);
			Registrant.Update(new Hashtable(){{"RegistrantTypeId",0}}, new Hashtable(){{"RegistrantTypeId", DBNull.Value}}, Registrant.ObjectMap);
			Registrant.Update(new Hashtable(){{"IsMaster",false}}, new Hashtable(){{"IsMaster", DBNull.Value}}, Registrant.ObjectMap);
			Registrant.Update(new Hashtable(){{"IsContactOnly",false}}, new Hashtable(){{"IsContactOnly", DBNull.Value}}, Registrant.ObjectMap);
			ActivitySchedule.Update(new Hashtable(){{"RepeatsWeekly",false}}, new Hashtable(){{"RepeatsWeekly", DBNull.Value}}, ActivitySchedule.ObjectMap);
			ActivityGroup.Update(new Hashtable(){{"RegistrationModeId",0}}, new Hashtable(){{"RegistrationModeId", DBNull.Value}}, ActivityGroup.ObjectMap);
			ActivityGroup.Update(new Hashtable(){{"IsPublished",false}}, new Hashtable(){{"IsPublished", DBNull.Value}}, ActivityGroup.ObjectMap);
			ActivityGroup.Update(new Hashtable(){{"IsArchived",false}}, new Hashtable(){{"IsArchived", DBNull.Value}}, ActivityGroup.ObjectMap);
			ActivityGroup.Update(new Hashtable(){{"IsInviteOnly",false}}, new Hashtable(){{"IsInviteOnly", DBNull.Value}}, ActivityGroup.ObjectMap);
			ActivityGroup.Update(new Hashtable(){{"HasLotteryRun",false}}, new Hashtable(){{"HasLotteryRun", DBNull.Value}}, ActivityGroup.ObjectMap);
			RegistrantActivity.Update(new Hashtable(){{"OnWaitingList",false}}, new Hashtable(){{"OnWaitingList", DBNull.Value}}, RegistrantActivity.ObjectMap);
			RegistrantActivity.Update(new Hashtable(){{"PaymentTypeId",0}}, new Hashtable(){{"PaymentTypeId", DBNull.Value}}, RegistrantActivity.ObjectMap);
			RegistrantActivity.Update(new Hashtable(){{"Day",0}}, new Hashtable(){{"Day", DBNull.Value}}, RegistrantActivity.ObjectMap);
			FormQuestion.Update(new Hashtable(){{"IsTemplate",false}}, new Hashtable(){{"IsTemplate", DBNull.Value}}, FormQuestion.ObjectMap);
			FormQuestion.Update(new Hashtable(){{"QuestionTypeId",0}}, new Hashtable(){{"QuestionTypeId", DBNull.Value}}, FormQuestion.ObjectMap);
			FormQuestion.Update(new Hashtable(){{"QuestionDataTypeId",0}}, new Hashtable(){{"QuestionDataTypeId", DBNull.Value}}, FormQuestion.ObjectMap);
			FormQuestion.Update(new Hashtable(){{"IsActive",false}}, new Hashtable(){{"IsActive", DBNull.Value}}, FormQuestion.ObjectMap);
			FormQuestion.Update(new Hashtable(){{"IsRequired",false}}, new Hashtable(){{"IsRequired", DBNull.Value}}, FormQuestion.ObjectMap);
			FormQuestion.Update(new Hashtable(){{"IsLocked",false}}, new Hashtable(){{"IsLocked", DBNull.Value}}, FormQuestion.ObjectMap);
			//**ENDUPDATES**//

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

        public static void ExecuteTransaction(TransactionAction action, SuccessAction onSuccess = null, FailureAction onFailure = null, DatabaseConnection dc = null, bool forceRollback = false)
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
                msgs.Add(ex.Message, MessageType.Error);
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