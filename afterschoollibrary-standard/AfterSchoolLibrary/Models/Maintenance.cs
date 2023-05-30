using BasicLibrary;
using System;
using System.Collections.Generic;
using BasicLibrary.DataAccess;
using System.Data;
using System.Linq;
using System.Collections;

namespace AfterSchoolLibrary.Models
{
    internal class Maintenance : BasicObject
    {
        public Maintenance()
        {
            Config(ObjectMap, DatabaseQuery, nameof(Maintenance), "ID");
            LoadObjectValues(ObjectMap);
        }

        public DateTime StartTime
        {
            get => (DateTime) MDV["StartTime"].Value;
            set => MDV["StartTime"].Value = value;
        }
        public DateTime EndTime
        {
            get => (DateTime) MDV["EndTime"].Value;
            set => MDV["EndTime"].Value = value;
        }
        public string Message
        {
            get => (string) MDV["Message"].Value;
            set => MDV["Message"].Value = value;
        }

        public static DatabaseParameters ObjectMap
        {
            get
            {
                const string className = nameof(Maintenance);
                var dp = get_BaseObjectMap(className);
                dp.Add(className, "StartTime", SqlDbType.DateTime);
                dp.Add(className, "EndTime", SqlDbType.DateTime);
                dp.Add(className, "Message", SqlDbType.NVarChar, "500");

                return dp;
            }
        }

        protected static DatabaseQuery DatabaseQuery
        {
            get
            {
                var dq = get_DatabaseQuery(ObjectMap, nameof(Maintenance));
                return dq;
            }
        }

        public static Maintenance LoadObject(DataRow row)
        {
            var obj = new Maintenance();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        public static Maintenance Load(int id, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<Maintenance> Load(Hashtable criteria = null, int? start = null, int? length = null, SystemMessages msgs = null,
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

            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters { ObjectMap["StartTime"] }));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters { ObjectMap["EndTime"] }));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters { ObjectMap["Message"] }));

            return msgs;
        }

        public SystemMessages Save(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            if (StartTime.IsEmpty())
                msgs.Add("Start time is required for maintenance", MessageType.Error);
            if (EndTime.IsEmpty())
                msgs.Add("End time is required for maintenance", MessageType.Error);
            if (Message.IsEmpty())
                msgs.Add("Message is required for maintenance", MessageType.Error);

            if (msgs.HasNoErrors())
                msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, nameof(Maintenance)));

            return msgs;
        }

    }
}
