using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Data;
using System.Collections;

namespace AfterSchoolLibrary.Models
{
	/// <summary>
    /// Copy of each instance of a WebSessionObject each time it is changed so that things can be restored to any point in time
    /// </summary>
	[Serializable]
    public class WebSessionObjectSnapshot : BasicObjectWithModified
    {
		#region "ClassGenerator"

		public int? Action { get { return (int?)MDV["Action"].Value; } set { MDV["Action"].Value = value; } }
		public string ObjectBlob { get { return (string)MDV["ObjectBlob"].Value; } set { MDV["ObjectBlob"].Value = value; } }
		public int? ParentObjectSnapshotId { get { return (int?)MDV["ParentObjectSnapshotId"].Value; } set { MDV["ParentObjectSnapshotId"].Value = value; } }
		

        public WebSessionObjectSnapshot()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(WebSessionObjectSnapshot).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(WebSessionObjectSnapshot).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "Action", System.Data.SqlDbType.Int);
				_dp.Add(className, "ObjectBlob", System.Data.SqlDbType.NVarChar , "MAX");
				_dp.Add(className, "ParentObjectSnapshotId", System.Data.SqlDbType.Int);
				
                return _dp;
            }
        }

        public static WebSessionObjectSnapshot LoadObject(System.Data.DataRow row)
        {
            WebSessionObjectSnapshot obj = new WebSessionObjectSnapshot();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(WebSessionObjectSnapshot).Name);
				dq.Joins.Add(WebUser.ObjectMap["ID"], WebSessionObjectSnapshot.ObjectMap["WebUserId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.LeftOuterJoin, "WebUser");
				dq.Joins.Add(WebUser.ObjectMap["ID"], WebSessionObjectSnapshot.ObjectMap["CreatedBy"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.LeftOuterJoin, "CreatedBy");
				
                return dq;
            }
        }

        public static WebSessionObjectSnapshot Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<WebSessionObjectSnapshot> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return WebSessionObjectSnapshot.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return WebSessionObjectSnapshot.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(WebSessionObjectSnapshot).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(WebSessionObjectSnapshot).Name);
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
			dps.Add(ObjectMap["Action"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ObjectBlob"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ParentObjectSnapshotId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public WebSessionObjectSnapshot Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return WebSessionObjectSnapshot.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<WebSessionObjectSnapshot> LoadByWebUserId(string WebUserId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["WebUserId"], WebUserId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<WebSessionObjectSnapshot> LoadByAccountId(string AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		

        #endregion

		#region "UserDefined"

        #endregion
    }
}
