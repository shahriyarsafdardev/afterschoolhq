using System;
using System.Collections.Generic;
using System.Text;
using BasicLibrary;
using System.Data;
using System.Collections;
using BasicLibrary.DataAccess;
using System.Linq;
using Stripe;

namespace AfterSchoolLibrary.Models
{
    public class SubscriptionPaymentHistory : BasicObjectWithModified
    {
        #region "ClassGenerator"

        public Guid Uid { get { return (Guid) MDV["Uid"].Value; } set { MDV["Uid"].Value = value; } }

        public int SubscriptionScheduleId { get { return (int) MDV["SubscriptionScheduleId"].Value; } set { MDV["SubscriptionScheduleId"].Value = value; } }

        public decimal AmountInCents { get { return (decimal) MDV["AmountInCents"].Value; } set { MDV["AmountInCents"].Value = value; } }

        public DateTime DateCharged { get { return (DateTime) MDV["DateCharged"].Value; } set { MDV["DateCharged"].Value = value; } }

        public string StripeChargeId { get { return (string) MDV["StripeChargeId"].Value; } set { MDV["StripeChargeId"].Value = value; } }

        public bool ProratedCharge { get { return (bool) MDV["ProratedCharge"].Value; } set { MDV["ProratedCharge"].Value = value; } }

        public LastChargeStatus Status { get { return (LastChargeStatus) MDV["Status"].Value; } set { MDV["Status"].Value = value; } }

        public string StripeRawResponse { get { return (string) MDV["StripeRawResponse"].Value; } set { MDV["StripeRawResponse"].Value = value; } }

        public decimal AmountToConnectAccountInCents { get { return (decimal) MDV["AmountToConnectAccountInCents"].Value; } set { MDV["AmountToConnectAccountInCents"].Value = value; } }

        public decimal AmountToStripeInCents { get { return (decimal) MDV["AmountToStripeInCents"].Value; } set { MDV["AmountToStripeInCents"].Value = value; } }

        public int RetryNumber { get { return (int) MDV["RetryNumber"].Value; } set { MDV["RetryNumber"].Value = value; } }


        public SubscriptionPaymentHistory()
        {
            Config(ObjectMap, DatabaseQuery, typeof(SubscriptionPaymentHistory).Name, "ID");
            LoadObjectValues(ObjectMap);
            Uid = Guid.NewGuid();
            ProratedCharge = false;
            Status = LastChargeStatus.NotSet;
            RetryNumber = 0;
        }

        public static DatabaseParameters ObjectMap
        {
            get
            {
                var _dp = new DatabaseParameters();
                var className = typeof(SubscriptionPaymentHistory).Name;
                _dp = get_BaseObjectMap(className);
                _dp.Add(className, "SubscriptionScheduleId", System.Data.SqlDbType.Int);
                _dp.Add(className, "AmountInCents", System.Data.SqlDbType.Money);
                _dp.Add(className, "DateCharged", System.Data.SqlDbType.DateTime);
                _dp.Add(className, "StripeChargeId", SqlDbType.NVarChar, "50");
                _dp.Add(className, "ProratedCharge", System.Data.SqlDbType.Bit);
                _dp.Add(className, "Status", System.Data.SqlDbType.Int);
                _dp.Add(className, "StripeRawResponse", SqlDbType.NVarChar, "1000");
                _dp.Add(className, "AmountToConnectAccountInCents", System.Data.SqlDbType.Money);
                _dp.Add(className, "AmountToStripeInCents", System.Data.SqlDbType.Money);
                _dp.Add(className, "RetryNumber", System.Data.SqlDbType.Int);
                _dp.Add(className, "Uid", SqlDbType.UniqueIdentifier);

                return _dp;
            }
        }

        protected static DatabaseQuery DatabaseQuery
        {
            get
            {
                var dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(SubscriptionPaymentHistory).Name);

                return dq;
            }
        }

        public static SubscriptionPaymentHistory LoadObject(DataRow row)
        {
            var obj = new SubscriptionPaymentHistory();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        public static SystemMessages Create(DatabaseConnection dc = null)
        {
            return _Create(DatabaseQuery, dc, ObjectMap);
        }

        public SystemMessages Delete(DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(SubscriptionPaymentHistory).Name);
        }

        public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            if (this.SubscriptionScheduleId.IsEmpty())
                msgs.Add("SubscriptionScheduleId is required on SubscriptionPaymentHostory object", MessageType.Error);
            if (this.DateCharged.IsEmpty())
                msgs.Add("DateCharged is required on SubscriptionPaymentHostory object", MessageType.Error);
            if (this.StripeChargeId.IsEmpty())
                msgs.Add("StripeChargeId is required on SubscriptionPaymentHostory object", MessageType.Error);
            if (this.StripeRawResponse.IsEmpty())
                msgs.Add("StripeRawResponse is required on SubscriptionPaymentHostory object", MessageType.Error);

            if (msgs.HasNoErrors())
                msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(SubscriptionPaymentHistory).Name));

            return msgs;
        }

        public static SystemMessages Alter(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            SystemMessages msgs = new SystemMessages();
            BasicLibrary.DataAccess.DatabaseParameters dps;

            dps = new BasicLibrary.DataAccess.DatabaseParameters();
            dps.Add(ObjectMap["SubscriptionScheduleId"]);
            msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

            dps = new BasicLibrary.DataAccess.DatabaseParameters();
            dps.Add(ObjectMap["DateCharged"]);
            msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

            dps = new BasicLibrary.DataAccess.DatabaseParameters();
            dps.Add(ObjectMap["StripeChargeId"]);
            msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

            dps = new BasicLibrary.DataAccess.DatabaseParameters();
            dps.Add(ObjectMap["AmountInCents"]);
            msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

            dps = new BasicLibrary.DataAccess.DatabaseParameters();
            dps.Add(ObjectMap["ProratedCharge"]);
            msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

            dps = new BasicLibrary.DataAccess.DatabaseParameters();
            dps.Add(ObjectMap["Status"]);
            msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

            dps = new BasicLibrary.DataAccess.DatabaseParameters();
            dps.Add(ObjectMap["StripeRawResponse"]);
            msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

            dps = new BasicLibrary.DataAccess.DatabaseParameters();
            dps.Add(ObjectMap["AmountToConnectAccountInCents"]);
            msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

            dps = new BasicLibrary.DataAccess.DatabaseParameters();
            dps.Add(ObjectMap["AmountToStripeInCents"]);
            msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

            dps = new BasicLibrary.DataAccess.DatabaseParameters();
            dps.Add(ObjectMap["RetryNumber"]);
            msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

            dps = new BasicLibrary.DataAccess.DatabaseParameters();
            dps.Add(ObjectMap["Uid"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            return msgs;
        }

        public static SubscriptionPaymentHistory Load(int id, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static SubscriptionPaymentHistory LoadBySubscriptionScheduleId(int SubscriptionScheduleId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["SubscriptionScheduleId"], SubscriptionScheduleId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<SubscriptionPaymentHistory> LoadByStatus(LastChargeStatus status, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["Status"], status);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }
        #endregion
    }
}