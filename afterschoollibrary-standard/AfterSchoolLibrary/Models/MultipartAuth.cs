using BasicLibrary;
using System;
using System.Collections.Generic;
using System.Text;
using BasicLibrary.DataAccess;
using System.Data;
using System.Linq;
using System.Collections;

namespace AfterSchoolLibrary.Models
{
    public class MultipartAuth : BasicObjectWithModified
    {
        public MultipartAuth()
        {
            Config(ObjectMap, DatabaseQuery, nameof(MultipartAuth), "ID");
            LoadObjectValues(ObjectMap);
        }

        public int Code
        {
            get => (int) MDV["Code"].Value;
            set => MDV["Code"].Value = value;
        }

        public int? WebUserId
        {
            get => (int?) MDV["WebUserId"].Value;
            set => MDV["WebUserId"].Value = value;
        }

        public DateTime ExpirationDate
        {
            get => (DateTime) MDV["ExpirationDate"].Value;
            set => MDV["ExpirationDate"].Value = value;
        }


        public static DatabaseParameters ObjectMap
        {
            get
            {
                const string className = nameof(MultipartAuth);
                var dp = get_BaseObjectMap(className);
                dp.Add(className, "Code", SqlDbType.Int);
                dp.Add(className, "WebUserId", SqlDbType.Int);
                dp.Add(className, "ExpirationDate", SqlDbType.DateTime);

                return dp;
            }
        }

        protected static DatabaseQuery DatabaseQuery
        {
            get
            {
                var dq = get_DatabaseQuery(ObjectMap, nameof(MultipartAuth));
                return dq;
            }
        }

        public static MultipartAuth LoadObject(DataRow row)
        {
            var obj = new MultipartAuth();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        public static MultipartAuth Load(int id, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<MultipartAuth> Load(Hashtable criteria = null, int? start = null, int? length = null, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return BaseLoad(criteria, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static MultipartAuth LoadLatestCodeByWebUserId(int? Id, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.Criterias.Add(ObjectMap["WebUserId"], Id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).LastOrDefault();
        }

        public static List<MultipartAuth> LoadCodesListByWebUserId(int? Id, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.Criterias.Add(ObjectMap["WebUserId"], Id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }
        public static SystemMessages Create(DatabaseConnection dc = null)
        {
            return _Create(DatabaseQuery, dc, ObjectMap);
        }

        public static SystemMessages Alter(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters { ObjectMap["Code"] }));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters { ObjectMap["WebUserId"] }));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters { ObjectMap["ExpirationDate"] }));

            return msgs;
        }

        public SystemMessages Save(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            if (Code.IsEmpty())
                msgs.Add("Code is required on Multipart Auth", MessageType.Error);
            if (WebUserId.IsEmpty())
                msgs.Add("WebUserId is required on Multipart Auth", MessageType.Error);
            if (ExpirationDate.IsEmpty())
                msgs.Add("Expiration Date is required on Multipart Auth", MessageType.Error);

            if (msgs.HasNoErrors())
                msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, nameof(MultipartAuth)));

            return msgs;
        }

    }
}
