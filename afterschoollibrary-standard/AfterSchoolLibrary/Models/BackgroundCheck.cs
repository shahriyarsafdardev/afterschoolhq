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
            SterlingProcessStepId = SterlingProcessStep.NotStarted;
        }


        public int WebUserId
        {
            get => (int) MDV["WebUserId"].Value;
            set => MDV["WebUserId"].Value = value;
        }
        
        public int AccountId
        {
            get => (int) MDV["AccountId"].Value;
            set => MDV["AccountId"].Value = value;
        }

        public BackgroundCheckStatus BackgroundCheckStatusId
        {
            get => (BackgroundCheckStatus) MDV["BackgroundCheckStatusId"].Value;
            set => MDV["BackgroundCheckStatusId"].Value = value;
        }

        public SterlingProcessStep SterlingProcessStepId
        {
            get => (SterlingProcessStep) MDV["SterlingProcessStepId"].Value;
            set => MDV["SterlingProcessStepId"].Value = value;
        }

        public BackgroundCheckSource BackgroundCheckSourceId
        {
            get => (BackgroundCheckSource) MDV["BackgroundCheckSourceId"].Value;
            set => MDV["BackgroundCheckSourceId"].Value = value;
        }

        public string ExternalCandidateId
        {
            get => (string) MDV["ExternalCandidateId"].Value;
            set => MDV["ExternalCandidateId"].Value = value;
        }

        public string SterlingInviteUrl
        {
            get => (string) MDV["SterlingInviteUrl"].Value;
            set => MDV["SterlingInviteUrl"].Value = value;
        }


        public static DatabaseParameters ObjectMap
        {
            get
            {
                const string className = nameof(BackgroundCheck);
                var dp = get_BaseObjectMap(className);
                dp.Add(className, "WebUserId", SqlDbType.Int);
                dp.Add(className, "BackgroundCheckStatusId", SqlDbType.Int);
                dp.Add(className, "SterlingProcessStepId", SqlDbType.Int);
                dp.Add(className, "ExternalCandidateId", SqlDbType.NVarChar, "500");
                dp.Add(className, "SterlingInviteUrl", SqlDbType.NVarChar, "500");

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
            if (BackgroundCheckStatusId.IsEmpty())
                msgs.Add("BackgroundCheckStatusId is required on BackgroundCheck object", MessageType.Error);
            if (SterlingProcessStepId.IsEmpty())
                msgs.Add("SterlingProcessStepId is required on BackgroundCheck object", MessageType.Error);

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

            var dps = new DatabaseParameters {ObjectMap["WebUserId"]};
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters {ObjectMap["BackgroundCheckStatusId"]};
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters {ObjectMap["SterlingProcessStepId"]};
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters {ObjectMap["ExternalCandidateId"]};
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters {ObjectMap["SterlingInviteUrl"]};
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

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

        public static List<BackgroundCheck> LoadByAccountId(string accountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], accountId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }
    }
}