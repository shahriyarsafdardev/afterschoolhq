using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AfterSchoolLibrary.Enums;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;

namespace AfterSchoolLibrary.Models
{
    [Serializable]
    public class VolunteerApproval : BasicObjectWithModified
    {
        public VolunteerApproval()
        {
            Config(ObjectMap, DatabaseQuery, nameof(VolunteerApproval), "ID");
            LoadObjectValues(ObjectMap);
            ApprovalStatusId = ApprovalStatus.NotSet;
        }
        
        public int? WebUserId
        {
            get => (int) MDV["WebUserId"].Value;
            set => MDV["WebUserId"].Value = value;
        }
        
        public int? AccountId
        {
            get => (int) MDV["AccountId"].Value;
            set => MDV["AccountId"].Value = value;
        }
        
        public ApprovalStatus ApprovalStatusId
        {
            get => (ApprovalStatus) MDV["ApprovalStatusId"].Value;
            set => MDV["ApprovalStatusId"].Value = value;
        }
        
        public string Reason
        {
            get => (string) MDV["Reason"].Value;
            set => MDV["Reason"].Value = value;
        }
        
        public DateTime? ExpirationDate
        {
            get => (DateTime?) MDV["ExpirationDate"].Value;
            set => MDV["ExpirationDate"].Value = value;
        }
        
        public bool? IsActive
        {
            get => (bool?) MDV["IsActive"].Value;
            set => MDV["IsActive"].Value = value;
        }
        
        public static DatabaseParameters ObjectMap
        {
            get
            {
                const string className = nameof(VolunteerApproval);
                var dp = get_BaseObjectMap(className);
                dp.Add(className, "WebUserId", SqlDbType.Int);
                dp.Add(className, "AccountId", SqlDbType.Int);
                dp.Add(className, "ApprovalStatusId", SqlDbType.Int);
                dp.Add(className, "Reason", SqlDbType.NVarChar, "100");
                dp.Add(className, "ExpirationDate", SqlDbType.DateTime);
                dp.Add(className, "IsActive", SqlDbType.Bit);
                return dp;
            }
        }
        
        protected static DatabaseQuery DatabaseQuery
        {
            get
            {
                var dq = get_DatabaseQuery(ObjectMap, nameof(VolunteerApproval));
                return dq;
            }
        }
        
        public static VolunteerApproval LoadObject(DataRow row)
        {
            var obj = new VolunteerApproval();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }
        
        public static List<VolunteerApproval> Load(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).ToList();
        }
        
        public SystemMessages Save(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            if (msgs.HasNoErrors())
                msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, nameof(VolunteerApproval)));

            return msgs;
        }

        public SystemMessages Delete(DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, nameof(VolunteerApproval));
        }

        public static SystemMessages Create(DatabaseConnection dc = null)
        {
            return _Create(DatabaseQuery, dc, ObjectMap);
        }
        
        public static List<VolunteerApproval> LoadByWebUserId(int webUserId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["WebUserId"], webUserId);
            dq.Criterias.Add(ObjectMap["IsActive"], true.ToInteger());
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).ToList();
        }
        
        public static SystemMessages Alter(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["WebUserId"]}));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["AccountId"]}));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["ApprovalStatusId"]}));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["Reason"]}));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["ExpirationDate"]}));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["IsActive"]}));

            return msgs;
        }
        
    }
}