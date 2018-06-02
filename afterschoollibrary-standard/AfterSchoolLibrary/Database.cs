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
			// ActivityRecurrence.Create(dc);
			ActivityDay.Create(dc);
			ActivitySchedule.Create(dc);
			ActivityInstance.Create(dc);
			ActivityGradeLevel.Create(dc);
			ActivityMessage.Create(dc);
			ActivityMessageRegistrant.Create(dc);
			//Archive.Create(dc);
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
			// Leader.Create(dc);
			// ActivityLeader.Create(dc);
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
			// ActivityRecurrence.Alter(dc);
			ActivityDay.Alter(dc);
			ActivitySchedule.Alter(dc);
			ActivityInstance.Alter(dc);
			ActivityGradeLevel.Alter(dc);
			ActivityMessage.Alter(dc);
			ActivityMessageRegistrant.Alter(dc);
			// Archive.Alter(dc);
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
			// Leader.Alter(dc);
			// ActivityLeader.Alter(dc);
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