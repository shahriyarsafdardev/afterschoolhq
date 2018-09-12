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
    /// The actual registration for a registrant. A registrant can have multiple registrations.
    /// </summary>
	
    public class Registration : BasicObjectWithModified
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required.
		/// </summary>
		public int? RegistrantId { get { return (int?)MDV["RegistrantId"].Value; } set { MDV["RegistrantId"].Value = value; } }
		public int? RegistrationPeriodId { get { return (int?)MDV["RegistrationPeriodId"].Value; } set { MDV["RegistrationPeriodId"].Value = value; } }
		public int? AccountId { get { return (int?)MDV["AccountId"].Value; } set { MDV["AccountId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public Guid Uid { get { return (Guid)MDV["Uid"].Value; } set { MDV["Uid"].Value = value; } }
		public DateTime? DatePaid { get { return (DateTime?)MDV["DatePaid"].Value; } set { MDV["DatePaid"].Value = value; } }
		public string StripeChargeId { get { return (string)MDV["StripeChargeId"].Value; } set { MDV["StripeChargeId"].Value = value; } }
		public string PayPalTransactionId { get { return (string)MDV["PayPalTransactionId"].Value; } set { MDV["PayPalTransactionId"].Value = value; } }
		public decimal? AmountPaid { get { return (decimal?)MDV["AmountPaid"].Value; } set { MDV["AmountPaid"].Value = value; } }
		public PaymentType PaymentTypeId { get { return (PaymentType)MDV["PaymentTypeId"].Value; } set { MDV["PaymentTypeId"].Value = value; } }
		public DateTime? LastUpdated { get { return (DateTime?)MDV["LastUpdated"].Value; } set { MDV["LastUpdated"].Value = value; } }
		public int? ActivityGroupId { get { return (int?)MDV["ActivityGroupId"].Value; } set { MDV["ActivityGroupId"].Value = value; } }
		public int? AccountActivityGroupId { get { return (int?)MDV["AccountActivityGroupId"].Value; } set { MDV["AccountActivityGroupId"].Value = value; } }
		public int? FirstRegistrationId { get { return (int?)MDV["FirstRegistrationId"].Value; } set { MDV["FirstRegistrationId"].Value = value; } }
		public string ESignature { get { return (string)MDV["ESignature"].Value; } set { MDV["ESignature"].Value = value; } }
		public RegistrationCompletionStatus CompletionStatusId { get { return (RegistrationCompletionStatus)MDV["CompletionStatusId"].Value; } set { MDV["CompletionStatusId"].Value = value; } }
		public int? PurchaseId { get { return (int?)MDV["PurchaseId"].Value; } set { MDV["PurchaseId"].Value = value; } }
		public string RegistrantEmail { get { return (string)MDV["RegistrantEmail"].Value; } }
		public string RegistrantFirstName { get { return (string)MDV["RegistrantFirstName"].Value; } }
		public string RegistrantLastName { get { return (string)MDV["RegistrantLastName"].Value; } }
		public Guid RegistrantUid { get { return (Guid)MDV["RegistrantUid"].Value; } }
		public string RegistrantPhone { get { return (string)MDV["RegistrantPhone"].Value; } }
		public string RegistrantCompany { get { return (string)MDV["RegistrantCompany"].Value; } }
		public RegistrantType RegistrantRegistrantTypeId { get { return (RegistrantType)MDV["RegistrantRegistrantTypeId"].Value; } }
		

        public Registration()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(Registration).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.Uid = Guid.NewGuid();
			this.PaymentTypeId = PaymentType.NotSet;
			this.CompletionStatusId = RegistrationCompletionStatus.NotSet;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(Registration).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "RegistrantId", System.Data.SqlDbType.Int);
				_dp.Add(className, "RegistrationPeriodId", System.Data.SqlDbType.Int);
				_dp.Add(className, "AccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "Uid", System.Data.SqlDbType.UniqueIdentifier);
				_dp.Add(className, "DatePaid", System.Data.SqlDbType.DateTime);
				_dp.Add(className, "StripeChargeId", System.Data.SqlDbType.NVarChar , "100");
				_dp.Add(className, "PayPalTransactionId", System.Data.SqlDbType.NVarChar , "50");
				_dp.Add(className, "AmountPaid", System.Data.SqlDbType.Money);
				_dp.Add(className, "PaymentTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "LastUpdated", System.Data.SqlDbType.DateTime);
				_dp.Add(className, "ActivityGroupId", System.Data.SqlDbType.Int);
				_dp.Add(className, "AccountActivityGroupId", System.Data.SqlDbType.Int);
				_dp.Add(className, "FirstRegistrationId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ESignature", System.Data.SqlDbType.NVarChar , "50");
				_dp.Add(className, "CompletionStatusId", System.Data.SqlDbType.Int);
				_dp.Add(className, "PurchaseId", System.Data.SqlDbType.Int);
				_dp.Add(className, "RegistrantEmail", typeof(Registrant).Name, "Email", System.Data.SqlDbType.NVarChar, "Registrant");
				_dp.Add(className, "RegistrantFirstName", typeof(Registrant).Name, "FirstName", System.Data.SqlDbType.NVarChar, "Registrant");
				_dp.Add(className, "RegistrantLastName", typeof(Registrant).Name, "LastName", System.Data.SqlDbType.NVarChar, "Registrant");
				_dp.Add(className, "RegistrantUid", typeof(Registrant).Name, "Uid", System.Data.SqlDbType.UniqueIdentifier, "Registrant");
				_dp.Add(className, "RegistrantPhone", typeof(Registrant).Name, "Phone", System.Data.SqlDbType.NVarChar, "Registrant");
				_dp.Add(className, "RegistrantCompany", typeof(Registrant).Name, "Company", System.Data.SqlDbType.NVarChar, "Registrant");
				_dp.Add(className, "RegistrantRegistrantTypeId", typeof(Registrant).Name, "RegistrantTypeId", System.Data.SqlDbType.Int, "Registrant");
				
                return _dp;
            }
        }

        protected static Registration LoadObject(System.Data.DataRow row)
        {
            Registration obj = new Registration();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(Registration).Name);
				dq.Joins.Add(Registrant.ObjectMap["ID"], Registration.ObjectMap["RegistrantId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "Registrant");
				
                return dq;
            }
        }

        public static Registration Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<Registration> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return Registration.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return Registration.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.RegistrantId.IsEmpty())
				msgs.Add("RegistrantId is required on Registration object", MessageType.Error);
			if (this.Uid.IsEmpty())
				msgs.Add("Uid is required on Registration object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(Registration).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(Registration).Name);
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
			dps.Add(ObjectMap["RegistrationPeriodId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AccountId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Uid"]);
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
			dps.Add(ObjectMap["LastUpdated"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ActivityGroupId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AccountActivityGroupId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["FirstRegistrationId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ESignature"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["CompletionStatusId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["PurchaseId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public Registration Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return Registration.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static Registration LoadByUid(Guid Uid, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static List<Registration> LoadByRegistrantId(int RegistrantId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["RegistrantId"], RegistrantId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Registration> LoadByAccountId(int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Registration> LoadByRegistrationPeriodId(int RegistrationPeriodId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["RegistrationPeriodId"], RegistrationPeriodId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Registration> LoadByAccountIdByRegistrantId(int AccountId, int RegistrantId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			dq.Criterias.Add(ObjectMap["RegistrantId"], RegistrantId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Registration> LoadByActivityGroupId(int ActivityGroupId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityGroupId"], ActivityGroupId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Registration> LoadByAccountActivityGroupId(int AccountActivityGroupId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountActivityGroupId"], AccountActivityGroupId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Registration> LoadByFirstRegistrationId(int FirstRegistrationId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["FirstRegistrationId"], FirstRegistrationId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		

        #endregion

		#region "UserDefined"

        public string Name
        {
            get
            {
                return WebUser.BuildDisplayName(RegistrantFirstName, RegistrantLastName);
            }
        }

        public string AddToRegistrationUrl
        {
            get
            {
                return $"{Utilities.BaseUrl(this.GetAccount().IsAfterSchool)}/reg/add/{this.Uid.ToString()}";
            }
        }

        public Registrant ToRegistrant()
        {
            return new Registrant()
            {
                FirstName = RegistrantFirstName,
                LastName = RegistrantLastName,
                Uid = RegistrantUid,
                Email = RegistrantEmail,
                Company = RegistrantCompany,
                RegistrantTypeId = RegistrantRegistrantTypeId,
                Phone = RegistrantPhone
            };
        }

        public static List<Registration> LoadByAccountId(int AccountId, int? start, int? length, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.OrderBys.Add(ObjectMap["RegistrantFirstName"], OrderByDirectionEnum.Ascending);
            dq.OrderBys.Add(ObjectMap["RegistrantLastName"], OrderByDirectionEnum.Ascending);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Registration> LoadList(List<int> ids, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], ids, BasicLibrary.DataAccess.DatabaseCriteriaOperatorEnum.In);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public SystemMessages MergeInto(Registration mergedFrom, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    if (this.RegistrationPeriodId != mergedFrom.RegistrationPeriodId)
                    {
                        amsgs.Add("You can only merge registrations from the same registration window", MessageType.UserError);
                    }

                    if (amsgs.HasNoErrors())
                    {
                        var mergedFromActivities = mergedFrom.GetRegistrantActivities(amsgs, adc);

                        foreach (var activity in mergedFromActivities)
                        {
                            amsgs.AddRange(this.MoveActivity(activity, loggedInUserId, adc));
                        }

                        // delete previous registration
                        if (amsgs.HasNoErrors())
                        {
                            amsgs.AddRange(mergedFrom.DeleteWithRelatedRecords(adc));
                        }

                        this.LastModifiedBy = loggedInUserId;
                        this.LastModifiedDate = DateTime.UtcNow;
                        amsgs.AddRange(this.Save(adc));
                    }
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

        internal static List<int> ValidCompletionStatus
        {
            get
            {
                return new List<int>() { (int)RegistrationCompletionStatus.Completed, (int)RegistrationCompletionStatus.RequiresParentSignature };
            }
        } 

        public static List<Registration> LoadFirstRegistrations(int accountId, int? start, int? length, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var completionStatus = new List<int>() { (int)RegistrationCompletionStatus.Completed, (int)RegistrationCompletionStatus.RequiresParentSignature };

            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.Criterias.Add(ObjectMap["AccountId"], accountId);
            dq.Criterias.Add(ObjectMap["CompletionStatusId"], ValidCompletionStatus, DatabaseCriteriaOperatorEnum.In);
            dq.Criterias.Add(ObjectMap["FirstRegistrationId"], DBNull.Value, DatabaseCriteriaOperatorEnum.Is);
            dq.OrderBys.Add(ObjectMap["RegistrantFirstName"], OrderByDirectionEnum.Ascending);
            dq.OrderBys.Add(ObjectMap["RegistrantLastName"], OrderByDirectionEnum.Ascending);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public SystemMessages FindAndSetFirstRegistration(string firstName, string lastName, string email, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.Criterias.Add(ObjectMap["AccountId"], this.AccountId);
            dq.Criterias.Add(ObjectMap["CompletionStatusId"], ValidCompletionStatus, DatabaseCriteriaOperatorEnum.In);
            dq.Criterias.Add(ObjectMap["FirstRegistrationId"], DBNull.Value, DatabaseCriteriaOperatorEnum.Is);
            dq.Criterias.Add(ObjectMap["RegistrantFirstName"], firstName.Trim());
            dq.Criterias.Add(ObjectMap["RegistrantLastName"], lastName.Trim());
            dq.Criterias.Add(ObjectMap["RegistrantEmail"], email.Trim());
            dq.Criterias.Add(ObjectMap["ID"], this.ID.Value, DatabaseCriteriaOperatorEnum.NotEqualTo);
            dq.OrderBys.Add(ObjectMap["Id"], OrderByDirectionEnum.Ascending);

            var firstRegistration = dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
            if (firstRegistration.IsNotEmpty())
            {
                this.FirstRegistrationId = firstRegistration.ID;
                this.LastModifiedBy = 0;
                this.LastModifiedDate = DateTime.UtcNow;
                msgs.AddRange(this.Save(dc: dc));
            }

            return msgs;
        }

        public static List<Registration> FindRelatedRegistrations(int AccountId, string firstName, string lastName, string email, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.Criterias.Add(ObjectMap["RegistrantFirstName"], firstName.Trim());
            dq.Criterias.Add(ObjectMap["RegistrantLastName"], lastName.Trim());
            dq.Criterias.Add(ObjectMap["RegistrantEmail"], email.Trim());
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public List<Registration> FindRelatedRegistrations(string firstName = null, string lastName = null, string email = null, DatabaseConnection dc = null)
        {            
            return FindRelatedRegistrations(
                this.AccountId.Value, 
                firstName ?? this.RegistrantFirstName, 
                lastName ?? this.RegistrantLastName, 
                email ?? this.RegistrantEmail, 
                dc);
        }

        public SystemMessages AddRelatedRegistration(Registration registration, int loggedInUserId, DatabaseConnection dc = null)
        {
            registration.FirstRegistrationId = this.ID.Value;
            registration.LastModifiedBy = loggedInUserId;
            registration.LastModifiedDate = DateTime.UtcNow;
            return registration.Save(dc: dc);
        }

        public List<Registration> GetRelatedRegistrations(DatabaseConnection dc = null)
        {
            var registrations = new List<Registration>();
            if (this.FirstRegistrationId.HasValue)
            {
                registrations.Add(Registration.Load(this.FirstRegistrationId.Value, dc: dc));
                registrations.AddRange(Registration.LoadByFirstRegistrationId(this.FirstRegistrationId.Value, dc: dc));
            }
            else
            {
                registrations.AddRange(Registration.LoadByFirstRegistrationId(this.ID.Value, dc: dc));
            }
            return registrations.Where(x => x.ID != this.ID).ToList();
        }

        public List<Registration> GetCurrentRelatedRegistrations(DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.UseParameterizedQuery = false;

            var sql = $@"
                        select
	                        *
                        from
	                        Registration r inner join
	                        Registrant re on r.registrantid = re.id
                        where
	                        r.id in
	                        (
		                        select
			                        r2.id	
		                        from 
			                        registration r2 inner join 
			                        registrationactivity ra2 on r2.id = ra2.RegistrationId inner join
			                        activity a2 on ra2.activityid = a2.id inner join
			                        activityinstance ai2 on a2.activityscheduleid = ai2.activityscheduleid
		                        where
			                        ai2.StartDateTime >= getdate() and
                                    r2.AccountId = {this.AccountId}
	                        ) and
	                        (
		                        r.FirstRegistrationId = {this.FirstRegistrationId.IfEmpty(0)} or
		                        r.ID = {this.FirstRegistrationId.IfEmpty(0)} or
		                        r.FirstRegistrationId = {this.ID} or
		                        r.ID = {this.ID}
	                        ) and r.CompletionStatusId in ({ValidCompletionStatus.ToArray().ToStringArray().ToStringList()})";

            var registrations = dq.ExecuteSelect(sql).AsEnumerable().ToList().ConvertAll(LoadObject);

            return registrations.Where(x => x.ID != this.ID).ToList();
        }

        public RegistrationPeriod GetRegistrationPeriod(DatabaseConnection dc = null)
        {
            return RegistrationPeriod.Load(this.RegistrationPeriodId.Value, dc: dc);
        }

        public List<FormQuestionAnswer> GetAnswers(DatabaseConnection dc = null)
        {
            return FormQuestionAnswer.LoadByRegistrationId(this.ID.Value, dc: dc);
        }

        public FormQuestionAnswer AddAnswer(string answerText, FormQuestion question, Registrant registrant, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();                        

            var answer = new FormQuestionAnswer();
            answer.RegistrantId = registrant.ID;
            answer.RegistrationId = this.ID;
            answer.Text = answerText;
            answer.FormQuestionId = question.ID; // question id not as important as the question data type id because 1 answer may apply to multiple question ids but one type of question data
            answer.QuestionDataTypeId = question.QuestionDataTypeId;
            answer.CreateDate = answer.LastModifiedDate = DateTime.UtcNow;
            answer.CreatedBy = answer.LastModifiedBy = loggedInUserId;
            msgs.AddRange(answer.Save(dc: dc));
            
            msgs.AddRange(answer.UpdateRegistrantInfo(registrant, dc: dc));

            return answer;
        }

        public SystemMessages MoveActivity(RegistrantActivity ra, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            // delete previous relationships to old registration
            //msgs.AddRange(ra.DeleteRegistrationActivity(dc: dc));

            // move to new registrant and registration
            ra.RegistrantId = this.RegistrantId;
            ra.RegistrationId = this.ID;
            ra.LastModifiedBy = loggedInUserId;
            ra.LastModifiedDate = DateTime.UtcNow;
            msgs.AddRange(ra.Save(dc: dc));

            // add to this registration record 
            // TODO: hopefully will remove this in the future, the whole registration activity object :(
            //this.AddActivity(ra.ActivityId.Value, loggedInUserId, msgs, dc);

            return msgs;
        }

        //public RegistrationActivity AddActivity(int activityId, int loggedInUserId, SystemMessages msgs = null, DatabaseConnection dc = null)
        //{
        //    if (msgs.IsEmpty()) msgs = new SystemMessages();

        //    var ra = RegistrationActivity.LoadByActivityIdByRegistrationId(activityId, this.ID.Value, dc: dc);
        //    if (ra.IsEmpty())
        //    {
        //        ra = new RegistrationActivity
        //        {
        //            ActivityId = activityId,
        //            RegistrationId = this.ID,
        //            CreateDate = DateTime.UtcNow,
        //            CreatedBy = loggedInUserId
        //        };
        //        msgs.AddRange(ra.Save(dc));
        //    }

        //    return ra;
        //}

        //public RegistrationActivity AddActivity(Activity activity, int loggedInUserId, SystemMessages msgs = null, DatabaseConnection dc = null)
        //{
        //    return AddActivity(activity.ID.Value, loggedInUserId, msgs, dc);
        //}

        public Account GetAccount(DatabaseConnection dc = null)
        {
            return Account.Load(this.AccountId.Value, dc: dc);
        }

        private ActivityGroup _ActivityGroup = null;
        public ActivityGroup GetActivityGroup(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (this.ActivityGroupId.HasValue && _ActivityGroup.IsEmpty())
            {
                _ActivityGroup = ActivityGroup.Load(this.ActivityGroupId.Value, msgs, dc);
            }
            return _ActivityGroup;
        }

        public Registrant GetRegistrant(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registrant.Load(this.RegistrantId.Value, msgs, dc);
        }

        //public List<RegistrationActivity> GetRegistrationActivities(DatabaseConnection dc = null)
        //{
        //    return RegistrationActivity.LoadByRegistrationId(this.ID.Value, dc: dc);
        //}

        public List<RegistrantActivity> GetRegistrantActivities(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var registrantActivities = RegistrantActivity.LoadByRegistrationId(this.ID.Value, msgs, dc);
            var participationObjectTypes = registrantActivities.Select(registrantActivity => registrantActivity.ParticipationObjectTypeId)
                .DistinctBy(registrantActivity => registrantActivity)
                .ToList();
            var rasToReturn = new List<RegistrantActivity>();

            participationObjectTypes.ForEach(participationObjectType => {

                switch (participationObjectType)
                {
                    case ParticipationObjectType.Activity:
                        var activityIds = registrantActivities.Where(x => x.ParticipationObjectTypeId == participationObjectType)
                            .Select(x => x.ParticipationObjectId.Value)
                            .ToList();
                        var activities = Activity.LoadList(activityIds, msgs, dc);
                        var validActivityIds = activities.Where(x =>
                            !x.IsDeleted &&
                            !x.IsArchived &&
                            (!x.ActivityGroupId.HasValue || !x.ActivityGroupIsArchived.Value))
                            .Select(activity => activity.ID.Value);

                        rasToReturn.AddRange(registrantActivities.Where(
                            registrantActivity => 
                                registrantActivity.ParticipationObjectTypeId == participationObjectType && 
                                registrantActivity.ParticipationObjectId.Value.In(validActivityIds)
                            ));

                        break;

                    case ParticipationObjectType.ActivityGroup:
                        var activityGroupIds = registrantActivities.Where(x => x.ParticipationObjectTypeId == participationObjectType)
                            .Select(x => x.ParticipationObjectId.Value)
                            .ToList();
                        var activityGroups = ActivityGroup.LoadList(activityGroupIds, msgs, dc);
                        var validActivityGroupIds = activityGroups.Where(x => !x.IsArchived)
                            .Select(activity => activity.ID.Value);

                        rasToReturn.AddRange(registrantActivities.Where(
                            registrantActivity => 
                                registrantActivity.ParticipationObjectTypeId == participationObjectType && 
                                registrantActivity.ParticipationObjectId.Value.In(validActivityGroupIds)
                            ));

                        break;

                    case ParticipationObjectType.AccountActivityGroup:
                        var accountActivityGroupIds = registrantActivities.Where(x => x.ParticipationObjectTypeId == participationObjectType)
                            .Select(x => x.ParticipationObjectId.Value)
                            .ToList();
                        var accountActivityGroups = AccountActivityGroup.LoadList(accountActivityGroupIds, msgs, dc);
                        var validAccountActivityGroupIds = accountActivityGroups.Where(x => !x.ActivityGroupIsArchived)
                            .Select(activity => activity.ID.Value);

                        rasToReturn.AddRange(registrantActivities.Where(
                            registrantActivity => 
                                registrantActivity.ParticipationObjectTypeId == participationObjectType && 
                                registrantActivity.ParticipationObjectId.Value.In(validAccountActivityGroupIds)
                            ));

                        break;
                }

            });

            return rasToReturn;
        }

        public SystemMessages RemoveParticipation(int participationObjectId, ParticipationObjectType participationObjectType, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            var ra = RegistrantActivity.LoadByParticipationObjectByRegistrant(
                participationObjectId, 
                participationObjectType, 
                this.RegistrantId.Value,
                msgs,
                dc);

            if (ra.IsNotEmpty())
                msgs.AddRange(ra.CancelRegistration(dc: dc));

            return msgs;
        }

        public SystemMessages WasUpdated(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            this.LastUpdated = DateTime.UtcNow;
            msgs.AddRange(this.Save(dc: dc));

            return msgs;
        }

        //public static string PrintRegisteredActivities(List<RegistrantActivity> registrantActivities, string text, DatabaseConnection dc = null)
        //{
        //    var activities = "<p>" + text + ":<ul>";
        //    foreach (var ra in registrantActivities)
        //    {
        //        var showChoice = registrantActivities.Where(x => x.PreferenceNumber != ra.PreferenceNumber).Any();
        //        var activityname = "";
        //        activityname = ra.ToActivity().GetDaysOfTheWeek(dc: dc).Select(x => x.ToString()).ToStringList(", ", " & ") + ": ";
        //        activityname += ra.ActivityName;
        //        if (showChoice) activityname += " (" + ra.PreferenceNumber.Value.ToOrdinal() + " Choice)";
        //        activities += string.Format("<li>{0}</li>", activityname);
        //    }
        //    activities += "</ul></p>";
        //    return activities;
        //}

        //public static string GetRegistrationConfirmationHtml(Registration registration, Account account, Registrant registrant, List<RegistrantActivity> registrantActivities, SystemMessages msgs, DatabaseConnection dc = null)
        //{
        //    if (msgs.IsEmpty()) msgs = new SystemMessages();

        //    var group = registration.GetActivityGroup(msgs, dc);

        //    if (registrant.IsEmpty())
        //        registrant = registration.GetRegistrant(msgs, dc);

        //    if (registrantActivities.IsEmpty())
        //        registrantActivities = RegistrantActivity.LoadByRegistrationId(registration.ID.Value, msgs, dc)
        //            .OrderBy(x => x.PreferenceNumber)
        //            .ToList();

        //    AccountProperty thankyou = null;

        //    var reservedRas = registrantActivities.Where(x => x.NumberOfSeats > 0)
        //        .DistinctBy(x => x.ActivityId)
        //        .ToList();
        //    var waitingListRas = registrantActivities.Where(x => x.NumberOfWaitingListSeats > 0)
        //        .DistinctBy(x => x.ActivityId)
        //        .ToList();
        //    var applicationRas = registrantActivities.Where(x => x.NumberOfApplicationSeats > 0)
        //        .DistinctBy(x => x.ActivityId)
        //        .ToList();
        //    var hasLotteryRan = group.IsNotEmpty() && group.HasLotteryRun;

        //    string activities = "";

        //    // only has an application
        //    if (applicationRas.Any() && !reservedRas.Any() && !waitingListRas.Any())
        //    {
        //        if (group.IsNotEmpty())
        //            thankyou = group.GetGroupProperty(AccountPropertyType.ApplicationThankYouEmail, dc: dc);
        //        else
        //            thankyou = account.GetAccountProperty(AccountPropertyType.ApplicationThankYouEmail, dc: dc);
        //    }
        //    // has registered as normal
        //    else
        //    {
        //        if (group.IsNotEmpty())
        //            thankyou = group.GetGroupProperty(AccountPropertyType.RegistrationThankYouEmail, dc: dc);
        //        else
        //            thankyou = account.GetAccountProperty(AccountPropertyType.RegistrationThankYouEmail, dc: dc);
        //    }

        //    // have any reserved activities
        //    if (reservedRas.Any())
        //        activities += PrintRegisteredActivities(
        //            reservedRas,
        //            hasLotteryRan ? "You have been selected for" : "You have registered for",
        //            dc);

        //    // if the lottery has been run and the registrant didn't make it in some of the desired activities
        //    if (hasLotteryRan && (waitingListRas.Any() || applicationRas.Any()))
        //        activities += "<p>Unfortunately, you have <u>not</u> been selected for the following</p>";

        //    if (waitingListRas.Any())
        //        activities += PrintRegisteredActivities(waitingListRas, "You have been placed on the waiting list for", dc: dc);

        //    if (applicationRas.Any())
        //        activities += PrintRegisteredActivities(
        //            applicationRas,
        //            hasLotteryRan ? "You were <u>not</u> placed on the waiting list for" : "You have applied for",
        //            dc);

        //    var custommessage = "";
        //    if (thankyou.IsNotEmpty())
        //        custommessage = thankyou.Value;

        //    var accountInformation = "";
        //    var masterRegistrant = registrant.GetMasterRegistrant(msgs, dc);
        //    if (masterRegistrant.IsNotEmpty())
        //    {
        //        accountInformation = "<p>";
        //        var masterRegistrantAccount = masterRegistrant.GetAccount(msgs, dc);

        //        if (masterRegistrantAccount?.AccountTypeId == AccountType.Registrant)
        //            accountInformation += $"<a href=\"{Email.BaseUrl(masterRegistrantAccount.IsAfterSchool)}login\">Login to view your {Utilities.AppName(masterRegistrantAccount.IsAfterSchool)} account</a>";

        //        accountInformation += "</p>";
        //    }

            

        //    return $"{custommessage}{activities}{accountInformation}";
        //}

        public static SystemMessages SendConfirmation(CheckoutObject checkoutObject, bool sendBcc, DatabaseConnection dc = null)
        {
            return EmailHelper.SendRegistrationConfirmed(checkoutObject, sendBcc);
        }

        //[Obsolete]
        //public static SystemMessages SendConfirmation(int registrationId, bool sendBcc, DatabaseConnection dc = null)
        //{
        //    var msgs = new SystemMessages();
        //    var registration = Registration.Load(registrationId, dc: dc);
        //    var registrantActivities = RegistrantActivity.LoadByRegistrationId(registration.ID.Value, msgs, dc)
        //        .OrderBy(x => x.PreferenceNumber)
        //        .ToList();
        //    var account = registration.GetAccount(dc: dc);
        //    var registrant = registration.GetRegistrant(dc: dc);

        //    var message = GetRegistrationConfirmationHtml(registration, account, registrant, registrantActivities, msgs, dc);
        //    var fullmessage = $"<p>Hello {registrant.Name},</p>{message}<p>{account.Name}</p>";

        //    // send message to registrant
        //    SendGridHelper.SendMail(
        //        fromEmail: new SendGrid.Helpers.Mail.EmailAddress(
        //                        Email.AppEmailAddress, 
        //                        $"{account.Name} via {Email.AppEmailFromName(account.IsAfterSchool)}"
        //                    ),
        //        toEmail: new SendGrid.Helpers.Mail.EmailAddress(registrant.Email, registrant.Name),
        //        subject: $"Your {account.Name} registration",
        //        body: $"{fullmessage}{(Email.GetAppSignature(isAfterSchool: account.IsAfterSchool, utm_campaign: "registration_confirmation"))}",
        //        replyTo: new SendGrid.Helpers.Mail.EmailAddress(account.Email, account.Name),
        //        emailTemplateId: SendGridHelper.GetGenericTemplateId(account.IsAfterSchool)
        //    );

        //    if (sendBcc)
        //    {
        //        SendGridHelper.SendMail(
        //            fromEmail: new SendGrid.Helpers.Mail.EmailAddress(
        //                            Email.AppEmailAddress, 
        //                            $"{account.Name} via {Utilities.AppName(account.IsAfterSchool)}"
        //                       ),
        //            toEmail: new SendGrid.Helpers.Mail.EmailAddress(account.Email),
        //            subject: $"CC: Your {account.Name} registration",
        //            body: $"{"A CC of the email is below: <br />-------------------<br />"}{fullmessage}{(Email.GetAppSignature(isAfterSchool: account.IsAfterSchool, utm_campaign: "registration_confirmation"))}",
        //            emailTemplateId: SendGridHelper.GetGenericTemplateId(account.IsAfterSchool)
        //        );
        //    }

        //    registrantActivities.ForEach(ra => ra.SendNewRegistrationNotice(account, null, null));

        //    return msgs;
        //}        

        private static DatabaseQuery GetSearchQuery(string searchText, int accountId, int? questionId, RegistrationStatus regStatus, List<int> participationObjectIds = null, ParticipationObjectType participationObjectType = ParticipationObjectType.NotSet, RegistrationAssignmentType rat = RegistrationAssignmentType.NotSet, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.Criterias.Add(ObjectMap["AccountId"], accountId);
            dq.Criterias.Add(ObjectMap["CompletionStatusId"], ValidCompletionStatus, DatabaseCriteriaOperatorEnum.In);
            // dq.Criterias.Add(ObjectMap["FirstRegistrationId"], DBNull.Value, DatabaseCriteriaOperatorEnum.Is);

            // aggregate related registrations
            var regQuery = Registration.DatabaseQuery;
            regQuery.UseParameterizedQuery = false;
            regQuery.Joins[0].Alias = "Registrant1";
            regQuery.SelectParameters.Clear();
            regQuery.SelectParameters.Add(
                Registration.ObjectMap["Id"], 
                "regid", 
                string.Format("ISNULL({0}, {1})", DatabaseQuery.FunctionReplaceValue, ObjectMap["ID"].ParameterFullName)
                );

            regQuery.Criterias.Add(ObjectMap["AccountId"], accountId);

            // getting only registered seats
            if (rat != RegistrationAssignmentType.NotSet || (participationObjectIds.IsNotEmpty() && participationObjectIds.Any()))
            {
                var raQuery = BaseObject.get_DatabaseQuery(ObjectMap, typeof(RegistrantActivity).Name);
                raQuery.UseParameterizedQuery = false;
                raQuery.SelectParameters.Clear();
                raQuery.SelectParameters.Add(RegistrantActivity.ObjectMap["RegistrationId"]);
                raQuery.Joins.Add(ObjectMap["ID"], RegistrantActivity.ObjectMap["RegistrationId"], DatabaseJoinTypeEnum.InnerJoin);

                if (participationObjectIds.IsNotEmpty() && participationObjectIds.Any())
                {
                    raQuery.Criterias.Add(RegistrantActivity.ObjectMap["ParticipationObjectId"], participationObjectIds, DatabaseCriteriaOperatorEnum.In);
                    raQuery.Criterias.Add(RegistrantActivity.ObjectMap["ParticipationObjectTypeId"], (int)participationObjectType);
                }

                if (rat != RegistrationAssignmentType.NotSet)
                    raQuery.Criterias.Add(RegistrantActivity.ObjectMap["AssignmentTypeId"], (int)rat);

                regQuery.Criterias.Add(ObjectMap["Id"], raQuery, DatabaseCriteriaOperatorEnum.In);
            }

            if (regStatus != RegistrationStatus.NotSet)
            {
                if (regStatus == RegistrationStatus.Current)
                {
                    var aQuery = BaseObject.get_DatabaseQuery(ObjectMap, typeof(RegistrantActivity).Name);
                    aQuery.UseParameterizedQuery = false;
                    aQuery.SelectParameters.Clear();
                    aQuery.SelectParameters.Add(RegistrantActivity.ObjectMap["RegistrationId"]);
                    aQuery.Joins.Add(ObjectMap["ID"], RegistrantActivity.ObjectMap["RegistrationId"], DatabaseJoinTypeEnum.InnerJoin);
                    aQuery.Joins.Add(Activity.ObjectMap["ID"], RegistrantActivity.ObjectMap["ActivityId"], DatabaseJoinTypeEnum.InnerJoin);
                    aQuery.Joins.Add(ActivityInstance.ObjectMap["ActivityScheduleId"], Activity.ObjectMap["ActivityScheduleId"], DatabaseJoinTypeEnum.InnerJoin);

                    aQuery.Criterias.Add(ObjectMap["AccountId"], accountId);
                    // has start dates that are less than or equal to now
                    aQuery.Criterias.Add(ActivityInstance.ObjectMap["StartDateTime"], DateTime.UtcNow.LastDateOfWeek(), DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

                    if (rat != RegistrationAssignmentType.NotSet)
                        aQuery.Criterias.Add(RegistrantActivity.ObjectMap["AssignmentTypeId"], (int)rat);

                    regQuery.Criterias.Add(ObjectMap["Id"], aQuery, DatabaseCriteriaOperatorEnum.In);

                    var bQuery = BaseObject.get_DatabaseQuery(ObjectMap, typeof(RegistrantActivity).Name);
                    bQuery.UseParameterizedQuery = false;
                    bQuery.SelectParameters.Clear();
                    bQuery.SelectParameters.Add(RegistrantActivity.ObjectMap["RegistrationId"]);
                    bQuery.Joins.Add(ObjectMap["ID"], RegistrantActivity.ObjectMap["RegistrationId"], DatabaseJoinTypeEnum.InnerJoin);
                    bQuery.Joins.Add(Activity.ObjectMap["ID"], RegistrantActivity.ObjectMap["ActivityId"], DatabaseJoinTypeEnum.InnerJoin);
                    bQuery.Joins.Add(ActivityInstance.ObjectMap["ActivityScheduleId"], Activity.ObjectMap["ActivityScheduleId"], DatabaseJoinTypeEnum.InnerJoin);

                    bQuery.Criterias.Add(ObjectMap["AccountId"], accountId);
                    // has end dates that are greater than or equal to now
                    bQuery.Criterias.Add(ActivityInstance.ObjectMap["EndDateTime"], DateTime.UtcNow.FirstDateOfWeek(), DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

                    if (rat != RegistrationAssignmentType.NotSet)
                        bQuery.Criterias.Add(RegistrantActivity.ObjectMap["AssignmentTypeId"], (int)rat);

                    regQuery.Criterias.Add(ObjectMap["Id"], bQuery, DatabaseCriteriaOperatorEnum.In);
                }
                else if (regStatus == RegistrationStatus.Future)
                {
                    // has no end dates in the past
                    var aQuery = BaseObject.get_DatabaseQuery(ObjectMap, typeof(RegistrantActivity).Name);
                    aQuery.UseParameterizedQuery = false;
                    aQuery.SelectParameters.Clear();
                    aQuery.SelectParameters.Add(RegistrantActivity.ObjectMap["RegistrationId"]);
                    aQuery.Joins.Add(ObjectMap["ID"], RegistrantActivity.ObjectMap["RegistrationId"], DatabaseJoinTypeEnum.InnerJoin);
                    aQuery.Joins.Add(Activity.ObjectMap["ID"], RegistrantActivity.ObjectMap["ActivityId"], DatabaseJoinTypeEnum.InnerJoin);
                    aQuery.Joins.Add(ActivityInstance.ObjectMap["ActivityScheduleId"], Activity.ObjectMap["ActivityScheduleId"], DatabaseJoinTypeEnum.InnerJoin);

                    aQuery.Criterias.Add(ObjectMap["AccountId"], accountId);
                    aQuery.Criterias.Add(ActivityInstance.ObjectMap["EndDateTime"], DateTime.UtcNow.LastDateOfWeek(), DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

                    if (rat != RegistrationAssignmentType.NotSet)
                        aQuery.Criterias.Add(RegistrantActivity.ObjectMap["AssignmentTypeId"], (int)rat);

                    var parameter = Registrant.ObjectMap["id"];
                    parameter.TableAlias = "Registrant1";
                    aQuery.Criterias.Add(RegistrantActivity.ObjectMap["RegistrantId"], parameter);

                    regQuery.Criterias.Add(ObjectMap["Id"], aQuery, DatabaseCriteriaOperatorEnum.NotIn);

                    // has some records in the activity instance table though
                    var bQuery = BaseObject.get_DatabaseQuery(ObjectMap, typeof(RegistrantActivity).Name);
                    bQuery.UseParameterizedQuery = false;
                    bQuery.SelectParameters.Clear();
                    bQuery.SelectParameters.Add(RegistrantActivity.ObjectMap["RegistrationId"]);
                    bQuery.Joins.Add(ObjectMap["ID"], RegistrantActivity.ObjectMap["RegistrationId"], DatabaseJoinTypeEnum.InnerJoin);
                    bQuery.Joins.Add(Activity.ObjectMap["ID"], RegistrantActivity.ObjectMap["ActivityId"], DatabaseJoinTypeEnum.InnerJoin);
                    bQuery.Joins.Add(ActivityInstance.ObjectMap["ActivityScheduleId"], Activity.ObjectMap["ActivityScheduleId"], DatabaseJoinTypeEnum.InnerJoin);
                    bQuery.Criterias.Add(ObjectMap["AccountId"], accountId);

                    if (rat != RegistrationAssignmentType.NotSet)
                        bQuery.Criterias.Add(RegistrantActivity.ObjectMap["AssignmentTypeId"], (int)rat);

                    bQuery.Criterias.Add(RegistrantActivity.ObjectMap["RegistrantId"], parameter);

                    regQuery.Criterias.Add(ObjectMap["Id"], bQuery, DatabaseCriteriaOperatorEnum.In);
                }
                else if (regStatus == RegistrationStatus.Past)
                {
                    // has no start dates in the future
                    var aQuery = BaseObject.get_DatabaseQuery(ObjectMap, typeof(RegistrantActivity).Name);
                    aQuery.UseParameterizedQuery = false;
                    aQuery.SelectParameters.Clear();
                    aQuery.SelectParameters.Add(RegistrantActivity.ObjectMap["RegistrationId"]);
                    aQuery.Joins.Add(ObjectMap["ID"], RegistrantActivity.ObjectMap["RegistrationId"], DatabaseJoinTypeEnum.InnerJoin);
                    aQuery.Joins.Add(Activity.ObjectMap["ID"], RegistrantActivity.ObjectMap["ActivityId"], DatabaseJoinTypeEnum.InnerJoin);
                    aQuery.Joins.Add(ActivityInstance.ObjectMap["ActivityScheduleId"], Activity.ObjectMap["ActivityScheduleId"], DatabaseJoinTypeEnum.InnerJoin);

                    aQuery.Criterias.Add(ObjectMap["AccountId"], accountId);
                    aQuery.Criterias.Add(ActivityInstance.ObjectMap["StartDateTime"], DateTime.UtcNow.FirstDateOfWeek(), DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

                    if (rat != RegistrationAssignmentType.NotSet)
                        aQuery.Criterias.Add(RegistrantActivity.ObjectMap["AssignmentTypeId"], (int)rat);

                    var parameter = Registrant.ObjectMap["id"];
                    parameter.TableAlias = "Registrant1";
                    aQuery.Criterias.Add(RegistrantActivity.ObjectMap["RegistrantId"], parameter);

                    regQuery.Criterias.Add(ObjectMap["Id"], aQuery, DatabaseCriteriaOperatorEnum.NotIn);
                   
                }
            }

            if (searchText.IsNotBlank())
            {
                var itemList = new List<string>();

                if (questionId.HasValue)
                {
                    itemList.Add(searchText);
                }
                else
                {
                    itemList.AddRange(searchText.Split(' ').Select(x => x.Trim()));
                }

                foreach (var item in itemList)
                {
                    if (item.IsNotBlank())
                    {
                        var answerQuery = BaseObject.get_DatabaseQuery(ObjectMap, typeof(FormQuestionAnswer).Name);
                        answerQuery.UseParameterizedQuery = false;
                        answerQuery.SelectParameters.Clear();
                        answerQuery.SelectParameters.Add(FormQuestionAnswer.ObjectMap["RegistrationId"]);
                        answerQuery.Joins.Add(ObjectMap["ID"], FormQuestionAnswer.ObjectMap["RegistrationId"], DatabaseJoinTypeEnum.InnerJoin);
                        answerQuery.Criterias.Add(FormQuestionAnswer.ObjectMap["RegistrationAccountId"], accountId);

                        if (questionId.HasValue)
                        {
                            answerQuery.Joins.Add(FormQuestion.ObjectMap["ID"], FormQuestionAnswer.ObjectMap["FormQuestionId"], DatabaseJoinTypeEnum.InnerJoin);

                            answerQuery.Criterias.Add(FormQuestionAnswer.ObjectMap["FormQuestionId"], questionId.Value);

                            var criteriaGroup1 = new DatabaseCriterias(LogicalOperatorType.Or);

                            var criteriaGroup2 = new DatabaseCriterias(LogicalOperatorType.And);
                            criteriaGroup2.Add(FormQuestionAnswer.ObjectMap["Text"], item);
                            criteriaGroup2.Add(FormQuestion.ObjectMap["Options"], DBNull.Value, DatabaseCriteriaOperatorEnum.IsNot);
                            criteriaGroup1.Add(criteriaGroup2);

                            var criteriaGroup3 = new DatabaseCriterias(LogicalOperatorType.And);
                            criteriaGroup3.Add(FormQuestionAnswer.ObjectMap["Text"], item, DatabaseCriteriaOperatorEnum.Like);
                            criteriaGroup3.Add(FormQuestion.ObjectMap["Options"], DBNull.Value, DatabaseCriteriaOperatorEnum.Is);
                            criteriaGroup1.Add(criteriaGroup3);

                            answerQuery.Criterias.Add(criteriaGroup1);
                        }
                        else
                        {
                            answerQuery.Criterias.Add(FormQuestionAnswer.ObjectMap["Text"], item, DatabaseCriteriaOperatorEnum.Like);
                        }

                        regQuery.Criterias.Add(ObjectMap["Id"], answerQuery, DatabaseCriteriaOperatorEnum.In);
                    }
                }
            }

            dq.Criterias.Add(ObjectMap["ID"], regQuery, DatabaseCriteriaOperatorEnum.In);

            dq.OrderBys.Add(ObjectMap["RegistrantFirstName"], OrderByDirectionEnum.Ascending);
            dq.OrderBys.Add(ObjectMap["RegistrantLastName"], OrderByDirectionEnum.Ascending);

            return dq;
        }

        public static List<Registration> Search(string searchText, int accountId, int? questionId, List<int> participationObjectIds, ParticipationObjectType participationObjectType, RegistrationStatus regStatus, RegistrationAssignmentType rat, int? start, int? length, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = GetSearchQuery(searchText, accountId, questionId, regStatus, participationObjectIds, participationObjectType, rat, msgs, dc);
            dq.UseParameterizedQuery = false;
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;

            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int SearchCount(string searchText, int accountId, int? questionId, List<int> participationObjectIds, ParticipationObjectType participationObjectType, RegistrationStatus regStatus, RegistrationAssignmentType rat, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = GetSearchQuery(searchText, accountId, questionId, regStatus, participationObjectIds, participationObjectType, rat, msgs, dc);
            return dq.GetCount(msgs);
        }

        public SystemMessages CancelAll(DatabaseConnection dc = null)
        {
            return this.DeleteWithRelatedRecords(dc: dc);
        }

        public static List<Registration> LoadByRegistrationPeriodId(int RegistrationPeriodId, int? start, int? length, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["RegistrationPeriodId"], RegistrationPeriodId);
            dq.Criterias.Add(ObjectMap["CompletionStatusId"], ValidCompletionStatus, DatabaseCriteriaOperatorEnum.In);
            dq.OrderBys.Add(ObjectMap["RegistrantFirstName"], OrderByDirectionEnum.Ascending);
            dq.OrderBys.Add(ObjectMap["RegistrantLastName"], OrderByDirectionEnum.Ascending);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Registration> LoadByActivityGroupId(int ActivityGroupId, int? start, int? length, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ActivityGroupId"], ActivityGroupId);
            dq.Criterias.Add(ObjectMap["CompletionStatusId"], ValidCompletionStatus, DatabaseCriteriaOperatorEnum.In);
            dq.OrderBys.Add(ObjectMap["RegistrantFirstName"], OrderByDirectionEnum.Ascending);
            dq.OrderBys.Add(ObjectMap["RegistrantLastName"], OrderByDirectionEnum.Ascending);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Registration> LoadByAccountActivityGroupId(int AccountActivityGroupId, int? start, int? length, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountActivityGroupId"], AccountActivityGroupId);
            dq.Criterias.Add(ObjectMap["CompletionStatusId"], ValidCompletionStatus, DatabaseCriteriaOperatorEnum.In);
            dq.OrderBys.Add(ObjectMap["RegistrantFirstName"], OrderByDirectionEnum.Ascending);
            dq.OrderBys.Add(ObjectMap["RegistrantLastName"], OrderByDirectionEnum.Ascending);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public decimal GetAmountPaid(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var registrantActivities = this.GetRegistrantActivities(msgs, dc);

            if (registrantActivities.Any())
                return registrantActivities.First().GetAmountPaid(msgs, dc);

            return 0;
        }

        #region CSVReport

        public static SystemMessages EmailCSVReport(int accountId, string searchText, int? questionId, List<int> participationObjectIds, ParticipationObjectType participationObjectType, string filename, RegistrationReportType reportType, RegistrationAssignmentType rat, string uiActivityLabel, string emails, string subject, string body)
        {
            var msgs = new SystemMessages();
            var account = Account.Load(accountId);
            var registrations = account.SearchRegistrations(
                searchText,
                questionId,
                participationObjectIds,
                participationObjectType,
                RegistrationStatus.NotSet,
                rat,
                null,
                null);

            string csv = Registration.ExportCSV(
                registrations,
                account,
                reportType,
                RegistrationStatus.NotSet,
                RegistrationAssignmentType.NotSet);

            var ms = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(csv));
            var ah = new AmazonHelper(
                Utilities.AmazonBucketName, 
                Utilities.AmazonAccessKeyId, 
                Utilities.AmazonSecretAccessKeyId, 
                Utilities.AmazonRegionEndPoint);

            if (filename.IsNotEmpty())
            {
                filename = new System.Text.RegularExpressions.Regex(BasicLibrary.Utilities.NoAlphaNumericCharactersRegEx)
                    .Replace(filename, "");

                if (!filename.Contains(".csv"))
                    filename += ".csv";
            }
            else
                filename = "registrations.csv";

            string amazonLocation = $"{account.Uid.ToString()}/{Guid.NewGuid().ToString()}/{filename}";
            msgs.AddRange(ah.AddFile(ms, amazonLocation, true));

            string fileLink = ah.GetFileLink(amazonLocation, false, null, true);

            if (subject.IsBlank())
                subject = uiActivityLabel + " Report";

            if (body.IsBlank())
                body = "";

            body += $"<p>Here is your {uiActivityLabel} report: <a href=\"{fileLink}\">{fileLink}</a></p>{(Email.GetAppSignature(isAfterSchool: account.IsAfterSchool, utm_campaign: "student_report"))}";

            var emailList = new List<string>();

            // add account email address
            if (account.Email.IsNotBlank())
                emailList.Add(account.Email);

            // get additional emails the user enters
            if (emails.IsNotBlank())
                emailList.AddRange(
                    emails.Split(',')
                    .Select(x => x.Trim())
                    .Where(email => email.IsRegExMatch(BasicLibrary.Utilities.EmailRegEx)));

            // send to email list
            emailList.ForEach(email =>
                msgs.AddRange(SendGridHelper.SendMail(
                    fromEmail: new SendGrid.Helpers.Mail.EmailAddress(
                        Email.AppEmailAddress,
                        $"{account.Name} via {Utilities.AppName(account.IsAfterSchool)}"
                        ),
                    toEmail: new SendGrid.Helpers.Mail.EmailAddress(email),
                    subject: subject,
                    body: body,
                    emailTemplateId: SendGridHelper.GetGenericTemplateId(account.IsAfterSchool)
                ))
            );

            return msgs;
        }

        public static string ExportCSV(List<Registration> registrations, Account account, RegistrationReportType reportType, RegistrationStatus registrationStatus, RegistrationAssignmentType assignmentType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var registrantActivities = new List<RegistrantActivity>();
            var registrantIds = registrations.Select(x => x.RegistrantId.Value).ToList();

            registrantActivities = RegistrantActivity.LoadByRegistrantIds(registrantIds, msgs, dc);

            if (assignmentType != RegistrationAssignmentType.NotSet)
                registrantActivities = registrantActivities.Where(x => x.AssignmentTypeId == assignmentType)
                .ToList();

            switch(registrationStatus)
            {
                case RegistrationStatus.Current:
                    registrantActivities = registrantActivities.Where(x => x.IsInSession(msgs, dc)).ToList();
                    break;
                case RegistrationStatus.Past:
                    registrantActivities = registrantActivities.Where(x => x.WasInSession(msgs, dc)).ToList();
                    break;
                case RegistrationStatus.Future:
                    registrantActivities = registrantActivities.Where(x => x.WillBeInSession(msgs, dc)).ToList();
                    break;
            }

            var answers = FormQuestionAnswer.LoadByRegistrationIds(registrations.Select(x => x.ID.Value).ToList(), msgs, dc)
               .Where(x => x.Text.IsNotBlank())
               .ToList();

            var questionIds = answers.Select(x => x.FormQuestionId.Value).Distinct().ToList();

            var questions = FormQuestion.LoadList(questionIds, msgs, dc)
                .OrderBy(x => x.DisplayOrder)
                .OrderBy(x => x.FormQuestionGroupDisplayOrder)
                .ToList();

            var standardQuestions = questions
                .Where(x => x.QuestionDataTypeId != QuestionDataType.NotSet)
                .DistinctBy(x => x.QuestionDataTypeId)
                .ToList();
            var customQuestions = questions
                .Where(x => x.QuestionDataTypeId == QuestionDataType.NotSet)
                .DistinctBy(x => x.Text)
                .ToList();

            var assignmentTypes = Enum.GetValues(typeof(RegistrationAssignmentType))
                .Cast<RegistrationAssignmentType>()
                .Where(x => x != RegistrationAssignmentType.NotSet &&
                            x != RegistrationAssignmentType.Declined &&
                            x != RegistrationAssignmentType.Invited)
                .OrderBy(x => x)
                .ToList();

            var table = new List<List<string>>();

            table.Add(AddHeadersToCSVReport(
                reportType,
                standardQuestions,
                customQuestions
                ));

            // print data for each registration
            registrations.ForEach(registration =>
                table.Add(AddEachRegistrationToCSVReport(
                    registration,
                    account,
                    reportType,
                    assignmentTypes,
                    registrantActivities,
                    standardQuestions,
                    customQuestions,
                    answers,
                    msgs,
                    dc))
            );

            return CsvFile.Create(table);
        }

        private static List<string> AddHeadersToCSVReport(RegistrationReportType reportType, List<FormQuestion> standardQuestions, List<FormQuestion> customQuestions)
        {
            var row = new List<string>();

            row.Add($"Date Registered");

            row.Add("First Name");
            row.Add("Last Name");
            row.Add("Email");
            row.Add("Activities");

            if (reportType != RegistrationReportType.Basic)
            {
                row.Add("Amount Paid");
                AddQuestionHeadersToCSVReport(row, standardQuestions, customQuestions);
            }

            return row;
        }

        private static List<string> AddEachRegistrationToCSVReport(Registration registration, Account account, RegistrationReportType reportType, List<RegistrationAssignmentType> assignmentTypes, List<RegistrantActivity> registrantActivities, List<FormQuestion> standardQuestions, List<FormQuestion> customQuestions, List<FormQuestionAnswer> answers, SystemMessages msgs, DatabaseConnection dc = null)
        {            
            var row = new List<string>();

            row.Add(account.ConvertToAccountTimeZone(registration.CreateDate.Value).ToShortDateTimeString());

            row.Add(registration.RegistrantFirstName);
            row.Add(registration.RegistrantLastName);
            row.Add(registration.RegistrantEmail);

            AddActivitiesToCSVReport(row, registration, registrantActivities);

            if (reportType != RegistrationReportType.Basic)
            {
                row.Add(registration.GetAmountPaid(msgs, dc).ToCurrency());
                AddQuestionsToCSVReport(row, standardQuestions, customQuestions, answers, registration);
            }

            return row;
        }

        private static void AddActivitiesToCSVReport(List<string> row, Registration registration, List<RegistrantActivity> registrantActivities, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var regIds = registration.ID.Value.AsList();
            var ras = registrantActivities.Where(registrantActivity => registrantActivity.RegistrationId.Value.In(regIds))
                .ToList();
            var participationObjects = ras.Select(registrantActivity =>
                new
                {
                    registrantActivity.ParticipationObjectId,
                    registrantActivity.ParticipationObjectTypeId
                })
                .ToList();

            var activitiesList = new List<string>();
            participationObjects.ForEach(participationObject => {
                switch(participationObject.ParticipationObjectTypeId)
                {
                    case ParticipationObjectType.AccountActivityGroup:
                        var aag = AccountActivityGroup.Load(participationObject.ParticipationObjectId.Value, msgs, dc);
                        activitiesList.Add(ActivityGroup.Load(aag.ActivityGroupId.Value, msgs, dc).Name);
                        break;
                    case ParticipationObjectType.Activity:
                        activitiesList.Add(Activity.Load(participationObject.ParticipationObjectId.Value, msgs, dc).Name);
                        break;
                    case ParticipationObjectType.ActivityGroup:
                        activitiesList.Add(ActivityGroup.Load(participationObject.ParticipationObjectId.Value, msgs, dc).Name);
                        break;
                }
            });

            row.Add(activitiesList.ToStringList("; "));
        }

        private static void AddQuestionsToCSVReport(List<string> row, List<FormQuestion> standardQuestions, List<FormQuestion> customQuestions, List<FormQuestionAnswer> answers, Registration registration)
        {
            foreach (var question in standardQuestions)
            {
                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantFirstName:
                    case QuestionDataType.RegistrantLastName:
                    case QuestionDataType.RegistrantEmail:
                        break;
                    default:
                        var answer = answers
                            .Where(item =>
                                registration.ID == item.RegistrationId &&
                                item.FormQuestionQuestionDataTypeId == question.QuestionDataTypeId)
                            .OrderBy(x => x.RegistrationId)
                            .LastOrDefault();

                        if (answer.IsNotEmpty())
                            row.Add(answer.Text);
                        else
                            row.Add("");
                        break;
                }
            }

            foreach (var question in customQuestions)
            {
                var answer = answers
                    .Where(item =>
                        registration.ID == item.RegistrationId &&
                        item.FormQuestionText == question.Text)
                    .OrderBy(x => x.RegistrationId)
                    .LastOrDefault();

                if (answer.IsNotEmpty())
                    row.Add(answer.Text);
                else
                    row.Add("");
            }
        }

        private static void AddQuestionHeadersToCSVReport(List<string> row, List<FormQuestion> standardQuestions, List<FormQuestion> customQuestions)
        {
            foreach (var question in standardQuestions)
            {
                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantFirstName:
                    case QuestionDataType.RegistrantLastName:
                    case QuestionDataType.RegistrantEmail:
                        break;
                    default:
                        row.Add($"{question.FormQuestionGroupName} - {question.Text.ToString()}");
                        break;
                }
            }
            foreach (var question in customQuestions)
            {
                row.Add(question.Text.ToString());
            }
        }

        //public static string ExportCSV(List<Registration> registrations, Account account, RegistrationReportType reportType, RegistrationStatus regStatus, RegistrationAssignmentType rat, List<int> groupIds, DatabaseConnection dc = null)
        //{
        //    var allRegistrations = new List<Registration>();
        //    var loopingRegistrations = registrations;
        //    var days = new List<DayOfWeekASL>();
        //    var registrantActivities = new List<RegistrantActivity>();
        //    var allDayActivities = new List<Activity>();

        //    allRegistrations.AddRange(registrations);

        //    if (reportType != RegistrationReportType.SingleActivity)
        //    {
        //        registrations.ForEach(registration => allRegistrations.AddRange(registration.GetRelatedRegistrations(dc: dc)));

        //        var firstRegistrations = allRegistrations.Where(x => !x.FirstRegistrationId.HasValue).ToList();
        //        var registrantIds = allRegistrations.Select(x => x.RegistrantId.Value).ToList();
        //        registrantActivities = RegistrantActivity.LoadByRegistrantIds(registrantIds, dc: dc)
        //            .Where(x => x.AssignmentTypeId == rat)
        //            .ToList();

        //        days = Enum.GetValues(typeof(DayOfWeekASL))
        //                        .Cast<DayOfWeekASL>()
        //                        .Where(x => x != DayOfWeekASL.NotSet)
        //                        .ToList();

        //        var allActivityIds = registrantActivities.Select(x => x.ActivityId.Value).Distinct().ToList();
        //        allDayActivities = Activity.GetByActivitiesByRegistrants(allActivityIds, registrantIds, DayOfWeekASL.NotSet, regStatus, groupIds, dc: dc);

        //        // preload days for each activity (trying to find a faster way to do this)
        //        allDayActivities.ForEach(x => x.GetDays(dc: dc));

        //        loopingRegistrations = firstRegistrations;
        //    }

        //    // only load last registration answers
        //    var answers = FormQuestionAnswer.LoadByRegistrationIds(allRegistrations.Select(x => x.ID.Value).ToList(), dc: dc)
        //        .Where(x => x.Text.IsNotBlank())
        //        .ToList();

        //    var questionIds = answers.Select(x => x.FormQuestionId.Value).Distinct().ToList();

        //    var questions = FormQuestion.LoadList(questionIds, dc: dc)
        //        .OrderBy(x => x.DisplayOrder)
        //        .OrderBy(x => x.FormQuestionGroupDisplayOrder)
        //        .ToList();

        //    var standardQuestions = questions
        //        .Where(x => x.QuestionDataTypeId != QuestionDataType.NotSet)
        //        .DistinctBy(x => x.QuestionDataTypeId)
        //        .ToList();
        //    var customQuestions = questions
        //        .Where(x => x.QuestionDataTypeId == QuestionDataType.NotSet)
        //        .DistinctBy(x => x.Text)
        //        .ToList();

        //    var assignmentTypes = Enum.GetValues(typeof(RegistrationAssignmentType))
        //        .Cast<RegistrationAssignmentType>()
        //        .Where(x => x != RegistrationAssignmentType.NotSet &&
        //                    x != RegistrationAssignmentType.Declined &&
        //                    x != RegistrationAssignmentType.Invited)
        //        .OrderBy(x => x)
        //        .ToList();

        //    var table = new List<List<string>>();

        //    table.Add(AddHeadersToCSVReport(
        //        reportType,
        //        days,
        //        standardQuestions,
        //        customQuestions
        //        ));

        //    // print data for each registration
        //    loopingRegistrations.ForEach(registration =>
        //        table.Add(AddEachRegistrationToCSVReport(
        //            registration,
        //            allRegistrations,
        //            account,
        //            reportType,
        //            assignmentTypes,
        //            days,
        //            registrantActivities,
        //            allDayActivities,
        //            standardQuestions,
        //            customQuestions,
        //            answers,
        //            dc))
        //    );

        //    return CsvFile.Create(table);
        //}

        //private static List<string> AddHeadersToCSVReport(RegistrationReportType reportType, List<DayOfWeekASL> days, List<FormQuestion> standardQuestions, List<FormQuestion> customQuestions)
        //{
        //    var row = new List<string>();

        //    row.Add($"Date {(reportType == RegistrationReportType.SingleActivity ? "" : "First ")}Registered");

        //    if (reportType == RegistrationReportType.Detailed)
        //        row.Add("Date Last Registered");

        //    row.Add("First Name");
        //    row.Add("Last Name");
        //    row.Add("Email");

        //    /*
        //    if (reportType == RegistrationReportType.Basic && false)
        //    {
        //        assignmentTypes = new List<RegistrationAssignmentType>() { RegistrationAssignmentType.Reserved };

        //        foreach (var type in assignmentTypes)
        //        {
        //            row.Add(type.ToString().SplitByUpperCase());
        //        }
        //    }
        //    */

        //    if (reportType != RegistrationReportType.SingleActivity)
        //        days.ForEach(day => row.Add(day.ToString()));

        //    if (reportType != RegistrationReportType.Basic)
        //    {
        //        row.Add("Amount Paid");
        //        AddQuestionHeadersToCSVReport(row, standardQuestions, customQuestions);
        //    }

        //    return row;
        //}

        //private static List<string> AddEachRegistrationToCSVReport(Registration registration, List<Registration> allRegistrations, Account account, RegistrationReportType reportType, List<RegistrationAssignmentType> assignmentTypes, List<DayOfWeekASL> days, List<RegistrantActivity> registrantActivities, List<Activity> allDayActivities, List<FormQuestion> standardQuestions, List<FormQuestion> customQuestions, List<FormQuestionAnswer> answers, DatabaseConnection dc = null)
        //{
        //    var relatedRegistrations = new List<Registration> { registration };
        //    relatedRegistrations.AddRange(allRegistrations
        //        .Where(x =>
        //            x.FirstRegistrationId.HasValue &&
        //            x.FirstRegistrationId.Value == registration.ID.Value)
        //        .ToList()
        //    );

        //    var lastRegistration = relatedRegistrations.OrderBy(x => x.ID.Value).Last();

        //    var row = new List<string>();

        //    row.Add(account.ConvertToAccountTimeZone(registration.CreateDate.Value).ToShortDateTimeString());
        //    if (reportType == RegistrationReportType.Detailed)
        //        row.Add(account.ConvertToAccountTimeZone(lastRegistration.CreateDate.Value).ToShortDateTimeString());

        //    row.Add(registration.RegistrantFirstName);
        //    row.Add(registration.RegistrantLastName);
        //    row.Add(registration.RegistrantEmail);

        //    /*
        //    if (reportType == RegistrationReportType.Basic && false)
        //    {
        //        var reservedRas = RegistrantActivity.GetByActivityIds(activityIds, regIds, DateTime.UtcNow, dc: dc);
        //        // var reservedRas = ras.Where(x => x.NumberOfSeats.Value > 0).ToList();
        //        // var waitingListRas = ras.Where(x => x.NumberOfWaitingListSeats.Value > 0).ToList();
        //        // var applicationRas = ras.Where(x => x.NumberOfApplicationSeats.Value > 0).ToList();

        //        foreach (var type in assignmentTypes)
        //        {
        //            switch (type)
        //            {
        //                case RegistrationAssignmentType.Application:
        //                    // row.Add(applicationRas.OrderBy(x => x.PreferenceNumber.Value).Select(x => x.ActivityName).ToStringList(", "));
        //                    break;
        //                case RegistrationAssignmentType.Reserved:
        //                    row.Add(reservedRas.OrderBy(x => x.PreferenceNumber.Value).Select(x => x.ActivityName).ToStringList(", "));
        //                    break;
        //                case RegistrationAssignmentType.WaitingList:
        //                    // row.Add(waitingListRas.OrderBy(x => x.PreferenceNumber.Value).Select(x => x.ActivityName).ToStringList(", "));
        //                    break;
        //            }
        //        }
        //    }
        //    */

        //    if (reportType != RegistrationReportType.SingleActivity)
        //    {
        //        AddDaysAndActivitiesToCSVReport(row, relatedRegistrations, days, registrantActivities, allDayActivities);
        //    }

        //    if (reportType != RegistrationReportType.Basic)
        //    {
        //        AddTotalPaidToCSVReport(row, relatedRegistrations);
        //        AddQuestionsToCSVReport(row, standardQuestions, customQuestions, answers, relatedRegistrations);
        //    }

        //    return row;
        //}
        //private static void AddDaysAndActivitiesToCSVReport(List<string> row, List<Registration> registrations, List<DayOfWeekASL> days, List<RegistrantActivity> registrantActivities, List<Activity> allDayActivities, SystemMessages msgs = null, DatabaseConnection dc = null)
        //{
        //    var regIds = registrations.Select(registration => registration.RegistrantId.Value)
        //        .ToList();
        //    var ras = registrantActivities.Where(registrantActivity => regIds.Contains(registrantActivity.RegistrantId.Value))
        //        .ToList();
        //    var participationObjects = ras.Select(registrantActivity => 
        //        new
        //        {
        //            registrantActivity.ParticipationObjectId,
        //            registrantActivity.ParticipationObjectTypeId
        //        })
        //        .ToList();

        //    days.ForEach(day =>
        //    {
        //        var dayActivities = allDayActivities.Where(x => 
        //            participationObjects.Contains(x.ID.Value) && 
        //            x.GetDaysOfTheWeek(msgs, dc).Contains(day)
        //            );
        //        if (dayActivities.Any())
        //            row.Add(dayActivities.Select(x => x.Name).ToStringList(", "));
        //        else
        //            row.Add("");
        //    });
        //}

        //private static void AddTotalPaidToCSVReport(List<string> row, List<Registration> registrations)
        //{
        //    var totalPaid = (decimal)0;

        //    if (registrations.Where(x => x.DatePaid.HasValue).Any())
        //        totalPaid = registrations.Where(x => x.DatePaid.HasValue).Sum(x => x.AmountPaid.Value);

        //    row.Add(totalPaid.ToCurrency());
        //}

        //private static void AddQuestionsToCSVReport(List<string> row, List<FormQuestion> standardQuestions, List<FormQuestion> customQuestions, List<FormQuestionAnswer> answers, List<Registration> registrations)
        //{
        //    foreach (var question in standardQuestions)
        //    {
        //        switch (question.QuestionDataTypeId)
        //        {
        //            case QuestionDataType.RegistrantFirstName:
        //            case QuestionDataType.RegistrantLastName:
        //            case QuestionDataType.RegistrantEmail:
        //                break;
        //            default:
        //                var answer = answers
        //                    .Where(item =>
        //                        registrations
        //                            .Select(registration => registration.ID.Value)
        //                            .ToList()
        //                            .Contains(item.RegistrationId.Value) &&
        //                        item.FormQuestionQuestionDataTypeId == question.QuestionDataTypeId)
        //                    .OrderBy(x => x.RegistrationId)
        //                    .LastOrDefault();

        //                if (answer.IsNotEmpty())
        //                    row.Add(answer.Text);
        //                else
        //                    row.Add("");
        //                break;
        //        }
        //    }

        //    foreach (var question in customQuestions)
        //    {
        //        var answer = answers
        //            .Where(item =>
        //                registrations
        //                    .Select(registration => registration.ID.Value)
        //                    .ToList()
        //                    .Contains(item.RegistrationId.Value) &&
        //                item.FormQuestionText == question.Text)
        //            .OrderBy(x => x.RegistrationId)
        //            .LastOrDefault();

        //        if (answer.IsNotEmpty())
        //            row.Add(answer.Text);
        //        else
        //            row.Add("");
        //    }
        //}

        #endregion

        public bool IsRegisteredFor(int participationObjectId, ParticipationObjectType participationObjectType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return this.GetRegistrantActivities(msgs, dc)
                .Where(x => 
                    x.ParticipationObjectId.Value == participationObjectId && 
                    x.ParticipationObjectTypeId == participationObjectType)
                .Count() > 0;
        }
        public bool IsRegisteredFor(ParticipationObject participationObject, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return IsRegisteredFor(participationObject.ObjectID.Value, participationObject.ObjectType, msgs, dc);
        }

        public static int TotalCurrentRegistrationsCount(int accountId, DatabaseConnection dc = null)
        {
            var sql = $@"select count(id) 
                        from Registration r
                        where 
                        accountId = {accountId} and
                        completionStatusId in ({ValidCompletionStatus.ToArray().ToStringArray().ToStringList()}) and
                        (
                            select count(ra.id) 
                            from RegistrantActivity ra
                                Inner Join Activity a on ra.ParticipationObjectId = a.ID and ParticipationObjectTypeId = {(int)ParticipationObjectType.Activity}
                                Left Outer Join ActivityGroup ag on a.ActivityGroupid = ag.ID
                            where 
                                ra.RegistrationId = r.ID and
                                a.IsArchived = 0 and
                                a.IsDeleted = 0 and
                                (a.ActivityGroupId is null or ag.IsArchived = 0)
                        ) > 0";

            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.UseDistinct = false;

            var dt = dq.ExecuteSelect(sql);
            if (dt.Rows.Any())
                return dq.ExecuteSelect(sql).Rows[0][0].ToString().ToInteger();

            return 0;
        }

        public static List<Registration> LoadCurrentByAccountId(int AccountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;

            string sql = $@"
                    select 
	                    distinct *
                    from 
	                    Registration r
                    where
	                    (
		                    id in
		                    (
			                    select r2.ID
			                    from registration r2 inner join activitygroup ag on r2.ActivityGroupId = ag.ID
			                    where r2.AccountId = r.AccountId and
			                    ag.IsArchived = 0
		                    )  or
		                    id in
		                    (
			                    select r3.ID
			                    from 
				                    registration r3 inner join 
				                    RegistrantActivity ra on r3.ID = ra.RegistrationId inner join 
				                    Activity a on ra.ParticipationObjectId = a.id and ra.ParticipationObjectTypeId = {(int)ParticipationObjectType.Activity} left outer join
				                    ActivityGroup ag on ag.id = a.ActivityGroupId
			                    where 
				                    r3.AccountId = r.AccountId and
				                    (
					                    (a.ActivityGroupId is null and a.IsArchived = 0 and a.IsDeleted = 0) or
					                    (a.ActivityGroupId is not null and ag.IsArchived = 0)
				                    )
		                    )
	                    ) and
	                    r.AccountId = {AccountId}";

            return dq.ExecuteSelect(sql, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public SystemMessages DeleteWithRelatedRecords(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    // right now there is one new registrant record per registration
                    // so we can know that it's okay to delete the registrant
                    // TODO: Notice that this may be a dangerous assumption one day
                    // Registration activities are deleted with the registrant
                    amsgs.AddRange(this.GetRegistrant(amsgs, adc).DeleteWithRelatedRecords(adc));
                    amsgs.AddRange(this.DeleteSessionActions(adc));
                    amsgs.AddRange(this.DeleteAnswers(adc));
                    amsgs.AddRange(this.Delete(adc));
                    amsgs.AddRange(this.UpdateRelatedRegistrationsAfterDelete(adc));
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

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    // delete all related session actions
                    var actions = SessionAction.LoadByRegistrationId(this.ID.Value, dc: adc);
                    foreach (var action in actions)
                    {
                        amsgs.AddRange(action.Delete(adc));
                    }
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

        internal SystemMessages DeleteAnswers(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    // delete all answers associated to this registration
                    var answers = this.GetAnswers(adc);
                    foreach (var answer in answers)
                    {
                        amsgs.AddRange(answer.Delete(adc));
                    }
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

        internal SystemMessages UpdateRelatedRegistrationsAfterDelete(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            // this may be the first registration of other registrations :(
            // and we're about to delete this registration, so we have to
            // switch the first registration id to a new one, 
            // which kind of sucks because we need to know when their real first reg was
            // TODO: Figure out way to keep the date of their first registration (MAYBE)
            // it's the first registration if FirstRegistrationId is null
            if (!this.FirstRegistrationId.HasValue)
            {
                // get reg's where the first reg id is equal to the mergedfrom
                var laterRegs = Registration.LoadByFirstRegistrationId(this.ID.Value, dc: dc);

                // reset each one
                foreach (var laterReg in laterRegs)
                {
                    // reset the first registration id
                    laterReg.FirstRegistrationId = null;
                    laterReg.Save(dc: dc);

                    msgs.AddRange(laterReg.FindAndSetFirstRegistration(laterReg.RegistrantFirstName, laterReg.RegistrantLastName, laterReg.RegistrantEmail, dc: dc));
                }
            }

            return msgs;
        }

        public SystemMessages FinalizeRegistration(WebUser loggedInUser, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            this.CompletionStatusId = RegistrationCompletionStatus.Completed;
            this.LastModifiedDate = DateTime.UtcNow;
            this.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);
            msgs.AddRange(this.Save(dc));

            msgs.AddRange(this.FindAndSetFirstRegistration(this.RegistrantFirstName, this.RegistrantLastName, this.RegistrantEmail, dc));

            return msgs;
        }

        public bool IsPaid(SystemMessages msgs, DatabaseConnection dc = null)
        {
            var registrantActivities = this.GetRegistrantActivities(msgs, dc);

            if (registrantActivities.Any())
            {
                // this only works now because we're doing on registration at a time
                var registrantActivity = registrantActivities.First();

                return ActivityPayment.GetTotalPaidByParticipationObjectByRegistrant(
                    registrantActivity.ParticipationObjectId.Value,
                    registrantActivity.ParticipationObjectTypeId,
                    this.RegistrantId.Value,
                    msgs,
                    dc) > 0;
            }

            msgs.Add("Incomplete registration", MessageType.UserError);

            return false;
        }

        public ParticipationObject GetParticipationObject(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var registrantActivities = this.GetRegistrantActivities(msgs, dc);
            if (registrantActivities.Any())
            {
                var registrantActivity = registrantActivities.First();

                switch (registrantActivity.ParticipationObjectTypeId)
                {
                    case ParticipationObjectType.AccountActivityGroup:
                        return AccountActivityGroup.Load(registrantActivity.ParticipationObjectId.Value, msgs, dc)
                            .ToParticipationObject(msgs, dc);
                    case ParticipationObjectType.ActivityGroup:
                        return ActivityGroup.Load(registrantActivity.ParticipationObjectId.Value, msgs, dc)
                            .ToParticipationObject(msgs, dc);
                    case ParticipationObjectType.Activity:
                        return Activity.Load(registrantActivity.ParticipationObjectId.Value, msgs, dc)
                            .ToParticipationObject(msgs, dc);
                }
            }

            return null;
        }

        private static RegistrationAssignmentType ProcessRegistrationSeats(ParticipationObject participationObject, int numberOfSeatsRequested, RegistrantActivity ra, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var seatsAvailable = participationObject.SeatsRemaining;
            var waitingListSeatsAvailable = participationObject.WaitingListSeatsRemaining;
            var isPendingOrReserved = ra.IsNotEmpty() && (ra.AssignmentTypeId == RegistrationAssignmentType.Reserved || ra.AssignmentTypeId == RegistrationAssignmentType.ReservedPendingPayment);

            // if they're paying for a registration they already completed
            if (isPendingOrReserved)
                return RegistrationAssignmentType.Reserved;

            if (participationObject.RegistrationMode == RegistrationMode.Apply)
                return RegistrationAssignmentType.Applied;
            // if there is room
            else if (!seatsAvailable.HasValue || seatsAvailable >= numberOfSeatsRequested)
            {
                // if the person didn't pay
                if (!participationObject.IsFree && participationObject.GetAmountPaidByRegistrant(ra.RegistrantId.Value, msgs, dc) == 0.ToDecimal())
                    return RegistrationAssignmentType.ReservedPendingPayment;

                return RegistrationAssignmentType.Reserved;
            }
            // if there aren't any more seats available, but there's a waiting list
            else if (!waitingListSeatsAvailable.HasValue || waitingListSeatsAvailable >= numberOfSeatsRequested)
                return RegistrationAssignmentType.WaitingList;

            msgs.Add(
                $"Sorry, there are no more reserved seats or waiting list seats available for {participationObject.Name}",
                MessageType.UserError);

            return RegistrationAssignmentType.NotSet;
        }

        public static void ProcessRegistration(CheckoutObject checkoutObject, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            ProcessRegistration(
                checkoutObject.Registration,
                checkoutObject.ParticipationObject,
                checkoutObject.Registration.GetRegistrant(msgs, dc),
                checkoutObject.NumberOfSeats,
                checkoutObject.ParticipationObject.IsActivity() ? checkoutObject.ParticipationObject.GetActivityGroupId() : new int?(),
                checkoutObject.ParticipationObject.GetCurrentRegistrationPeriod(msgs, dc)?.ID,
                loggedInUser,
                msgs,
                dc);
        }

        public static void ProcessRegistration(Registration registration, ParticipationObject participationObject, Registrant registrant, int seatsRequested, int? activityGroupId, int? periodId, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            //var participationObject = checkoutObject.ParticipationObject;
            var ra = registration.GetRegistrantActivities(msgs, dc).FirstOrDefault();
            var assignmentType = Registration.ProcessRegistrationSeats(participationObject, seatsRequested, ra, msgs, dc);

            if (msgs.HasErrors())
                return;

            // add or update the registrant for the activity
            ra = registrant.AddOrUpdateParticipationObject(
                registrationId: registration.ID.Value,
                participationObjectId: participationObject.ObjectID.Value,
                participationObjectType: participationObject.ObjectType,
                assignmentType: assignmentType,
                registrationPeriodId: periodId,
                activityGroupId: activityGroupId,
                loggedInUserId: loggedInUser.ID.Value,
                msgs: msgs,
                dc: dc);
        }

        public string GetPaymentUrl
        {
            get
            {
                return $"{Utilities.BaseUrl(true)}/account/makepayment/{this.Uid}";
            }
        }

        #endregion
    }
}
