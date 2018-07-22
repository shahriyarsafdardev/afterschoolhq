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
	/// <summary>
    /// An account can have one or more Locations
    /// </summary>
	
    public class Location : BaseLocation
    {
		#region "ClassGenerator"

		public int? HostAccountId { get { return (int?)MDV["HostAccountId"].Value; } set { MDV["HostAccountId"].Value = value; } }
		public bool IsDefault { get { return (bool)MDV["IsDefault"].Value; } set { MDV["IsDefault"].Value = value; } }
		public string Details { get { return (string)MDV["Details"].Value; } set { MDV["Details"].Value = value; } }
		public LocationType LocationTypeId { get { return (LocationType)MDV["LocationTypeId"].Value; } set { MDV["LocationTypeId"].Value = value; } }
		

        public Location()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(Location).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.IsDefault = false;
			this.LocationTypeId = LocationType.NotSet;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(Location).Name;
                _dp = BaseLocation.get_BaseObjectMap(className);
				_dp.Add(className, "HostAccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "IsDefault", System.Data.SqlDbType.Bit);
				_dp.Add(className, "Details", System.Data.SqlDbType.NVarChar, 4000);
				_dp.Add(className, "LocationTypeId", System.Data.SqlDbType.Int);
				
                return _dp;
            }
        }

        protected static Location LoadObject(System.Data.DataRow row)
        {
            Location obj = new Location();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(Location).Name);
				
                return dq;
            }
        }

        public static Location Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<Location> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return Location.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return Location.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.Name.IsEmpty())
				msgs.Add("Name is required on Location object", MessageType.Error);
			if (this.Address1.IsEmpty())
				msgs.Add("Address1 is required on Location object", MessageType.Error);
			if (this.City.IsEmpty())
				msgs.Add("City is required on Location object", MessageType.Error);
			if (this.State.IsEmpty())
				msgs.Add("State is required on Location object", MessageType.Error);
			if (this.PostalCode.IsEmpty())
				msgs.Add("PostalCode is required on Location object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(Location).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(Location).Name);
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
			dps.Add(ObjectMap["IsDefault"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Details"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["LocationTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public Location Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return Location.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<Location> LoadByHostAccountId(int HostAccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["HostAccountId"], HostAccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static Location LoadByHostAccountIdByIsDefault(int HostAccountId, bool IsDefault, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["HostAccountId"], HostAccountId);
			dq.Criterias.Add(ObjectMap["IsDefault"], IsDefault);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static List<Location> LoadByLocationTypeId(LocationType LocationTypeId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["LocationTypeId"], LocationTypeId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		

        #endregion

		#region "UserDefined"


        public static List<Location> Search(string query, int? start, int? length, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.LogicalOperator = LogicalOperatorType.Or;
            dq.Criterias.Add(ObjectMap["Name"], query, DatabaseCriteriaOperatorEnum.Like);
            dq.Criterias.Add(ObjectMap["Address1"], query, DatabaseCriteriaOperatorEnum.LikeRight);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        #endregion
    }
}
