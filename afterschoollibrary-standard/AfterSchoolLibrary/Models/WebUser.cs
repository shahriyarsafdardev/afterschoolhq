using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Models
{
    /// <summary>
    ///     A login account. A person cannot login to the system without a WebUser account.
    /// </summary>
    [Serializable]
    public class WebUser : BaseWebUser
    {
        #region "ClassGenerator"

        public string FacebookId
        {
            get => (string) MDV["FacebookId"].Value;
            set => MDV["FacebookId"].Value = value;
        }

        /// <summary>
        ///     Required.
        /// </summary>
        public Guid Uid
        {
            get => (Guid) MDV["Uid"].Value;
            set => MDV["Uid"].Value = value;
        }

        public bool IsActive
        {
            get => (bool) MDV["IsActive"].Value;
            set => MDV["IsActive"].Value = value;
        }

        public DateTime? LastActivity
        {
            get => (DateTime?) MDV["LastActivity"].Value;
            set => MDV["LastActivity"].Value = value;
        }

        public bool VerifiedAccount
        {
            get => (bool) MDV["VerifiedAccount"].Value;
            set => MDV["VerifiedAccount"].Value = value;
        }

        public bool IsRegistered
        {
            get => (bool) MDV["IsRegistered"].Value;
            set => MDV["IsRegistered"].Value = value;
        }

        public int? DefaultAccountId
        {
            get => (int?) MDV["DefaultAccountId"].Value;
            set => MDV["DefaultAccountId"].Value = value;
        }

        public bool AllowTextMessages
        {
            get => (bool) MDV["AllowTextMessages"].Value;
            set => MDV["AllowTextMessages"].Value = value;
        }

        public string CleverUserId
        {
            get => (string) MDV["CleverUserId"].Value;
            set => MDV["CleverUserId"].Value = value;
        }

        public string CleverUserName
        {
            get => (string) MDV["CleverUserName"].Value;
            set => MDV["CleverUserName"].Value = value;
        }

        public string HowHearAboutUs
        {
            get => (string) MDV["HowHearAboutUs"].Value;
            set => MDV["HowHearAboutUs"].Value = value;
        }


        public WebUser()
        {
            Config(ObjectMap, DatabaseQuery, typeof(WebUser).Name, "ID");
            LoadObjectValues(ObjectMap);
            Uid = Guid.NewGuid();
            IsActive = false;
            VerifiedAccount = false;
            IsRegistered = false;
            AllowTextMessages = false;
        }

        public static DatabaseParameters ObjectMap
        {
            get
            {
                var _dp = new DatabaseParameters();
                var className = typeof(WebUser).Name;
                _dp = get_BaseObjectMap(className);
                _dp.Add(className, "FacebookId", SqlDbType.NVarChar, "100");
                _dp.Add(className, "Uid", SqlDbType.UniqueIdentifier);
                _dp.Add(className, "IsActive", SqlDbType.Bit);
                _dp.Add(className, "LastActivity", SqlDbType.DateTime);
                _dp.Add(className, "VerifiedAccount", SqlDbType.Bit);
                _dp.Add(className, "IsRegistered", SqlDbType.Bit);
                _dp.Add(className, "DefaultAccountId", SqlDbType.Int);
                _dp.Add(className, "AllowTextMessages", SqlDbType.Bit);
                _dp.Add(className, "CleverUserId", SqlDbType.NVarChar, "100");
                _dp.Add(className, "CleverUserName", SqlDbType.NVarChar, "100");
                _dp.Add(className, "HowHearAboutUs", SqlDbType.NVarChar, "500");

                return _dp;
            }
        }

        public static WebUser LoadObject(DataRow row)
        {
            var obj = new WebUser();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static DatabaseQuery DatabaseQuery
        {
            get
            {
                var dq = get_DatabaseQuery(ObjectMap, typeof(WebUser).Name);

                return dq;
            }
        }

        public static WebUser Load(int id, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<WebUser> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null,
            DatabaseConnection dc = null)
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

            if (Uid.IsEmpty())
                msgs.Add("Uid is required on WebUser object", MessageType.Error);

            if (msgs.HasNoErrors())
                msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(WebUser).Name));

            return msgs;
        }

        public SystemMessages Delete(DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(WebUser).Name);
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
            dps.Add(ObjectMap["FacebookId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["Uid"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["IsActive"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["LastActivity"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["VerifiedAccount"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["IsRegistered"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["DefaultAccountId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["AllowTextMessages"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["CleverUserId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["CleverUserName"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));


            dps = new DatabaseParameters();
            dps.Add(ObjectMap["HowHearAboutUs"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));


            return msgs;
        }

        public WebUser Reload(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Load(ID.Value, msgs, dc);
        }

        public static WebUser LoadByUid(Guid Uid, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["Uid"], Uid);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static WebUser LoadByUidByIsActive(Guid Uid, bool IsActive, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["Uid"], Uid);
            dq.Criterias.Add(ObjectMap["IsActive"], IsActive);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<WebUser> LoadByIsActive(bool IsActive, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["IsActive"], IsActive);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static WebUser LoadByFacebookId(string FacebookId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["FacebookId"], FacebookId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static WebUser LoadByCleverUserIdByIsActive(string CleverUserId, bool IsActive, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["CleverUserId"], CleverUserId);
            dq.Criterias.Add(ObjectMap["IsActive"], IsActive);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        #endregion

        #region "UserDefined"

        public static WebUser LoadByEmail(string email, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return LoadByEmail(email, null, msgs, dc);
        }

        public static WebUser LoadByEmail(string email, bool? hasPassword, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["email"], email);

            if (hasPassword.HasValue)
            {
                var op = hasPassword.Value ? DatabaseCriteriaOperatorEnum.IsNot : DatabaseCriteriaOperatorEnum.Is;
                dq.UseParameterizedQuery = false;
                dq.Criterias.Add(ObjectMap["password"], DBNull.Value, op);
            }

            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static WebUser LoadByPhone(string phone, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return LoadByPhone(phone, null, msgs, dc);
        }

        public static WebUser LoadByPhone(string phone, bool? hasPassword, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["phone"], phone);

            if (hasPassword.HasValue)
            {
                var op = hasPassword.Value ? DatabaseCriteriaOperatorEnum.IsNot : DatabaseCriteriaOperatorEnum.Is;
                dq.UseParameterizedQuery = false;
                dq.Criterias.Add(ObjectMap["password"], DBNull.Value, op);
            }

            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static WebUser LoadByUsername(string username, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return LoadByUsername(username, null, msgs, dc);
        }

        public static WebUser LoadByUsername(string username, bool? hasPassword, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["username"], username);

            if (hasPassword.HasValue)
            {
                var op = hasPassword.Value ? DatabaseCriteriaOperatorEnum.IsNot : DatabaseCriteriaOperatorEnum.Is;
                dq.UseParameterizedQuery = false;
                dq.Criterias.Add(ObjectMap["password"], DBNull.Value, op);
            }

            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public bool CanEmailBeUsed(string email, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["email"], email);
            dq.Criterias.Add(ObjectMap["ID"], ID.Value, DatabaseCriteriaOperatorEnum.NotEqualTo);
            return dq.GetCount() == 0;
        }

        public static bool ExistsByEmail(string email, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return GetCount(
                new Hashtable
                {
                    {"email", email},
                },
                dc: dc
            ) > 0;
        }

        public static bool ExistsByPhone(string phone, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return GetCount(
                new Hashtable
                {
                    {"phone", phone},
                },
                dc: dc
            ) > 0;
        }

        public static bool ExistsByUid(Guid uid, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return GetCount(
                new Hashtable
                {
                    {"uid", uid},
                },
                dc: dc
            ) > 0;
        }

        public static int GetSearchCount(string search, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.LogicalOperator = LogicalOperatorType.Or;
            if (search.IsNotEmpty())
            {
                dq.Criterias.Add(ObjectMap["FirstName"], search, DatabaseCriteriaOperatorEnum.Like);
                dq.Criterias.Add(ObjectMap["LastName"], search, DatabaseCriteriaOperatorEnum.Like);
                dq.Criterias.Add(ObjectMap["Email"], search, DatabaseCriteriaOperatorEnum.Like);
            }

            return dq.GetCount(msgs);
        }

        public static List<WebUser> Search(string search, int? start, int? length, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;

            dq.Criterias.LogicalOperator = LogicalOperatorType.Or;
            if (search.IsNotBlank())
            {
                dq.Criterias.Add(ObjectMap["FirstName"], search, DatabaseCriteriaOperatorEnum.Like);
                dq.Criterias.Add(ObjectMap["LastName"], search, DatabaseCriteriaOperatorEnum.Like);
                dq.Criterias.Add(ObjectMap["Email"], search, DatabaseCriteriaOperatorEnum.Like);
            }

            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        #endregion
    }
}