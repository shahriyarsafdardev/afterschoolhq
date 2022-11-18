using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Models
{
    [Serializable]
    public class VolunteerTask : BasicIDObject
    {
        public VolunteerTask()
        {
            Config(ObjectMap, DatabaseQuery, nameof(VolunteerTask), "ID");
            LoadObjectValues(ObjectMap);
            ApplicationStatus = ApplicationStatus.NotSet;
            Task = TaskStatus.NotSet;
        }
        public int? WebUserId
        {
            get => (int) MDV["WebUserId"].Value;
            set => MDV["WebUserId"].Value = value;
        }
        
        public TaskStatus Task
        {
            get => (TaskStatus) MDV["Task"].Value;
            set => MDV["Task"].Value = value;
        }
        
        public ApplicationStatus ApplicationStatus
        {
            get => (ApplicationStatus) MDV["ApplicationStatus"].Value;
            set => MDV["ApplicationStatus"].Value = value;
        }
        
        public DateTime DateSubmitted
        {
            get => (DateTime) MDV["DateSubmitted"].Value;
            set => MDV["DateSubmitted"].Value = value;
        }
        
        public static DatabaseParameters ObjectMap
        {
            get
            {
                const string className = nameof(VolunteerTask);
                var dp = get_BaseObjectMap(className);
                dp.Add(className, "WebUserId", SqlDbType.Int);
                dp.Add(className, "Task", SqlDbType.Int);
                dp.Add(className, "ApplicationStatus", SqlDbType.Int);
                dp.Add(className, "DateSubmitted", SqlDbType.DateTime);

                return dp;
            }
        }
        
        protected static DatabaseQuery DatabaseQuery
        {
            get
            {
                var dq = get_DatabaseQuery(ObjectMap, nameof(VolunteerTask));
                return dq;
            }
        }
        
        public static VolunteerTask LoadObject(DataRow row)
        {
            var obj = new VolunteerTask();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }
        
        public static VolunteerTask Load(int id, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }
        
        public static List<VolunteerTask> Load(Hashtable criteria = null, int? start = null, int? length = null, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return BaseLoad(criteria, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static SystemMessages Create(DatabaseConnection dc = null)
        {
            return _Create(DatabaseQuery, dc, ObjectMap);
        }
        
        public static SystemMessages Alter(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["WebUserId"]}));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["Task"]}));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["ApplicationStatus"]}));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["DateSubmitted"]}));

            return msgs;
        }
    }
}