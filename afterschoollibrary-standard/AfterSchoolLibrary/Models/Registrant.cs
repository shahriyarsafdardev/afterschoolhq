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
	/// <summary>
    /// A single registrant for an activity, this is the main person who registered
    /// </summary>
	[Serializable]
    public class Registrant : BaseContact
    {
		#region "ClassGenerator"

		public int? AccountId { get { return (int?)MDV["AccountId"].Value; } set { MDV["AccountId"].Value = value; } }
		public int? WebUserId { get { return (int?)MDV["WebUserId"].Value; } set { MDV["WebUserId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public Guid Uid { get { return (Guid)MDV["Uid"].Value; } set { MDV["Uid"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public RegistrantType RegistrantTypeId { get { return (RegistrantType)MDV["RegistrantTypeId"].Value; } set { MDV["RegistrantTypeId"].Value = value; } }
		public int? ParentRegistrantId { get { return (int?)MDV["ParentRegistrantId"].Value; } set { MDV["ParentRegistrantId"].Value = value; } }
		public bool IsMaster { get { return (bool)MDV["IsMaster"].Value; } set { MDV["IsMaster"].Value = value; } }
		public bool IsContactOnly { get { return (bool)MDV["IsContactOnly"].Value; } set { MDV["IsContactOnly"].Value = value; } }
		public int? FirstRegistrantId { get { return (int?)MDV["FirstRegistrantId"].Value; } set { MDV["FirstRegistrantId"].Value = value; } }
		public bool IsParentOrGuardian { get { return (bool)MDV["IsParentOrGuardian"].Value; } set { MDV["IsParentOrGuardian"].Value = value; } }
		public bool AllowTextMessages { get { return (bool)MDV["AllowTextMessages"].Value; } set { MDV["AllowTextMessages"].Value = value; } }
		/// <summary>
		/// The location id of the school the registrant is attending
		/// </summary>
		public int? SchoolLocationId { get { return (int?)MDV["SchoolLocationId"].Value; } set { MDV["SchoolLocationId"].Value = value; } }
		

        public Registrant()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(Registrant).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.Uid = Guid.NewGuid();
			this.RegistrantTypeId = RegistrantType.NotSet;
			this.IsMaster = false;
			this.IsContactOnly = false;
			this.IsParentOrGuardian = false;
			this.AllowTextMessages = false;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(Registrant).Name;
                _dp = BaseContact.get_BaseObjectMap(className);
				_dp.Add(className, "AccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "WebUserId", System.Data.SqlDbType.Int);
				_dp.Add(className, "Uid", System.Data.SqlDbType.UniqueIdentifier);
				_dp.Add(className, "RegistrantTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ParentRegistrantId", System.Data.SqlDbType.Int);
				_dp.Add(className, "IsMaster", System.Data.SqlDbType.Bit);
				_dp.Add(className, "IsContactOnly", System.Data.SqlDbType.Bit);
				_dp.Add(className, "FirstRegistrantId", System.Data.SqlDbType.Int);
				_dp.Add(className, "IsParentOrGuardian", System.Data.SqlDbType.Bit);
				_dp.Add(className, "AllowTextMessages", System.Data.SqlDbType.Bit);
				_dp.Add(className, "SchoolLocationId", System.Data.SqlDbType.Int);
				
                return _dp;
            }
        }

        protected static Registrant LoadObject(System.Data.DataRow row)
        {
            Registrant obj = new Registrant();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(Registrant).Name);
				
                return dq;
            }
        }

        public static Registrant Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<Registrant> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return Registrant.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return Registrant.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.Uid.IsEmpty())
				msgs.Add("Uid is required on Registrant object", MessageType.Error);
			if (this.RegistrantTypeId.IsEmpty())
				msgs.Add("RegistrantTypeId is required on Registrant object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(Registrant).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(Registrant).Name);
        }

        public static SystemMessages Create(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return BaseObject._Create(DatabaseQuery, dc, ObjectMap);
        }

        public static SystemMessages Alter(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            SystemMessages msgs = new SystemMessages();
            BasicLibrary.DataAccess.DatabaseParameters dps;

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AccountId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["WebUserId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Uid"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["RegistrantTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ParentRegistrantId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsMaster"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsContactOnly"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["FirstRegistrantId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsParentOrGuardian"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AllowTextMessages"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["SchoolLocationId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public Registrant Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return Registrant.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<Registrant> LoadByAccountId(int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Registrant> LoadByParentRegistrantId(int ParentRegistrantId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ParentRegistrantId"], ParentRegistrantId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Registrant> LoadByAccountIdByRegistrantTypeId(int AccountId, RegistrantType RegistrantTypeId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			dq.Criterias.Add(ObjectMap["RegistrantTypeId"], RegistrantTypeId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Registrant> LoadByAccountIdByIsContactOnly(int AccountId, bool IsContactOnly, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			dq.Criterias.Add(ObjectMap["IsContactOnly"], IsContactOnly);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static Registrant LoadByUid(Guid Uid, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static List<Registrant> LoadByWebUserId(int WebUserId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["WebUserId"], WebUserId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Registrant> LoadByWebUserIdByIsMaster(int WebUserId, bool IsMaster, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["WebUserId"], WebUserId);
			dq.Criterias.Add(ObjectMap["IsMaster"], IsMaster);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Registrant> LoadByFirstRegistrantId(int FirstRegistrantId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["FirstRegistrantId"], FirstRegistrantId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Registrant> LoadByParentRegistrantIdByAccountId(int ParentRegistrantId, int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ParentRegistrantId"], ParentRegistrantId);
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		

        #endregion

		#region "UserDefined"        

        public string Name
        {
            get
            {
                string rtn = "";
                if (this.RegistrantTypeId == RegistrantType.Individual)
                    rtn = BaseWebUser.BuildDisplayName(this.FirstName, this.LastName);
                else if(this.RegistrantTypeId == RegistrantType.Organization)
                    rtn = this.Company.ToProperCase();

                return rtn;
            }
        }

        public List<FormQuestionAnswer> GetAnswers(int activityId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return FormQuestionAnswer.LoadByRegistrantIdByActivityId(this.ID.Value, activityId, dc: dc)
                .OrderBy(x => x.FormQuestionDisplayOrder)
                .ToList();
        }

        public List<FormQuestionAnswer> GetAnswers(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return FormQuestionAnswer.LoadByRegistrantId(this.ID.Value, msgs, dc)
                .OrderBy(x => x.FormQuestionDisplayOrder)
                .ToList();
        }

        public FormQuestionAnswer GetAnswer(QuestionDataType questionDataType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return FormQuestionAnswer.Load(
                        criterias: new Hashtable() { { "QuestionDataTypeId", (int)questionDataType }, { "RegistrantId", this.ID.Value } }, 
                        msgs: msgs,
                        dc: dc)
                    .FirstOrDefault();
        }

        public static List<Registrant> LoadList(List<int> ids, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], ids, BasicLibrary.DataAccess.DatabaseCriteriaOperatorEnum.In);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public List<Activity> GetActivities(DatabaseConnection dc = null)
        {
            return RegistrantActivity.LoadByRegistrantId(this.ID.Value, dc: dc).Select(x => x.ToActivity()).ToList();
        }

        public List<RegistrantActivity> GetRegistrantActivities(DatabaseConnection dc = null)
        {
            return RegistrantActivity.LoadByRegistrantId(this.ID.Value, dc: dc).ToList();
        }

        private static DatabaseQuery SearchRegistrantsQuery(string searchText, int? contactListId, int accountId, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], accountId);

            if (contactListId.HasValue)
            {
                var contactListQuery = BaseObject.get_DatabaseQuery(ObjectMap, typeof(ContactListRegistrant).Name);
                contactListQuery.UseParameterizedQuery = false;
                contactListQuery.SelectParameters.Clear();
                contactListQuery.SelectParameters.Add(ContactListRegistrant.ObjectMap["RegistrantId"]);
                contactListQuery.Criterias.Add(ContactListRegistrant.ObjectMap["contactListId"], contactListId.Value);

                dq.Criterias.Add(ObjectMap["Id"], contactListQuery, BasicLibrary.DataAccess.DatabaseCriteriaOperatorEnum.In);
            }

            if (searchText.IsNotEmpty())
            {
                var itemList = new List<string>();

                // split list
                itemList.AddRange(searchText.Split(' ').Select(x => x.Trim()).Where(x => x.IsNotBlank()));

                var dcsAnd = new DatabaseCriterias(LogicalOperatorType.And);

                foreach (var item in itemList)
                {
                    var dcs = new DatabaseCriterias(LogicalOperatorType.Or);
                    dcs.Add(ObjectMap["FirstName"], item, BasicLibrary.DataAccess.DatabaseCriteriaOperatorEnum.Like);
                    dcs.Add(ObjectMap["LastName"], item, BasicLibrary.DataAccess.DatabaseCriteriaOperatorEnum.Like);
                    dcs.Add(ObjectMap["Email"], item, BasicLibrary.DataAccess.DatabaseCriteriaOperatorEnum.Like);
                    dcsAnd.Add(dcs);
                }

                dq.Criterias.Add(dcsAnd);
            }

            dq.OrderBys.Add(ObjectMap["FirstName"], OrderByDirectionEnum.Ascending);
            dq.OrderBys.Add(ObjectMap["LastName"], OrderByDirectionEnum.Ascending);

            return dq;
        }

        public static int SearchRegistrantCount(string searchText, int? contactListId, int accountId, DatabaseConnection dc = null)
        {
            return SearchRegistrantsQuery(searchText, contactListId, accountId, dc: dc).GetCount();
        }
        public static List<Registrant> SearchRegistrants(string searchText, int? contactListId, int accountId, int? start, int? length, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = SearchRegistrantsQuery(searchText, contactListId, accountId, dc: dc);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Registrant> LoadByAccountId(int AccountId, int? start, int? length, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCountByAccountId(int AccountId, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            return dq.GetCount();
        }

        public static SystemMessages SendIncompletePayPalPaymentEmail(int id, ActivityForm af)
        {
            var msgs = new SystemMessages();
            var reg = Registrant.Load(id);
            var isAfterSchool = af.ToActivity().GetAccount().IsAfterSchool;

            var body = $"<p>{reg.DisplayName.IfBlank(reg.Company)},</p><p>Our records from PayPal show you have not paid the full amount of {af.ActivityCostPerRegistrant.ToCurrency()} required to register for {af.ActivityName}.</p><p>If you believe this is incorrect, please get in contact with {af.ActivityContactName} via email (<a href=\"mailto:{af.ActivityContactEmail}\">{af.ActivityContactEmail}</a>) to resolve the issue. Until this issue is resolved your registration will be placed on the waiting list.</p>";

            SendGridHelper.SendMail(
                fromEmail: new SendGrid.Helpers.Mail.EmailAddress(
                    AfterSchoolLibrary.Email.AppEmailAddress, 
                    $"{af.ActivityContactName} via {Utilities.AppName(isAfterSchool)}"
                    ),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(reg.Email, reg.DisplayName.IfBlank(reg.Company)),
                subject: $"Error processing your registration for {af.Name}",
                body: $"{body}{(AfterSchoolLibrary.Email.GetAppSignature(isAfterSchool: isAfterSchool, utm_campaign: "paypal_notification"))}",
                replyTo: new SendGrid.Helpers.Mail.EmailAddress(
                    af.ActivityContactEmail, 
                    af.ActivityContactName),
                emailTemplateId: SendGridHelper.GetGenericTemplateId(isAfterSchool)
            );

            return msgs;
        }

        public static SystemMessages SendPayPalPaymentError(int id, ActivityForm af)
        {
            var msgs = new SystemMessages();
            var reg = Registrant.Load(id);
            var isAfterSchool = af.ToActivity().GetAccount().IsAfterSchool;

            var body = $"<p>Hey {reg.DisplayName.IfBlank(reg.Company)},</p><p>Our records from PayPal show your PayPal account or credit card has not been processed for the amount of {af.ActivityCostPerRegistrant.ToCurrency()} required to register for {af.ActivityName}.</p><p>If you believe this is incorrect, please get in contact with {af.ActivityContactName} via email (<a href=\"mailto:{af.ActivityContactEmail}\">{af.ActivityContactEmail}</a>) to resolve the issue. Until this issue is resolved your registration will be placed on the waiting list.</p>";

            SendGridHelper.SendMail(
                fromEmail: new SendGrid.Helpers.Mail.EmailAddress(
                    AfterSchoolLibrary.Email.AppEmailAddress, 
                    $"{af.ActivityContactName} via {Utilities.AppName(isAfterSchool)}"
                ),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(reg.Email, reg.DisplayName.IfBlank(reg.Company)),
                subject: $"Error processing your registration for {af.Name}",
                body: $"{body}{(AfterSchoolLibrary.Email.GetAppSignature(isAfterSchool: isAfterSchool, utm_campaign: "paypal_notification"))}",
                replyTo: new SendGrid.Helpers.Mail.EmailAddress(
                    af.ActivityContactEmail, 
                    af.ActivityContactName),
                emailTemplateId: SendGridHelper.GetGenericTemplateId(isAfterSchool)
            );

            return msgs;
        }
        
        public Registration AddRegistration(int ownerAccountId, int? activityGroupId, RegistrationCompletionStatus completionStatus, int loggedInUserId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var reg = new Registration
            {
                AccountId = ownerAccountId,
                RegistrantId = this.ID,
                ActivityGroupId = activityGroupId,
                CompletionStatusId = completionStatus,
                CreateDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                CreatedBy = loggedInUserId,
                LastModifiedBy = loggedInUserId,
                LastUpdated = DateTime.UtcNow
            };
            msgs.AddRange(reg.Save(dc));

            if (FirstName.IsNotEmpty() && LastName.IsNotEmpty() && Email.IsNotEmpty())
                msgs.AddRange(reg.FindAndSetFirstRegistration(this.FirstName, this.LastName, this.Email, dc));

            return reg;
        }

        public Registration AddRegistrationWithESignature(int ownerAccountId, int? activityGroupId, string esignature, RegistrationCompletionStatus completionStatus, int loggedInUserId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var reg = new Registration
            {
                AccountId = ownerAccountId,
                RegistrantId = this.ID,
                ActivityGroupId = activityGroupId,
                ESignature = esignature,
                CompletionStatusId = completionStatus,
                CreateDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                CreatedBy = loggedInUserId,
                LastModifiedBy = loggedInUserId,
                LastUpdated = DateTime.UtcNow
            };
            msgs.AddRange(reg.Save(dc));

            if (completionStatus == RegistrationCompletionStatus.Completed)
                msgs.AddRange(reg.FindAndSetFirstRegistration(this.FirstName, this.LastName, this.Email, dc));

            return reg;
        }

        public List<Registration> GetRegistrations(DatabaseConnection dc = null)
        {
            return Registration.LoadByRegistrantId(this.ID.Value, dc: dc);
        }

        public int GetRegistrationCount(DatabaseConnection dc = null)
        {
            return Registration.GetCount(new Hashtable() { { "RegistrantId", this.ID.Value } }, dc: dc);
        }
        

        public RegistrantActivity AddOrUpdateActivity(int registrationId, int activityId, RegistrationAssignmentType assignmentType, int? registrationPeriodId, int? activityGroupId, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            int seats = 0, waitingListSeats = 0, applicationSeats = 0, invitationSeats = 0, declinedSeats = 0;

            switch(assignmentType)
            {
                case RegistrationAssignmentType.Applied:
                    applicationSeats = 1;
                    break;
                case RegistrationAssignmentType.Declined:
                    declinedSeats = 1;
                    break;
                case RegistrationAssignmentType.Invited:
                    invitationSeats = 1;
                    break;
                case RegistrationAssignmentType.Reserved:
                    seats = 1;
                    break;
                case RegistrationAssignmentType.WaitingList:
                    waitingListSeats = 1;
                    break;
            }

            return AddOrUpdateActivity(registrationId, activityId, seats, waitingListSeats, applicationSeats, invitationSeats, declinedSeats, assignmentType, 1, registrationPeriodId, activityGroupId, loggedInUserId, msgs, dc);
        }

        [Obsolete]
        public RegistrantActivity AddOrUpdateActivity(int registrationId, int activityId, int seats, int waitingListSeats, int applicationSeats, int invitationSeats, int declinedSeats, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var assignmentType = RegistrationAssignmentType.NotSet;

            if (seats > 0)
                assignmentType = RegistrationAssignmentType.Reserved;
            else if (waitingListSeats > 0)
                assignmentType = RegistrationAssignmentType.WaitingList;
            else if (applicationSeats > 0)
                assignmentType = RegistrationAssignmentType.Applied;
            else if (invitationSeats > 0)
                assignmentType = RegistrationAssignmentType.Invited;
            else if (declinedSeats > 0)
                assignmentType = RegistrationAssignmentType.Declined;

            return AddOrUpdateActivity(registrationId, activityId, seats, waitingListSeats, applicationSeats, invitationSeats, declinedSeats, assignmentType, 1, null, null, loggedInUserId, msgs, dc);
        }

        public RegistrantActivity AddOrUpdateActivity(int registrationId, int activityId, int seats, int waitingListSeats, int applicationSeats, int invitationSeats, int declinedSeats, RegistrationAssignmentType assignmentType, int preferenceNumber, int? registrationPeriodId, int? activityGroupId, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            // var changingDays = false;

            var ra = RegistrantActivity.LoadByActivityIdByRegistrantId(activityId, this.ID.Value, msgs, dc);

            if (ra.IsEmpty())
            {
                ra = new RegistrantActivity
                {
                    ActivityId = activityId,
                    RegistrantId = this.ID,
                    RegistrationId = registrationId,
                    ActivityGroupId = activityGroupId,
                    RegistrationPeriodId = registrationPeriodId,
                    CreateDate = DateTime.UtcNow,
                    CreatedBy = loggedInUserId
                };
                // changingDays = true;
            }
            else
            {
                /*
                if (day != ra.Day)
                    changingDays = true;
                */
            }

            // ra.Day = day;
            ra.LastModifiedDate = DateTime.UtcNow;
            ra.LastModifiedBy = loggedInUserId;
            ra.PreferenceNumber = preferenceNumber;
            ra.AssignmentTypeId = assignmentType;

            //set indexes
            //if (changingDays)
            //{
                ra.NumberOfSeats = seats;
                ra.NumberOfWaitingListSeats = waitingListSeats;
                ra.NumberOfApplicationSeats = applicationSeats;
                ra.NumberOfInvitationSeats = invitationSeats;
                ra.NumberOfSeatsDeclined = declinedSeats;

                //var days = new List<DayOfWeekASL>();
                //if (day != DayOfWeekASL.NotSet)
                //    days.Add(day);

                if (seats > 0)
                    ra.SeatIndex = RegistrantActivity.GetSeatCountByAssignmentType(
                        rat: RegistrationAssignmentType.Reserved, 
                        activityId: activityId, 
                        msgs: msgs, 
                        dc: dc) + 1;

                if (waitingListSeats > 0)
                    ra.WaitingListSeatIndex = RegistrantActivity.GetSeatCountByAssignmentType(
                        rat: RegistrationAssignmentType.WaitingList, 
                        activityId: activityId, 
                        msgs: msgs, 
                        dc: dc) + 1;

                if (applicationSeats > 0)
                    ra.ApplicationSeatIndex = RegistrantActivity.GetSeatCountByAssignmentType(
                        rat: RegistrationAssignmentType.Applied, 
                        activityId: activityId, 
                        msgs: msgs, 
                        dc: dc) + 1;
            // }

            msgs.AddRange(ra.Save(dc: dc));

            return ra;
        }

        public FormQuestionAnswer MapAndAddAnswer(List<FormQuestion> questions, Registrant fromRegistrant, QuestionDataType fromType, QuestionDataType toType, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var parentAnswer = fromRegistrant.GetAnswer(fromType, msgs, dc);
            var studentQuestion = questions.Find(question => question.QuestionDataTypeId == toType);
            return this.AddAnswer(parentAnswer.Text, studentQuestion, loggedInUser.ID.Value, msgs, dc);
        }

        public FormQuestionAnswer AddAnswer(string answerText, FormQuestion question, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return AddAnswer(answerText, question, null, loggedInUserId, msgs, dc);
        }

        public FormQuestionAnswer AddAnswer(string answerText, FormQuestion question, int? registrationId, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return AddAnswer(answerText, question.ID, question.QuestionDataTypeId, question.Text, registrationId, loggedInUserId, msgs, dc);
        }

        public FormQuestionAnswer AddAnswer(string answerText, int? questionId, QuestionDataType questionDataType, string questionText, int? registrationId, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            //check if answer for this one already exists, and where formquestion is null
            FormQuestionAnswer answer = null;

            if (questionDataType == QuestionDataType.NotSet)
                answer = FormQuestionAnswer.LoadByRegistrantIdByFormQuestionIdByRegistrationId(
                    RegistrantId: this.ID.Value,
                    FormQuestionId: questionId.Value,
                    RegistrationId: registrationId.Value,
                    msgs: msgs, 
                    dc: dc);
            else
                answer = FormQuestionAnswer.LoadByRegistrantIdByQuestionTypeId(this.ID.Value, questionDataType, msgs, dc);

            if (answer.IsEmpty())
            {
                answer = new FormQuestionAnswer
                {
                    RegistrantId = this.ID,
                    FormQuestionId = questionId.Value,
                    QuestionDataTypeId = questionDataType,
                    QuestionText = questionText,
                    CreateDate = DateTime.UtcNow,
                    CreatedBy = loggedInUserId
                };
            }
            answer.RegistrationId = registrationId;
            answer.Text = answerText;
            answer.LastModifiedDate = DateTime.UtcNow;
            answer.LastModifiedBy = loggedInUserId;
            msgs.AddRange(answer.Save(dc));

            msgs.AddRange(answer.UpdateRegistrantInfo(this, dc: dc));

            return answer;
        }

        public RegistrantActivity AddInviteToActivity(int activityId, int loggedInUserId, int? activityGroupId, int? registrationPeriodId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var ra = RegistrantActivity.LoadByActivityIdByRegistrantId(activityId, this.ID.Value, msgs, dc);
            if (ra.IsEmpty())
            {
                ra = new RegistrantActivity();
                ra.ActivityId = activityId;
                ra.RegistrantId = this.ID;
                ra.CreateDate = DateTime.UtcNow;
                ra.CreatedBy = loggedInUserId;
                ra.ActivityGroupId = activityGroupId;
            }
            ra.LastModifiedDate = DateTime.UtcNow;
            ra.LastModifiedBy = loggedInUserId;

            // TODO: re-evaluate if re-using this record for registration also
            ra.NumberOfInvitationSeats = 1;
            ra.NumberOfApplicationSeats = 0;
            ra.NumberOfSeats = 0;
            ra.NumberOfWaitingListSeats = 0;
            ra.NumberOfSeatsDeclined = 0;

            ra.RegistrationPeriodId = registrationPeriodId;
            msgs.AddRange(ra.Save(dc: dc));
            return ra;
        }

        public SystemMessages AddInviteToActivities(List<int> activityIds, int loggedInUserId, int? activityGroupId, int? registrationPeriodId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            activityIds.ForEach(activityId =>
                this.AddInviteToActivity(activityId, loggedInUserId, activityGroupId, registrationPeriodId, msgs, dc)
            );
            return msgs;
        }        

        public Registrant CreateMasterCopy(List<FormQuestion> questions, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            var master = this.CopyWithAnswers(questions, loggedInUserId, msgs, dc);
            master.IsMaster = true;
            master.IsContactOnly = true;
            master.AccountId = null;
            msgs.AddRange(master.Save(dc));

            this.ParentRegistrantId = master.ID;
            msgs.AddRange(this.Save(dc: dc));

            return master;
        }

        public Registrant Copy(int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var copy = new Registrant();
            copy.CopyDataValues(this.OriginalDataValues, this.ModifiedDataValues);
            copy.CreateDate = copy.LastModifiedDate = DateTime.UtcNow;
            copy.CreatedBy = copy.LastModifiedBy = loggedInUserId;
            copy.Uid = Guid.NewGuid();
            copy.ResetID();
            msgs.AddRange(copy.Save(dc: dc));

            return copy;
        }

        public Registrant CopyWithAnswers(List<FormQuestion> questions, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var copy = this.Copy(loggedInUserId, msgs, dc);

            if (questions.IsEmpty())
                questions.ForEach(question => 
                    copy.AddAnswer(question.GetAnswer(this.ID.Value, msgs, dc).Text, question, null, loggedInUserId, msgs, dc)
                );

            return copy;
        }

        public void CopyAnswers(Registrant copyingRegistrant, Registration registration, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Database.ExecuteTransaction
              (
                  action: (SystemMessages amsgs, DatabaseConnection adc) =>
                  {
                      // copy all answers
                      var answers = this.GetAnswers(amsgs, adc);
                      answers.ForEach(answer =>
                      {
                          copyingRegistrant.AddAnswer(
                              answerText: answer.Text,
                              question: answer.ToFormQuestion(true, amsgs, adc),
                              registrationId: registration?.ID,
                              loggedInUserId: loggedInUser?.ID ?? 0,
                              msgs: amsgs,
                              dc: adc);
                      });
                  },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);
                },
                onFailure: (SystemMessages fmsgs) =>
                {
                    msgs.AddRange(fmsgs);
                },
                dc: dc
            );
        }

        public bool IsRegisteredFor(ParticipationObject participationObject, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (participationObject.ObjectType == ParticipationObjectType.Activity)
            {
                var childrenRegistrants = this.GetChildren(participationObject.OwnerAccount.ID.Value, msgs, dc);

                return childrenRegistrants.Any() && RegistrantActivity.ExistsByActivityIdByRegistrantIds(
                    activityId: participationObject.ObjectID.Value,
                    registrantIds: childrenRegistrants.Select(registrant => registrant.ID.Value).ToList(),
                    msgs: msgs,
                    dc: dc);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public Account GetAccount(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return this.AccountId.HasValue ? Account.Load(this.AccountId.Value, msgs, dc) : null;
        }

        public Registrant GetMasterRegistrant(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return this.ParentRegistrantId.HasValue ? Registrant.Load(this.ParentRegistrantId.Value, msgs, dc) : null;
        }

        public Registrant CopyToAccount(Account ownerAccount, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Registrant copy = null;

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    copy = this.Copy(loggedInUser?.ID ?? 0, msgs, dc);
                    copy.AccountId = ownerAccount.ID;
                    copy.FirstRegistrantId = null;
                    copy.ParentRegistrantId = this.ID;
                    copy.IsContactOnly = false;
                    copy.IsMaster = false;
                    amsgs.AddRange(copy.Save(adc));

                    // need to relate registrants within the same account, specifically provider or school accounts
                    amsgs.AddRange(copy.FindAndSetFirstRegistrant(loggedInUser, adc));
                   
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);
                },
                onFailure: (SystemMessages fmsgs) =>
                {
                    msgs.AddRange(fmsgs);
                },
                dc: dc
            );

            

            return copy;
        }

        public List<Registrant> GetChildren(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registrant.LoadByParentRegistrantId(this.ID.Value, msgs, dc);
        }

        public List<Registrant> GetChildren(int accountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registrant.LoadByParentRegistrantIdByAccountId(this.ID.Value, accountId, msgs, dc);
        }

        public static Registrant LoadByNameByEmail(int accountId, string firstName, string lastName, string email, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["accountId"], accountId);
            dq.Criterias.Add(ObjectMap["firstName"], firstName);
            dq.Criterias.Add(ObjectMap["lastName"], lastName);
            dq.Criterias.Add(ObjectMap["email"], email);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static Registrant LoadByEmailOnly(int accountId, string email, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;

            dq.Criterias.Add(ObjectMap["accountId"], accountId);
            dq.Criterias.Add(ObjectMap["email"], email);

            var dcs1 = new DatabaseCriterias(LogicalOperatorType.Or);
            dcs1.Add(ObjectMap["firstname"], DBNull.Value, DatabaseCriteriaOperatorEnum.Is);
            dcs1.Add(ObjectMap["firstname"], "");
            dq.Criterias.Add(dcs1);

            var dcs2 = new DatabaseCriterias(LogicalOperatorType.Or);
            dcs2.Add(ObjectMap["lastname"], DBNull.Value, DatabaseCriteriaOperatorEnum.Is);
            dcs2.Add(ObjectMap["lastname"], "");
            dq.Criterias.Add(dcs2);

            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public List<Activity> GetActivitiesByDay(DayOfWeekASL day, DatabaseConnection dc = null)
        {
            return Activity.GetActivitiesByRegistrantByDay(this.ID.Value, day, DateTime.UtcNow, dc: dc);
        }
        
        public static List<Registrant> LoadFirstRegistrants(int accountId, int? start, int? length, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.Criterias.Add(ObjectMap["AccountId"], accountId);
            dq.Criterias.Add(ObjectMap["FirstRegistrantId"], DBNull.Value, DatabaseCriteriaOperatorEnum.Is);
            dq.OrderBys.Add(ObjectMap["FirstName"], OrderByDirectionEnum.Ascending);
            dq.OrderBys.Add(ObjectMap["LastName"], OrderByDirectionEnum.Ascending);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public SystemMessages FindAndSetFirstRegistrant(WebUser loggedInUser, DatabaseConnection dc = null)
        {
            return FindAndSetFirstRegistrant(this.FirstName, this.LastName, this.Email, loggedInUser, dc);
        }

        public SystemMessages FindAndSetFirstRegistrant(string firstName, string lastName, string email, WebUser loggedInUser, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.Criterias.Add(ObjectMap["AccountId"], this.AccountId);
            dq.Criterias.Add(ObjectMap["FirstRegistrantId"], DBNull.Value, DatabaseCriteriaOperatorEnum.Is);
            dq.Criterias.Add(ObjectMap["FirstName"], firstName);
            dq.Criterias.Add(ObjectMap["LastName"], lastName);
            dq.Criterias.Add(ObjectMap["Email"], email);
            dq.Criterias.Add(ObjectMap["ID"], this.ID.Value, DatabaseCriteriaOperatorEnum.NotEqualTo);
            dq.OrderBys.Add(ObjectMap["Id"], OrderByDirectionEnum.Ascending);

            var firstRegistrant = dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();

            if (firstRegistrant.IsNotEmpty())
            {
                this.FirstRegistrantId = firstRegistrant.ID;
                this.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);
                this.LastModifiedDate = DateTime.UtcNow;
                msgs.AddRange(this.Save(dc));
            }

            return msgs;
        }

        public static List<Registrant> FindRelatedRegistrants(int AccountId, string firstName, string lastName, string email, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.Criterias.Add(ObjectMap["FirstName"], firstName);
            dq.Criterias.Add(ObjectMap["LastName"], lastName);
            dq.Criterias.Add(ObjectMap["Email"], email);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public List<Registrant> FindRelatedRegistrants(string firstName = null, string lastName = null, string email = null, DatabaseConnection dc = null)
        {
            return FindRelatedRegistrants(
                this.AccountId.Value,
                firstName ?? this.FirstName,
                lastName ?? this.LastName,
                email ?? this.Email,
                dc);
        }

        public SystemMessages AddRelatedRegistrant(Registrant registrant, int loggedInUserId, DatabaseConnection dc = null)
        {
            registrant.FirstRegistrantId = this.ID.Value;
            registrant.LastModifiedBy = loggedInUserId;
            registrant.LastModifiedDate = DateTime.UtcNow;
            return registrant.Save(dc: dc);
        }

        public List<Registrant> GetRelatedRegistrants(DatabaseConnection dc = null)
        {
            var registrants = new List<Registrant>();
            if (this.FirstRegistrantId.HasValue)
            {
                registrants.Add(Registrant.Load(this.FirstRegistrantId.Value, dc: dc));
                registrants.AddRange(Registrant.LoadByFirstRegistrantId(this.FirstRegistrantId.Value, dc: dc));
            }
            else
                registrants.AddRange(Registrant.LoadByFirstRegistrantId(this.ID.Value, dc: dc));

            return registrants.Where(x => x.ID != this.ID).OrderByDescending(x => x.CreateDate).ToList();
        }

        public bool IsRelated(Registrant registrant)
        {
            return this.FirstName.Trim().ToLower() == registrant.FirstName.Trim().ToLower() &&
                    this.LastName.Trim().ToLower() == registrant.LastName.Trim().ToLower() &&
                    this.Email.Trim().ToLower() == registrant.Email.Trim().ToLower();
        }

        public SystemMessages DeleteWithRelatedRecords(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    amsgs.AddRange(this.DeleteSessionActions(adc));
                    amsgs.AddRange(this.DeleteRegistrantActivities(adc));
                    amsgs.AddRange(this.Delete(adc));
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);
                },
                onFailure: (SystemMessages fmsgs) =>
                {
                    msgs.AddRange(fmsgs);
                },
                dc: dc
            );

            return msgs;
        }

        internal SystemMessages DeleteSessionActions(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            // delete related session actions
            var sas = SessionAction.LoadByRegistrantId(this.ID.Value, dc: dc);
            foreach (var sa in sas)
            {
                msgs.AddRange(sa.Delete(dc: dc));
            }

            return msgs;
        }

        internal SystemMessages DeleteRegistrantActivities(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            // delete related activities
            var ras = this.GetRegistrantActivities(dc: dc);
            foreach (var ra in ras)
            {
                msgs.AddRange(ra.CancelRegistration(dc: dc));
            }

            return msgs;
        }        

        public Registrant GetParentOrGuardian(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registrant.LoadByAccountId(this.AccountId.Value, msgs, dc).Where(registrant => registrant.IsParentOrGuardian).OrderBy(registrant => registrant.ID).FirstOrDefault();
        }

        public SystemMessages DeleteRegistrantActivity(int activityId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var registrantAcitivity = RegistrantActivity.LoadByActivityIdByRegistrantId(activityId, this.ID.Value, msgs, dc);

            msgs.AddRange(registrantAcitivity.Delete(dc));

            return msgs;
        }
        
        public decimal GetAmountPaid(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityPayment.LoadByRegistrantId(this.ID.Value, msgs, dc).Sum(payment => payment.Amount.Value);
        }
        
        /*
        public List<Attendance> GetAttendance(int ActivityId, DatabaseConnection dc = null)
        {
            return Attendance.LoadByRegistrantIdByActivityId(this.ID.Value, ActivityId, dc: dc);
        }

        public List<Attendance> GetAttendanceByDate(DateTime date, DatabaseConnection dc = null)
        {
            return Attendance.LoadByRegistrantIdByDate(this.ID.Value, date, dc: dc);
        }

        public Attendance GetAttendanceByActivityByDate(int ActivityId, DateTime date, DatabaseConnection dc = null)
        {
            return Attendance.LoadByActivityIdByRegistrantIdByDate(ActivityId, this.ID.Value, date, dc: dc);
        }       
         * */

        #endregion
    }
}
