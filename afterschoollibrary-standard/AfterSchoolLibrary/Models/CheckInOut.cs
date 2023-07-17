using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Mime;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Models
{
    [Serializable]
    public class CheckInOut : BasicObjectWithModified
    {
        public CheckInOut()
        {
            Config(ObjectMap, DatabaseQuery, nameof(CheckInOut), "ID");
            LoadObjectValues(ObjectMap);
        }
        public DateTime Date
        {
            get => (DateTime) MDV["Date"].Value;
            set => MDV["Date"].Value = value;
        }

        public string Signature
        {
            get =>  (string) MDV["Signature"].Value;
            set => MDV["Signature"].Value = value;
        }

        public string Browser
        {
            get => (string) MDV["Browser"].Value;
            set => MDV["Browser"].Value = value;
        }

        public string IPAddress
        {
            get => (string) MDV["IPAddress"].Value;
            set => MDV["IPAddress"].Value = value;
        }
        
        public int WebUserId
        {
            get => (int) MDV["WebUserId"].Value;
            set => MDV["WebUserId"].Value = value;
        }
        
        public int AttendanceRegistrantId
        {
            get => (int) MDV["AttendanceRegistrantId"].Value;
            set => MDV["AttendanceRegistrantId"].Value = value;
        }
        
        public CheckType CheckType
        {
            get => (CheckType) MDV["CheckType"].Value;
            set => MDV["CheckType"].Value = value;
        }

        public static DatabaseParameters ObjectMap
        {
            get
            {
                const string className = nameof(CheckInOut);
                var dp = get_BaseObjectMap(className);
                dp.Add(className, "Date", SqlDbType.DateTime);
                dp.Add(className, "Signature", SqlDbType.NVarChar,"1000");
                dp.Add(className, "Browser", SqlDbType.NVarChar,"400");
                dp.Add(className, "IPAddress", SqlDbType.NVarChar,"100");
                dp.Add(className, "WebUserId", SqlDbType.Int);
                dp.Add(className, "AttendanceRegistrantId", SqlDbType.Int);
                dp.Add(className, "CheckType", SqlDbType.Int);
                return dp;
            }
        }

        protected static DatabaseQuery DatabaseQuery
        {
            get
            {
                var dq = get_DatabaseQuery(ObjectMap, nameof(CheckInOut));
                return dq;
            }
        }

        public static CheckInOut LoadObject(DataRow row)
        {
            var obj = new CheckInOut();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        public static CheckInOut Load(int id, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }
        
        public static CheckInOut LoadByAttendanceRegistrantId(int id, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AttendanceRegistrantId"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }
        
        public SystemMessages Delete(DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(CheckInOut).Name);
        }

        public static List<CheckInOut> Load(Hashtable criteria = null, int? start = null, int? length = null, SystemMessages msgs = null,
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

            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters { ObjectMap["Date"] }));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters { ObjectMap["Signature"] }));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters { ObjectMap["Browser"] }));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters { ObjectMap["IPAddress"] }));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters { ObjectMap["WebUserId"] }));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters { ObjectMap["AttendanceRegistrantId"] }));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters { ObjectMap["CheckType"] }));
            return msgs;
        }
        
        public SystemMessages Save(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, nameof(CheckInOut)));
            return msgs;
        }
    }
}