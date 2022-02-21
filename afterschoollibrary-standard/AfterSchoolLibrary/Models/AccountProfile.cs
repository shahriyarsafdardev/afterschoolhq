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
		public int? LocationId { get { return (int?)MDV["LocationId"].Value; } set { MDV["LocationId"].Value = value; } }
		public string Logo { get { return (string)MDV["Logo"].Value; } set { MDV["Logo"].Value = value; } }
		public string HeaderImage { get { return (string)MDV["HeaderImage"].Value; } set { MDV["HeaderImage"].Value = value; } }
		public string Title { get { return (string)MDV["Title"].Value; } set { MDV["Title"].Value = value; } }
		public string Headline { get { return (string)MDV["Headline"].Value; } set { MDV["Headline"].Value = value; } }
		public string About { get { return (string)MDV["About"].Value; } set { MDV["About"].Value = value; } }
		public string PrimaryColor { get { return (string)MDV["PrimaryColor"].Value; } set { MDV["PrimaryColor"].Value = value; } }
		public string Website { get { return (string)MDV["Website"].Value; } set { MDV["Website"].Value = value; } }
		public int? GradeLevelStart { get { return (int?)MDV["GradeLevelStart"].Value; } set { MDV["GradeLevelStart"].Value = value; } }
		public int? GradeLevelEnd { get { return (int?)MDV["GradeLevelEnd"].Value; } set { MDV["GradeLevelEnd"].Value = value; } }
		public int? AgeStart { get { return (int?)MDV["AgeStart"].Value; } set { MDV["AgeStart"].Value = value; } }
		public int? AgeEnd { get { return (int?)MDV["AgeEnd"].Value; } set { MDV["AgeEnd"].Value = value; } }
		public string Twitter { get { return (string)MDV["Twitter"].Value; } set { MDV["Twitter"].Value = value; } }
		public string Facebook { get { return (string)MDV["Facebook"].Value; } set { MDV["Facebook"].Value = value; } }
		public string Instagram { get { return (string)MDV["Instagram"].Value; } set { MDV["Instagram"].Value = value; } }
		public string LinkedIn { get { return (string)MDV["LinkedIn"].Value; } set { MDV["LinkedIn"].Value = value; } }
		public string YouTube { get { return (string)MDV["YouTube"].Value; } set { MDV["YouTube"].Value = value; } }
		public string CalendarBookingLink { get { return (string)MDV["CalendarBookingLink"].Value; } set { MDV["CalendarBookingLink"].Value = value; } }
		public GenderRestrictionType GenderRestriction { get { return (GenderRestrictionType)MDV["GenderRestriction"].Value; } set { MDV["GenderRestriction"].Value = value; } }
		public string ProfileEmail { get { return (string)MDV["ProfileEmail"].Value; } set { MDV["ProfileEmail"].Value = value; } }
		public string ProfilePhone { get { return (string)MDV["ProfilePhone"].Value; } set { MDV["ProfilePhone"].Value = value; } }
		public bool IsOpenDuringCovid { get { return (bool)MDV["IsOpenDuringCovid"].Value; } set { MDV["IsOpenDuringCovid"].Value = value; } }
		public bool IsELearningCenter { get { return (bool)MDV["IsELearningCenter"].Value; } set { MDV["IsELearningCenter"].Value = value; } }
		public bool IsAHQApproved { get { return (bool)MDV["IsAHQApproved"].Value; } set { MDV["IsAHQApproved"].Value = value; } }
		public bool IsVirtualProgram { get { return (bool)MDV["IsVirtualProgram"].Value; } set { MDV["IsVirtualProgram"].Value = value; } }
		public string RegistrationUrl { get { return (string)MDV["RegistrationUrl"].Value; } set { MDV["RegistrationUrl"].Value = value; } }
		public string ButtonText { get { return (string)MDV["ButtonText"].Value; } set { MDV["ButtonText"].Value = value; } }
		public string VideoUrl { get { return (string)MDV["VideoUrl"].Value; } set { MDV["VideoUrl"].Value = value; } }
		public string AccountName { get { return (string)MDV["AccountName"].Value; } }
		public string AccountUniqueName { get { return (string)MDV["AccountUniqueName"].Value; } }
		public bool AccountIsTest { get { return (bool)MDV["AccountIsTest"].Value; } }
		public bool AccountIsDeleted { get { return (bool)MDV["AccountIsDeleted"].Value; } }
		public bool AccountIsActive { get { return (bool)MDV["AccountIsActive"].Value; } }
		public AccountType AccountAccountTypeId { get { return (AccountType)MDV["AccountAccountTypeId"].Value; } }
		

        public AccountProfile()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(AccountProfile).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.GenderRestriction = GenderRestrictionType.None;
			this.IsOpenDuringCovid = false;
			this.IsELearningCenter = false;
			this.IsAHQApproved = false;
			this.IsVirtualProgram = false;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(AccountProfile).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "AccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "LocationId", System.Data.SqlDbType.Int);
				_dp.Add(className, "Logo", System.Data.SqlDbType.NVarChar , "400");
				_dp.Add(className, "HeaderImage", System.Data.SqlDbType.NVarChar , "400");
				_dp.Add(className, "Title", System.Data.SqlDbType.NVarChar , "400");
				_dp.Add(className, "Headline", System.Data.SqlDbType.NVarChar , "400");
				_dp.Add(className, "About", System.Data.SqlDbType.NVarChar , "4000");
				_dp.Add(className, "PrimaryColor", System.Data.SqlDbType.NVarChar , "20");
				_dp.Add(className, "Website", System.Data.SqlDbType.NVarChar , "400");
				_dp.Add(className, "GradeLevelStart", System.Data.SqlDbType.Int);
				_dp.Add(className, "GradeLevelEnd", System.Data.SqlDbType.Int);
				_dp.Add(className, "AgeStart", System.Data.SqlDbType.Int);
				_dp.Add(className, "AgeEnd", System.Data.SqlDbType.Int);
				_dp.Add(className, "Twitter", System.Data.SqlDbType.NVarChar , "100");
				_dp.Add(className, "Facebook", System.Data.SqlDbType.NVarChar , "100");
				_dp.Add(className, "Instagram", System.Data.SqlDbType.NVarChar , "100");
				_dp.Add(className, "LinkedIn", System.Data.SqlDbType.NVarChar , "100");
				_dp.Add(className, "YouTube", System.Data.SqlDbType.NVarChar , "100");
				_dp.Add(className, "CalendarBookingLink", System.Data.SqlDbType.NVarChar , "100");
				_dp.Add(className, "GenderRestriction", System.Data.SqlDbType.Int);
				_dp.Add(className, "ProfileEmail", System.Data.SqlDbType.NVarChar , "100");
				_dp.Add(className, "ProfilePhone", System.Data.SqlDbType.NVarChar , "20");
				_dp.Add(className, "IsOpenDuringCovid", System.Data.SqlDbType.Bit);
				_dp.Add(className, "IsELearningCenter", System.Data.SqlDbType.Bit);
				_dp.Add(className, "IsAHQApproved", System.Data.SqlDbType.Bit);
				_dp.Add(className, "IsVirtualProgram", System.Data.SqlDbType.Bit);
				_dp.Add(className, "RegistrationUrl", System.Data.SqlDbType.NVarChar , "200");
				_dp.Add(className, "ButtonText", System.Data.SqlDbType.NVarChar , "100");
				_dp.Add(className, "VideoUrl", System.Data.SqlDbType.NVarChar , "200");
				_dp.Add(className, "AccountName", typeof(Account).Name, "Name", System.Data.SqlDbType.NVarChar, "Account");
				_dp.Add(className, "AccountUniqueName", typeof(Account).Name, "UniqueName", System.Data.SqlDbType.NVarChar, "Account");
				_dp.Add(className, "AccountIsTest", typeof(Account).Name, "IsTest", System.Data.SqlDbType.Bit, "Account");
				_dp.Add(className, "AccountIsDeleted", typeof(Account).Name, "IsDeleted", System.Data.SqlDbType.Bit, "Account");
				_dp.Add(className, "AccountIsActive", typeof(Account).Name, "IsActive", System.Data.SqlDbType.Bit, "Account");
				_dp.Add(className, "AccountAccountTypeId", typeof(Account).Name, "AccountTypeId", System.Data.SqlDbType.Int, "Account");
				
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
				dq.Joins.Add(Account.ObjectMap["ID"], AccountProfile.ObjectMap["AccountId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "Account");
				
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
			dps.Add(ObjectMap["LocationId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Logo"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["HeaderImage"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Title"]);
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
			dps.Add(ObjectMap["AgeStart"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AgeEnd"]);
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

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["YouTube"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["CalendarBookingLink"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["GenderRestriction"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ProfileEmail"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ProfilePhone"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsOpenDuringCovid"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsELearningCenter"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsAHQApproved"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsVirtualProgram"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["RegistrationUrl"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ButtonText"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["VideoUrl"]);
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

		public string TwitterUrl
		{
			get
			{
				if (Twitter.IsNotEmpty())
				{
					if (!Twitter.Contains("http"))
						return $"https://twitter.com/{Twitter.TrimStart('@')}";
					
					if (Twitter.Contains("twitter.com"))
						return Twitter;
				}

				return "";
			}
		}

		public string InstagramUrl
		{
			get
			{
				if (Instagram.IsNotEmpty())
				{
					if (!Instagram.Contains("http"))
						return $"https://instagram.com/{Instagram.TrimStart('@')}";

					if (Instagram.Contains("instagram.com"))
						return Instagram;
				}

				return "";
			}
		}

		public static AccountProfile GetLastUpdatedProfile(SystemMessages msgs, DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.SelectRange.Start = 0;
			dq.SelectRange.Length = 1;
			dq.OrderBys.Add(ObjectMap["LastModifiedDate"], OrderByDirectionEnum.Descending);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static int GetCountByAccountId(int accountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], accountId);
			return dq.GetCount(msgs);
		}

		public static List<AccountProfile> LoadList(List<int> ids, SystemMessages msgs = null, DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.UseDistinct = false;
			dq.Criterias.Add(ObjectMap["ID"], ids.ToArray(), BasicLibrary.DataAccess.DatabaseCriteriaOperatorEnum.In);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<AccountProfile> Search(IEnumerable<AccountType> accountTypes, bool? isOpenDuringCovid = null, bool? isELearningCenter = null, IEnumerable<string> neighborhoods = null, IEnumerable<string> cities = null, IEnumerable<string> states = null, int? start = null, int? length = null, bool getRandomSample = false, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.UseParameterizedQuery = false;

			dq.Joins.Add(Location.ObjectMap["ID"], ObjectMap["LocationId"], DatabaseJoinTypeEnum.InnerJoin);

			dq.Criterias.Add(ObjectMap["AccountAccountTypeId"], accountTypes.Select(x => (int)x).ToList(), DatabaseCriteriaOperatorEnum.In);
			dq.Criterias.Add(ObjectMap["AccountIsDeleted"], false);
			dq.Criterias.Add(ObjectMap["AccountIsActive"], true);
			dq.Criterias.Add(ObjectMap["AccountIsTest"], false);
			dq.Criterias.Add(ObjectMap["IsAHQApproved"], true);

			dq.Criterias.Add(ObjectMap["LocationId"], DBNull.Value, DatabaseCriteriaOperatorEnum.IsNot);
			dq.Criterias.Add(ObjectMap["GradeLevelStart"], DBNull.Value, DatabaseCriteriaOperatorEnum.IsNot);
			dq.Criterias.Add(ObjectMap["GradeLevelEnd"], DBNull.Value, DatabaseCriteriaOperatorEnum.IsNot);

			if (isOpenDuringCovid.HasValue)
				dq.Criterias.Add(ObjectMap["isOpenDuringCovid"], isOpenDuringCovid.Value);

			if (isELearningCenter.HasValue)
				dq.Criterias.Add(ObjectMap["isELearningCenter"], isELearningCenter.Value);

			if (neighborhoods.IsNotEmpty())
				dq.Criterias.Add(Location.ObjectMap["neighborhood"], neighborhoods.ToList(), DatabaseCriteriaOperatorEnum.In);

			if (cities.IsNotEmpty())
				dq.Criterias.Add(Location.ObjectMap["city"], cities.ToList(), DatabaseCriteriaOperatorEnum.In);

			if (states.IsNotEmpty())
				dq.Criterias.Add(Location.ObjectMap["state"], states.ToList(), DatabaseCriteriaOperatorEnum.In);

			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;

			if (getRandomSample)
			{
				dq.SelectParameters.Add("RandomSort", "NEWID()");
				dq.OrderBys.Add("RandomSort", OrderByDirectionEnum.Ascending);
			}

			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		/// <summary>
		/// this code is not in use at the moment, I've created it to get data in order of activity closing soon, opening soon.
		/// but later we decided to have old logic and not this one.
		/// </summary>
		/// <param name="accountTypes"></param>
		/// <param name="count"></param>
		/// <param name="msgs"></param>
		/// <param name="dc"></param>
		/// <returns></returns>
		public static List<AccountProfile> Search(IEnumerable<AccountType> accountTypes, int? count, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var dq = DatabaseQuery;
			dq.Connection = dc;

			if (count.IsEmpty())
				count = 10;

			var sql = $@"
				SELECT
					TOP ({count})
					ap.[ID],
					ap.[CreateDate],
					ap.[CreatedBy],
					ap.[LastModifiedDate],
					ap.[LastModifiedBy],
					ap.[AccountId],
					ap.[LocationId],
					ap.[Logo],
					ap.[HeaderImage],
					ap.[Title],
					ap.[Headline],
					ap.[About],
					ap.[PrimaryColor],
					ap.[Website],
					ap.[GradeLevelStart],
					ap.[GradeLevelEnd],
					ap.[Twitter],
					ap.[Facebook],
					ap.[Instagram],
					ap.[LinkedIn],
					ap.[YouTube],
					ap.[CalendarBookingLink],
					ap.[GenderRestriction],
					ap.[ProfileEmail],
					ap.[ProfilePhone],
					ap.[IsOpenDuringCovid],
					ap.[IsELearningCenter],
					ap.[IsAHQApproved],
					ap.[VideoUrl],
					ap.[AgeStart],
					ap.[AgeEnd],
					ap.[IsVirtualProgram]
				FROM	
					Account a 
					INNER JOIN AccountProfile ap ON ap.AccountId = a.ID
					LEFT  JOIN Activity act ON act.AccountId = a.ID AND act.IsDeleted <> 1 AND act.IsPublished = 1 AND act.IsInviteOnly <> 1
					LEFT  JOIN ActivityRegistrationPeriod arp ON arp.ActivityId = act.ID AND arp.IsActive = 1
					LEFT  JOIN RegistrationPeriod rp ON rp.Id = arp.RegistrationPeriodId
				WHERE 
					a.IsDeleted <> 1 AND 
					a.IsTest <> 1 AND 
					a.IsActive = 1 AND 
					ap.IsAHQApproved = 1 AND 
					(rp.EndDateTime IS NULL OR rp.EndDateTime >= GETDATE()) AND --closed activities filter
					a.AccountTypeId in ({ string.Join(",", accountTypes.Select(x => (int)x)) })
				ORDER BY
					(CASE WHEN GETDATE() BETWEEN rp.StartDateTime AND rp.EndDateTime THEN 1 ELSE 2 END), 
					(CASE WHEN rp.EndDateTime IS NULL THEN 2 ELSE 1 END), 
						rp.EndDateTime ASC,
					(CASE WHEN rp.StartDateTime IS NULL THEN 2 ELSE 1 END), 
						rp.StartDateTime DESC, 
					a.Name ASC";

			return dq.ExecuteSelect(sql, msgs).AsEnumerable().ToList()?.ConvertAll(LoadObject)?.DistinctBy(m => m.Title).ToList();
        }
		#endregion
    }
}
