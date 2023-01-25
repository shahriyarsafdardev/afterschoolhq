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
    public class AvailableTag : BasicIDObject
    {
        public AvailableTag()
        {
            Config(ObjectMap, DatabaseQuery, nameof(AvailableTag), "ID");
            LoadObjectValues(ObjectMap);
            ObjectType = ObjectType.NotSet;
            IsDeleted = false;
        }
        
        public string Tag
        {
            get => (string) MDV["Tag"].Value;
            set => MDV["Tag"].Value = value;
        }
        
        public int AccountId
        {
            get => (int) MDV["AccountId"].Value;
            set => MDV["AccountId"].Value = value;
        }
        
        public ObjectType ObjectType
        {
            get => (ObjectType) MDV["ObjectType"].Value;
            set => MDV["ObjectType"].Value = value;
        }
        
        public bool IsDeleted
        {
            get => (bool) MDV["IsDeleted"].Value;
            set => MDV["IsDeleted"].Value = value;
        }
        
        public static DatabaseParameters ObjectMap
        {
            get
            {
                const string className = nameof(AvailableTag);
                var dp = get_BaseObjectMap(className);
                dp.Add(className, "Tag", SqlDbType.NVarChar , "500");
                dp.Add(className, "AccountId", SqlDbType.Int);
                dp.Add(className, "ObjectType", SqlDbType.Int);
                dp.Add(className, "IsDeleted", SqlDbType.Bit);
                
                return dp;
            }
        }
        
        protected static DatabaseQuery DatabaseQuery
        {
            get
            {
                var dq = get_DatabaseQuery(ObjectMap, nameof(AvailableTag));
                return dq;
            }
        }
        
        public static AvailableTag LoadObject(DataRow row)
        {
            var obj = new AvailableTag();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }
        
        public static List<AvailableTag> Load(Hashtable criteria = null, int? start = null, int? length = null, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return BaseLoad(criteria, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }
        
        public static List<AvailableTag> LoadByObjectType(int objectType, int accountId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ObjectType"], objectType);
            dq.Criterias.Add(ObjectMap["AccountId"], accountId);
            dq.Criterias.Add(ObjectMap["IsDeleted"], false);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }
        
        public static AvailableTag LoadById(int? Id, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], Id);
            dq.Criterias.Add(ObjectMap["IsDeleted"], false);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }
        
        public static bool UniqueTag(string tag, int objectType, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["tag"], tag);
            dq.Criterias.Add(ObjectMap["ObjectType"], objectType);
            return dq.GetCount(msgs) > 0;
        }

        public static SystemMessages Create(DatabaseConnection dc = null)
        {
            return _Create(DatabaseQuery, dc, ObjectMap);
        }
        
        public static SystemMessages Alter(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["Tag"]}));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["AccountId"]}));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["ObjectType"]}));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["IsDeleted"]}));

            return msgs;
        }
        
        public SystemMessages Save(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            
            if (Tag.IsEmpty())
                msgs.Add("Tag is required", MessageType.Error);
            
            if (AccountId.IsEmpty())
                msgs.Add("AccountId is required", MessageType.Error);
            
            if (ObjectType.IsEmpty())
                msgs.Add("Object Type is required", MessageType.Error);

            if (msgs.HasNoErrors())
                msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, nameof(AvailableTag)));

            return msgs;
        }
    }
}