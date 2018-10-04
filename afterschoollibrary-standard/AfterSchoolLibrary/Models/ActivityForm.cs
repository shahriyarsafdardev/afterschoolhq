using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Data;
using System.Collections;

namespace AfterSchoolLibrary.Models
{
	/// <summary>
    /// The relationship between an Activity and a Form. An activity can have multiple forms assigned, but only one can be active at a time. Would love to deprecate this in the future and store the form id directly on the activity class.
    /// </summary>
	
    public class ActivityForm : BasicLookUpObject
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required.
		/// </summary>
		public int? FormId { get { return (int?)MDV["FormId"].Value; } set { MDV["FormId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int? ActivityId { get { return (int?)MDV["ActivityId"].Value; } set { MDV["ActivityId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int? AccountId { get { return (int?)MDV["AccountId"].Value; } set { MDV["AccountId"].Value = value; } }
		public bool IsActive { get { return (bool)MDV["IsActive"].Value; } set { MDV["IsActive"].Value = value; } }
		public bool IsPublished { get { return (bool)MDV["IsPublished"].Value; } set { MDV["IsPublished"].Value = value; } }
		public Guid Uid { get { return (Guid)MDV["Uid"].Value; } set { MDV["Uid"].Value = value; } }
		public string ActivityName { get { return (string)MDV["ActivityName"].Value; } }
		public string ActivityDetails { get { return (string)MDV["ActivityDetails"].Value; } }
		public string ActivityLocationDetails { get { return (string)MDV["ActivityLocationDetails"].Value; } }
		public string ActivityScheduleDetails { get { return (string)MDV["ActivityScheduleDetails"].Value; } }
		public string ActivityRegistrationTimeZone { get { return (string)MDV["ActivityRegistrationTimeZone"].Value; } }
		public int? ActivitySeatsAvailable { get { return (int?)MDV["ActivitySeatsAvailable"].Value; } }
		public string ActivityContactName { get { return (string)MDV["ActivityContactName"].Value; } }
		public string ActivityContactPhone { get { return (string)MDV["ActivityContactPhone"].Value; } }
		public string ActivityContactEmail { get { return (string)MDV["ActivityContactEmail"].Value; } }
		public int? ActivityWaitingListSeats { get { return (int?)MDV["ActivityWaitingListSeats"].Value; } }
		public string ActivityAddress1 { get { return (string)MDV["ActivityAddress1"].Value; } }
		public string ActivityAddress2 { get { return (string)MDV["ActivityAddress2"].Value; } }
		public string ActivityCity { get { return (string)MDV["ActivityCity"].Value; } }
		public string ActivityState { get { return (string)MDV["ActivityState"].Value; } }
		public string ActivityPostalCode { get { return (string)MDV["ActivityPostalCode"].Value; } }
		public decimal? ActivityCostPerRegistrant { get { return (decimal?)MDV["ActivityCostPerRegistrant"].Value; } }
		public string ActivityCostDescription { get { return (string)MDV["ActivityCostDescription"].Value; } }
		public string FormName { get { return (string)MDV["FormName"].Value; } }
		public string FormDescription { get { return (string)MDV["FormDescription"].Value; } }
		public bool FormIsActive { get { return (bool)MDV["FormIsActive"].Value; } }
		public Guid FormUid { get { return (Guid)MDV["FormUid"].Value; } }
		public string AccountName { get { return (string)MDV["AccountName"].Value; } }
		public Guid AccountUid { get { return (Guid)MDV["AccountUid"].Value; } }
		

        public ActivityForm()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(ActivityForm).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.IsActive = false;
			this.IsPublished = false;
			this.Uid = Guid.NewGuid();
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(ActivityForm).Name;
                _dp = BasicLookUpObject.get_BaseObjectMap(className);
				_dp.Add(className, "FormId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ActivityId", System.Data.SqlDbType.Int);
				_dp.Add(className, "AccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "IsActive", System.Data.SqlDbType.Bit);
				_dp.Add(className, "IsPublished", System.Data.SqlDbType.Bit);
				_dp.Add(className, "Uid", System.Data.SqlDbType.UniqueIdentifier);
				_dp.Add(className, "ActivityName", typeof(Activity).Name, "Name", System.Data.SqlDbType.NVarChar, "Activity");
				_dp.Add(className, "ActivityDetails", typeof(Activity).Name, "Details", System.Data.SqlDbType.NVarChar, "Activity");
				_dp.Add(className, "ActivityLocationDetails", typeof(Activity).Name, "LocationDetails", System.Data.SqlDbType.NVarChar, "Activity");
				_dp.Add(className, "ActivityScheduleDetails", typeof(Activity).Name, "ScheduleDetails", System.Data.SqlDbType.NVarChar, "Activity");
				_dp.Add(className, "ActivityRegistrationTimeZone", typeof(Activity).Name, "RegistrationTimeZone", System.Data.SqlDbType.NVarChar, "Activity");
				_dp.Add(className, "ActivitySeatsAvailable", typeof(Activity).Name, "SeatsAvailable", System.Data.SqlDbType.Int, "Activity");
				_dp.Add(className, "ActivityContactName", typeof(Activity).Name, "ContactName", System.Data.SqlDbType.NVarChar, "Activity");
				_dp.Add(className, "ActivityContactPhone", typeof(Activity).Name, "ContactPhone", System.Data.SqlDbType.NVarChar, "Activity");
				_dp.Add(className, "ActivityContactEmail", typeof(Activity).Name, "ContactEmail", System.Data.SqlDbType.NVarChar, "Activity");
				_dp.Add(className, "ActivityWaitingListSeats", typeof(Activity).Name, "WaitingListSeats", System.Data.SqlDbType.Int, "Activity");
				_dp.Add(className, "ActivityAddress1", typeof(Activity).Name, "Address1", System.Data.SqlDbType.NVarChar, "Activity");
				_dp.Add(className, "ActivityAddress2", typeof(Activity).Name, "Address2", System.Data.SqlDbType.NVarChar, "Activity");
				_dp.Add(className, "ActivityCity", typeof(Activity).Name, "City", System.Data.SqlDbType.NVarChar, "Activity");
				_dp.Add(className, "ActivityState", typeof(Activity).Name, "State", System.Data.SqlDbType.NVarChar, "Activity");
				_dp.Add(className, "ActivityPostalCode", typeof(Activity).Name, "PostalCode", System.Data.SqlDbType.NVarChar, "Activity");
				_dp.Add(className, "ActivityCostPerRegistrant", typeof(Activity).Name, "CostPerRegistrant", System.Data.SqlDbType.Money, "Activity");
				_dp.Add(className, "ActivityCostDescription", typeof(Activity).Name, "CostDescription", System.Data.SqlDbType.NVarChar, "Activity");
				_dp.Add(className, "FormName", typeof(Form).Name, "Name", System.Data.SqlDbType.NVarChar, "Form");
				_dp.Add(className, "FormDescription", typeof(Form).Name, "Description", System.Data.SqlDbType.NVarChar, "Form");
				_dp.Add(className, "FormIsActive", typeof(Form).Name, "IsActive", System.Data.SqlDbType.Bit, "Form");
				_dp.Add(className, "FormUid", typeof(Form).Name, "Uid", System.Data.SqlDbType.UniqueIdentifier, "Form");
				_dp.Add(className, "AccountName", typeof(Account).Name, "Name", System.Data.SqlDbType.NVarChar, "Account");
				_dp.Add(className, "AccountUid", typeof(Account).Name, "Uid", System.Data.SqlDbType.UniqueIdentifier, "Account");
				
                return _dp;
            }
        }

        protected static ActivityForm LoadObject(System.Data.DataRow row)
        {
            ActivityForm obj = new ActivityForm();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(ActivityForm).Name);
				dq.Joins.Add(Form.ObjectMap["ID"], ActivityForm.ObjectMap["FormId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "Form");
				dq.Joins.Add(Activity.ObjectMap["ID"], ActivityForm.ObjectMap["ActivityId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "Activity");
				dq.Joins.Add(Account.ObjectMap["ID"], ActivityForm.ObjectMap["AccountId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "Account");
				
                return dq;
            }
        }

        public static ActivityForm Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<ActivityForm> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return ActivityForm.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return ActivityForm.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.FormId.IsEmpty())
				msgs.Add("FormId is required on ActivityForm object", MessageType.Error);
			if (this.ActivityId.IsEmpty())
				msgs.Add("ActivityId is required on ActivityForm object", MessageType.Error);
			if (this.AccountId.IsEmpty())
				msgs.Add("AccountId is required on ActivityForm object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(ActivityForm).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(ActivityForm).Name);
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
			dps.Add(ObjectMap["FormId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ActivityId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AccountId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsActive"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsPublished"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Uid"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public ActivityForm Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return ActivityForm.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<ActivityForm> LoadByFormId(int FormId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["FormId"], FormId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<ActivityForm> LoadByActivityId(int ActivityId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static ActivityForm LoadByActivityIdByFormId(int ActivityId, int FormId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
			dq.Criterias.Add(ObjectMap["FormId"], FormId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static ActivityForm LoadByActivityIdByIsActive(int ActivityId, bool IsActive, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
			dq.Criterias.Add(ObjectMap["IsActive"], IsActive);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static List<ActivityForm> LoadByAccountId(int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static ActivityForm LoadByUid(Guid Uid, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static ActivityForm LoadByUidByIsPublished(Guid Uid, bool IsPublished, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			dq.Criterias.Add(ObjectMap["IsPublished"], IsPublished);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		

        #endregion

		#region "UserDefined"

        public string ActivityRegistrationTimeZoneAbbr
        {
            get
            {
                if (ActivityRegistrationTimeZone.IsNotBlank())
                {
                    return ActivityRegistrationTimeZone.Split('-')[1].Trim();
                }
                else
                {
                    return "";
                }
            }
        }

        public Form ToForm(bool loadFromDb = false, DatabaseConnection dc = null)
        {
            if (loadFromDb)
            {
                return Form.Load(this.FormId.Value, dc: dc);
            }
            else
            {
                return new Form()
                {
                    ID = this.FormId,
                    Name = this.FormName,
                    Description = this.FormDescription,
                    IsActive = this.FormIsActive,
                    Uid = this.FormUid
                };
            }
        }

        public Activity ToActivity(bool loadFromDb = false, DatabaseConnection dc = null)
        {
            if (loadFromDb)
            {
                return Activity.Load(this.ActivityId.Value, dc: dc);
            }
            else
            {
                return new Activity()
                {
                    ID = this.ActivityId,
                    Details = this.ActivityDetails,
                    LocationDetails = this.ActivityLocationDetails,
                    Name = this.ActivityName,
                    // RegistrationTimeZone = this.ActivityRegistrationTimeZone,
                    SeatsAvailable = this.ActivitySeatsAvailable,
                    ContactName = this.ActivityContactName,
                    ContactEmail = this.ActivityContactEmail,
                    ContactPhone = this.ActivityContactPhone,
                    Address1 = this.ActivityAddress1,
                    Address2 = this.ActivityAddress2,
                    City = this.ActivityCity,
                    State = this.ActivityState,
                    PostalCode = this.ActivityPostalCode,
                    //AcceptCreditCard = this.ActivityAcceptCreditCard,
                    //AcceptPayPal = this.ActivityAcceptPayPal,
                    CostPerRegistrant = this.ActivityCostPerRegistrant,
                    CostDescription = this.ActivityCostDescription,
                    AccountId = this.AccountId,
                    WaitingListSeats = this.ActivityWaitingListSeats
                };
            }
        }

        //public string GetUrl()
        //{
        //    return string.Format("{0}/reg/new/{1}", Utilities.BaseUrl, this.Uid.ToString());
        //}

        //public string GetPreviewUrl()
        //{
        //    return string.Format("{0}/reg/new/{1}?preview={2}", Utilities.BaseUrl, this.Uid.ToString(), this.AccountUid.ToString());
        //}

        public List<FormQuestion> GetAllQuestions(DatabaseConnection dc = null)
        {
            return this.ToForm().GetAllQuestions(dc: dc);
        }

        public List<FormQuestion> GetActiveQuestions(DatabaseConnection dc = null)
        {
            return this.ToForm().GetActiveQuestions(dc: dc);
        }

        public List<FormQuestionGroup> GetQuestionGroups(DatabaseConnection dc = null)
        {
            return this.ToForm().GetQuestionGroups(dc: dc);
        }

        public int GetSeatsRemaining(DatabaseConnection dc = null)
        {            
            if (ActivitySeatsAvailable.HasValue)
            {
                return ActivitySeatsAvailable.Value - this.GetRegistrantCountByAssignmentType(RegistrationAssignmentType.Reserved, dc: dc);
            }
            else
            {
                return 0;
            }
        }
        public int GetWaitingListSeatsRemaining(DatabaseConnection dc = null)
        {
            if (ActivityWaitingListSeats.HasValue)
            {
                return ActivityWaitingListSeats.Value - this.GetRegistrantCountByAssignmentType(RegistrationAssignmentType.WaitingList, dc: dc);
            }
            else
            {
                return 0;
            }
        }

        public int GetRegistrantCount(DatabaseConnection dc = null)
        {
            return RegistrantActivity.GetSeatCountByParticipationObject(
                this.ActivityId.Value, 
                ParticipationObjectType.Activity, 
                dc: dc);
        }

        public int GetRegistrantCountByAssignmentType(RegistrationAssignmentType rat, DatabaseConnection dc = null)
        {
            return RegistrantActivity.GetSeatCountByAssignmentType(assignmentType: rat, participationObjectId: this.ActivityId, participationObjectType: ParticipationObjectType.Activity, dc: dc);
        }

        public SystemMessages SetIsPublished(bool isPublished, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            this.IsPublished = isPublished;
            this.LastModifiedBy = loggedInUserId;
            this.LastModifiedDate = DateTime.UtcNow;
            msgs.AddRange(this.Save(dc: dc));
            
            //var activity = this.ToActivity(true, dc: dc);
            //if (!activity.IsChildActivity)
            //{
            //    var childActivities = activity.GetChildActivities(dc: dc);
            //    foreach (var a in childActivities)
            //    {
            //        var af = a.GetActiveActivityForm(dc: dc);
            //        if (af.IsNotEmpty())
            //        {
            //            msgs.AddRange(af.SetIsPublished(isPublished, loggedInUserId, dc: dc));
            //        }
            //    }
            //}

            return msgs;
        }

        //public SystemMessages ProcessStripePayment(RegistrantActivity ra, string ccname, string ccnumber, string cccvc, string ccexpmonth, string ccexpyear, DatabaseConnection dc = null)
        //{
        //    var msgs = new SystemMessages();
        //    var account = Account.Load(this.AccountId.Value, dc: dc);
        //    if(account.IsNotEmpty())
        //    {
        //        Stripe.StripeChargeCreateOptions options = new Stripe.StripeChargeCreateOptions();
        //        options.Amount = (this.ActivityCostPerRegistrant.Value * ra.TotalSeats * 100).ToInteger();
        //        options.Currency = "usd";
        //        options.Description = this.ActivityName;

        //        options.Destination = account.StripeUserId;

        //        options.Source = new Stripe.StripeSourceOptions();
        //        options.Source.Name = ccname;
        //        options.Source.Number = ccnumber;
        //        options.Source.Cvc = cccvc;
        //        options.Source.ExpirationMonth = ccexpmonth;
        //        options.Source.ExpirationYear = ccexpyear;
        //        options.Source.StatementDescriptor = string.Format("{0} - {1}", this.ActivityName, this.AccountName);

        //        try
        //        {
        //            var service = new Stripe.StripeChargeService(AfterSchoolLibrary.StripeHelper.SecretKey);
        //            var charge = service.Create(options);

        //            ra.StripeChargeId = charge.Id;
        //            ra.DatePaid = DateTime.UtcNow;
        //            ra.AmountPaid = this.ActivityCostPerRegistrant;
        //            ra.PaymentTypeId = PaymentType.CreditCard;
        //            msgs.AddRange(ra.Save(dc: dc));

        //        }
        //        catch (Stripe.StripeException ex)
        //        {
        //            msgs.Add(ex.Message, MessageType.Error, ex);
        //        }
        //        catch (Exception ex)
        //        {
        //            msgs.Add("Error charging credit card", MessageType.Error, ex);
        //        }

        //    }
        //    return msgs;
        //}

        #endregion
    }
}
