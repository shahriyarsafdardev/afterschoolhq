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

        public int RegistrantId
        {
            get => (int) MDV["RegistrantId"].Value;
            set => MDV["RegistrantId"].Value = value;
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

        public string CandidateId
        {
            get => (string) MDV["CandidateId"].Value;
            set => MDV["CandidateId"].Value = value;
        }

        public string SterlingUrl
        {
            get => (string) MDV["SterlingUrl"].Value;
            set => MDV["SterlingUrl"].Value = value;
        }

        public static DatabaseParameters ObjectMap
        {
            get
            {
                const string className = nameof(BackgroundCheck);
                var dp = get_BaseObjectMap(className);
                dp.Add(className, "RegistrantId", SqlDbType.Int);
                dp.Add(className, "BackgroundCheckStatusId", SqlDbType.Int);
                dp.Add(className, "SterlingProcessStepId", SqlDbType.Int);
                dp.Add(className, "CandidateId", SqlDbType.NVarChar, "500");
                dp.Add(className, "SterlingUrl", SqlDbType.NVarChar, "500");

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

            if (RegistrantId.IsEmpty())
                msgs.Add("RegistrantId is required on BackgroundCheck object", MessageType.Error);
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

            var dps = new DatabaseParameters {ObjectMap["RegistrantId"]};
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters {ObjectMap["BackgroundCheckStatusId"]};
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters {ObjectMap["SterlingProcessStepId"]};
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters {ObjectMap["CandidateId"]};
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters {ObjectMap["SterlingUrl"]};
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            return msgs;
        }

        public BackgroundCheck Reload(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ID.HasValue ? Load(ID.Value, msgs, dc) : null;
        }

        public static List<BackgroundCheck> LoadByRegistrantId(string registrantId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["RegistrantId"], registrantId);
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