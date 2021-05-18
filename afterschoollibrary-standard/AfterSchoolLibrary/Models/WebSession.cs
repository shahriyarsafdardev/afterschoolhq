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
    /// keeps track of a user session where a user is hitting our system and taking actions, basically data analytics inside of our system. Not sure if we should keep this, but good for keeping some basic updates of what is happening in the system.
    /// </summary>
	[Serializable]
    public class WebSession : BasicObjectWithModified
    {
		#region "ClassGenerator"

		public string IPAddress { get { return (string)MDV["IPAddress"].Value; } set { MDV["IPAddress"].Value = value; } }
		public string IPAddressTimeZone { get { return (string)MDV["IPAddressTimeZone"].Value; } set { MDV["IPAddressTimeZone"].Value = value; } }
		public string UserAgent { get { return (string)MDV["UserAgent"].Value; } set { MDV["UserAgent"].Value = value; } }
		public string UserLocation { get { return (string)MDV["UserLocation"].Value; } set { MDV["UserLocation"].Value = value; } }
		public string UrlReferrer { get { return (string)MDV["UrlReferrer"].Value; } set { MDV["UrlReferrer"].Value = value; } }
		public int? PreviousId { get { return (int?)MDV["PreviousId"].Value; } set { MDV["PreviousId"].Value = value; } }
		public int? WebUserId { get { return (int?)MDV["WebUserId"].Value; } set { MDV["WebUserId"].Value = value; } }
		public int? AccountId { get { return (int?)MDV["AccountId"].Value; } set { MDV["AccountId"].Value = value; } }
		

        public WebSession()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(WebSession).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(WebSession).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "IPAddress", System.Data.SqlDbType.NVarChar , "50");
				_dp.Add(className, "IPAddressTimeZone", System.Data.SqlDbType.NVarChar , "400");
				_dp.Add(className, "UserAgent", System.Data.SqlDbType.NVarChar , "400");
				_dp.Add(className, "UserLocation", System.Data.SqlDbType.NVarChar , "400");
				_dp.Add(className, "UrlReferrer", System.Data.SqlDbType.NVarChar , "400");
				_dp.Add(className, "PreviousId", System.Data.SqlDbType.Int);
				_dp.Add(className, "WebUserId", System.Data.SqlDbType.Int);
				_dp.Add(className, "AccountId", System.Data.SqlDbType.Int);
				
                return _dp;
            }
        }

        public static WebSession LoadObject(System.Data.DataRow row)
        {
            WebSession obj = new WebSession();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(WebSession).Name);
				dq.Joins.Add(WebUser.ObjectMap["ID"], WebSession.ObjectMap["WebUserId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.LeftOuterJoin, "WebUser");
				dq.Joins.Add(WebUser.ObjectMap["ID"], WebSession.ObjectMap["CreatedBy"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.LeftOuterJoin, "CreatedBy");
				
                return dq;
            }
        }

        public static WebSession Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<WebSession> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return WebSession.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return WebSession.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(WebSession).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(WebSession).Name);
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
			dps.Add(ObjectMap["IPAddress"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IPAddressTimeZone"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["UserAgent"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["UserLocation"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["UrlReferrer"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["PreviousId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["WebUserId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AccountId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public WebSession Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return WebSession.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<WebSession> LoadByWebUserId(int WebUserId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["WebUserId"], WebUserId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<WebSession> LoadByAccountId(int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		

        #endregion

		#region "UserDefined"

		public static DataTable GetLogs(SystemMessages msgs = null, DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;

			var sql = $@"
				SELECT TOP 100
				w.IPAddress, w.UrlReferrer, ws.ActionStringFormat, ws.ActionString 
				FROM WebSession w 
				JOIN WebSessionAction ws ON ws.SessionId = w.ID 
				ORDER BY ws.ID desc";

			return dq.ExecuteSelect(sql, msgs);
		}

		public static WebSession LoadMostRecentByUserId(int webUserId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["WebUserId"], webUserId);
			dq.SelectRange.Start = 0;
			dq.SelectRange.Length = 1;
			dq.OrderBys.Add("CreateDate", OrderByDirectionEnum.Descending);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static WebSession LoadMostRecentByAccountId(int accountId, SystemMessages msgs = null, DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], accountId);
			dq.SelectRange.Start = 0;
			dq.SelectRange.Length = 1;
			dq.OrderBys.Add("CreateDate", OrderByDirectionEnum.Descending);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}



		public static bool DoesTableExists(SystemMessages msgs = null, DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;

			var sql = $@"
				SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'WebSession'";

			var result = dq.ExecuteSelect(sql, msgs);

			if (result != null && result.Rows.Count > 0)
				return true;
			
		return false;
		}

		#endregion
    }
}
