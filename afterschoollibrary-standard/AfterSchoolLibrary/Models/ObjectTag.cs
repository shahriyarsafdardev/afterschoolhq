using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Data;
using System.Collections;

namespace AfterSchoolLibrary
{
	
    public class ObjectTag : BasicObject
    {
		#region "ClassGenerator"

		public int? ActivityId { get { return (int?)MDV["ActivityId"].Value; } set { MDV["ActivityId"].Value = value; } }
		public int? TagId { get { return (int?)MDV["TagId"].Value; } set { MDV["TagId"].Value = value; } }
		public string TagName { get { return (string)MDV["TagName"].Value; } }
		

        public ObjectTag()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(ObjectTag).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(ObjectTag).Name;
                _dp = BasicObject.get_BaseObjectMap(className);
				_dp.Add(className, "ActivityId", System.Data.SqlDbType.Int);
				_dp.Add(className, "TagId", System.Data.SqlDbType.Int);
				_dp.Add(className, "TagName", typeof(Tag).Name, "Name", System.Data.SqlDbType.NVarChar, "Tag");
				
                return _dp;
            }
        }

        protected static ObjectTag LoadObject(System.Data.DataRow row)
        {
            ObjectTag obj = new ObjectTag();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(ObjectTag).Name);
				dq.Joins.Add(Tag.ObjectMap["ID"], ObjectTag.ObjectMap["TagId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "Tag");
				
                return dq;
            }
        }

        public static ObjectTag Load(int id, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<ObjectTag> Load(Hashtable criterias = null, int? start = null, int? length = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return ObjectTag.BaseLoad(criterias, ObjectMap, dq).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return ObjectTag.BaseGetCount(criterias, ObjectMap, dq);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.TagId.IsEmpty())
				msgs.Add("TagId is required on ObjectTag object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(ObjectTag).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(ObjectTag).Name);
        }

        public static SystemMessages Create(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return BaseObject._Create(DatabaseQuery, dc, ObjectMap);
        }

        public static SystemMessages Alter(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            SystemMessages msgs = new SystemMessages();
            BasicLibrary.DataAccess.DatabaseParameters dps;

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ActivityId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["TagId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public ObjectTag Reload(BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return ObjectTag.Load(this.ID.Value, dc);
		}
		
		public static List<ObjectTag> LoadByActivityId(int ActivityId, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<ObjectTag> LoadByTagId(int TagId, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["TagId"], TagId);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		

        #endregion

		#region "UserDefined"

        #endregion
    }
}
