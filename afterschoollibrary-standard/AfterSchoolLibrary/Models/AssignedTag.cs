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
    public class AssignedTag : BasicObject
    {
        public AssignedTag()
        {
            Config(ObjectMap, DatabaseQuery, nameof(AssignedTag), "ID");
            LoadObjectValues(ObjectMap);
            ObjectType = ObjectType.NotSet;
        }
        
        public string Tag
        {
            get => (string) MDV["Tag"].Value;
            set => MDV["Tag"].Value = value;
        }
        
        public int ObjectId
        {
            get => (int) MDV["ObjectId"].Value;
            set => MDV["ObjectId"].Value = value;
        }
        
        public ObjectType ObjectType
        {
            get => (ObjectType) MDV["ObjectType"].Value;
            set => MDV["ObjectType"].Value = value;
        }

        public static DatabaseParameters ObjectMap
        {
            get
            {
                const string className = nameof(AssignedTag);
                var dp = get_BaseObjectMap(className);
                dp.Add(className, "Tag", SqlDbType.NVarChar , "500");
                dp.Add(className, "ObjectId", SqlDbType.Int);
                dp.Add(className, "ObjectType", SqlDbType.Int);
                
                return dp;
            }
        }
        
        protected static DatabaseQuery DatabaseQuery
        {
            get
            {
                var dq = get_DatabaseQuery(ObjectMap, nameof(AssignedTag));
                return dq;
            }
        }
        
        public static AssignedTag LoadObject(DataRow row)
        {
            var obj = new AssignedTag();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }
        
        public static List<AssignedTag> Load(Hashtable criteria = null, int? start = null, int? length = null, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return BaseLoad(criteria, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }
        
        public static List<AssignedTag> LoadByObjectIdByObjectType(int objectId, int objectType, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["objectId"], objectId);
            dq.Criterias.Add(ObjectMap["ObjectType"], objectType);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }
        public static AssignedTag LoadByObjectIdByObjectTypeByTag(int objectId, int objectType, string tag, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["tag"], tag);
            dq.Criterias.Add(ObjectMap["objectId"], objectId);
            dq.Criterias.Add(ObjectMap["ObjectType"], objectType);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }
        
        public static AssignedTag Load(int Id, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], Id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(AssignedTag).Name);
        }

        public static SystemMessages Create(DatabaseConnection dc = null)
        {
            return _Create(DatabaseQuery, dc, ObjectMap);
        }
        
        public static SystemMessages Alter(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["Tag"]}));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["ObjectId"]}));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters {ObjectMap["ObjectType"]}));

            return msgs;
        }
        
        public SystemMessages Save(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            
            if (Tag.IsEmpty())
                msgs.Add("Tag is required", MessageType.Error);
            
            if (ObjectId.IsEmpty())
                msgs.Add("ObjectId is required", MessageType.Error);
            
            if (ObjectType.IsEmpty())
                msgs.Add("Object Type is required", MessageType.Error);

            if (msgs.HasNoErrors())
                msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, nameof(AssignedTag)));

            return msgs;
        }
    }
}