using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchoolLibrary.Models
{
    /// <summary>
    ///     The actual registration for a registrant. A registrant can have multiple registrations.
    /// </summary>
    public class Registration : BasicObjectWithModified
    {
        #region "ClassGenerator"

        /// <summary>
        ///     Required.
        /// </summary>
        public int? RegistrantId
        {
            get => (int?)MDV["RegistrantId"].Value;
            set => MDV["RegistrantId"].Value = value;
        }

        public int? RegistrationPeriodId
        {
            get => (int?)MDV["RegistrationPeriodId"].Value;
            set => MDV["RegistrationPeriodId"].Value = value;
        }

        public int? AccountId
        {
            get => (int?)MDV["AccountId"].Value;
            set => MDV["AccountId"].Value = value;
        }

        /// <summary>
        ///     Required.
        /// </summary>
        public Guid Uid
        {
            get => (Guid)MDV["Uid"].Value;
            set => MDV["Uid"].Value = value;
        }

        public DateTime? DatePaid
        {
            get => (DateTime?)MDV["DatePaid"].Value;
            set => MDV["DatePaid"].Value = value;
        }

        public string StripeChargeId
        {
            get => (string)MDV["StripeChargeId"].Value;
            set => MDV["StripeChargeId"].Value = value;
        }

        public string PayPalTransactionId
        {
            get => (string)MDV["PayPalTransactionId"].Value;
            set => MDV["PayPalTransactionId"].Value = value;
        }

        public decimal? AmountPaid
        {
            get => (decimal?)MDV["AmountPaid"].Value;
            set => MDV["AmountPaid"].Value = value;
        }

        public decimal? ConvenienceFeePaid
        {
            get => (decimal?)MDV["ConvenienceFeePaid"].Value;
            set => MDV["ConvenienceFeePaid"].Value = value;
        }

        public decimal? PaymentTypeFeePaid
        {
            get => (decimal?)MDV["PaymentTypeFeePaid"].Value;
            set => MDV["PaymentTypeFeePaid"].Value = value;
        }

        public PaymentType PaymentTypeId
        {
            get => (PaymentType)MDV["PaymentTypeId"].Value;
            set => MDV["PaymentTypeId"].Value = value;
        }

        public DateTime? LastUpdated
        {
            get => (DateTime?)MDV["LastUpdated"].Value;
            set => MDV["LastUpdated"].Value = value;
        }

        public int? ActivityGroupId
        {
            get => (int?)MDV["ActivityGroupId"].Value;
            set => MDV["ActivityGroupId"].Value = value;
        }

        public int? AccountActivityGroupId
        {
            get => (int?)MDV["AccountActivityGroupId"].Value;
            set => MDV["AccountActivityGroupId"].Value = value;
        }

        public int? FirstRegistrationId
        {
            get => (int?)MDV["FirstRegistrationId"].Value;
            set => MDV["FirstRegistrationId"].Value = value;
        }

        public string ESignature
        {
            get => (string)MDV["ESignature"].Value;
            set => MDV["ESignature"].Value = value;
        }

        public string ESignatureUserAgent
        {
            get => (string)MDV["ESignatureUserAgent"].Value;
            set => MDV["ESignatureUserAgent"].Value = value;
        }

        public string ESignatureIPAddress
        {
            get => (string)MDV["ESignatureIPAddress"].Value;
            set => MDV["ESignatureIPAddress"].Value = value;
        }

        public RegistrationCompletionStatus CompletionStatusId
        {
            get => (RegistrationCompletionStatus)MDV["CompletionStatusId"].Value;
            set => MDV["CompletionStatusId"].Value = value;
        }

        public int? PurchaseId
        {
            get => (int?)MDV["PurchaseId"].Value;
            set => MDV["PurchaseId"].Value = value;
        }

        public string StripeSourceId
        {
            get => (string)MDV["StripeSourceId"].Value;
            set => MDV["StripeSourceId"].Value = value;
        }

        public decimal? CostPerClass
        {
            get => (decimal?)MDV["CostPerClass"].Value;
            set => MDV["CostPerClass"].Value = value;
        }

        /// <summary>
        ///     which account is paying the payment type (stripe or ach) fee
        /// </summary>
        public int? PaymentTypeFeePayerAccountId
        {
            get => (int?)MDV["PaymentTypeFeePayerAccountId"].Value;
            set => MDV["PaymentTypeFeePayerAccountId"].Value = value;
        }

        /// <summary>
        ///     which account is paying the convenience fee
        /// </summary>
        public int? ConvenienceFeePayerAccountId
        {
            get => (int?)MDV["ConvenienceFeePayerAccountId"].Value;
            set => MDV["ConvenienceFeePayerAccountId"].Value = value;
        }

        public int? CouponId
        {
            get => (int?)MDV["CouponId"].Value;
            set => MDV["CouponId"].Value = value;
        }

        public decimal? CouponValue
        {
            get => (decimal?)MDV["CouponValue"].Value;
            set => MDV["CouponValue"].Value = value;
        }

        public bool? IsDeleted
        {
            get => (bool?)MDV["IsDeleted"].Value;
            set => MDV["IsDeleted"].Value = value;
        }

        public string RegistrantEmail => (string)MDV["RegistrantEmail"].Value;
        public string RegistrantFirstName => (string)MDV["RegistrantFirstName"].Value;
        public string RegistrantLastName => (string)MDV["RegistrantLastName"].Value;
        public Guid RegistrantUid => (Guid)MDV["RegistrantUid"].Value;
        public string RegistrantPhone => (string)MDV["RegistrantPhone"].Value;
        public string RegistrantCompany => (string)MDV["RegistrantCompany"].Value;
        public RegistrantType RegistrantRegistrantTypeId => (RegistrantType)MDV["RegistrantRegistrantTypeId"].Value;


        public Registration()
        {
            Config(ObjectMap, DatabaseQuery, typeof(Registration).Name, "ID");
            LoadObjectValues(ObjectMap);
            Uid = Guid.NewGuid();
            PaymentTypeId = PaymentType.NotSet;
            CompletionStatusId = RegistrationCompletionStatus.NotSet;
        }

        public static DatabaseParameters ObjectMap
        {
            get
            {
                var _dp = new DatabaseParameters();
                var className = typeof(Registration).Name;
                _dp = get_BaseObjectMap(className);
                _dp.Add(className, "RegistrantId", SqlDbType.Int);
                _dp.Add(className, "RegistrationPeriodId", SqlDbType.Int);
                _dp.Add(className, "AccountId", SqlDbType.Int);
                _dp.Add(className, "Uid", SqlDbType.UniqueIdentifier);
                _dp.Add(className, "DatePaid", SqlDbType.DateTime);
                _dp.Add(className, "StripeChargeId", SqlDbType.NVarChar, "100");
                _dp.Add(className, "PayPalTransactionId", SqlDbType.NVarChar, "50");
                _dp.Add(className, "AmountPaid", SqlDbType.Money);
                _dp.Add(className, "ConvenienceFeePaid", SqlDbType.Money);
                _dp.Add(className, "PaymentTypeFeePaid", SqlDbType.Money);
                _dp.Add(className, "PaymentTypeId", SqlDbType.Int);
                _dp.Add(className, "LastUpdated", SqlDbType.DateTime);
                _dp.Add(className, "ActivityGroupId", SqlDbType.Int);
                _dp.Add(className, "AccountActivityGroupId", SqlDbType.Int);
                _dp.Add(className, "FirstRegistrationId", SqlDbType.Int);
                _dp.Add(className, "ESignature", SqlDbType.NVarChar, "50");
                _dp.Add(className, "ESignatureUserAgent", SqlDbType.NVarChar, "400");
                _dp.Add(className, "ESignatureIPAddress", SqlDbType.NVarChar, "50");
                _dp.Add(className, "CompletionStatusId", SqlDbType.Int);
                _dp.Add(className, "PurchaseId", SqlDbType.Int);
                _dp.Add(className, "StripeSourceId", SqlDbType.NVarChar, "100");
                _dp.Add(className, "CostPerClass", SqlDbType.Money);
                _dp.Add(className, "PaymentTypeFeePayerAccountId", SqlDbType.Int);
                _dp.Add(className, "ConvenienceFeePayerAccountId", SqlDbType.Int);
                _dp.Add(className, "CouponId", SqlDbType.Int);
                _dp.Add(className, "CouponValue", SqlDbType.Decimal, "10,2");
                _dp.Add(className, "IsDeleted", SqlDbType.Bit);
                _dp.Add(className, "RegistrantEmail", typeof(Registrant).Name, "Email", SqlDbType.NVarChar,
                    "Registrant");
                _dp.Add(className, "RegistrantFirstName", typeof(Registrant).Name, "FirstName", SqlDbType.NVarChar,
                    "Registrant");
                _dp.Add(className, "RegistrantLastName", typeof(Registrant).Name, "LastName", SqlDbType.NVarChar,
                    "Registrant");
                _dp.Add(className, "RegistrantUid", typeof(Registrant).Name, "Uid", SqlDbType.UniqueIdentifier,
                    "Registrant");
                _dp.Add(className, "RegistrantPhone", typeof(Registrant).Name, "Phone", SqlDbType.NVarChar,
                    "Registrant");
                _dp.Add(className, "RegistrantCompany", typeof(Registrant).Name, "Company", SqlDbType.NVarChar,
                    "Registrant");
                _dp.Add(className, "RegistrantRegistrantTypeId", typeof(Registrant).Name, "RegistrantTypeId",
                    SqlDbType.Int, "Registrant");

                return _dp;
            }
        }

        public static Registration LoadObject(DataRow row)
        {
            var obj = new Registration();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static DatabaseQuery DatabaseQuery
        {
            get
            {
                var dq = get_DatabaseQuery(ObjectMap, typeof(Registration).Name);
                dq.Joins.Add(Registrant.ObjectMap["ID"], ObjectMap["RegistrantId"], DatabaseJoinTypeEnum.InnerJoin,
                    "Registrant");

                return dq;
            }
        }

        public static Registration Load(int id, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<Registration> Load(Hashtable criterias = null, int? start = null, int? length = null,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            return BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

        public SystemMessages Save(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            if (RegistrantId.IsEmpty())
                msgs.Add("RegistrantId is required on Registration object", MessageType.Error);
            if (Uid.IsEmpty())
                msgs.Add("Uid is required on Registration object", MessageType.Error);

            if (msgs.HasNoErrors())
                msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(Registration).Name));

            return msgs;
        }

        public SystemMessages Delete(DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(Registration).Name);
        }

        public static SystemMessages Create(DatabaseConnection dc = null)
        {
            return _Create(DatabaseQuery, dc, ObjectMap);
        }

        public static SystemMessages Alter(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            DatabaseParameters dps;

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["RegistrantId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["RegistrationPeriodId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["AccountId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["Uid"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["DatePaid"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["StripeChargeId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["PayPalTransactionId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["AmountPaid"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["ConvenienceFeePaid"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["PaymentTypeFeePaid"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["PaymentTypeId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["LastUpdated"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["ActivityGroupId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["AccountActivityGroupId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["FirstRegistrationId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["ESignature"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["ESignatureUserAgent"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["ESignatureIPAddress"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["CompletionStatusId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["PurchaseId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["StripeSourceId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["CostPerClass"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["PaymentTypeFeePayerAccountId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["ConvenienceFeePayerAccountId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["CouponId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["CouponValue"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["IsDeleted"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));


            return msgs;
        }

        public Registration Reload(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Load(ID.Value, msgs, dc);
        }

        public static Registration LoadByUid(Guid Uid, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["Uid"], Uid);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static Registration LoadByUidByAccountId(Guid Uid, int AccountId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["Uid"], Uid);
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<Registration> LoadByRegistrantId(int RegistrantId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["RegistrantId"], RegistrantId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Registration> LoadByAccountId(int AccountId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Registration> LoadByRegistrationPeriodId(int RegistrationPeriodId,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["RegistrationPeriodId"], RegistrationPeriodId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Registration> LoadByAccountIdByRegistrantId(int AccountId, int RegistrantId,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.Criterias.Add(ObjectMap["RegistrantId"], RegistrantId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Registration> LoadByActivityGroupId(int ActivityGroupId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ActivityGroupId"], ActivityGroupId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Registration> LoadByAccountActivityGroupId(int AccountActivityGroupId,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountActivityGroupId"], AccountActivityGroupId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Registration> LoadByFirstRegistrationId(int FirstRegistrationId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["FirstRegistrationId"], FirstRegistrationId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Registration> LoadByCouponIdByIsDeleted(int CouponId, bool IsDeleted,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["CouponId"], CouponId);
            dq.Criterias.Add(ObjectMap["IsDeleted"], IsDeleted);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        #endregion

        #region "UserDefined"

        [Obsolete] public string Name => RegistrantName;

        public string RegistrantName => BasePerson.BuildDisplayName(RegistrantFirstName, RegistrantLastName);

        public Registrant ToRegistrant()
        {
            return new Registrant
            {
                ID = RegistrantId,
                FirstName = RegistrantFirstName,
                LastName = RegistrantLastName,
                Email = RegistrantEmail,
                Uid = RegistrantUid,
                Phone = RegistrantPhone,
                Company = RegistrantCompany,
                RegistrantTypeId = RegistrantRegistrantTypeId,
            };
        }

        public static List<Registration> LoadByAccountId(int AccountId, int? start, int? length,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.OrderBys.Add(ObjectMap["RegistrantFirstName"], OrderByDirectionEnum.Ascending);
            dq.OrderBys.Add(ObjectMap["RegistrantLastName"], OrderByDirectionEnum.Ascending);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Registration> LoadByAccountIdByDate(int AccountId, DateTime startDate, DateTime endDate,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.Criterias.Add(ObjectMap["CreateDate"], startDate,
                DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);
            dq.Criterias.Add(ObjectMap["CreateDate"], endDate,
                DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);
            dq.Criterias.Add(ObjectMap["CompletionStatusId"], ValidCompletionStatus.Select(x => (int)x).ToArray(),
                DatabaseCriteriaOperatorEnum.In);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCountByAccountIdByDate(int AccountId, DateTime startDate, DateTime endDate,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.Criterias.Add(ObjectMap["CreateDate"], startDate,
                DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);
            dq.Criterias.Add(ObjectMap["CreateDate"], endDate,
                DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);
            dq.Criterias.Add(ObjectMap["CompletionStatusId"], ValidCompletionStatus.Select(x => (int)x).ToArray(),
                DatabaseCriteriaOperatorEnum.In);
            return dq.GetCount(msgs);
        }

        public static List<Registration> LoadList(List<int> ids, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], ids, DatabaseCriteriaOperatorEnum.In);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        internal static List<RegistrationCompletionStatus> ValidCompletionStatus =>
            new List<RegistrationCompletionStatus>
            {
                RegistrationCompletionStatus.Completed,
                RegistrationCompletionStatus.RequiresParentSignature,
            };

        public static List<RegistrationAssignmentType> ReservedStatuses =>
            new List<RegistrationAssignmentType>
            {
                RegistrationAssignmentType.Reserved,
                RegistrationAssignmentType.ReservedPendingPayment,
                RegistrationAssignmentType.ReservedPendingParentalConsent,
            };

        public static List<Registration> LoadFirstRegistrations(int accountId, int? start, int? length,
            DatabaseConnection dc = null)
        {
            var completionStatus = new List<int>
            {
                (int)RegistrationCompletionStatus.Completed,
                (int)RegistrationCompletionStatus.RequiresParentSignature,
            };

            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.Criterias.Add(ObjectMap["AccountId"], accountId);
            dq.Criterias.Add(ObjectMap["CompletionStatusId"], ValidCompletionStatus.Select(x => (int)x).ToArray(),
                DatabaseCriteriaOperatorEnum.In);
            dq.Criterias.Add(ObjectMap["FirstRegistrationId"], DBNull.Value, DatabaseCriteriaOperatorEnum.Is);
            dq.OrderBys.Add(ObjectMap["RegistrantFirstName"], OrderByDirectionEnum.Ascending);
            dq.OrderBys.Add(ObjectMap["RegistrantLastName"], OrderByDirectionEnum.Ascending);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public void FindAndSetFirstRegistration(string firstName, string lastName, string email,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.Criterias.Add(ObjectMap["CompletionStatusId"], ValidCompletionStatus.Select(x => (int)x).ToArray(),
                DatabaseCriteriaOperatorEnum.In);
            dq.Criterias.Add(ObjectMap["FirstRegistrationId"], DBNull.Value, DatabaseCriteriaOperatorEnum.Is);
            dq.Criterias.Add(ObjectMap["RegistrantFirstName"], firstName.Trim());
            dq.Criterias.Add(ObjectMap["RegistrantLastName"], lastName.Trim());
            dq.Criterias.Add(ObjectMap["RegistrantEmail"], email.Trim());
            dq.Criterias.Add(ObjectMap["ID"], ID.Value, DatabaseCriteriaOperatorEnum.NotEqualTo);
            dq.OrderBys.Add(ObjectMap["Id"], OrderByDirectionEnum.Ascending);

            var firstRegistration = dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
            if (firstRegistration.IsEmpty()) return;

            FirstRegistrationId = firstRegistration.ID;
            LastModifiedBy = 0;
            LastModifiedDate = DateTime.UtcNow;
            msgs.AddRange(Save(dc));
        }

        private static string GetSearchSql(string searchText, int accountId, int? questionId,
            RegistrationStatus regStatus, bool getVolunteers, List<int> participationObjectIds = null,
            ParticipationObjectType participationObjectType = ParticipationObjectType.NotSet,
            List<RegistrationAssignmentType> rats = null, bool getCount = false)
        {
            var searchTextJoin = "";
            var whereClause = "";
            var searchTextWhere = "";

            if (searchText.IsNotBlank())
            {
                searchTextJoin = "inner join FormQuestionAnswer fqa on r2.ID = fqa.RegistrationId";

                var itemList = new List<string>();

                if (questionId.HasValue)
                    itemList.Add(searchText);
                else
                    itemList.AddRange(searchText.Split(' ').Select(x => x.Trim()));

                itemList.ForEach(item =>
                {
                    if (item.Trim().IsNotBlank())
                    {
                        if (searchTextWhere.IsNotBlank())
                            searchTextWhere += " or ";

                        searchTextWhere += $"[Text] like '%{item.Replace("\'", "\\\'")}%'";
                    }
                });

                searchTextWhere = $"({searchTextWhere})";

                //if (questionId.HasValue)
                //{
                //    searchTextWhere = " and QuestionDataTypeId ";
                //}
            }

            var hasAssignmentTypes = rats.IsNotEmpty() && rats.Any();
            var hasActivityIds = participationObjectIds.IsNotEmpty() && participationObjectIds.Any();
            var hasRegStatus = regStatus != RegistrationStatus.NotSet;
            var activityJoin = "";
            var assignmentTypeWhere = "";
            var activityIdWhere = "";


            activityJoin = $"inner join RegistrantActivity ra on r2.ID = ra.RegistrationId inner join Activity a on ra.ParticipationObjectId = a.ID and ra.ParticipationObjectTypeId = {(int)participationObjectType}";

            if (hasAssignmentTypes)
                assignmentTypeWhere =
                    $"ra.AssignmentTypeId in ({rats.Select(x => (int)x).ToStrings().ToStringList()})";

            if (hasActivityIds)
                activityIdWhere =
                    $"ra.ParticipationObjectId in ({participationObjectIds.ToStrings().ToStringList()}) and ra.ParticipationObjectTypeId = {(int)participationObjectType}";


            var regStatusJoin = "";
            var regStatusWhere = "";
            
            var activityTypeWhere = getVolunteers
                ? $"a.ActivityTypeId = {(int)ActivityType.VolunteerOpportunity}"
                : $"a.ActivityTypeId != {(int)ActivityType.VolunteerOpportunity}";

            if (hasRegStatus)
            {
                regStatusJoin = "inner join ActivityInstance ai on a.ActivityScheduleId = ai.ActivityScheduleId";
                switch (regStatus)
                {
                    case RegistrationStatus.Past:
                        regStatusWhere = $"ai.EndDateTime < '{DateTime.UtcNow.LastDateOfWeek()}'";
                        break;
                    case RegistrationStatus.Current:
                        regStatusWhere =
                            $"ai.StartDateTime <= '{DateTime.UtcNow.FirstDateOfWeek()}' and ai.EndDateTime >= '{DateTime.UtcNow.LastDateOfWeek()}'";
                        break;
                    case RegistrationStatus.Future:
                        regStatusWhere = $"ai.StartDateTime > '{DateTime.UtcNow.FirstDateOfWeek()}'";
                        break;
                }
            }

            whereClause += searchTextWhere;
            whereClause += assignmentTypeWhere.IsNotEmpty()
                ? (whereClause.IsNotEmpty() ? " and " : "") + assignmentTypeWhere
                : "";
            whereClause += activityIdWhere.IsNotEmpty()
                ? (whereClause.IsNotEmpty() ? " and " : "") + activityIdWhere
                : "";
            whereClause += regStatusWhere.IsNotEmpty()
                ? (whereClause.IsNotEmpty() ? " and " : "") + regStatusWhere
                : "";
            whereClause += (whereClause.IsNotEmpty() ? " and " : "") + activityTypeWhere;
            whereClause = $@"where 
                                r2.AccountId = {accountId} and 
                                r2.CompletionStatusId in ({ValidCompletionStatus.Select(x => (int)x).ToStrings().ToStringList()})" +
                          (whereClause.IsNotEmpty() ? $" and {whereClause}" : "");

            var select = @"r.*,
                            reg.FirstName as RegistrantFirstName,
                            reg.LastName as RegistrantLastName,
                            reg.Email as RegistrantEmail,
                            reg.RegistrantTypeId as RegistrantRegistrantTypeId,
                            reg.Uid as RegistrantUid,
                            reg.Phone as RegistrantPhone,
                            reg.Company as RegistrantCompany";

            var orderBy = @"order by
                                reg.FirstName,
                                reg.LastName";

            if (getCount)
            {
                select = "COUNT(r.id)";
                orderBy = "";
            }

            var sql = $@"select {select}
                        from 
	                        Registration r inner join
                            Registrant reg on r.RegistrantId = reg.ID
                        where
	                        r.FirstRegistrationId is null and
                            r.AccountId = {accountId} and
	                        r.ID in 
	                        (
		                        select 
			                        isnull(r2.FirstRegistrationId, r2.ID)
		                        from
			                        Registration r2
                                    {searchTextJoin}
			                        {activityJoin}
			                        {regStatusJoin}
		                        {whereClause}
	                        )
                        {orderBy}";

            return sql;
        }

        private static string GetSearchSqlByActivityIds(IReadOnlyCollection<int> activityIds)
        {
            if (activityIds.IsEmpty() || !activityIds.Any()) throw new Exception("No activity IDs were passed in for the search");

            var validCompletionStatuses = ValidCompletionStatus.Select(x => (int) x).ToStrings().ToStringList();
            var activityIdList = activityIds.ToStrings().ToStringList();
            const int activityParticipationType = (int) ParticipationObjectType.Activity;
            var sql = $@"select r.*,
                            reg.FirstName as RegistrantFirstName,
                            reg.LastName as RegistrantLastName,
                            reg.Email as RegistrantEmail,
                            reg.RegistrantTypeId as RegistrantRegistrantTypeId,
                            reg.Uid as RegistrantUid,
                            reg.Phone as RegistrantPhone,
                            reg.Company as RegistrantCompany
                        from 
	                        Registration r
	                        inner join Registrant reg on r.RegistrantId = reg.ID
                            inner join RegistrantActivity ra on r.ID = ra.RegistrationId
	                        inner join Activity a on ra.ParticipationObjectId = a.ID and ra.ParticipationObjectTypeId = {activityParticipationType}
                        where
                            r.CompletionStatusId in ({validCompletionStatuses})
                            and ra.ParticipationObjectId in ({activityIdList}) and ra.ParticipationObjectTypeId = {activityParticipationType}
                            and a.ActivityTypeId != {(int) ActivityType.VolunteerOpportunity}
                        order by
                            reg.FirstName,
                            reg.LastName";

            return sql;
        }

        public static List<Registration> Search(string searchText, int accountId, int? questionId,
            List<int> participationObjectIds, ParticipationObjectType participationObjectType,
            RegistrationStatus regStatus, List<RegistrationAssignmentType> rats, int? start, int? length,
            bool getVolunteers, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            // var dq = GetSearchQuery(searchText, accountId, questionId, regStatus, participationObjectIds, participationObjectType, rats, msgs, dc);
            var dq = DatabaseQuery;
            dq.Connection = dc;
            var sql = GetSearchSql(searchText, accountId, questionId, regStatus, getVolunteers, participationObjectIds,
                participationObjectType, rats);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;

            return dq.ExecuteSelect(sql, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Registration> LoadByActivityIds(IReadOnlyCollection<int> activityIds, SystemMessages msgs, DatabaseConnection dc)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            var sql = GetSearchSqlByActivityIds(activityIds);

            return dq.ExecuteSelect(sql, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int SearchCount(string searchText, int accountId, int? questionId,
            List<int> participationObjectIds, ParticipationObjectType participationObjectType,
            RegistrationStatus regStatus, List<RegistrationAssignmentType> rats, bool getVolunteers, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            var sql = GetSearchSql(searchText, accountId, questionId, regStatus, getVolunteers, participationObjectIds,
                participationObjectType, rats, true);
            return dq.ExecuteSelect(sql).Rows[0][0].ToString().ToInteger();
        }

        public static List<Registration> LoadByRegistrationPeriodId(int RegistrationPeriodId, int? start, int? length,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["RegistrationPeriodId"], RegistrationPeriodId);
            dq.Criterias.Add(ObjectMap["CompletionStatusId"], ValidCompletionStatus.Select(x => (int)x).ToArray(),
                DatabaseCriteriaOperatorEnum.In);
            dq.OrderBys.Add(ObjectMap["RegistrantFirstName"], OrderByDirectionEnum.Ascending);
            dq.OrderBys.Add(ObjectMap["RegistrantLastName"], OrderByDirectionEnum.Ascending);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Registration> LoadByActivityGroupId(int ActivityGroupId, int? start, int? length,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ActivityGroupId"], ActivityGroupId);
            dq.Criterias.Add(ObjectMap["CompletionStatusId"], ValidCompletionStatus.Select(x => (int)x).ToArray(),
                DatabaseCriteriaOperatorEnum.In);
            dq.OrderBys.Add(ObjectMap["RegistrantFirstName"], OrderByDirectionEnum.Ascending);
            dq.OrderBys.Add(ObjectMap["RegistrantLastName"], OrderByDirectionEnum.Ascending);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Registration> LoadByAccountActivityGroupId(int AccountActivityGroupId, int? start,
            int? length, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountActivityGroupId"], AccountActivityGroupId);
            dq.Criterias.Add(ObjectMap["CompletionStatusId"], ValidCompletionStatus.Select(x => (int)x).ToArray(),
                DatabaseCriteriaOperatorEnum.In);
            dq.OrderBys.Add(ObjectMap["RegistrantFirstName"], OrderByDirectionEnum.Ascending);
            dq.OrderBys.Add(ObjectMap["RegistrantLastName"], OrderByDirectionEnum.Ascending);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int TotalCurrentRegistrationsCount(int accountId, DatabaseConnection dc = null)
        {
            var sql = $@"select count(id) 
                        from Registration r
                        where 
                        accountId = {accountId} and
                        completionStatusId in ({ValidCompletionStatus.Select(x => (int)x).ToStrings().ToStringList()}) and
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

        public static List<Registration> LoadCurrentByAccountId(int AccountId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;

            var sql = $@"
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

        public string GetParentalConsentUrl => $"{Utilities.BaseUrl()}/account/parentalconsent/{Uid}";

        public string GetPaymentUrl => $"{Utilities.BaseUrl()}/account/makepayment/{Uid}";

        public static List<Registration> LoadByCouponId(int CouponId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["CouponId"], CouponId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Registration> LoadByCouponIdByPurchaseAccountId(int CouponId, int PurchaserAccountId,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["CouponId"], CouponId);
            dq.Criterias.Add(ObjectMap["PaymentTypeFeePayerAccountId"], PurchaserAccountId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        #endregion
    }
}