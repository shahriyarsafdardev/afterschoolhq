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
    ///     The status of a background check process
    /// </summary>
    [Serializable]
    public class BackgroundCheck : BasicObjectWithModified
    {
        public BackgroundCheck()
        {
            Config(ObjectMap, DatabaseQuery, nameof(BackgroundCheck), "ID");
            LoadObjectValues(ObjectMap);
            BackgroundCheckStatusId = BackgroundCheckStatus.NotSet;
            BackgroundCheckSourceId = BackgroundCheckSource.NotSet;
        }

        public int WebUserId
        {
            get => (int) MDV["WebUserId"].Value;
            set => MDV["WebUserId"].Value = value;
        }

        public int RequestedForFiscalAccountId
        {
            get => (int) MDV["RequestedForFiscalAccountId"].Value;
            set => MDV["RequestedForFiscalAccountId"].Value = value;
        }

        public BackgroundCheckStatus BackgroundCheckStatusId
        {
            get => (BackgroundCheckStatus) MDV["BackgroundCheckStatusId"].Value;
            set => MDV["BackgroundCheckStatusId"].Value = value;
        }

        public BackgroundCheckSource BackgroundCheckSourceId
        {
            get => (BackgroundCheckSource) MDV["BackgroundCheckSourceId"].Value;
            set => MDV["BackgroundCheckSourceId"].Value = value;
        }

        public string SterlingCandidateId
        {
            get => (string) MDV["SterlingCandidateId"].Value;
            set => MDV["SterlingCandidateId"].Value = value;
        }

        public string SterlingInviteUrl
        {
            get => (string) MDV["SterlingInviteUrl"].Value;
            set => MDV["SterlingInviteUrl"].Value = value;
        }

        public DateTime? ExpirationDate
        {
            get => (DateTime?) MDV["ExpirationDate"].Value;
            set => MDV["ExpirationDate"].Value = value;
        }

        public DateTime? SterlingEmailSentDate
        {
            get => (DateTime?) MDV["SterlingEmailSentDate"].Value;
            set => MDV["SterlingEmailSentDate"].Value = value;
        }

        public DateTime? VolunteerCompletedDate
        {
            get => (DateTime?) MDV["VolunteerCompletedDate"].Value;
            set => MDV["VolunteerCompletedDate"].Value = value;
        }

        public DateTime? SterlingReviewCompleteDate
        {
            get => (DateTime?) MDV["SterlingReviewCompleteDate"].Value;
            set => MDV["SterlingReviewCompleteDate"].Value = value;
        }

        public DateTime? ManualReviewCompleteDate
        {
            get => (DateTime?) MDV["ManualReviewCompleteDate"].Value;
            set => MDV["ManualReviewCompleteDate"].Value = value;
        }

        public static DatabaseParameters ObjectMap
        {
            get
            {
                const string className = nameof(BackgroundCheck);
                var dp = get_BaseObjectMap(className);
                dp.Add(className, "WebUserId", SqlDbType.Int);
                dp.Add(className, "RequestedForFiscalAccountId", SqlDbType.Int);
                dp.Add(className, "BackgroundCheckStatusId", SqlDbType.Int);
                dp.Add(className, "BackgroundCheckSourceId", SqlDbType.Int);
                dp.Add(className, "SterlingCandidateId", SqlDbType.NVarChar, "40");
                dp.Add(className, "SterlingInviteUrl", SqlDbType.NVarChar, "500");
                dp.Add(className, "ExpirationDate", SqlDbType.DateTime);
                dp.Add(className, "SterlingEmailSentDate", SqlDbType.DateTime);
                dp.Add(className, "VolunteerCompletedDate", SqlDbType.DateTime);
                dp.Add(className, "SterlingReviewCompleteDate", SqlDbType.DateTime);
                dp.Add(className, "ManualReviewCompleteDate", SqlDbType.DateTime);

                return dp;
            }
        }

        protected static DatabaseQuery DatabaseQuery
        {
            get
            {
                var dq = get_DatabaseQuery(ObjectMap, nameof(BackgroundCheck));
                return dq;
            }
        }

        public static BackgroundCheck LoadObject(DataRow row)
        {
            var obj = new BackgroundCheck();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        public static BackgroundCheck Load(int id, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<BackgroundCheck> Load(Hashtable criteria = null, int? start = null, int? length = null, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return BaseLoad(criteria, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criteria = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            return BaseGetCount(criteria, ObjectMap, dq, msgs);
        }

        public SystemMessages Save(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            if (WebUserId.IsEmpty())
                msgs.Add("WebUserId is required on BackgroundCheck object", MessageType.Error);
            if (RequestedForFiscalAccountId.IsEmpty())
                msgs.Add("RequestedForFiscalAccountId is required on BackgroundCheck object", MessageType.Error);
            if (BackgroundCheckStatusId.IsEmpty())
                msgs.Add("BackgroundCheckStatusId is required on BackgroundCheck object", MessageType.Error);

            if (msgs.HasNoErrors())
                msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, nameof(BackgroundCheck)));

            return msgs;
        }

        public SystemMessages Delete(DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, nameof(BackgroundCheck));
        }

        public static SystemMessages Create(DatabaseConnection dc = null)
        {
            return _Create(DatabaseQuery, dc, ObjectMap);
        }

        public static SystemMessages Alter(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["WebUserId"]}));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["RequestedForFiscalAccountId"]}));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["BackgroundCheckStatusId"]}));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["BackgroundCheckSourceId"]}));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["SterlingCandidateId"]}));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["SterlingInviteUrl"]}));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["ExpirationDate"]}));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["SterlingEmailSentDate"]}));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["VolunteerCompletedDate"]}));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["SterlingReviewCompleteDate"]}));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["ManualReviewCompleteDate"]}));

            return msgs;
        }

        public BackgroundCheck Reload(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ID.HasValue ? Load(ID.Value, msgs, dc) : null;
        }

        public static List<BackgroundCheck> LoadByWebUserId(string webUserId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["WebUserId"], webUserId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<BackgroundCheck> LoadByRequestedForFiscalAccountId(string requestedForFiscalAccountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["RequestedForFiscalAccountId"], requestedForFiscalAccountId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }
    }
}