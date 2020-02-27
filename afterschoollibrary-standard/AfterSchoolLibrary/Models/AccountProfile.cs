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
    /// holds all the profile and style information for the account
    /// </summary>
	
    public class AccountProfile : BasicObjectWithModified
    {
		#region "ClassGenerator"

		public int? AccountId { get { return (int?)MDV["AccountId"].Value; } set { MDV["AccountId"].Value = value; } }
		public string Logo { get { return (string)MDV["Logo"].Value; } set { MDV["Logo"].Value = value; } }
		public string HeaderImage { get { return (string)MDV["HeaderImage"].Value; } set { MDV["HeaderImage"].Value = value; } }
		public string Headline { get { return (string)MDV["Headline"].Value; } set { MDV["Headline"].Value = value; } }
		public string About { get { return (string)MDV["About"].Value; } set { MDV["About"].Value = value; } }
		public string PrimaryColor { get { return (string)MDV["PrimaryColor"].Value; } set { MDV["PrimaryColor"].Value = value; } }
		public string Website { get { return (string)MDV["Website"].Value; } set { MDV["Website"].Value = value; } }
		public int? GradeLevelStart { get { return (int?)MDV["GradeLevelStart"].Value; } set { MDV["GradeLevelStart"].Value = value; } }
		public int? GradeLevelEnd { get { return (int?)MDV["GradeLevelEnd"].Value; } set { MDV["GradeLevelEnd"].Value = value; } }
		public string Twitter { get { return (string)MDV["Twitter"].Value; } set { MDV["Twitter"].Value = value; } }
		public string Facebook { get { return (string)MDV["Facebook"].Value; } set { MDV["Facebook"].Value = value; } }
		public string Instagram { get { return (string)MDV["Instagram"].Value; } set { MDV["Instagram"].Value = value; } }
		public string LinkedIn { get { return (string)MDV["LinkedIn"].Value; } set { MDV["LinkedIn"].Value = value; } }
		

        public AccountProfile()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(AccountProfile).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(AccountProfile).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "AccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "Logo", System.Data.SqlDbType.NVarChar , "400");
				_dp.Add(className, "HeaderImage", System.Data.SqlDbType.NVarChar , "400");
				_dp.Add(className, "Headline", System.Data.SqlDbType.NVarChar , "400");
				_dp.Add(className, "About", System.Data.SqlDbType.NVarChar , "4000");
				_dp.Add(className, "PrimaryColor", System.Data.SqlDbType.NVarChar , "20");
				_dp.Add(className, "Website", System.Data.SqlDbType.NVarChar , "400");
				_dp.Add(className, "GradeLevelStart", System.Data.SqlDbType.Int);
				_dp.Add(className, "GradeLevelEnd", System.Data.SqlDbType.Int);
				_dp.Add(className, "Twitter", System.Data.SqlDbType.NVarChar , "100");
				_dp.Add(className, "Facebook", System.Data.SqlDbType.NVarChar , "100");
				_dp.Add(className, "Instagram", System.Data.SqlDbType.NVarChar , "100");
				_dp.Add(className, "LinkedIn", System.Data.SqlDbType.NVarChar , "100");
				
                return _dp;
            }
        }

        public static AccountProfile LoadObject(System.Data.DataRow row)
        {
            AccountProfile obj = new AccountProfile();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(AccountProfile).Name);
				
                return dq;
            }
        }

        public static AccountProfile Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<AccountProfile> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return AccountProfile.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return AccountProfile.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(AccountProfile).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(AccountProfile).Name);
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
			dps.Add(ObjectMap["AccountId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Logo"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["HeaderImage"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Headline"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["About"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["PrimaryColor"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Website"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["GradeLevelStart"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["GradeLevelEnd"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Twitter"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Facebook"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Instagram"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["LinkedIn"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public AccountProfile Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return AccountProfile.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static AccountProfile LoadByAccountId(int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		

        #endregion

		#region "UserDefined"

        #endregion
    }
}
