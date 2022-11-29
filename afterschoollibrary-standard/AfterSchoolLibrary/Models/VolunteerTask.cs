using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Twilio.Rest.Preview.TrustedComms;

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
            get => (int?) MDV["WebUserId"].Value;
            set => MDV["WebUserId"].Value = value;
        }
        
        public int? AccountId
        {
            get => (int?) MDV["AccountId"].Value;
            set => MDV["AccountId"].Value = value;
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
        
        public DateTime? DateCompleted
        {
            get => (DateTime?) MDV["DateCompleted"].Value;
            set => MDV["DateCompleted"].Value = value;
        }
        
        public static DatabaseParameters ObjectMap
        {
            get
            {
                const string className = nameof(VolunteerTask);
                var dp = get_BaseObjectMap(className);
                dp.Add(className, "WebUserId", SqlDbType.Int);
                dp.Add(className, "AccountId", SqlDbType.Int);
                dp.Add(className, "Task", SqlDbType.Int);
                dp.Add(className, "ApplicationStatus", SqlDbType.Int);
                dp.Add(className, "DateSubmitted", SqlDbType.DateTime);
                dp.Add(className, "DateCompleted", SqlDbType.DateTime);

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
        
        public static VolunteerTask LoadByWebUser(int? Id, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.Criterias.Add(ObjectMap["WebUserId"], Id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }
        
        public static List<VolunteerTask> LoadByTaskTypeAndAppStatus(TaskStatus taskStatus, ApplicationStatus applicationStatus, List<VolunteerApproval> volunteerApprovals, Account currentAccount, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.Joins.Add(WebUser.ObjectMap["Id"], ObjectMap["WebUserId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "WebUser");
            dq.Criterias.Add(ObjectMap["Task"], (int)taskStatus);
            dq.Criterias.Add(ObjectMap["ApplicationStatus"], (int)applicationStatus);
            dq.Criterias.Add(ObjectMap["AccountId"], currentAccount.ID);
            dq.Criterias.Add(ObjectMap["DateCompleted"], DBNull.Value, DatabaseCriteriaOperatorEnum.Is);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).ToList();
        }

        public static SystemMessages Create(DatabaseConnection dc = null)
        {
            return _Create(DatabaseQuery, dc, ObjectMap);
        }
        
        public static SystemMessages Alter(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["WebUserId"]}));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["AccountId"]}));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["Task"]}));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["ApplicationStatus"]}));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["DateSubmitted"]}));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["DateCompleted"]}));

            return msgs;
        }
        
        public SystemMessages Save(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            if (Task.IsEmpty())
                msgs.Add("Task is required on Volunteer Task object", MessageType.Error);
            if (ApplicationStatus.IsEmpty())
                msgs.Add("ApplicationStatus is required on Volunteer Task object", MessageType.Error);

            if (msgs.HasNoErrors())
                msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, nameof(VolunteerTask)));

            return msgs;
        }
    }
}