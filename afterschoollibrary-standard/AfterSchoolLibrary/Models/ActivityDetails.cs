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
    ///     Copy of each instance of a WebSessionObject each time it is changed so that things can be restored to any point in time
    /// </summary>
    [Serializable]
    public class ActivityDetails : BasicObjectWithModified
    {
        public ActivityDetails()
        {
            Config(ObjectMap, DatabaseQuery, nameof(ActivityDetails), "ID");
            LoadObjectValues(ObjectMap);
        }

        public int? ActivityId
        {
            get => (int) MDV["ActivityId"].Value;
            set => MDV["ActivityId"].Value = value;
        }

        public string Value
        {
            get => (string) MDV["Value"].Value;
            set => MDV["Value"].Value = value;
        }

        public static DatabaseParameters ObjectMap
        {
            get
            {
                const string className = nameof(ActivityDetails);
                var dp = get_BaseObjectMap(className);
                dp.Add(className, "ActivityId", SqlDbType.Int);
                dp.Add(className, "Value", SqlDbType.NVarChar, "MAX");

                return dp;
            }
        }

        protected static DatabaseQuery DatabaseQuery
        {
            get
            {
                var dq = get_DatabaseQuery(ObjectMap, nameof(ActivityDetails));
                return dq;
            }
        }

        public static ActivityDetails LoadObject(DataRow row)
        {
            var obj = new ActivityDetails();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        public static ActivityDetails Load(int id, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<ActivityDetails> Load(Hashtable criteria = null, int? start = null, int? length = null, SystemMessages msgs = null,
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

            if (ActivityId.IsEmpty())
                msgs.Add("ActivityId is required on ActivityDetails object", MessageType.Error);
            if (Value.IsEmpty())
                msgs.Add("Value is required on ActivityDetails object", MessageType.Error);

            if (msgs.HasNoErrors())
                msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, nameof(ActivityDetails)));

            return msgs;
        }

        public SystemMessages Delete(DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, nameof(ActivityDetails));
        }

        public static SystemMessages Create(DatabaseConnection dc = null)
        {
            return _Create(DatabaseQuery, dc, ObjectMap);
        }

        public static SystemMessages Alter(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            var dps = new DatabaseParameters {ObjectMap["ActivityId"]};
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters {ObjectMap["Value"]};
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            return msgs;
        }

        public ActivityDetails Reload(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Load(ID!.Value, msgs, dc);
        }

        internal static ActivityDetails LoadByActivityId(int activityId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ActivityId"], activityId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }
    }
}