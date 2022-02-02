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
    /// An action taken during a particular user session.
    /// </summary>
	
    public class WebSessionAction : BasicObject
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required.
		/// </summary>
		public int? SessionId { get { return (int?)MDV["SessionId"].Value; } set { MDV["SessionId"].Value = value; } }
		public int? ActorId { get { return (int?)MDV["ActorId"].Value; } set { MDV["ActorId"].Value = value; } }
		public int? AccountId { get { return (int?)MDV["AccountId"].Value; } set { MDV["AccountId"].Value = value; } }
		public WebSessionActionType ActionTypeId { get { return (WebSessionActionType)MDV["ActionTypeId"].Value; } set { MDV["ActionTypeId"].Value = value; } }
		public ObjectType ObjectTypeId { get { return (ObjectType)MDV["ObjectTypeId"].Value; } set { MDV["ObjectTypeId"].Value = value; } }
		public int? ObjectId { get { return (int?)MDV["ObjectId"].Value; } set { MDV["ObjectId"].Value = value; } }
		public int? IndirectObjectId { get { return (int?)MDV["IndirectObjectId"].Value; } set { MDV["IndirectObjectId"].Value = value; } }
		public ObjectType IndirectObjectTypeId { get { return (ObjectType)MDV["IndirectObjectTypeId"].Value; } set { MDV["IndirectObjectTypeId"].Value = value; } }
		public string ActionStringFormat { get { return (string)MDV["ActionStringFormat"].Value; } set { MDV["ActionStringFormat"].Value = value; } }
		public string ActionString { get { return (string)MDV["ActionString"].Value; } set { MDV["ActionString"].Value = value; } }
		public string WebUserUsername { get { return (string)MDV["WebUserUsername"].Value; } }
		public string WebUserEmail { get { return (string)MDV["WebUserEmail"].Value; } }
		public string WebUserFirstName { get { return (string)MDV["WebUserFirstName"].Value; } }
		public string WebUserLastName { get { return (string)MDV["WebUserLastName"].Value; } }
		public string CreatedByUsername { get { return (string)MDV["CreatedByUsername"].Value; } }
		public string CreatedByEmail { get { return (string)MDV["CreatedByEmail"].Value; } }
		public string CreatedByFirstName { get { return (string)MDV["CreatedByFirstName"].Value; } }
		public string CreatedByLastName { get { return (string)MDV["CreatedByLastName"].Value; } }
		public string AccountName { get { return (string)MDV["AccountName"].Value; } }
		public int? AccountAccountTypeId { get { return (int?)MDV["AccountAccountTypeId"].Value; } }
		

        public WebSessionAction()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(WebSessionAction).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.ActionTypeId = WebSessionActionType.NotSet;
			this.ObjectTypeId = ObjectType.NotSet;
			this.IndirectObjectTypeId = ObjectType.NotSet;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(WebSessionAction).Name;
                _dp = BasicObject.get_BaseObjectMap(className);
				_dp.Add(className, "SessionId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ActorId", System.Data.SqlDbType.Int);
				_dp.Add(className, "AccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ActionTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ObjectTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ObjectId", System.Data.SqlDbType.Int);
				_dp.Add(className, "IndirectObjectId", System.Data.SqlDbType.Int);
				_dp.Add(className, "IndirectObjectTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ActionStringFormat", System.Data.SqlDbType.NVarChar , "400");
				_dp.Add(className, "ActionString", System.Data.SqlDbType.NVarChar , "400");
				_dp.Add(className, "WebUserUsername", typeof(WebUser).Name, "Username", System.Data.SqlDbType.NVarChar, "WebUser");
				_dp.Add(className, "WebUserEmail", typeof(WebUser).Name, "Email", System.Data.SqlDbType.NVarChar, "WebUser");
				_dp.Add(className, "WebUserFirstName", typeof(WebUser).Name, "FirstName", System.Data.SqlDbType.NVarChar, "WebUser");
				_dp.Add(className, "WebUserLastName", typeof(WebUser).Name, "LastName", System.Data.SqlDbType.NVarChar, "WebUser");
				_dp.Add(className, "CreatedByUsername", typeof(WebUser).Name, "Username", System.Data.SqlDbType.NVarChar, "CreatedBy");
				_dp.Add(className, "CreatedByEmail", typeof(WebUser).Name, "Email", System.Data.SqlDbType.NVarChar, "CreatedBy");
				_dp.Add(className, "CreatedByFirstName", typeof(WebUser).Name, "FirstName", System.Data.SqlDbType.NVarChar, "CreatedBy");
				_dp.Add(className, "CreatedByLastName", typeof(WebUser).Name, "LastName", System.Data.SqlDbType.NVarChar, "CreatedBy");
				_dp.Add(className, "AccountName", typeof(Account).Name, "Name", System.Data.SqlDbType.NVarChar, "Account");
				_dp.Add(className, "AccountAccountTypeId", typeof(Account).Name, "AccountTypeId", System.Data.SqlDbType.Int, "Account");
				
                return _dp;
            }
        }

        public static WebSessionAction LoadObject(System.Data.DataRow row)
        {
            WebSessionAction obj = new WebSessionAction();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(WebSessionAction).Name);
				dq.Joins.Add(WebUser.ObjectMap["ID"], WebSessionAction.ObjectMap["ActorId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.LeftOuterJoin, "WebUser");
				dq.Joins.Add(WebUser.ObjectMap["ID"], WebSessionAction.ObjectMap["CreatedBy"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.LeftOuterJoin, "CreatedBy");
				dq.Joins.Add(Account.ObjectMap["ID"], WebSessionAction.ObjectMap["AccountId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.LeftOuterJoin, "Account");
				
                return dq;
            }
        }

        public static WebSessionAction Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<WebSessionAction> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return WebSessionAction.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return WebSessionAction.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.SessionId.IsEmpty())
				msgs.Add("SessionId is required on WebSessionAction object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(WebSessionAction).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(WebSessionAction).Name);
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
			dps.Add(ObjectMap["SessionId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ActorId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AccountId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ActionTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ObjectTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ObjectId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IndirectObjectId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IndirectObjectTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ActionStringFormat"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ActionString"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public WebSessionAction Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return WebSessionAction.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<WebSessionAction> LoadById(string Id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Id"], Id);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<WebSessionAction> LoadByWebUserId(string WebUserId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["WebUserId"], WebUserId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<WebSessionAction> LoadByIdByWebUserId(string Id, string WebUserId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Id"], Id);
			dq.Criterias.Add(ObjectMap["WebUserId"], WebUserId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<WebSessionAction> LoadByIdByAccountId(string Id, int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Id"], Id);
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<WebSessionAction> LoadByIdByWebUserIdByAccountId(string Id, string WebUserId, int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Id"], Id);
			dq.Criterias.Add(ObjectMap["WebUserId"], WebUserId);
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<WebSessionAction> LoadByActionTypeId(WebSessionActionType ActionTypeId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActionTypeId"], ActionTypeId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<WebSessionAction> LoadByActionTypeIdByObjectTypeId(WebSessionActionType ActionTypeId, ObjectType ObjectTypeId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActionTypeId"], ActionTypeId);
			dq.Criterias.Add(ObjectMap["ObjectTypeId"], ObjectTypeId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<WebSessionAction> LoadByObjectIdByObjectTypeId(int ObjectId, ObjectType ObjectTypeId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ObjectId"], ObjectId);
			dq.Criterias.Add(ObjectMap["ObjectTypeId"], ObjectTypeId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<WebSessionAction> LoadByObjectIdByActionId(int ObjectId, string ActionId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ObjectId"], ObjectId);
			dq.Criterias.Add(ObjectMap["ActionId"], ActionId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<WebSessionAction> LoadByObjectIdByIndirectObjectId(int ObjectId, int IndirectObjectId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ObjectId"], ObjectId);
			dq.Criterias.Add(ObjectMap["IndirectObjectId"], IndirectObjectId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<WebSessionAction> LoadByIndirectObjectIdByIndirectObjectTypeId(int IndirectObjectId, ObjectType IndirectObjectTypeId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["IndirectObjectId"], IndirectObjectId);
			dq.Criterias.Add(ObjectMap["IndirectObjectTypeId"], IndirectObjectTypeId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<WebSessionAction> LoadByWebUserIdByObjectIdByObjectTypeId(string WebUserId, int ObjectId, ObjectType ObjectTypeId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["WebUserId"], WebUserId);
			dq.Criterias.Add(ObjectMap["ObjectId"], ObjectId);
			dq.Criterias.Add(ObjectMap["ObjectTypeId"], ObjectTypeId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<WebSessionAction> LoadByWebUserIdByObjectIdByObjectTypeIdByIndirectObjectIdByIndirectObjectTypeId(string WebUserId, int ObjectId, ObjectType ObjectTypeId, int IndirectObjectId, ObjectType IndirectObjectTypeId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["WebUserId"], WebUserId);
			dq.Criterias.Add(ObjectMap["ObjectId"], ObjectId);
			dq.Criterias.Add(ObjectMap["ObjectTypeId"], ObjectTypeId);
			dq.Criterias.Add(ObjectMap["IndirectObjectId"], IndirectObjectId);
			dq.Criterias.Add(ObjectMap["IndirectObjectTypeId"], IndirectObjectTypeId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}



		#endregion

		#region "UserDefined"
		public static List<WebSessionAction> LoadByAccountId(int AccountId, int? start, int? length, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);

			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
			dq.OrderBys.Add(ObjectMap["CreateDate"], OrderByDirectionEnum.Descending);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<WebSessionAction> LoadByAccountIdActivityId(int activityId, int AccountId, int? start, int? length, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			dq.Criterias.Add(ObjectMap["ObjectId"], activityId);
			dq.Criterias.Add(ObjectMap["ObjectTypeId"], 10);

			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
			dq.OrderBys.Add(ObjectMap["CreateDate"], OrderByDirectionEnum.Descending);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<WebSessionAction> LoadByActivityGroupId(int groupId, int AccountId, int? start, int? length, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			dq.Criterias.Add(ObjectMap["ObjectId"], groupId);

			dq.Criterias.Add(ObjectMap["ObjectTypeId"], 15);

			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
			dq.OrderBys.Add(ObjectMap["CreateDate"], OrderByDirectionEnum.Descending);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}
		#endregion
	}
}
