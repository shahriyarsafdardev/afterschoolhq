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
	
    public class AccountRelationship : BasicObjectWithModified
    {
		#region "ClassGenerator"

		public int? HostAccountId { get { return (int?)MDV["HostAccountId"].Value; } set { MDV["HostAccountId"].Value = value; } }
		public int? VisitorAccountId { get { return (int?)MDV["VisitorAccountId"].Value; } set { MDV["VisitorAccountId"].Value = value; } }
		public HostPermissionType HostPermissionTypeId { get { return (HostPermissionType)MDV["HostPermissionTypeId"].Value; } set { MDV["HostPermissionTypeId"].Value = value; } }
		public DateTime? ApprovedDate { get { return (DateTime?)MDV["ApprovedDate"].Value; } set { MDV["ApprovedDate"].Value = value; } }
		public int? ApprovedBy { get { return (int?)MDV["ApprovedBy"].Value; } set { MDV["ApprovedBy"].Value = value; } }
		

        public AccountRelationship()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(AccountRelationship).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.HostPermissionTypeId = HostPermissionType.NotSet;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(AccountRelationship).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "HostAccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "VisitorAccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "HostPermissionTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ApprovedDate", System.Data.SqlDbType.DateTime);
				_dp.Add(className, "ApprovedBy", System.Data.SqlDbType.Int);
				
                return _dp;
            }
        }

        protected static AccountRelationship LoadObject(System.Data.DataRow row)
        {
            AccountRelationship obj = new AccountRelationship();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(AccountRelationship).Name);
				
                return dq;
            }
        }

        public static AccountRelationship Load(int id, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<AccountRelationship> Load(Hashtable criterias = null, int? start = null, int? length = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return AccountRelationship.BaseLoad(criterias, ObjectMap, dq).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return AccountRelationship.BaseGetCount(criterias, ObjectMap, dq);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.HostAccountId.IsEmpty())
				msgs.Add("HostAccountId is required on AccountRelationship object", MessageType.Error);
			if (this.VisitorAccountId.IsEmpty())
				msgs.Add("VisitorAccountId is required on AccountRelationship object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(AccountRelationship).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(AccountRelationship).Name);
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
			dps.Add(ObjectMap["HostAccountId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["VisitorAccountId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["HostPermissionTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ApprovedDate"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ApprovedBy"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public AccountRelationship Reload(BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return AccountRelationship.Load(this.ID.Value, dc);
		}
		
		public static List<AccountRelationship> LoadByHostAccountId(int HostAccountId, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["HostAccountId"], HostAccountId);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<AccountRelationship> LoadByVisitorAccountId(int VisitorAccountId, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["VisitorAccountId"], VisitorAccountId);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static AccountRelationship LoadByHostAccountIdByVisitorAccountId(int HostAccountId, int VisitorAccountId, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["HostAccountId"], HostAccountId);
			dq.Criterias.Add(ObjectMap["VisitorAccountId"], VisitorAccountId);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}


        public static List<AccountRelationship> LoadByApproved( int HostAccountId ,BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["HostAccountId"], HostAccountId);
            dq.Criterias.Add(ObjectMap["HostPermissionTypeId"], HostPermissionType.Approve);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<AccountRelationship> LoadByDenied(int HostAccountId, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["HostAccountId"], HostAccountId);
            dq.Criterias.Add(ObjectMap["HostPermissionTypeId"], HostPermissionType.Deny);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }



        #endregion

        #region "UserDefined"

        #endregion
    }
}
