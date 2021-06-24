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
    /// An account can have one or more Locations
    /// </summary>
	
    public class Location : BaseLocation
    {
		#region "ClassGenerator"

		public int? HostAccountId { get { return (int?)MDV["HostAccountId"].Value; } set { MDV["HostAccountId"].Value = value; } }
		public bool IsDefault { get { return (bool)MDV["IsDefault"].Value; } set { MDV["IsDefault"].Value = value; } }
		public string Details { get { return (string)MDV["Details"].Value; } set { MDV["Details"].Value = value; } }
		public LocationType LocationTypeId { get { return (LocationType)MDV["LocationTypeId"].Value; } set { MDV["LocationTypeId"].Value = value; } }
		public bool IsTest { get { return (bool)MDV["IsTest"].Value; } set { MDV["IsTest"].Value = value; } }
		public string GooglePlaceId { get { return (string)MDV["GooglePlaceId"].Value; } set { MDV["GooglePlaceId"].Value = value; } }
		public string StreetNumber { get { return (string)MDV["StreetNumber"].Value; } set { MDV["StreetNumber"].Value = value; } }
		public string StreetName { get { return (string)MDV["StreetName"].Value; } set { MDV["StreetName"].Value = value; } }
		public string Neighborhood { get { return (string)MDV["Neighborhood"].Value; } set { MDV["Neighborhood"].Value = value; } }
		public string County { get { return (string)MDV["County"].Value; } set { MDV["County"].Value = value; } }
		public decimal? Latitude { get { return (decimal?)MDV["Latitude"].Value; } set { MDV["Latitude"].Value = value; } }
		public decimal? Longitude { get { return (decimal?)MDV["Longitude"].Value; } set { MDV["Longitude"].Value = value; } }
		public decimal? DistanceFromOrigin { get { return (decimal?)MDV["DistanceFromOrigin"].Value; } set { MDV["DistanceFromOrigin"].Value = value; } }
		

        public Location()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(Location).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.IsDefault = false;
			this.LocationTypeId = LocationType.NotSet;
			this.IsTest = false;
			
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
				_dp.Add(className, "Details", System.Data.SqlDbType.NVarChar , "4000");
				_dp.Add(className, "LocationTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "IsTest", System.Data.SqlDbType.Bit);
				_dp.Add(className, "GooglePlaceId", System.Data.SqlDbType.NVarChar , "400");
				_dp.Add(className, "StreetNumber", System.Data.SqlDbType.NVarChar , "50");
				_dp.Add(className, "StreetName", System.Data.SqlDbType.NVarChar , "50");
				_dp.Add(className, "Neighborhood", System.Data.SqlDbType.NVarChar , "100");
				_dp.Add(className, "County", System.Data.SqlDbType.NVarChar , "100");
                _dp.Add(className, "Latitude", System.Data.SqlDbType.Decimal, "10,2");
                _dp.Add(className, "Longitude", System.Data.SqlDbType.Decimal, "10,2");
                _dp.Add(className, "DistanceFromOrigin", System.Data.SqlDbType.Decimal, "10,2");

                return _dp;
            }
        }

        public static Location LoadObject(System.Data.DataRow row)
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

			if (this.State.IsEmpty())
				msgs.Add("State is required on Location object", MessageType.Error);
			
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

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsTest"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["GooglePlaceId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["StreetNumber"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["StreetName"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Neighborhood"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["County"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Latitude"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Longitude"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["DistanceFromOrigin"]);
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

		public static Location LoadByGooglePlaceId(string GooglePlaceId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["GooglePlaceId"], GooglePlaceId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static List<Location> LoadByCityByStateByCountry(string City, string State, string Country, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["City"], City);
			dq.Criterias.Add(ObjectMap["State"], State);
			dq.Criterias.Add(ObjectMap["Country"], Country);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Location> LoadByCountyByStateByCountry(string County, string State, string Country, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["County"], County);
			dq.Criterias.Add(ObjectMap["State"], State);
			dq.Criterias.Add(ObjectMap["Country"], Country);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Location> LoadByStateByCountry(string State, string Country, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["State"], State);
			dq.Criterias.Add(ObjectMap["Country"], Country);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Location> LoadByCountry(string Country, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Country"], Country);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		

        #endregion

		#region "UserDefined"

        public static List<Location> Search(string query, LocationType? locationType, int? start, int? length, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var searchTerms = query.Split(' ').Where(term =>  
                term.IsNotBlank() &&
                term.ToLower().NotIn(Helpers.Utilities.FullTextSearchWordsToIgnore));

            if (searchTerms.Any())
            {
                var dq = DatabaseQuery;
                dq.Connection = dc;

                var sql = $@"
                    select * 
                    from Location
                    where 
                        (
                            Contains(name, '{"\""}{searchTerms.ToStringList("*\" AND \"")}{"*\""}') or
                            Contains(address1,'{"\""}{searchTerms.ToStringList("*\" AND \"")}{"*\""}')
                        ) and IsTest = 0
                ";

                if (locationType.HasValue && locationType.Value != LocationType.NotSet)
                    sql += $@" and LocationTypeId = {(int)locationType.Value}";

                dq.SelectRange.Start = start;
                dq.SelectRange.Length = length;

                return dq.ExecuteSelect(sql, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
            }

            return new List<Location>();
        }

        public static List<Location> LoadByLocationTypeIdByCityByState(LocationType LocationTypeId, string city, string state, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["LocationTypeId"], LocationTypeId);
            dq.Criterias.Add(ObjectMap["City"], city);
            dq.Criterias.Add(ObjectMap["State"], state);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

		public static List<string> LoadAllCities(string county = null, string state = null, string country = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseDistinct = true;

            dq.SelectParameters.Clear();
            dq.SelectParameters.Add(ObjectMap["City"]);

            if (county.IsNotEmpty())
                dq.Criterias.Add(ObjectMap["county"], county);

            if (state.IsNotEmpty())
                dq.Criterias.Add(ObjectMap["state"], state);

            if (country.IsNotEmpty())
                dq.Criterias.Add(ObjectMap["country"], country);

            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(row => { return row[0].ToString(); });
        }

        public static List<string> LoadAllNeighborhoods(string city, string county = null, string state = null, string country = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseDistinct = true;

            dq.SelectParameters.Clear();
            dq.SelectParameters.Add(ObjectMap["Neighborhood"]);

            if (city.IsNotEmpty())
                dq.Criterias.Add(ObjectMap["city"], city);

            if (county.IsNotEmpty())
                dq.Criterias.Add(ObjectMap["county"], county);

            if (state.IsNotEmpty())
                dq.Criterias.Add(ObjectMap["state"], state);

            if (country.IsNotEmpty())
                dq.Criterias.Add(ObjectMap["country"], country);

            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(row => { return row[0].ToString(); });
        }

        public static List<string> LoadNeighborhoodsByHostAccount(int hostAccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;

            var sql = $@"
					SELECT 
						DISTINCT Neighborhood 
					FROM Location
					WHERE Id IN (
						SELECT LocationId FROM AccountProfile 
						WHERE AccountId IN (
							SELECT VisitorAccountId 
							FROM AccountRelationship 
							WHERE HostAccountId = {hostAccountId})
						) AND (COALESCE (Neighborhood, '') <> '')
					ORDER BY Neighborhood ASC";

            return dq.ExecuteSelect(sql, msgs).AsEnumerable().ToList().ConvertAll(row => { return row[0].ToString(); });
		}

        #endregion
    }
}
