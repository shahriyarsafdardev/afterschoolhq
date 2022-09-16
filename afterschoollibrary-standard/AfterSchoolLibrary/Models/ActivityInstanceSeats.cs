using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Models
{
    /// <summary>
    ///     Store number of seats per actual date of an activity useful for events and tutoring
    /// </summary>
    public class ActivityInstanceSeats : BasicObjectWithModified
    {
        #region "UserDefined"

        public static int GetSeatsTakenByActivityInstanceId(int ActivityInstanceId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;

            dq.SelectParameters.Clear();
            dq.SelectParameters.Add(ObjectMap["NumberOfSeats"], "SumOfSeats", $"ISNULL(SUM({DatabaseQuery.FunctionReplaceValue}), 0)");

            dq.Criterias.Add(ObjectMap["ActivityInstanceId"], ActivityInstanceId);
            dq.Criterias.Add(RegistrantActivity.ObjectMap["AssignmentTypeId"],
                Registration.ReservedStatuses.Select(x => (int)x).ToList(),
                DatabaseCriteriaOperatorEnum.In);
            dq.Joins.Add(RegistrantActivity.ObjectMap["ID"], ObjectMap["RegistrantActivityId"], DatabaseJoinTypeEnum.InnerJoin);

            var dt = dq.Select(msgs);

            if (dt.Rows.Count > 0)
                return dt.Rows[0][0].ToString().ToInteger();

            return 0;
        }

        #endregion

        #region "ClassGenerator"

        /// <summary>
        ///     Required.
        /// </summary>
        public int? RegistrantActivityId
        {
            get => (int?)MDV["RegistrantActivityId"].Value;
            set => MDV["RegistrantActivityId"].Value = value;
        }

        /// <summary>
        ///     Required.
        /// </summary>
        public int? ActivityInstanceId
        {
            get => (int?)MDV["ActivityInstanceId"].Value;
            set => MDV["ActivityInstanceId"].Value = value;
        }

        /// <summary>
        ///     Required.
        /// </summary>
        public int? NumberOfSeats
        {
            get => (int?)MDV["NumberOfSeats"].Value;
            set => MDV["NumberOfSeats"].Value = value;
        }


        public ActivityInstanceSeats()
        {
            Config(ObjectMap, DatabaseQuery, typeof(ActivityInstanceSeats).Name, "ID");
            LoadObjectValues(ObjectMap);
        }

        public static DatabaseParameters ObjectMap
        {
            get
            {
                var _dp = new DatabaseParameters();
                var className = typeof(ActivityInstanceSeats).Name;
                _dp = get_BaseObjectMap(className);
                _dp.Add(className, "RegistrantActivityId", SqlDbType.Int);
                _dp.Add(className, "ActivityInstanceId", SqlDbType.Int);
                _dp.Add(className, "NumberOfSeats", SqlDbType.Int);

                return _dp;
            }
        }

        public static ActivityInstanceSeats LoadObject(DataRow row)
        {
            var obj = new ActivityInstanceSeats();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static DatabaseQuery DatabaseQuery
        {
            get
            {
                var dq = get_DatabaseQuery(ObjectMap, typeof(ActivityInstanceSeats).Name);

                return dq;
            }
        }

        public static ActivityInstanceSeats Load(int id, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<ActivityInstanceSeats> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null,
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

            if (RegistrantActivityId.IsEmpty())
                msgs.Add("RegistrantActivityId is required on ActivityInstanceSeats object", MessageType.Error);
            if (ActivityInstanceId.IsEmpty())
                msgs.Add("ActivityInstanceId is required on ActivityInstanceSeats object", MessageType.Error);
            if (NumberOfSeats.IsEmpty())
                msgs.Add("NumberOfSeats is required on ActivityInstanceSeats object", MessageType.Error);

            if (msgs.HasNoErrors())
                msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(ActivityInstanceSeats).Name));

            return msgs;
        }

        public SystemMessages Delete(DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(ActivityInstanceSeats).Name);
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
            dps.Add(ObjectMap["RegistrantActivityId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["ActivityInstanceId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["NumberOfSeats"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));


            return msgs;
        }

        public ActivityInstanceSeats Reload(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Load(ID.Value, msgs, dc);
        }

        public static List<ActivityInstanceSeats> LoadByRegistrantActivityId(int RegistrantActivityId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["RegistrantActivityId"], RegistrantActivityId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<ActivityInstanceSeats> LoadByActivityInstanceId(int ActivityInstanceId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ActivityInstanceId"], ActivityInstanceId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static ActivityInstanceSeats LoadByRegistrantActivityIdByActivityInstanceId(int RegistrantActivityId, int ActivityInstanceId,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["RegistrantActivityId"], RegistrantActivityId);
            dq.Criterias.Add(ObjectMap["ActivityInstanceId"], ActivityInstanceId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        #endregion
    }
}