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
	
    public class Tag : BasicObjectWithModified
    {
		#region "ClassGenerator"

		public int? ParentTagId { get { return (int?)MDV["ParentTagId"].Value; } set { MDV["ParentTagId"].Value = value; } }
		public int? AccountId { get { return (int?)MDV["AccountId"].Value; } set { MDV["AccountId"].Value = value; } }
		public string Name { get { return (string)MDV["Name"].Value; } set { MDV["Name"].Value = value; } }
		

        public Tag()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(Tag).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(Tag).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "ParentTagId", System.Data.SqlDbType.Int);
				_dp.Add(className, "AccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "Name", System.Data.SqlDbType.NVarChar, 100);
				
                return _dp;
            }
        }

        protected static Tag LoadObject(System.Data.DataRow row)
        {
            Tag obj = new Tag();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(Tag).Name);
				
                return dq;
            }
        }

        public static Tag Load(int id, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<Tag> Load(Hashtable criterias = null, int? start = null, int? length = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return Tag.BaseLoad(criterias, ObjectMap, dq).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return Tag.BaseGetCount(criterias, ObjectMap, dq);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.Name.IsEmpty())
				msgs.Add("Name is required on Tag object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(Tag).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(Tag).Name);
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
			dps.Add(ObjectMap["ParentTagId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AccountId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Name"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public Tag Reload(BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return Tag.Load(this.ID.Value, dc);
		}
		
		public static Tag LoadByName(string Name, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Name"], Name);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static Tag LoadByNameByAccountId(string Name, int AccountId, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Name"], Name);
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static List<Tag> LoadByParentTagId(int ParentTagId, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ParentTagId"], ParentTagId);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Tag> LoadByAccountId(int AccountId, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Tag> LoadByAccountIdByParentTagId(int AccountId, int ParentTagId, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			dq.Criterias.Add(ObjectMap["ParentTagId"], ParentTagId);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		

        #endregion

		#region "UserDefined"

        #endregion
    }
}
