using BasicLibrary;
using BasicLibrary.DataAccess;
using Stripe;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace AfterSchoolLibrary.Models
{
    internal class SubscriptionSchedule : BasicObjectWithModified
    {
        #region "ClassGenerator"

        public DateTime StartDate { get { return (DateTime) MDV["StartDate"].Value; } set { MDV["StartDate"].Value = value; } }

        public decimal AmountInCents { get { return (decimal) MDV["AmountInCents"].Value; } set { MDV["AmountInCents"].Value = value; } }

        public DateTime? EndDate { get { return (DateTime?) MDV["EndDate"].Value; } set { MDV["EndDate"].Value = value; } }

        public DateTime? FirstRenewal { get { return (DateTime?) MDV["FirstRenewal"].Value; } set { MDV["FirstRenewal"].Value = value; } }

        public bool ProrateFirstCharge { get { return (bool) MDV["ProrateFirstCharge"].Value; } set { MDV["ProrateFirstCharge"].Value = value; } }

        public bool ProrateLastCharge { get { return (bool) MDV["ProrateLastCharge"].Value; } set { MDV["ProrateLastCharge"].Value = value; } }

        public DateTime? DateCancelled { get { return (DateTime?) MDV["DateCancelled"].Value; } set { MDV["DateCancelled"].Value = value; } }

        public bool? IsCancelled { get { return (bool?) MDV["IsCancelled"].Value; } set { MDV["IsCancelled"].Value = value; } }

        public BillingPeriodType BillingPeriodType { get { return (BillingPeriodType) MDV["BillingPeriodType"].Value; } set { MDV["BillingPeriodType"].Value = value; } }

        public string StripeCustomerId { get { return (string) MDV["StripeCustomerId"].Value; } set { MDV["StripeCustomerId"].Value = value; } }

        public string StripeSourceId { get { return (string) MDV["StripeSourceId"].Value; } set { MDV["StripeSourceId"].Value = value; } }

        public LastChargeStatus LastChargeStatus { get { return (LastChargeStatus) MDV["LastChargeStatus"].Value; } set { MDV["LastChargeStatus"].Value = value; } }

        public string StripeConnectId { get { return (string) MDV["StripeConnectId"].Value; } set { MDV["StripeConnectId"].Value = value; } }

        public decimal AmountToConnectAccountInCents { get { return (decimal) MDV["AmountToConnectAccountInCents"].Value; } set { MDV["AmountToConnectAccountInCents"].Value = value; } }

        public DateTime? LastDateToBill { get { return (DateTime?) MDV["LastDateToBill"].Value; } set { MDV["LastDateToBill"].Value = value; } }

        public string Description { get { return (string) MDV["Description"].Value; } set { MDV["Description"].Value = value; } }

        public DateTime? NextBillDate { get { return (DateTime?) MDV["NextBillDate"].Value; } set { MDV["NextBillDate"].Value = value; } }

        public bool LastChargeFailed { get { return (bool) MDV["LastChargeFailed"].Value; } set { MDV["LastChargeFailed"].Value = value; } }

        public string CancelledReason { get { return (string) MDV["CancelledReason"].Value; } set { MDV["CancelledReason"].Value = value; } }

        public string Metadata { get { return (string) MDV["Metadata"].Value; } set { MDV["Metadata"].Value = value; } }

        public bool? IsDeleted { get { return (bool?) MDV["IsDeleted"].Value; } set { MDV["IsDeleted"].Value = value; } }

        public Guid Uid { get { return (Guid) MDV["Uid"].Value; } set { MDV["Uid"].Value = value; } }

        public SubscriptionSchedule()
        {
            Config(ObjectMap, DatabaseQuery, typeof(SubscriptionSchedule).Name, "ID");
            LoadObjectValues(ObjectMap);
            ProrateFirstCharge = false;
            ProrateLastCharge = false;
            BillingPeriodType = BillingPeriodType.NotSet;
            LastChargeStatus = LastChargeStatus.NotSet;
            LastChargeFailed = false;
            IsCancelled = false;
            IsDeleted = false;
        }

        public static DatabaseParameters ObjectMap
        {
            get
            {
                var _dp = new DatabaseParameters();
                var className = typeof(SubscriptionSchedule).Name;
                _dp = get_BaseObjectMap(className);
                _dp.Add(className, "StartDate", System.Data.SqlDbType.DateTime);
                _dp.Add(className, "AmountInCents", System.Data.SqlDbType.Money);
                _dp.Add(className, "EndDate", System.Data.SqlDbType.DateTime);
                _dp.Add(className, "FirstRenewal", System.Data.SqlDbType.DateTime);
                _dp.Add(className, "ProrateFirstCharge", System.Data.SqlDbType.Bit);
                _dp.Add(className, "ProrateLastCharge", System.Data.SqlDbType.Bit);

                _dp.Add(className, "DateCancelled", System.Data.SqlDbType.DateTime);
                _dp.Add(className, "BillingPeriodType", System.Data.SqlDbType.Int);
                _dp.Add(className, "StripeCustomerId", SqlDbType.NVarChar, "50");
                _dp.Add(className, "StripeSourceId", SqlDbType.NVarChar, "50");
                _dp.Add(className, "LastChargeStatus", System.Data.SqlDbType.Int);
                _dp.Add(className, "StripeConnectId", SqlDbType.NVarChar, "50");
                _dp.Add(className, "AmountToConnectAccountInCents", System.Data.SqlDbType.Money);

                _dp.Add(className, "LastDateToBill", System.Data.SqlDbType.DateTime);
                _dp.Add(className, "Description", SqlDbType.NVarChar, "1000");
                _dp.Add(className, "NextBillDate", System.Data.SqlDbType.DateTime);
                _dp.Add(className, "LastChargeFailed", System.Data.SqlDbType.Bit);
                _dp.Add(className, "CancelledReason", SqlDbType.NVarChar, "1000");
                _dp.Add(className, "Metadata", SqlDbType.NVarChar, "1000");
                _dp.Add(className, "IsDeleted", System.Data.SqlDbType.Bit);
                _dp.Add(className, "IsCancelled", System.Data.SqlDbType.Bit);

                return _dp;
            }
        }

        protected static DatabaseQuery DatabaseQuery
        {
            get
            {
                var dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(SubscriptionSchedule).Name);

                return dq;
            }
        }

        public SystemMessages Save(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            if (this.StartDate.IsEmpty())
                msgs.Add("StartDate is required on SubscriptionSchedule object", MessageType.Error);
            if (this.StripeCustomerId.IsEmpty())
                msgs.Add("StripeCustomerId is required on SubscriptionSchedule object", MessageType.Error);
            if (this.StripeSourceId.IsEmpty())
                msgs.Add("StripeSourceId is required on SubscriptionSchedule object", MessageType.Error);
            if (this.StripeConnectId.IsEmpty())
                msgs.Add("StripeConnectId is required on SubscriptionSchedule object", MessageType.Error);
            if (this.Description.IsEmpty())
                msgs.Add("Description is required on SubscriptionSchedule object", MessageType.Error);

            msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, nameof(SubscriptionSchedule)));
            return msgs;
        }

        public static SubscriptionSchedule LoadObject(DataRow row)
        {
            var obj = new SubscriptionSchedule();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        public static SystemMessages Create(DatabaseConnection dc = null)
        {
            return _Create(DatabaseQuery, dc, ObjectMap);
        }

        public SystemMessages Delete(DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(SubscriptionSchedule).Name);
        }

        public static SystemMessages Alter(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            SystemMessages msgs = new SystemMessages();
            BasicLibrary.DataAccess.DatabaseParameters dps;

            dps = new BasicLibrary.DataAccess.DatabaseParameters();
            dps.Add(ObjectMap["StartDate"]);
            msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

            dps = new BasicLibrary.DataAccess.DatabaseParameters();
            dps.Add(ObjectMap["AmountInCents"]);
            msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

            dps = new BasicLibrary.DataAccess.DatabaseParameters();
            dps.Add(ObjectMap["EndDate"]);
            msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

            dps = new BasicLibrary.DataAccess.DatabaseParameters();
            dps.Add(ObjectMap["FirstRenewal"]);
            msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

            dps = new BasicLibrary.DataAccess.DatabaseParameters();
            dps.Add(ObjectMap["ProrateFirstCharge"]);
            msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

            dps = new BasicLibrary.DataAccess.DatabaseParameters();
            dps.Add(ObjectMap["ProrateLastCharge"]);
            msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

            dps = new BasicLibrary.DataAccess.DatabaseParameters();
            dps.Add(ObjectMap["DateCancelled"]);
            msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

            dps = new BasicLibrary.DataAccess.DatabaseParameters();
            dps.Add(ObjectMap["BillingPeriodType"]);
            msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

            dps = new BasicLibrary.DataAccess.DatabaseParameters();
            dps.Add(ObjectMap["StripeCustomerId"]);
            msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

            dps = new BasicLibrary.DataAccess.DatabaseParameters();
            dps.Add(ObjectMap["StripeSourceId"]);
            msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

            dps = new BasicLibrary.DataAccess.DatabaseParameters();
            dps.Add(ObjectMap["LastChargeStatus"]);
            msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

            dps = new BasicLibrary.DataAccess.DatabaseParameters();
            dps.Add(ObjectMap["StripeConnectId"]);
            msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

            dps = new BasicLibrary.DataAccess.DatabaseParameters();
            dps.Add(ObjectMap["AmountToConnectAccountInCents"]);
            msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

            dps = new BasicLibrary.DataAccess.DatabaseParameters();
            dps.Add(ObjectMap["LastDateToBill"]);
            msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

            dps = new BasicLibrary.DataAccess.DatabaseParameters();
            dps.Add(ObjectMap["Description"]);
            msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

            dps = new BasicLibrary.DataAccess.DatabaseParameters();
            dps.Add(ObjectMap["NextBillDate"]);
            msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

            dps = new BasicLibrary.DataAccess.DatabaseParameters();
            dps.Add(ObjectMap["LastChargeFailed"]);
            msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

            dps = new BasicLibrary.DataAccess.DatabaseParameters();
            dps.Add(ObjectMap["CancelledReason"]);
            msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

            dps = new BasicLibrary.DataAccess.DatabaseParameters();
            dps.Add(ObjectMap["Metadata"]);
            msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

            dps = new BasicLibrary.DataAccess.DatabaseParameters();
            dps.Add(ObjectMap["IsDeleted"]);
            msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

            dps = new BasicLibrary.DataAccess.DatabaseParameters();
            dps.Add(ObjectMap["IsCancelled"]);
            msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

            return msgs;
        }

        public static SubscriptionSchedule Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        #endregion
    }
}
