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
    /// 
    /// </summary>
	
    public class RegistrantActivity : BasicObjectWithModified
    {
		#region "ClassGenerator"

		public int? RegistrantId { get { return (int?)MDV["RegistrantId"].Value; } set { MDV["RegistrantId"].Value = value; } }
		public int? ActivityId { get { return (int?)MDV["ActivityId"].Value; } set { MDV["ActivityId"].Value = value; } }
		public int? RegistrationId { get { return (int?)MDV["RegistrationId"].Value; } set { MDV["RegistrationId"].Value = value; } }
		public Guid Uid { get { return (Guid)MDV["Uid"].Value; } set { MDV["Uid"].Value = value; } }
		public bool OnWaitingList { get { return (bool)MDV["OnWaitingList"].Value; } set { MDV["OnWaitingList"].Value = value; } }
		public DateTime? DatePaid { get { return (DateTime?)MDV["DatePaid"].Value; } set { MDV["DatePaid"].Value = value; } }
		public string StripeChargeId { get { return (string)MDV["StripeChargeId"].Value; } set { MDV["StripeChargeId"].Value = value; } }
		public string PayPalTransactionId { get { return (string)MDV["PayPalTransactionId"].Value; } set { MDV["PayPalTransactionId"].Value = value; } }
		public decimal? AmountPaid { get { return (decimal?)MDV["AmountPaid"].Value; } set { MDV["AmountPaid"].Value = value; } }
		public PaymentType PaymentTypeId { get { return (PaymentType)MDV["PaymentTypeId"].Value; } set { MDV["PaymentTypeId"].Value = value; } }
		public int? NumberOfSeats { get { return (int?)MDV["NumberOfSeats"].Value; } set { MDV["NumberOfSeats"].Value = value; } }
		public int? NumberOfWaitingListSeats { get { return (int?)MDV["NumberOfWaitingListSeats"].Value; } set { MDV["NumberOfWaitingListSeats"].Value = value; } }
		public int? NumberOfApplicationSeats { get { return (int?)MDV["NumberOfApplicationSeats"].Value; } set { MDV["NumberOfApplicationSeats"].Value = value; } }
		public int? NumberOfInvitationSeats { get { return (int?)MDV["NumberOfInvitationSeats"].Value; } set { MDV["NumberOfInvitationSeats"].Value = value; } }
		public int? NumberOfSeatsDeclined { get { return (int?)MDV["NumberOfSeatsDeclined"].Value; } set { MDV["NumberOfSeatsDeclined"].Value = value; } }
		public int? SeatIndex { get { return (int?)MDV["SeatIndex"].Value; } set { MDV["SeatIndex"].Value = value; } }
		public int? WaitingListSeatIndex { get { return (int?)MDV["WaitingListSeatIndex"].Value; } set { MDV["WaitingListSeatIndex"].Value = value; } }
		public int? ApplicationSeatIndex { get { return (int?)MDV["ApplicationSeatIndex"].Value; } set { MDV["ApplicationSeatIndex"].Value = value; } }
		public DayOfWeekASL Day { get { return (DayOfWeekASL)MDV["Day"].Value; } set { MDV["Day"].Value = value; } }
		public int? PreferenceNumber { get { return (int?)MDV["PreferenceNumber"].Value; } set { MDV["PreferenceNumber"].Value = value; } }
		public int? RegistrationPeriodId { get { return (int?)MDV["RegistrationPeriodId"].Value; } set { MDV["RegistrationPeriodId"].Value = value; } }
		public int? ActivityGroupId { get { return (int?)MDV["ActivityGroupId"].Value; } set { MDV["ActivityGroupId"].Value = value; } }
		public int? Rating { get { return (int?)MDV["Rating"].Value; } set { MDV["Rating"].Value = value; } }
		public string RegistrantEmail { get { return (string)MDV["RegistrantEmail"].Value; } }
		public string RegistrantFirstName { get { return (string)MDV["RegistrantFirstName"].Value; } }
		public string RegistrantLastName { get { return (string)MDV["RegistrantLastName"].Value; } }
		public Guid RegistrantUid { get { return (Guid)MDV["RegistrantUid"].Value; } }
		public string RegistrantPhone { get { return (string)MDV["RegistrantPhone"].Value; } }
		public string RegistrantCompany { get { return (string)MDV["RegistrantCompany"].Value; } }
		public RegistrantType RegistrantRegistrantTypeId { get { return (RegistrantType)MDV["RegistrantRegistrantTypeId"].Value; } }
		public int? RegistrantFirstRegistrantId { get { return (int?)MDV["RegistrantFirstRegistrantId"].Value; } }
		public string ActivityName { get { return (string)MDV["ActivityName"].Value; } }
		public Guid ActivityUid { get { return (Guid)MDV["ActivityUid"].Value; } }
		public int? ActivityAccountId { get { return (int?)MDV["ActivityAccountId"].Value; } }
		public decimal? ActivityCostPerRegistrant { get { return (decimal?)MDV["ActivityCostPerRegistrant"].Value; } }
		public bool ActivityIsDeleted { get { return (bool)MDV["ActivityIsDeleted"].Value; } }
		public bool ActivityScheduleRequiresAllDays { get { return (bool)MDV["ActivityScheduleRequiresAllDays"].Value; } }
		public string AccountName { get { return (string)MDV["AccountName"].Value; } }
		public bool ActivityIsArchived { get { return (bool)MDV["ActivityIsArchived"].Value; } }
		public int? ActivityActivityGroupId { get { return (int?)MDV["ActivityActivityGroupId"].Value; } }
		public int? ActivityActivityScheduleId { get { return (int?)MDV["ActivityActivityScheduleId"].Value; } }
		public bool ActivityGroupIsArchived { get { return (bool)MDV["ActivityGroupIsArchived"].Value; } }
		

        public RegistrantActivity()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(RegistrantActivity).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.Uid = Guid.NewGuid();
			this.OnWaitingList = false;
			this.PaymentTypeId = PaymentType.NotSet;
			this.Day = DayOfWeekASL.NotSet;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(RegistrantActivity).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "RegistrantId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ActivityId", System.Data.SqlDbType.Int);
				_dp.Add(className, "RegistrationId", System.Data.SqlDbType.Int);
				_dp.Add(className, "Uid", System.Data.SqlDbType.UniqueIdentifier);
				_dp.Add(className, "OnWaitingList", System.Data.SqlDbType.Bit);
				_dp.Add(className, "DatePaid", System.Data.SqlDbType.DateTime);
				_dp.Add(className, "StripeChargeId", System.Data.SqlDbType.NVarChar, 100);
				_dp.Add(className, "PayPalTransactionId", System.Data.SqlDbType.NVarChar, 50);
				_dp.Add(className, "AmountPaid", System.Data.SqlDbType.Money);
				_dp.Add(className, "PaymentTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "NumberOfSeats", System.Data.SqlDbType.Int);
				_dp.Add(className, "NumberOfWaitingListSeats", System.Data.SqlDbType.Int);
				_dp.Add(className, "NumberOfApplicationSeats", System.Data.SqlDbType.Int);
				_dp.Add(className, "NumberOfInvitationSeats", System.Data.SqlDbType.Int);
				_dp.Add(className, "NumberOfSeatsDeclined", System.Data.SqlDbType.Int);
				_dp.Add(className, "SeatIndex", System.Data.SqlDbType.Int);
				_dp.Add(className, "WaitingListSeatIndex", System.Data.SqlDbType.Int);
				_dp.Add(className, "ApplicationSeatIndex", System.Data.SqlDbType.Int);
				_dp.Add(className, "Day", System.Data.SqlDbType.Int);
				_dp.Add(className, "PreferenceNumber", System.Data.SqlDbType.Int);
				_dp.Add(className, "RegistrationPeriodId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ActivityGroupId", System.Data.SqlDbType.Int);
				_dp.Add(className, "Rating", System.Data.SqlDbType.Int);
				_dp.Add(className, "RegistrantEmail", typeof(Registrant).Name, "Email", System.Data.SqlDbType.NVarChar, "Registrant");
				_dp.Add(className, "RegistrantFirstName", typeof(Registrant).Name, "FirstName", System.Data.SqlDbType.NVarChar, "Registrant");
				_dp.Add(className, "RegistrantLastName", typeof(Registrant).Name, "LastName", System.Data.SqlDbType.NVarChar, "Registrant");
				_dp.Add(className, "RegistrantUid", typeof(Registrant).Name, "Uid", System.Data.SqlDbType.UniqueIdentifier, "Registrant");
				_dp.Add(className, "RegistrantPhone", typeof(Registrant).Name, "Phone", System.Data.SqlDbType.NVarChar, "Registrant");
				_dp.Add(className, "RegistrantCompany", typeof(Registrant).Name, "Company", System.Data.SqlDbType.NVarChar, "Registrant");
				_dp.Add(className, "RegistrantRegistrantTypeId", typeof(Registrant).Name, "RegistrantTypeId", System.Data.SqlDbType.Int, "Registrant");
				_dp.Add(className, "RegistrantFirstRegistrantId", typeof(Registrant).Name, "FirstRegistrantId", System.Data.SqlDbType.Int, "Registrant");
				_dp.Add(className, "ActivityName", typeof(Activity).Name, "Name", System.Data.SqlDbType.NVarChar, "Activity");
				_dp.Add(className, "ActivityUid", typeof(Activity).Name, "Uid", System.Data.SqlDbType.UniqueIdentifier, "Activity");
				_dp.Add(className, "ActivityAccountId", typeof(Activity).Name, "AccountId", System.Data.SqlDbType.Int, "Activity");
				_dp.Add(className, "ActivityCostPerRegistrant", typeof(Activity).Name, "CostPerRegistrant", System.Data.SqlDbType.Money, "Activity");
				_dp.Add(className, "ActivityIsDeleted", typeof(Activity).Name, "IsDeleted", System.Data.SqlDbType.Bit, "Activity");
				_dp.Add(className, "ActivityScheduleRequiresAllDays", typeof(Activity).Name, "ScheduleRequiresAllDays", System.Data.SqlDbType.Bit, "Activity");
				_dp.Add(className, "AccountName", typeof(Account).Name, "Name", System.Data.SqlDbType.NVarChar, "Account");
				_dp.Add(className, "ActivityIsArchived", typeof(Activity).Name, "IsArchived", System.Data.SqlDbType.Bit, "Activity");
				_dp.Add(className, "ActivityActivityGroupId", typeof(Activity).Name, "ActivityGroupId", System.Data.SqlDbType.Int, "Activity");
				_dp.Add(className, "ActivityActivityScheduleId", typeof(Activity).Name, "ActivityScheduleId", System.Data.SqlDbType.Int, "Activity");
				_dp.Add(className, "ActivityGroupIsArchived", typeof(ActivityGroup).Name, "IsArchived", System.Data.SqlDbType.Bit, "ActivityGroup");
				
                return _dp;
            }
        }

        protected static RegistrantActivity LoadObject(System.Data.DataRow row)
        {
            RegistrantActivity obj = new RegistrantActivity();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(RegistrantActivity).Name);
				dq.Joins.Add(Activity.ObjectMap["ID"], RegistrantActivity.ObjectMap["ActivityId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "Activity");
				dq.Joins.Add(Registrant.ObjectMap["ID"], RegistrantActivity.ObjectMap["RegistrantId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "Registrant");
				dq.Joins.Add(Account.ObjectMap["ID"], Activity.ObjectMap["AccountId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "Account");
				dq.Joins.Add(ActivityGroup.ObjectMap["ID"], Activity.ObjectMap["ActivityGroupId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.LeftOuterJoin, "ActivityGroup");
				
                return dq;
            }
        }

        public static RegistrantActivity Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<RegistrantActivity> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return RegistrantActivity.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return RegistrantActivity.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.RegistrantId.IsEmpty())
				msgs.Add("RegistrantId is required on RegistrantActivity object", MessageType.Error);
			if (this.ActivityId.IsEmpty())
				msgs.Add("ActivityId is required on RegistrantActivity object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(RegistrantActivity).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(RegistrantActivity).Name);
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
			dps.Add(ObjectMap["RegistrantId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ActivityId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["RegistrationId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Uid"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["OnWaitingList"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["DatePaid"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["StripeChargeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["PayPalTransactionId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AmountPaid"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["PaymentTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["NumberOfSeats"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["NumberOfWaitingListSeats"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["NumberOfApplicationSeats"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["NumberOfInvitationSeats"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["NumberOfSeatsDeclined"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["SeatIndex"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["WaitingListSeatIndex"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ApplicationSeatIndex"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Day"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["PreferenceNumber"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["RegistrationPeriodId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ActivityGroupId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Rating"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public RegistrantActivity Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return RegistrantActivity.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<RegistrantActivity> LoadByActivityId(int ActivityId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<RegistrantActivity> LoadByRegistrantId(int RegistrantId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["RegistrantId"], RegistrantId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<RegistrantActivity> LoadByRegistrationId(int RegistrationId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["RegistrationId"], RegistrationId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static RegistrantActivity LoadByUid(Guid Uid, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static RegistrantActivity LoadByActivityIdByRegistrantId(int ActivityId, int RegistrantId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
			dq.Criterias.Add(ObjectMap["RegistrantId"], RegistrantId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static List<RegistrantActivity> LoadByActivityIdByDay(int ActivityId, DayOfWeekASL Day, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
			dq.Criterias.Add(ObjectMap["Day"], Day);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<RegistrantActivity> LoadByActivityIdByOnWaitingList(int ActivityId, bool OnWaitingList, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
			dq.Criterias.Add(ObjectMap["OnWaitingList"], OnWaitingList);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<RegistrantActivity> LoadByActivityGroupId(int ActivityGroupId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityGroupId"], ActivityGroupId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		

        #endregion

		#region "UserDefined"

        public RegistrationAssignmentType RegistrationAssignmentTypeId
        {
            get
            {
                if (this.NumberOfSeats > 0)
                    return RegistrationAssignmentType.Reserved;
                else if (this.NumberOfWaitingListSeats > 0)
                    return RegistrationAssignmentType.WaitingList;
                else if (this.NumberOfApplicationSeats > 0)
                    return RegistrationAssignmentType.Application;
                else if (this.NumberOfInvitationSeats > 0)
                    return RegistrationAssignmentType.Invited;
                else if (this.NumberOfSeatsDeclined > 0)
                    return RegistrationAssignmentType.Declined;

                return RegistrationAssignmentType.NotSet;
            }
        }

        public int? AssignmentSeatIndex
        {
            get
            {
                switch (RegistrationAssignmentTypeId)
                {
                    case RegistrationAssignmentType.Reserved:
                        return this.SeatIndex;
                    case RegistrationAssignmentType.WaitingList:
                        return this.WaitingListSeatIndex;
                    case RegistrationAssignmentType.Application:
                        return this.ApplicationSeatIndex;
                    default:
                        return 1;
                }
            }
        }

        public string RegistrantName
        {
            get
            {
                if (this.RegistrantRegistrantTypeId == RegistrantType.Individual)
                    return BaseWebUser.BuildDisplayName(this.RegistrantFirstName, this.RegistrantLastName);
                else if (this.RegistrantRegistrantTypeId == RegistrantType.Organization)
                    return this.RegistrantCompany.ToProperCase();

                return "";
            }
        }

        public int TotalSeats
        {
            get
            {
                var value = 0;

                if (NumberOfSeats.HasValue)
                    value += NumberOfSeats.Value;

                if (NumberOfWaitingListSeats.HasValue)
                    value += NumberOfWaitingListSeats.Value;

                if (NumberOfApplicationSeats.HasValue)
                    value += NumberOfApplicationSeats.Value;

                return value;
            }
        }

        public Registrant ToRegistrant()
        {
            return new Registrant()
            {
                ID = this.RegistrantId,
                FirstName = this.RegistrantFirstName,
                LastName = this.RegistrantLastName,
                Email = this.RegistrantEmail,
                Uid = this.RegistrantUid,
                Phone = this.RegistrantPhone,
                Company = this.RegistrantCompany,
                RegistrantTypeId = this.RegistrantRegistrantTypeId,
                FirstRegistrantId = this.RegistrantFirstRegistrantId
            };
        }
        public Activity ToActivity(bool loadFromDb = false, DatabaseConnection dc = null)
        {
            if (loadFromDb)
                return Activity.Load(this.ActivityId.Value, dc: dc);
            else
                return new Activity()
                {
                    ID = this.ActivityId,
                    Name = this.ActivityName,
                    Uid = this.ActivityUid,
                    ActivityGroupId = this.ActivityActivityGroupId,
                    ActivityScheduleId = this.ActivityActivityScheduleId,
                    IsArchived = this.ActivityIsArchived,
                    AccountId = this.ActivityAccountId,
                    CostPerRegistrant = this.ActivityCostPerRegistrant,
                    IsDeleted = this.ActivityIsDeleted
                };
        }

        public static List<RegistrantActivity> LoadByActivityId(int ActivityId, int? start, int? length, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCountByActivityIdByAccountId(int ActivityId, int AccountId, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
            dq.Criterias.Add(ObjectMap["ActivityAccountId"], AccountId);
            return dq.GetCount();
        }

        private Registration _Registration = null;
        public Registration GetRegistration(DatabaseConnection dc = null)
        {
            if (_Registration.IsEmpty() && this.RegistrationId.HasValue)
                _Registration = Registration.Load(this.RegistrationId.Value, dc: dc);

            return _Registration;
        }

        public List<FormQuestionAnswer> GetAnswers(DatabaseConnection dc = null)
        {
            return this.GetRegistration(dc: dc).GetAnswers(dc: dc);
        }

        public static List<RegistrantActivity> LoadByAccountId(int AccountId, int? start = null, int? length = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ActivityAccountId"], AccountId);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetSeatCountByAssignmentType(RegistrationAssignmentType rat, int? accountId = null, int? activityGroupId = null, int? activityId = null, int? registrationPeriodId = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();
            SelectParameter sp = null;

            switch (rat)
            {
                case RegistrationAssignmentType.Application:
                    sp = new SelectParameter(ObjectMap["NumberOfApplicationSeats"]);
                    break;
                case RegistrationAssignmentType.Invited:
                    sp = new SelectParameter(ObjectMap["NumberOfInvitationSeats"]);
                    break;
                case RegistrationAssignmentType.WaitingList:
                    sp = new SelectParameter(ObjectMap["NumberOfWaitingListSeats"]);
                    break;
                default:
                    sp = new SelectParameter(ObjectMap["NumberOfSeats"]);
                    break;
            }

            sp.Function = "ISNULL(SUM(" + DatabaseQuery.FunctionReplaceValue + "), 0)";
            dq.SelectParameters.Add(sp);

            if (activityId.HasValue)
                dq.Criterias.Add(ObjectMap["activityId"], activityId.Value);

            if (accountId.HasValue)
                dq.Criterias.Add(ObjectMap["ActivityAccountId"], accountId.Value);

            if (activityGroupId.HasValue)
                dq.Criterias.Add(ObjectMap["ActivityGroupId"], activityGroupId.Value);

            if (registrationPeriodId.HasValue)
                dq.Criterias.Add(ObjectMap["RegistrationPeriodId"], registrationPeriodId.Value);

            var results = dq.Select();
            if (results.Rows.Any())
                return dq.Select().Rows[0][0].ToString().ToInteger();

            return 0;
        }

        public static int GetSeatCountByActivityId(int activityId, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();

            BasicLibrary.DataAccess.SelectParameter sp1 = new BasicLibrary.DataAccess.SelectParameter(ObjectMap["NumberOfSeats"]);
            sp1.Function = "ISNULL(SUM(" + BasicLibrary.DataAccess.DatabaseQuery.FunctionReplaceValue + "), 0)";
            dq.SelectParameters.Add(sp1);

            BasicLibrary.DataAccess.SelectParameter sp2 = new BasicLibrary.DataAccess.SelectParameter(ObjectMap["NumberOfWaitingListSeats"]);
            sp2.Function = "ISNULL(SUM(" + BasicLibrary.DataAccess.DatabaseQuery.FunctionReplaceValue + "), 0)";
            dq.SelectParameters.Add(sp2);

            BasicLibrary.DataAccess.SelectParameter sp3 = new BasicLibrary.DataAccess.SelectParameter(ObjectMap["NumberOfApplicationSeats"]);
            sp3.Function = "ISNULL(SUM(" + BasicLibrary.DataAccess.DatabaseQuery.FunctionReplaceValue + "), 0)";
            dq.SelectParameters.Add(sp3);

            dq.Criterias.Add(ObjectMap["activityId"], activityId);

            var results = dq.Select();
            if(results.Rows.Any())
                return results.Rows[0][0].ToString().ToInteger() + results.Rows[0][1].ToString().ToInteger() + results.Rows[0][2].ToString().ToInteger();

            return 0;
        }

        public static int GetSeatCountByAccountId(int accountId, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();

            BasicLibrary.DataAccess.SelectParameter sp1 = new BasicLibrary.DataAccess.SelectParameter(ObjectMap["NumberOfSeats"]);
            sp1.Function = "ISNULL(SUM(" + BasicLibrary.DataAccess.DatabaseQuery.FunctionReplaceValue + "), 0)";
            dq.SelectParameters.Add(sp1);

            BasicLibrary.DataAccess.SelectParameter sp2 = new BasicLibrary.DataAccess.SelectParameter(ObjectMap["NumberOfWaitingListSeats"]);
            sp2.Function = "ISNULL(SUM(" + BasicLibrary.DataAccess.DatabaseQuery.FunctionReplaceValue + "), 0)";
            dq.SelectParameters.Add(sp2);

            BasicLibrary.DataAccess.SelectParameter sp3 = new BasicLibrary.DataAccess.SelectParameter(ObjectMap["NumberOfApplicationSeats"]);
            sp3.Function = "ISNULL(SUM(" + BasicLibrary.DataAccess.DatabaseQuery.FunctionReplaceValue + "), 0)";
            dq.SelectParameters.Add(sp3);

            dq.Criterias.Add(ObjectMap["ActivityAccountId"], accountId);

            var results = dq.Select();
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToInteger() + results.Rows[0][1].ToString().ToInteger();

            return 0;
        }

        public static int GetSeatCountByRegistrationPeriodId(int RegistrationPeriodId, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();

            BasicLibrary.DataAccess.SelectParameter sp1 = new BasicLibrary.DataAccess.SelectParameter(ObjectMap["NumberOfSeats"]);
            sp1.Function = "ISNULL(SUM(" + BasicLibrary.DataAccess.DatabaseQuery.FunctionReplaceValue + "), 0)";
            dq.SelectParameters.Add(sp1);

            BasicLibrary.DataAccess.SelectParameter sp2 = new BasicLibrary.DataAccess.SelectParameter(ObjectMap["NumberOfWaitingListSeats"]);
            sp2.Function = "ISNULL(SUM(" + BasicLibrary.DataAccess.DatabaseQuery.FunctionReplaceValue + "), 0)";
            dq.SelectParameters.Add(sp2);

            BasicLibrary.DataAccess.SelectParameter sp3 = new BasicLibrary.DataAccess.SelectParameter(ObjectMap["NumberOfApplicationSeats"]);
            sp3.Function = "ISNULL(SUM(" + BasicLibrary.DataAccess.DatabaseQuery.FunctionReplaceValue + "), 0)";
            dq.SelectParameters.Add(sp3);

            dq.Criterias.Add(ObjectMap["RegistrationPeriodId"], RegistrationPeriodId);

            var results = dq.Select();
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToInteger() + results.Rows[0][1].ToString().ToInteger();

            return 0;
        }

        public static int GetSeatCountByActivityGroupId(int ActivityGroupId, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();

            BasicLibrary.DataAccess.SelectParameter sp1 = new BasicLibrary.DataAccess.SelectParameter(ObjectMap["NumberOfSeats"]);
            sp1.Function = "ISNULL(SUM(" + BasicLibrary.DataAccess.DatabaseQuery.FunctionReplaceValue + "), 0)";
            dq.SelectParameters.Add(sp1);

            BasicLibrary.DataAccess.SelectParameter sp2 = new BasicLibrary.DataAccess.SelectParameter(ObjectMap["NumberOfWaitingListSeats"]);
            sp2.Function = "ISNULL(SUM(" + BasicLibrary.DataAccess.DatabaseQuery.FunctionReplaceValue + "), 0)";
            dq.SelectParameters.Add(sp2);

            BasicLibrary.DataAccess.SelectParameter sp3 = new BasicLibrary.DataAccess.SelectParameter(ObjectMap["NumberOfApplicationSeats"]);
            sp3.Function = "ISNULL(SUM(" + BasicLibrary.DataAccess.DatabaseQuery.FunctionReplaceValue + "), 0)";
            dq.SelectParameters.Add(sp3);

            dq.Criterias.Add(ObjectMap["ActivityGroupId"], ActivityGroupId);

            var results = dq.Select();
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToInteger() + results.Rows[0][1].ToString().ToInteger();

            return 0;
        }

        public static List<RegistrantActivity> LoadByRegistrationIds(List<int> RegistrationIds, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["RegistrationId"], RegistrationIds, DatabaseCriteriaOperatorEnum.In);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<RegistrantActivity> LoadByRegistrantIds(List<int> registrantIds, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["RegistrantId"], registrantIds, DatabaseCriteriaOperatorEnum.In);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<RegistrantActivity> GetByActivityIds(List<int> activityIds, List<int> registrantIds, DateTime startDate, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;

            dq.Joins.Add(ActivityInstance.ObjectMap["ActivityScheduleId"], Activity.ObjectMap["ActivityScheduleId"]);

            dq.Criterias.Add(Activity.ObjectMap["Id"], activityIds, DatabaseCriteriaOperatorEnum.In);
            dq.Criterias.Add(ObjectMap["RegistrantId"], registrantIds, DatabaseCriteriaOperatorEnum.In);
            dq.Criterias.Add(ObjectMap["NumberOfSeats"], 1, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);
            dq.Criterias.Add(ActivityInstance.ObjectMap["StartDateTime"], startDate, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public RegistrationActivity GetRegistrationActivity(DatabaseConnection dc = null)
        {
            return RegistrationActivity.LoadByActivityIdByRegistrationId(this.ActivityId.Value, this.RegistrationId.Value, dc: dc);
        }

        public SystemMessages DeleteRegistrationActivity(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var ra = this.GetRegistrationActivity(dc: dc);

            if (ra.IsNotEmpty())
                msgs.AddRange(ra.Delete(dc: dc));

            return msgs;
        }

        public SystemMessages CancelRegistration(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    amsgs.AddRange(this.DeleteRegistrationActivity(adc));
                    amsgs.AddRange(this.AdjustPreferencesDuringRemoval(dc: adc));
                    amsgs.AddRange(this.AdjustSeatIndexesDuringRemoval(dc: adc));
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

        public SystemMessages ChangeSeatAssignment(RegistrationAssignmentType to, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            if(this.RegistrationAssignmentTypeId == to)
                return msgs;

            //if you move it before the change is made it will adjust everyone correctly. Has to be done before the change is saved.
            //need a better function that can allow setting which seat index parameter needs to be handled
            msgs.AddRange(this.AdjustSeatIndexesDuringRemoval(dc: dc));
            // msgs.AddRange(this.AdjustPreferencesDuringRemoval(dc: dc));

            if (this.RegistrationAssignmentTypeId == RegistrationAssignmentType.Application)
            {
                if (to == RegistrationAssignmentType.Reserved)
                {
                    this.NumberOfSeats = this.NumberOfApplicationSeats;
                    this.SeatIndex = RegistrantActivity.GetSeatCountByAssignmentType(rat: to, activityId: this.ActivityId, dc: dc) + 1;
                    this.NumberOfApplicationSeats = 0;
                    this.ApplicationSeatIndex = null;
                    msgs.AddRange(this.Save(dc: dc));
                }
                else if (to == RegistrationAssignmentType.WaitingList)
                {
                    this.NumberOfWaitingListSeats = this.NumberOfApplicationSeats;
                    this.WaitingListSeatIndex = RegistrantActivity.GetSeatCountByAssignmentType(rat: to, activityId: this.ActivityId, dc: dc) + 1;
                    this.NumberOfApplicationSeats = 0;
                    this.ApplicationSeatIndex = null;
                    msgs.AddRange(this.Save(dc: dc));
                }
                else if (to == RegistrationAssignmentType.Invited)
                {
                    this.NumberOfInvitationSeats = this.NumberOfApplicationSeats;
                    this.NumberOfApplicationSeats = 0;
                    this.ApplicationSeatIndex = null;
                    msgs.AddRange(this.Save(dc: dc));
                }
                else if (to == RegistrationAssignmentType.Declined)
                {
                    this.NumberOfSeatsDeclined = this.NumberOfApplicationSeats;
                    this.NumberOfApplicationSeats = 0;
                    this.ApplicationSeatIndex = null;
                    msgs.AddRange(this.Save(dc: dc));
                }
            }
            else if (this.RegistrationAssignmentTypeId == RegistrationAssignmentType.WaitingList)
            {
                if (to == RegistrationAssignmentType.Reserved)
                {
                    this.NumberOfSeats = this.NumberOfWaitingListSeats;
                    this.SeatIndex = RegistrantActivity.GetSeatCountByAssignmentType(rat: to, activityId: this.ActivityId, dc: dc) + 1;
                    this.NumberOfWaitingListSeats = 0;
                    this.WaitingListSeatIndex = null;
                    msgs.AddRange(this.Save(dc: dc));
                }
                else if (to == RegistrationAssignmentType.Application)
                {
                    this.NumberOfApplicationSeats = this.NumberOfWaitingListSeats;
                    this.ApplicationSeatIndex = RegistrantActivity.GetSeatCountByAssignmentType(rat: to, activityId: this.ActivityId, dc: dc) + 1;
                    this.NumberOfWaitingListSeats = 0;
                    this.WaitingListSeatIndex = null;
                    msgs.AddRange(this.Save(dc: dc));
                }
                else if (to == RegistrationAssignmentType.Invited)
                {
                    this.NumberOfInvitationSeats = this.NumberOfWaitingListSeats;
                    this.NumberOfWaitingListSeats = 0;
                    this.WaitingListSeatIndex = null;
                    msgs.AddRange(this.Save(dc: dc));
                }
                else if (to == RegistrationAssignmentType.Declined)
                {
                    this.NumberOfSeatsDeclined = this.NumberOfWaitingListSeats;
                    this.NumberOfWaitingListSeats = 0;
                    this.WaitingListSeatIndex = null;
                    msgs.AddRange(this.Save(dc: dc));
                }
            }
            else if (this.RegistrationAssignmentTypeId == RegistrationAssignmentType.Reserved)
            {
                if (to == RegistrationAssignmentType.WaitingList)
                {
                    this.NumberOfWaitingListSeats = this.NumberOfSeats;
                    this.WaitingListSeatIndex = RegistrantActivity.GetSeatCountByAssignmentType(rat: to, activityId: this.ActivityId, dc: dc) + 1;
                    this.NumberOfSeats = 0;
                    this.SeatIndex = null;
                    msgs.AddRange(this.Save(dc: dc));
                }
                else if (to == RegistrationAssignmentType.Application)
                {
                    this.NumberOfApplicationSeats = this.NumberOfSeats;
                    this.ApplicationSeatIndex = RegistrantActivity.GetSeatCountByAssignmentType(rat: to, activityId: this.ActivityId, dc: dc) + 1;
                    this.NumberOfSeats = 0;
                    this.SeatIndex = null;
                    msgs.AddRange(this.Save(dc: dc));
                }
                else if (to == RegistrationAssignmentType.Invited)
                {
                    this.NumberOfInvitationSeats = this.NumberOfSeats;
                    this.NumberOfSeats = 0;
                    this.SeatIndex = null;
                    msgs.AddRange(this.Save(dc: dc));
                }
                else if (to == RegistrationAssignmentType.Declined)
                {
                    this.NumberOfSeatsDeclined = this.NumberOfSeats;
                    this.NumberOfSeats = 0;
                    this.SeatIndex = null;
                    msgs.AddRange(this.Save(dc: dc));
                }
            }
            else if (this.RegistrationAssignmentTypeId == RegistrationAssignmentType.Invited)
            {
                if (to == RegistrationAssignmentType.Reserved)
                {
                    this.NumberOfSeats = this.NumberOfInvitationSeats;
                    this.SeatIndex = RegistrantActivity.GetSeatCountByAssignmentType(rat: to, activityId: this.ActivityId, dc: dc) + 1;
                    this.NumberOfInvitationSeats = 0;
                    msgs.AddRange(this.Save(dc: dc));
                }
                else if (to == RegistrationAssignmentType.WaitingList)
                {
                    this.NumberOfWaitingListSeats = this.NumberOfInvitationSeats;
                    this.WaitingListSeatIndex = RegistrantActivity.GetSeatCountByAssignmentType(rat: to, activityId: this.ActivityId, dc: dc) + 1;
                    this.NumberOfInvitationSeats = 0;
                    msgs.AddRange(this.Save(dc: dc));
                }
                else if (to == RegistrationAssignmentType.Application)
                {
                    this.NumberOfApplicationSeats = this.NumberOfInvitationSeats;
                    this.ApplicationSeatIndex = RegistrantActivity.GetSeatCountByAssignmentType(rat: to, activityId: this.ActivityId, dc: dc) + 1;
                    this.NumberOfInvitationSeats = 0;
                    msgs.AddRange(this.Save(dc: dc));
                }
                else if (to == RegistrationAssignmentType.Declined)
                {
                    this.NumberOfSeatsDeclined = this.NumberOfInvitationSeats;
                    this.NumberOfInvitationSeats = 0;
                    msgs.AddRange(this.Save(dc: dc));
                }
            }
            else if (this.RegistrationAssignmentTypeId == RegistrationAssignmentType.Declined)
            {
                if (to == RegistrationAssignmentType.Reserved)
                {
                    this.NumberOfSeats = this.NumberOfSeatsDeclined;
                    this.SeatIndex = RegistrantActivity.GetSeatCountByAssignmentType(rat: to, activityId: this.ActivityId, dc: dc) + 1;
                    this.NumberOfInvitationSeats = 0;
                    msgs.AddRange(this.Save(dc: dc));
                }
                else if (to == RegistrationAssignmentType.WaitingList)
                {
                    this.NumberOfWaitingListSeats = this.NumberOfSeatsDeclined;
                    this.WaitingListSeatIndex = RegistrantActivity.GetSeatCountByAssignmentType(rat: to, activityId: this.ActivityId, dc: dc) + 1;
                    this.NumberOfInvitationSeats = 0;
                    msgs.AddRange(this.Save(dc: dc));
                }
                else if (to == RegistrationAssignmentType.Application)
                {
                    this.NumberOfApplicationSeats = this.NumberOfSeatsDeclined;
                    this.ApplicationSeatIndex = RegistrantActivity.GetSeatCountByAssignmentType(rat: to, activityId: this.ActivityId, dc: dc) + 1;
                    this.NumberOfInvitationSeats = 0;
                    msgs.AddRange(this.Save(dc: dc));
                }
                else if (to == RegistrationAssignmentType.Invited)
                {
                    this.NumberOfInvitationSeats = this.NumberOfSeatsDeclined;
                    this.NumberOfSeatsDeclined = 0;
                    msgs.AddRange(this.Save(dc: dc));
                }
            }

            //adjust for the activities that they just left
            //adjust seat indexes for others behind then
            //adjust for preference numbers for one's they just left;

            return msgs;
        }

        public SystemMessages AdjustSeatIndexesDuringRemoval(Registrant registrant = null, DatabaseConnection dc = null)
        {
            // get all all registrant activities where the activity id is the same, 
            // the day is the same, and ony update people with the same assignment type, 
            // UPDATE: No longer the same day because registration is now according to a schedule, not a day
            // I think they need to be in the same registration period if it exists   
            // TODO: Review if I need to check the same registration period, probably more related to the activity group if anything?

            if (registrant.IsEmpty())
                registrant = this.ToRegistrant();

            var msgs = new SystemMessages();

            var otherPeoplesActivities = RegistrantActivity.LoadByActivityId(this.ActivityId.Value, dc: dc)
                .Where(x => x.RegistrantId != registrant.ID)
                .ToList();

            AdjustSeatIndexes(otherPeoplesActivities, RegistrationAssignmentType.Reserved, -1, dc: dc);
            AdjustSeatIndexes(otherPeoplesActivities, RegistrationAssignmentType.WaitingList, -1, dc: dc);
            AdjustSeatIndexes(otherPeoplesActivities, RegistrationAssignmentType.Application, -1, dc: dc);

            return msgs;
        }

        public SystemMessages AdjustSeatIndex(int amount, RegistrationAssignmentType rat, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            switch (rat)
            {
                case RegistrationAssignmentType.Application:
                    this.ApplicationSeatIndex += amount;
                    break;
                case RegistrationAssignmentType.Reserved:
                    this.SeatIndex += amount;
                    break;
                case RegistrationAssignmentType.WaitingList:
                    this.WaitingListSeatIndex += amount;
                    break;
            }
            msgs.AddRange(this.Save(dc: dc));
            return msgs;
        }

        public SystemMessages AdjustSeatIndexes(List<RegistrantActivity> ras, RegistrationAssignmentType rat, int factor, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            switch (rat)
            {
                case RegistrationAssignmentType.Reserved:
                    if (this.NumberOfSeats > 0)
                        ras
                            .Where(x => x.SeatIndex > this.SeatIndex)
                            .ToList()
                            .ForEach(item => item.AdjustSeatIndex(this.NumberOfSeats.Value * factor, RegistrationAssignmentType.Reserved, dc: dc));
                    break;
                case RegistrationAssignmentType.WaitingList:
                    if (this.NumberOfWaitingListSeats > 0)
                        ras
                            .Where(x => x.WaitingListSeatIndex > this.WaitingListSeatIndex)
                            .ToList()
                            .ForEach(item => item.AdjustSeatIndex(this.NumberOfWaitingListSeats.Value * factor, RegistrationAssignmentType.WaitingList, dc: dc));
                    break;
                case RegistrationAssignmentType.Application:
                    if (this.NumberOfApplicationSeats > 0)
                        ras
                            .Where(x => x.ApplicationSeatIndex > this.ApplicationSeatIndex)
                            .ToList()
                            .ForEach(item => item.AdjustSeatIndex(this.NumberOfApplicationSeats.Value * factor, RegistrationAssignmentType.Application, dc: dc));
                    break;
            }

            return msgs;
        }

        public SystemMessages AdjustPreferenceNumber(int amount, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            this.PreferenceNumber -= 1;
            msgs.AddRange(this.Save(dc: dc));
            return msgs;
        }

        public SystemMessages AdjustPreferencesDuringRemoval(List<RegistrantActivity> registrantActivities = null, DatabaseConnection dc = null)
        {
            if (registrantActivities.IsEmpty())
            {
                // has to be for the same registration period and same account
                // need a better function to filter out registration period in the query
                registrantActivities = RegistrantActivity
                                        .LoadByRegistrantId(this.RegistrantId.Value, dc: dc)
                                        .Where(x => x.RegistrationPeriodId == this.RegistrationPeriodId)
                                        .ToList();
            }

            // lower choices really mean higher preference numbers. 
            // Like choice #1 is a higher choice than choice #3, 
            // but of course in math we know that 1 is lower than 3.
            var msgs = new SystemMessages();

            registrantActivities
                .Where(x => x.PreferenceNumber.HasValue && x.PreferenceNumber.Value > this.PreferenceNumber.Value)
                .ToList()
                .ForEach(ra => ra.AdjustPreferenceNumber(-1, dc: dc));

            return msgs;
        }

        public SystemMessages SendNewRegistrationNotice(Account account = null, Activity activity = null, DatabaseConnection dc = null)
        {
            activity = activity ?? this.ToActivity(true, dc: dc);
            account = account ?? activity.GetAccount(dc: dc);

            if (account.Email.IsSameAs(activity.ContactEmail))
                return new SystemMessages();

            var details = $"<p><strong>{this.RegistrantName}</strong> has registered for <strong>{activity.Name}</strong> at <strong>{account.Name}</strong>.</p> <p>You currently have a total of: <ul><li><strong>{activity.GetRegistrantCountByApplicationType(RegistrationAssignmentType.Reserved, dc: dc)}</strong> reserved</li><li><strong>{activity.GetRegistrantCountByApplicationType(RegistrationAssignmentType.WaitingList, dc: dc)}</strong> on the waiting list</li><li><strong>{activity.GetRegistrantCountByApplicationType(RegistrationAssignmentType.Application, dc: dc)}</strong> applications</li></ul></p>";

            var message = $"<p>Hello {activity.ContactName},</p>{details}<p>{(Email.GetAppSignature(isAfterSchool: account.IsAfterSchool, utm_campaign: "registration_notification"))}</p>";

            return SendGridHelper.SendMail(
                    fromEmail: new SendGrid.Helpers.Mail.EmailAddress(
                                    Email.AppEmailAddress,
                                    Utilities.AppName(account.IsAfterSchool)
                               ),
                    toEmail: new SendGrid.Helpers.Mail.EmailAddress(account.Email),
                    subject: $"You have a new registration for {activity.Name}",
                    body: message,
                    emailTemplateId: SendGridHelper.GetGenericTemplateId(account.IsAfterSchool)
                );
        }

        #endregion
    }
}
