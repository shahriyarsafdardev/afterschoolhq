using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Models
{
	/// <summary>
    /// How a registrant is placed in an activity/team.
    /// </summary>
	
    public class RegistrantActivity : BasicObjectWithModified
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required.
		/// </summary>
		public int? RegistrantId { get { return (int?)MDV["RegistrantId"].Value; } set { MDV["RegistrantId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public ParticipationObjectType ParticipationObjectTypeId { get { return (ParticipationObjectType)MDV["ParticipationObjectTypeId"].Value; } set { MDV["ParticipationObjectTypeId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int? ParticipationObjectId { get { return (int?)MDV["ParticipationObjectId"].Value; } set { MDV["ParticipationObjectId"].Value = value; } }
		public int? RegistrationId { get { return (int?)MDV["RegistrationId"].Value; } set { MDV["RegistrationId"].Value = value; } }
		public Guid Uid { get { return (Guid)MDV["Uid"].Value; } set { MDV["Uid"].Value = value; } }
		public int? SeatIndex { get { return (int?)MDV["SeatIndex"].Value; } set { MDV["SeatIndex"].Value = value; } }
		public int? WaitingListSeatIndex { get { return (int?)MDV["WaitingListSeatIndex"].Value; } set { MDV["WaitingListSeatIndex"].Value = value; } }
		public int? ApplicationSeatIndex { get { return (int?)MDV["ApplicationSeatIndex"].Value; } set { MDV["ApplicationSeatIndex"].Value = value; } }
		public int? PreferenceNumber { get { return (int?)MDV["PreferenceNumber"].Value; } set { MDV["PreferenceNumber"].Value = value; } }
		public int? RegistrationPeriodId { get { return (int?)MDV["RegistrationPeriodId"].Value; } set { MDV["RegistrationPeriodId"].Value = value; } }
		public int? ActivityGroupId { get { return (int?)MDV["ActivityGroupId"].Value; } set { MDV["ActivityGroupId"].Value = value; } }
		public int? Rating { get { return (int?)MDV["Rating"].Value; } set { MDV["Rating"].Value = value; } }
		public RegistrationAssignmentType AssignmentTypeId { get { return (RegistrationAssignmentType)MDV["AssignmentTypeId"].Value; } set { MDV["AssignmentTypeId"].Value = value; } }
		public decimal? AmountCharged { get { return (decimal?)MDV["AmountCharged"].Value; } set { MDV["AmountCharged"].Value = value; } }
		public bool? IsDeleted { get { return (bool?)MDV["IsDeleted"].Value; } set { MDV["IsDeleted"].Value = value; } }
		public string RegistrantEmail { get { return (string)MDV["RegistrantEmail"].Value; } }
		public string RegistrantFirstName { get { return (string)MDV["RegistrantFirstName"].Value; } }
		public string RegistrantLastName { get { return (string)MDV["RegistrantLastName"].Value; } }
		public Guid RegistrantUid { get { return (Guid)MDV["RegistrantUid"].Value; } }
		public string RegistrantPhone { get { return (string)MDV["RegistrantPhone"].Value; } }
		public string RegistrantCompany { get { return (string)MDV["RegistrantCompany"].Value; } }
		public RegistrantType RegistrantRegistrantTypeId { get { return (RegistrantType)MDV["RegistrantRegistrantTypeId"].Value; } }
		public int? RegistrantFirstRegistrantId { get { return (int?)MDV["RegistrantFirstRegistrantId"].Value; } }
		

        public RegistrantActivity()
        {
            Config(ObjectMap, DatabaseQuery, typeof(RegistrantActivity).Name, "ID");
            LoadObjectValues(ObjectMap);
			ParticipationObjectTypeId = ParticipationObjectType.NotSet;
			Uid = Guid.NewGuid();
			AssignmentTypeId = RegistrationAssignmentType.NotSet;
			
        }

        public static DatabaseParameters ObjectMap
        {
            get
            {
                DatabaseParameters _dp = new DatabaseParameters();
                string className = typeof(RegistrantActivity).Name;
                _dp = get_BaseObjectMap(className);
				_dp.Add(className, "RegistrantId", SqlDbType.Int);
				_dp.Add(className, "ParticipationObjectTypeId", SqlDbType.Int);
				_dp.Add(className, "ParticipationObjectId", SqlDbType.Int);
				_dp.Add(className, "RegistrationId", SqlDbType.Int);
				_dp.Add(className, "Uid", SqlDbType.UniqueIdentifier);
				_dp.Add(className, "SeatIndex", SqlDbType.Int);
				_dp.Add(className, "WaitingListSeatIndex", SqlDbType.Int);
				_dp.Add(className, "ApplicationSeatIndex", SqlDbType.Int);
				_dp.Add(className, "PreferenceNumber", SqlDbType.Int);
				_dp.Add(className, "RegistrationPeriodId", SqlDbType.Int);
				_dp.Add(className, "ActivityGroupId", SqlDbType.Int);
				_dp.Add(className, "Rating", SqlDbType.Int);
				_dp.Add(className, "AssignmentTypeId", SqlDbType.Int);
				_dp.Add(className, "AmountCharged", SqlDbType.Money);
				_dp.Add(className, "IsDeleted", SqlDbType.Bit);
				_dp.Add(className, "RegistrantEmail", typeof(Registrant).Name, "Email", SqlDbType.NVarChar, "Registrant");
				_dp.Add(className, "RegistrantFirstName", typeof(Registrant).Name, "FirstName", SqlDbType.NVarChar, "Registrant");
				_dp.Add(className, "RegistrantLastName", typeof(Registrant).Name, "LastName", SqlDbType.NVarChar, "Registrant");
				_dp.Add(className, "RegistrantUid", typeof(Registrant).Name, "Uid", SqlDbType.UniqueIdentifier, "Registrant");
				_dp.Add(className, "RegistrantPhone", typeof(Registrant).Name, "Phone", SqlDbType.NVarChar, "Registrant");
				_dp.Add(className, "RegistrantCompany", typeof(Registrant).Name, "Company", SqlDbType.NVarChar, "Registrant");
				_dp.Add(className, "RegistrantRegistrantTypeId", typeof(Registrant).Name, "RegistrantTypeId", SqlDbType.Int, "Registrant");
				_dp.Add(className, "RegistrantFirstRegistrantId", typeof(Registrant).Name, "FirstRegistrantId", SqlDbType.Int, "Registrant");
				
                return _dp;
            }
        }

        public static RegistrantActivity LoadObject(DataRow row)
        {
            RegistrantActivity obj = new RegistrantActivity();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static DatabaseQuery DatabaseQuery
        {
            get
            {
                DatabaseQuery dq = get_DatabaseQuery(ObjectMap, typeof(RegistrantActivity).Name);
				dq.Joins.Add(Registrant.ObjectMap["ID"], ObjectMap["RegistrantId"], DatabaseJoinTypeEnum.InnerJoin, "Registrant");
				
                return dq;
            }
        }

        public static RegistrantActivity Load(int id, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<RegistrantActivity> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (RegistrantId.IsEmpty())
				msgs.Add("RegistrantId is required on RegistrantActivity object", MessageType.Error);
			if (ParticipationObjectTypeId.IsEmpty())
				msgs.Add("ParticipationObjectTypeId is required on RegistrantActivity object", MessageType.Error);
			if (ParticipationObjectId.IsEmpty())
				msgs.Add("ParticipationObjectId is required on RegistrantActivity object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(RegistrantActivity).Name));

			return msgs;
        }

        public SystemMessages Delete(DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(RegistrantActivity).Name);
        }

        public static SystemMessages Create(DatabaseConnection dc = null)
        {
            return _Create(DatabaseQuery, dc, ObjectMap);
        }

        public static SystemMessages Alter(DatabaseConnection dc = null)
        {
            SystemMessages msgs = new SystemMessages();
            DatabaseParameters dps;

			dps = new DatabaseParameters();
			dps.Add(ObjectMap["RegistrantId"]);
			msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

			dps = new DatabaseParameters();
			dps.Add(ObjectMap["ParticipationObjectTypeId"]);
			msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

			dps = new DatabaseParameters();
			dps.Add(ObjectMap["ParticipationObjectId"]);
			msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

			dps = new DatabaseParameters();
			dps.Add(ObjectMap["RegistrationId"]);
			msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

			dps = new DatabaseParameters();
			dps.Add(ObjectMap["Uid"]);
			msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

			dps = new DatabaseParameters();
			dps.Add(ObjectMap["SeatIndex"]);
			msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

			dps = new DatabaseParameters();
			dps.Add(ObjectMap["WaitingListSeatIndex"]);
			msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

			dps = new DatabaseParameters();
			dps.Add(ObjectMap["ApplicationSeatIndex"]);
			msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

			dps = new DatabaseParameters();
			dps.Add(ObjectMap["PreferenceNumber"]);
			msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

			dps = new DatabaseParameters();
			dps.Add(ObjectMap["RegistrationPeriodId"]);
			msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

			dps = new DatabaseParameters();
			dps.Add(ObjectMap["ActivityGroupId"]);
			msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

			dps = new DatabaseParameters();
			dps.Add(ObjectMap["Rating"]);
			msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

			dps = new DatabaseParameters();
			dps.Add(ObjectMap["AssignmentTypeId"]);
			msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

			dps = new DatabaseParameters();
			dps.Add(ObjectMap["AmountCharged"]);
			msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

			dps = new DatabaseParameters();
			dps.Add(ObjectMap["IsDeleted"]);
			msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public RegistrantActivity Reload(SystemMessages msgs = null, DatabaseConnection dc = null)
		{
			return Load(ID.Value, msgs, dc: dc);
		}
		
		public static List<RegistrantActivity> LoadByRegistrantId(int RegistrantId, SystemMessages msgs = null, DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["RegistrantId"], RegistrantId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<RegistrantActivity> LoadByRegistrationId(int RegistrationId, SystemMessages msgs = null, DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["RegistrationId"], RegistrationId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static RegistrantActivity LoadByUid(Guid Uid, SystemMessages msgs = null, DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static List<RegistrantActivity> LoadByParticipationObject(int ParticipationObjectId, ParticipationObjectType ParticipationObjectTypeId, SystemMessages msgs = null, DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ParticipationObjectId"], ParticipationObjectId);
			dq.Criterias.Add(ObjectMap["ParticipationObjectTypeId"], ParticipationObjectTypeId);
			dq.Criterias.Add(ObjectMap["IsDeleted"], false);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static RegistrantActivity LoadByParticipationObjectByRegistrant(int ParticipationObjectId, ParticipationObjectType ParticipationObjectTypeId, int RegistrantId, SystemMessages msgs = null, DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ParticipationObjectId"], ParticipationObjectId);
			dq.Criterias.Add(ObjectMap["ParticipationObjectTypeId"], ParticipationObjectTypeId);
			dq.Criterias.Add(ObjectMap["RegistrantId"], RegistrantId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		

        #endregion

		#region "UserDefined"        

        public string RegistrantName
        {
            get
            {
                if (RegistrantRegistrantTypeId == RegistrantType.Individual)
                    return BaseWebUser.BuildDisplayName(RegistrantFirstName, RegistrantLastName);
                if (RegistrantRegistrantTypeId == RegistrantType.Organization)
	                return RegistrantCompany.ToProperCase();

                return "";
            }
        }

        public int TotalSeats
        {
            get
            {
                return 1;
            }
        }

        public Registrant ToRegistrant()
        {
            return new Registrant
            {
                ID = RegistrantId,
                FirstName = RegistrantFirstName,
                LastName = RegistrantLastName,
                Email = RegistrantEmail,
                Uid = RegistrantUid,
                Phone = RegistrantPhone,
                Company = RegistrantCompany,
                RegistrantTypeId = RegistrantRegistrantTypeId,
                FirstRegistrantId = RegistrantFirstRegistrantId
            };
        }

        public static List<RegistrantActivity> LoadByParticipationObject(int participationObjectId, ParticipationObjectType participationObjectType, int? start, int? length, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["participationObjectId"], participationObjectId);
            dq.Criterias.Add(ObjectMap["participationObjectTypeId"], (int)participationObjectType);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCountByParticipationObject(int participationObjectId, ParticipationObjectType participationObjectType, int AccountId, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["participationObjectId"], participationObjectId);
            dq.Criterias.Add(ObjectMap["participationObjectTypeId"], (int)participationObjectType);
            return dq.GetCount();
        }

        public static bool ExistsByParticipationObjectByRegistrant(int participationObjectId, ParticipationObjectType participationObjectType, int registrantId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["participationObjectId"], participationObjectId);
            dq.Criterias.Add(ObjectMap["participationObjectTypeId"], (int)participationObjectType);
            dq.Criterias.Add(ObjectMap["RegistrantId"], registrantId);
            return dq.GetCount(msgs) > 0;
        }

        public static bool ExistsByParticipationObjectByRegistrants(int participationObjectId, ParticipationObjectType participationObjectType, List<int> registrantIds, List<RegistrationAssignmentType> assignmentTypes, bool? hasParentalConsent, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.Criterias.Add(ObjectMap["participationObjectId"], participationObjectId);
            dq.Criterias.Add(ObjectMap["participationObjectTypeId"], (int)participationObjectType);
            dq.Criterias.Add(ObjectMap["RegistrantId"], registrantIds, DatabaseCriteriaOperatorEnum.In);

            if (assignmentTypes.IsNotEmpty() && assignmentTypes.Count > 0)
                dq.Criterias.Add(ObjectMap["AssignmentTypeId"], assignmentTypes.Select(x => (int)x).ToArray(), DatabaseCriteriaOperatorEnum.In);

            if (hasParentalConsent.HasValue)
            {
                var criteriaOperator = hasParentalConsent.Value ? DatabaseCriteriaOperatorEnum.IsNot : DatabaseCriteriaOperatorEnum.Is;

                dq.Joins.Add(Registration.ObjectMap["ID"], ObjectMap["RegistrationId"], DatabaseJoinTypeEnum.InnerJoin);
                dq.Criterias.Add(Registration.ObjectMap["ESignature"], DBNull.Value, criteriaOperator);
            }

            return dq.GetCount(msgs) > 0;
        }

        public static bool ExistsByParticipationObjectsByRegistrants(List<ParticipationObject> participationObjects, List<int> registrantIds, List<RegistrationAssignmentType> assignmentTypes, bool? hasParentalConsent, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;

            if (participationObjects.Any())
            {
                var dcs1 = new DatabaseCriterias(LogicalOperatorType.Or);

                participationObjects.ForEach(participationObject => {
                    var dcs2 = new DatabaseCriterias(LogicalOperatorType.And);
                    dcs2.Add(ObjectMap["participationObjectId"], participationObject.ObjectID);
                    dcs2.Add(ObjectMap["participationObjectTypeId"], (int)participationObject.ObjectType);
                    dcs1.Add(dcs2);
                });

                dq.Criterias.Add(dcs1);
            }

            dq.Criterias.Add(ObjectMap["RegistrantId"], registrantIds, DatabaseCriteriaOperatorEnum.In);

            if (assignmentTypes.IsNotEmpty() && assignmentTypes.Count > 0)
                dq.Criterias.Add(ObjectMap["AssignmentTypeId"], assignmentTypes.Select(x => (int)x).ToArray(), DatabaseCriteriaOperatorEnum.In);

            if (hasParentalConsent.HasValue)
            {
                var criteriaOperator = hasParentalConsent.Value ? DatabaseCriteriaOperatorEnum.IsNot : DatabaseCriteriaOperatorEnum.Is;

                dq.Joins.Add(Registration.ObjectMap["ID"], ObjectMap["RegistrationId"], DatabaseJoinTypeEnum.InnerJoin);
                dq.Criterias.Add(Registration.ObjectMap["ESignature"], DBNull.Value, criteriaOperator);
            }

            return dq.GetCount(msgs) > 0;
        }

        public static List<RegistrantActivity> LoadByAccountId(int AccountId, int? start = null, int? length = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ActivityAccountId"], AccountId);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetSeatCountByAssignmentType(RegistrationAssignmentType assignmentType, int? accountId = null, int? activityGroupId = null, int? participationObjectId = null, ParticipationObjectType? participationObjectType = null, int? registrationPeriodId = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return GetSeatCountByAssignmentTypes(assignmentType.AsList(), accountId, activityGroupId, participationObjectId, participationObjectType, registrationPeriodId, msgs, dc);
        }

        public static int GetSeatCountByAssignmentTypes(List<RegistrationAssignmentType> assignmentTypes, int? accountId = null, int? activityGroupId = null, int? participationObjectId = null, ParticipationObjectType? participationObjectType = null, int? registrationPeriodId = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;

            if (participationObjectId.HasValue)
            {
                dq.Criterias.Add(ObjectMap["participationObjectId"], participationObjectId.Value);
                dq.Criterias.Add(ObjectMap["participationObjectTypeId"], (int)participationObjectType);
            }

            dq.Criterias.Add(ObjectMap["AssignmentTypeId"], assignmentTypes.Select(x => (int)x).ToList(), DatabaseCriteriaOperatorEnum.In);

            if (accountId.HasValue)
                dq.Criterias.Add(ObjectMap["ActivityAccountId"], accountId.Value);

            if (activityGroupId.HasValue)
                dq.Criterias.Add(ObjectMap["ActivityGroupId"], activityGroupId.Value);

            if (registrationPeriodId.HasValue)
                dq.Criterias.Add(ObjectMap["RegistrationPeriodId"], registrationPeriodId.Value);

            dq.Criterias.Add(new DatabaseCriterias(LogicalOperatorType.Or)
            {
	            { ObjectMap["IsDeleted"], DBNull.Value, DatabaseCriteriaOperatorEnum.Is },
	            { ObjectMap["IsDeleted"], false }
            });
            
            return dq.GetCount(msgs);
        }

        public static int GetSeatCountByParticipationObject(int participationObjectId, ParticipationObjectType participationObjectType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.Criterias.Add(ObjectMap["participationObjectId"], participationObjectId);
            dq.Criterias.Add(ObjectMap["participationObjectTypeId"], (int)participationObjectType);
            dq.Criterias.Add(new DatabaseCriterias(LogicalOperatorType.Or)
            {
	            { ObjectMap["IsDeleted"], DBNull.Value, DatabaseCriteriaOperatorEnum.Is },
	            { ObjectMap["IsDeleted"], false }
            });

            return dq.GetCount();
        }        

        public static int GetSeatCountByRegistrationPeriodId(int RegistrationPeriodId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;

            dq.Criterias.Add(ObjectMap["RegistrationPeriodId"], RegistrationPeriodId);
            dq.Criterias.Add(new DatabaseCriterias(LogicalOperatorType.Or)
            {
	            { ObjectMap["IsDeleted"], DBNull.Value, DatabaseCriteriaOperatorEnum.Is },
	            { ObjectMap["IsDeleted"], false }
            });

            return dq.GetCount();
        }

        public static List<RegistrantActivity> LoadByRegistrationIds(List<int> RegistrationIds, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["RegistrationId"], RegistrationIds.ToArray(), DatabaseCriteriaOperatorEnum.In);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<RegistrantActivity> LoadByRegistrantIds(List<int> registrantIds, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["RegistrantId"], registrantIds.ToArray(), DatabaseCriteriaOperatorEnum.In);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<RegistrantActivity> LoadByRegistrantId(int RegistrantId, List<RegistrationAssignmentType> assignmentTypes, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["RegistrantId"], RegistrantId);
            dq.Criterias.Add(ObjectMap["AssignmentTypeId"], assignmentTypes.Select(x => (int)x).ToArray(), DatabaseCriteriaOperatorEnum.In);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public bool IsForActivity()
        {
            return ParticipationObjectTypeId == ParticipationObjectType.Activity;
        }

        public bool IsForActivityGroup()
        {
            return ParticipationObjectTypeId == ParticipationObjectType.ActivityGroup;
        }

        public bool IsForAccountActivityGroup()
        {
            return ParticipationObjectTypeId == ParticipationObjectType.AccountActivityGroup;
        }

        public int? GetActivityId()
        {
            if (ParticipationObjectTypeId == ParticipationObjectType.Activity)
                return ParticipationObjectId;

            return null;
        }

        public int? GetActivityGroupId()
        {
            if (ParticipationObjectTypeId == ParticipationObjectType.ActivityGroup)
                return ParticipationObjectId;

            return null;
        }

        public int? GetAccountActivityGroupId()
        {
            if (ParticipationObjectTypeId == ParticipationObjectType.AccountActivityGroup)
                return ParticipationObjectId;

            return null;
        }

        #endregion
    }
}
