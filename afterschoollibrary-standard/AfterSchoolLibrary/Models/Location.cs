using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

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
		public new string Neighborhood { get { return (string)MDV["Neighborhood"].Value; } set { MDV["Neighborhood"].Value = value; } }
		public new string County { get { return (string)MDV["County"].Value; } set { MDV["County"].Value = value; } }
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

        public static DatabaseParameters ObjectMap
        {
            get
            {
                var _dp = new DatabaseParameters();
                var className = nameof(Location);
                _dp = get_BaseObjectMap(className);
				_dp.Add(className, "HostAccountId", SqlDbType.Int);
				_dp.Add(className, "IsDefault", SqlDbType.Bit);
				_dp.Add(className, "Details", SqlDbType.NVarChar , "4000");
				_dp.Add(className, "LocationTypeId", SqlDbType.Int);
				_dp.Add(className, "IsTest", SqlDbType.Bit);
				_dp.Add(className, "GooglePlaceId", SqlDbType.NVarChar , "400");
				_dp.Add(className, "StreetNumber", SqlDbType.NVarChar , "50");
				_dp.Add(className, "StreetName", SqlDbType.NVarChar , "50");
				_dp.Add(className, "Neighborhood", SqlDbType.NVarChar , "100");
				_dp.Add(className, "County", SqlDbType.NVarChar , "100");
				_dp.Add(className, "Latitude", SqlDbType.Decimal , "30,5");
				_dp.Add(className, "Longitude", SqlDbType.Decimal , "30,5");
				_dp.Add(className, "DistanceFromOrigin", SqlDbType.Decimal , "30,5");
				
                return _dp;
            }
        }

        public static Location LoadObject(DataRow row)
        {
            var obj = new Location();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static DatabaseQuery DatabaseQuery
        {
            get
            {
                var dq = get_DatabaseQuery(ObjectMap, nameof(Location));
				
                return dq;
            }
        }

        public static Location Load(int id, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<Location> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            return BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.State.IsEmpty())
				msgs.Add("State is required on Location object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(Location).Name));

			return msgs;
        }

        public SystemMessages Delete(DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(Location).Name);
        }

        public static SystemMessages Create(DatabaseConnection dc = null)
        {
            return _Create(DatabaseQuery, dc, ObjectMap);
        }

        public static SystemMessages Alter(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            DatabaseParameters dps;

			dps = new DatabaseParameters();
			dps.Add(ObjectMap["HostAccountId"]);
			msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

			dps = new DatabaseParameters();
			dps.Add(ObjectMap["IsDefault"]);
			msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

			dps = new DatabaseParameters();
			dps.Add(ObjectMap["Details"]);
			msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

			dps = new DatabaseParameters();
			dps.Add(ObjectMap["LocationTypeId"]);
			msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

			dps = new DatabaseParameters();
			dps.Add(ObjectMap["IsTest"]);
			msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

			dps = new DatabaseParameters();
			dps.Add(ObjectMap["GooglePlaceId"]);
			msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

			dps = new DatabaseParameters();
			dps.Add(ObjectMap["StreetNumber"]);
			msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

			dps = new DatabaseParameters();
			dps.Add(ObjectMap["StreetName"]);
			msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

			dps = new DatabaseParameters();
			dps.Add(ObjectMap["Neighborhood"]);
			msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

			dps = new DatabaseParameters();
			dps.Add(ObjectMap["County"]);
			msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

			dps = new DatabaseParameters();
			dps.Add(ObjectMap["Latitude"]);
			msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

			dps = new DatabaseParameters();
			dps.Add(ObjectMap["Longitude"]);
			msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

			dps = new DatabaseParameters();
			dps.Add(ObjectMap["DistanceFromOrigin"]);
			msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            return msgs;
        }

		public Location Reload(SystemMessages msgs = null, DatabaseConnection dc = null)
		{
			return Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<Location> LoadByHostAccountId(int HostAccountId, SystemMessages msgs = null, DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["HostAccountId"], HostAccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static Location LoadByHostAccountIdByIsDefault(int HostAccountId, bool IsDefault, SystemMessages msgs = null, DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["HostAccountId"], HostAccountId);
			dq.Criterias.Add(ObjectMap["IsDefault"], IsDefault);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static List<Location> LoadByLocationTypeId(LocationType LocationTypeId, SystemMessages msgs = null, DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["LocationTypeId"], LocationTypeId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static Location LoadByGooglePlaceId(string GooglePlaceId, SystemMessages msgs = null, DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["GooglePlaceId"], GooglePlaceId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static List<Location> LoadByCityByStateByCountry(string City, string State, string Country, SystemMessages msgs = null, DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["City"], City);
			dq.Criterias.Add(ObjectMap["State"], State);
			dq.Criterias.Add(ObjectMap["Country"], Country);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Location> LoadByCountyByStateByCountry(string County, string State, string Country, SystemMessages msgs = null, DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["County"], County);
			dq.Criterias.Add(ObjectMap["State"], State);
			dq.Criterias.Add(ObjectMap["Country"], Country);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Location> LoadByStateByCountry(string State, string Country, SystemMessages msgs = null, DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["State"], State);
			dq.Criterias.Add(ObjectMap["Country"], Country);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Location> LoadByCountry(string Country, SystemMessages msgs = null, DatabaseConnection dc = null)
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
                term.ToLower().NotIn(Utilities.FullTextSearchWordsToIgnore));

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

        public static List<Location> LoadByLocationTypeIdByCityByState(LocationType LocationTypeId, string city, string state, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["LocationTypeId"], LocationTypeId);
            dq.Criterias.Add(ObjectMap["City"], city);
            dq.Criterias.Add(ObjectMap["State"], state);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

		public static List<string> LoadAllCities(string county = null, string state = null, string country = null, SystemMessages msgs = null, DatabaseConnection dc = null)
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

        public static List<string> LoadAllNeighborhoods(string city, string county = null, string state = null, string country = null, SystemMessages msgs = null, DatabaseConnection dc = null)
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

        public static List<string> LoadNeighborhoodsByHostAccount(int hostAccountId, SystemMessages msgs = null, DatabaseConnection dc = null)
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
