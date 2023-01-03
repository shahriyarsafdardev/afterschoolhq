using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AfterSchoolLibrary.Enums;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Microsoft.Identity.Client;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchoolLibrary.Models
{
    /// <summary>
    ///     A single registrant for an activity, this is the main person who registered
    /// </summary>
    [Serializable]
    public class Registrant : BaseContact
    {
        #region "ClassGenerator"

        public int? AccountId
        {
            get => (int?) MDV["AccountId"].Value;
            set => MDV["AccountId"].Value = value;
        }

        public int? WebUserId
        {
            get => (int?) MDV["WebUserId"].Value;
            set => MDV["WebUserId"].Value = value;
        }

        /// <summary>
        ///     Required.
        /// </summary>
        public Guid Uid
        {
            get => (Guid) MDV["Uid"].Value;
            set => MDV["Uid"].Value = value;
        }

        /// <summary>
        ///     Required.
        /// </summary>
        public RegistrantType RegistrantTypeId
        {
            get => (RegistrantType) MDV["RegistrantTypeId"].Value;
            set => MDV["RegistrantTypeId"].Value = value;
        }

        public int? ParentRegistrantId
        {
            get => (int?) MDV["ParentRegistrantId"].Value;
            set => MDV["ParentRegistrantId"].Value = value;
        }

        public bool IsMaster
        {
            get => (bool) MDV["IsMaster"].Value;
            set => MDV["IsMaster"].Value = value;
        }

        public bool IsContactOnly
        {
            get => (bool) MDV["IsContactOnly"].Value;
            set => MDV["IsContactOnly"].Value = value;
        }

        public int? FirstRegistrantId
        {
            get => (int?) MDV["FirstRegistrantId"].Value;
            set => MDV["FirstRegistrantId"].Value = value;
        }

        public bool IsParentOrGuardian
        {
            get => (bool) MDV["IsParentOrGuardian"].Value;
            set => MDV["IsParentOrGuardian"].Value = value;
        }

        public bool AllowTextMessages
        {
            get => (bool) MDV["AllowTextMessages"].Value;
            set => MDV["AllowTextMessages"].Value = value;
        }

        /// <summary>
        ///     The location id of the school the registrant is attending
        /// </summary>
        public int? SchoolLocationId
        {
            get => (int?) MDV["SchoolLocationId"].Value;
            set => MDV["SchoolLocationId"].Value = value;
        }

        /// <summary>
        ///     The status of the profile - completed
        /// </summary>
        public RegistrantProfileStatus ProfileStatusId
        {
            get => (RegistrantProfileStatus) MDV["ProfileStatusId"].Value;
            set => MDV["ProfileStatusId"].Value = value;
        }

        public RegistrantPersonType PersonTypeId
        {
            get => (RegistrantPersonType) MDV["PersonTypeId"].Value;
            set => MDV["PersonTypeId"].Value = value;
        }

        public bool IsDeleted
        {
            get => (bool) MDV["IsDeleted"].Value;
            set => MDV["IsDeleted"].Value = value;
        }

        public bool FormDataFixProcessed
        {
            get => (bool) MDV["FormDataFixProcessed"].Value;
            set => MDV["FormDataFixProcessed"].Value = value;
        }


        public Registrant()
        {
            Config(ObjectMap, DatabaseQuery, typeof(Registrant).Name, "ID");
            LoadObjectValues(ObjectMap);
            Uid = Guid.NewGuid();
            RegistrantTypeId = RegistrantType.NotSet;
            IsMaster = false;
            IsContactOnly = false;
            IsParentOrGuardian = false;
            AllowTextMessages = false;
            ProfileStatusId = RegistrantProfileStatus.NotSet;
            IsDeleted = false;
            PersonTypeId = RegistrantPersonType.NotSet;
            FormDataFixProcessed = false;
        }

        public static DatabaseParameters ObjectMap
        {
            get
            {
                var _dp = new DatabaseParameters();
                var className = typeof(Registrant).Name;
                _dp = get_BaseObjectMap(className);
                _dp.Add(className, "AccountId", SqlDbType.Int);
                _dp.Add(className, "WebUserId", SqlDbType.Int);
                _dp.Add(className, "Uid", SqlDbType.UniqueIdentifier);
                _dp.Add(className, "RegistrantTypeId", SqlDbType.Int);
                _dp.Add(className, "ParentRegistrantId", SqlDbType.Int);
                _dp.Add(className, "IsMaster", SqlDbType.Bit);
                _dp.Add(className, "IsContactOnly", SqlDbType.Bit);
                _dp.Add(className, "FirstRegistrantId", SqlDbType.Int);
                _dp.Add(className, "IsParentOrGuardian", SqlDbType.Bit);
                _dp.Add(className, "AllowTextMessages", SqlDbType.Bit);
                _dp.Add(className, "SchoolLocationId", SqlDbType.Int);
                _dp.Add(className, "ProfileStatusId", SqlDbType.Int);
                _dp.Add(className, "IsDeleted", SqlDbType.Bit);
                _dp.Add(className, "PersonTypeId", SqlDbType.Int);
                _dp.Add(className, "FormDataFixProcessed", SqlDbType.Bit);

                return _dp;
            }
        }

        public static Registrant LoadObject(DataRow row)
        {
            var obj = new Registrant();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static DatabaseQuery DatabaseQuery
        {
            get
            {
                var dq = get_DatabaseQuery(ObjectMap, typeof(Registrant).Name);

                return dq;
            }
        }

        public static Registrant Load(int id, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<Registrant> Load(Hashtable criterias = null, List<string> orderBys = null, int? start = null, int? length = null, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
           
            return BaseLoad(criterias, orderBys, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
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

            if (Uid.IsEmpty())
                msgs.Add("Uid is required on Registrant object", MessageType.Error);
            if (RegistrantTypeId.IsEmpty())
                msgs.Add("RegistrantTypeId is required on Registrant object", MessageType.Error);

            if (msgs.HasNoErrors())
                msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(Registrant).Name));

            return msgs;
        }

        public SystemMessages Delete(DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(Registrant).Name);
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
            dps.Add(ObjectMap["AccountId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["WebUserId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["Uid"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["RegistrantTypeId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["ParentRegistrantId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["IsMaster"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["IsContactOnly"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["FirstRegistrantId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["IsParentOrGuardian"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["AllowTextMessages"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["SchoolLocationId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["ProfileStatusId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["IsDeleted"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["PersonTypeId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["FormDataFixProcessed"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));


            return msgs;
        }

        public Registrant Reload(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Load(ID.Value, msgs, dc);
        }

        public static List<Registrant> LoadByAccountIdByIsDeleted(int AccountId, bool IsDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.Criterias.Add(ObjectMap["IsDeleted"], IsDeleted);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Registrant> LoadByParentRegistrantId(int ParentRegistrantId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ParentRegistrantId"], ParentRegistrantId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Registrant> LoadByAccountIdByRegistrantTypeIdByIsDeleted(int AccountId, RegistrantType RegistrantTypeId, bool IsDeleted,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.Criterias.Add(ObjectMap["RegistrantTypeId"], RegistrantTypeId);
            dq.Criterias.Add(ObjectMap["IsDeleted"], IsDeleted);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Registrant> LoadByAccountIdByIsContactOnlyByIsDeleted(int AccountId, bool IsContactOnly, bool IsDeleted, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.Criterias.Add(ObjectMap["IsContactOnly"], IsContactOnly);
            dq.Criterias.Add(ObjectMap["IsDeleted"], IsDeleted);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static Registrant LoadByUid(Guid Uid, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["Uid"], Uid);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static Registrant LoadByUidByAccountId(Guid Uid, int AccountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["Uid"], Uid);
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<Registrant> LoadByWebUserIdByIsDeleted(int WebUserId, bool IsDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["WebUserId"], WebUserId);
            dq.Criterias.Add(ObjectMap["IsDeleted"], IsDeleted);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Registrant> LoadByWebUserIdByIsMasterByIsDeleted(int WebUserId, bool IsMaster, bool IsDeleted, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["WebUserId"], WebUserId);
            dq.Criterias.Add(ObjectMap["IsMaster"], IsMaster);
            dq.Criterias.Add(ObjectMap["IsDeleted"], IsDeleted);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Registrant> LoadByFirstRegistrantId(int FirstRegistrantId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["FirstRegistrantId"], FirstRegistrantId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Registrant> LoadByParentRegistrantIdByAccountId(int ParentRegistrantId, int AccountId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ParentRegistrantId"], ParentRegistrantId);
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        #endregion

        #region "UserDefined"

        public bool IsProfileCompleted => ProfileStatusId == RegistrantProfileStatus.Completed;

        public bool IsVolunteer => PersonTypeId == RegistrantPersonType.Volunteer;


        public bool IsStudent => PersonTypeId == RegistrantPersonType.Student;

        public string Name => BuildDisplayName(FirstName, LastName);

        public List<FormQuestionAnswer> GetAnswers(int activityId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return FormQuestionAnswer.LoadByRegistrantIdByActivityId(ID.Value, activityId, dc: dc)
                .OrderBy(x => x.FormQuestionDisplayOrder)
                .ToList();
        }

        public List<FormQuestionAnswer> GetAnswers(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return FormQuestionAnswer.LoadByRegistrantId(ID.Value, msgs, dc)
                .OrderBy(x => x.FormQuestionDisplayOrder)
                .ToList();
        }

        public FormQuestionAnswer GetAnswer(QuestionDataType questionDataType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return FormQuestionAnswer.Load(
                    new Hashtable {{"QuestionDataTypeId", (int) questionDataType}, {"RegistrantId", ID.Value}},
                    msgs: msgs,
                    dc: dc)
                .FirstOrDefault();
        }

        public string GetValue(QuestionDataType questionDataType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var s = GetAnswer(questionDataType, msgs, dc)?.Text.IfEmpty("-");
            if (s.IsNotEmpty() && Utilities.IsAlreadyJsonString(s))
                return Utilities.GetKeyValuePairs(s).FirstOrDefault()?.value;

            return s;
        }

        public static List<Registrant> LoadList(List<int> ids, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], ids, DatabaseCriteriaOperatorEnum.In);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public List<RegistrantActivity> GetRegistrantActivities(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RegistrantActivity.LoadByRegistrantId(ID.Value, msgs, dc).ToList();
        }

        public List<RegistrantActivity> GetRegistrantActivities(List<RegistrationAssignmentType> assignmentTypes, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return RegistrantActivity.LoadByRegistrantId(ID.Value, assignmentTypes, msgs, dc).ToList();
        }

        private static DatabaseQuery SearchRegistrantsQuery(string searchText, int? contactListId, int accountId, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], accountId);

            if (contactListId.HasValue)
            {
                var contactListQuery = get_DatabaseQuery(ObjectMap, typeof(ContactListRegistrant).Name);
                contactListQuery.UseParameterizedQuery = false;
                contactListQuery.SelectParameters.Clear();
                contactListQuery.SelectParameters.Add(ContactListRegistrant.ObjectMap["RegistrantId"]);
                contactListQuery.Criterias.Add(ContactListRegistrant.ObjectMap["contactListId"], contactListId.Value);

                dq.Criterias.Add(ObjectMap["Id"], contactListQuery, DatabaseCriteriaOperatorEnum.In);
            }

            if (searchText.IsNotEmpty())
            {
                var itemList = new List<string>();

                // split list
                itemList.AddRange(searchText.Split(' ').Select(x => x.Trim()).Where(x => x.IsNotBlank()));

                var dcsAnd = new DatabaseCriterias(LogicalOperatorType.And);

                foreach (var item in itemList)
                {
                    var dcs = new DatabaseCriterias(LogicalOperatorType.Or);
                    dcs.Add(ObjectMap["FirstName"], item, DatabaseCriteriaOperatorEnum.Like);
                    dcs.Add(ObjectMap["LastName"], item, DatabaseCriteriaOperatorEnum.Like);
                    dcs.Add(ObjectMap["Email"], item, DatabaseCriteriaOperatorEnum.Like);
                    dcsAnd.Add(dcs);
                }

                dq.Criterias.Add(dcsAnd);
            }

            dq.OrderBys.Add(ObjectMap["FirstName"], OrderByDirectionEnum.Ascending);
            dq.OrderBys.Add(ObjectMap["LastName"], OrderByDirectionEnum.Ascending);

            return dq;
        }

        public static int SearchRegistrantCount(string searchText, int? contactListId, int accountId, DatabaseConnection dc = null)
        {
            return SearchRegistrantsQuery(searchText, contactListId, accountId, dc).GetCount();
        }

        public static List<Registrant> SearchRegistrants(string searchText, int? contactListId, int accountId, int? start, int? length,
            DatabaseConnection dc = null)
        {
            var dq = SearchRegistrantsQuery(searchText, contactListId, accountId, dc);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Registrant> LoadByAccountIdByIsDeleted(int AccountId, bool isDeleted, int? start, int? length, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.Criterias.Add(ObjectMap["IsDeleted"], isDeleted);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCountByAccountId(int AccountId, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            return dq.GetCount();
        }

        public static Registrant LoadByNameByEmail(int accountId, string firstName, string lastName, string email, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["accountId"], accountId);
            dq.Criterias.Add(ObjectMap["firstName"], firstName);
            dq.Criterias.Add(ObjectMap["lastName"], lastName);
            dq.Criterias.Add(ObjectMap["email"], email);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static Registrant LoadByEmailOnly(int accountId, string email, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;

            dq.Criterias.Add(ObjectMap["accountId"], accountId);
            dq.Criterias.Add(ObjectMap["email"], email);

            var dcs1 = new DatabaseCriterias(LogicalOperatorType.Or);
            dcs1.Add(ObjectMap["firstname"], DBNull.Value, DatabaseCriteriaOperatorEnum.Is);
            dcs1.Add(ObjectMap["firstname"], "");
            dq.Criterias.Add(dcs1);

            var dcs2 = new DatabaseCriterias(LogicalOperatorType.Or);
            dcs2.Add(ObjectMap["lastname"], DBNull.Value, DatabaseCriteriaOperatorEnum.Is);
            dcs2.Add(ObjectMap["lastname"], "");
            dq.Criterias.Add(dcs2);

            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }


        public static List<Registrant> LoadFirstRegistrants(int accountId, int? start, int? length, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.Criterias.Add(ObjectMap["AccountId"], accountId);
            dq.Criterias.Add(ObjectMap["FirstRegistrantId"], DBNull.Value, DatabaseCriteriaOperatorEnum.Is);
            dq.OrderBys.Add(ObjectMap["FirstName"], OrderByDirectionEnum.Ascending);
            dq.OrderBys.Add(ObjectMap["LastName"], OrderByDirectionEnum.Ascending);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public SystemMessages FindAndSetFirstRegistrant(WebUser loggedInUser, DatabaseConnection dc = null)
        {
            return FindAndSetFirstRegistrant(FirstName, LastName, Email, loggedInUser, dc);
        }

        public SystemMessages FindAndSetFirstRegistrant(string firstName, string lastName, string email, WebUser loggedInUser, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.Criterias.Add(ObjectMap["FirstRegistrantId"], DBNull.Value, DatabaseCriteriaOperatorEnum.Is);
            dq.Criterias.Add(ObjectMap["FirstName"], firstName);
            dq.Criterias.Add(ObjectMap["LastName"], lastName);
            dq.Criterias.Add(ObjectMap["Email"], email);
            dq.Criterias.Add(ObjectMap["ID"], ID.Value, DatabaseCriteriaOperatorEnum.NotEqualTo);
            dq.OrderBys.Add(ObjectMap["Id"], OrderByDirectionEnum.Ascending);

            var firstRegistrant = dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();

            if (firstRegistrant.IsNotEmpty())
            {
                FirstRegistrantId = firstRegistrant.ID;
                LastModifiedBy = loggedInUser?.ID.IfEmpty(0);
                LastModifiedDate = DateTime.UtcNow;
                msgs.AddRange(Save(dc));
            }

            return msgs;
        }

        public static List<Registrant> FindRelatedRegistrants(int AccountId, string firstName, string lastName, string email, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.Criterias.Add(ObjectMap["FirstName"], firstName);
            dq.Criterias.Add(ObjectMap["LastName"], lastName);
            dq.Criterias.Add(ObjectMap["Email"], email);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Registrant> LoadMastersNotRecentlyUpdated(DateTime lastUpdatedDate, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["IsMaster"], true);
            dq.Criterias.Add(ObjectMap["LastModifiedDate"], lastUpdatedDate, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public Registrant Copy(WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var copy = new Registrant();
            copy.CopyDataValues(OriginalDataValues, ModifiedDataValues);
            copy.CreateDate = copy.LastModifiedDate = DateTime.UtcNow;
            copy.CreatedBy = copy.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);
            copy.Uid = Guid.NewGuid();
            copy.ResetID();
            msgs.AddRange(copy.Save(dc));

            return copy;
        }

        public string GetFirstNameLastInitial()
        {
            if (FirstName.Length < 3)
                return $"{FirstName} {LastName.MaskString(1)}.";

            return $"{FirstName.MaskString(Math.Max(FirstName.Length - 3, 3))} {LastName.MaskString(1)}.";
        }

        public static List<Registrant> LoadByWebUserId(int WebUserId, RegistrantPersonType personTypeId, bool IsMaster, bool IsDeleted,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["WebUserId"], WebUserId);
            dq.Criterias.Add(ObjectMap["IsMaster"], IsMaster);
            dq.Criterias.Add(ObjectMap["IsDeleted"], IsDeleted);
            dq.Criterias.Add(ObjectMap["PersonTypeId"], (int) personTypeId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Registrant> LoadByWebUserId(int WebUserId, List<RegistrantPersonType> personTypeIds, bool IsMaster, bool IsDeleted,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["WebUserId"], WebUserId);
            dq.Criterias.Add(ObjectMap["IsMaster"], IsMaster);
            dq.Criterias.Add(ObjectMap["IsDeleted"], IsDeleted);
            dq.Criterias.Add(ObjectMap["PersonTypeId"], personTypeIds.Select(x => (int) x).ToList(), DatabaseCriteriaOperatorEnum.In);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Registrant> LoadByAnsweredFormId(int formId, int accountId, int? start, int? length, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;

            string sql = @$"
                select distinct Registrant.* 
                from 
                    FormQuestionAnswer inner join 
                    FormQuestion on FormQuestionAnswer.FormQuestionId = FormQuestion.ID inner join 
                    Registrant on FormQuestionAnswer.RegistrantId = Registrant.ID 
                where 
                    FormQuestion.FormId = {formId} and
                    Registrant.AccountId = {accountId}
                order by Registrant.CreateDate";

            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;

            return dq.ExecuteSelect(sql, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCountByAnswered(int accountId, int? formId, List<int> activityIds, string search, DateTime? startDate, DateTime? endDate, 
            bool getFirstRegistrantsOnly, List<int> searchQuestionIds, List<QuestionDataType> searchQuestionTypeIds, RegistrantPersonType registrantPersonType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;

            var sql = LoadByAnsweredQuery(accountId, formId, activityIds, search, startDate, endDate, getFirstRegistrantsOnly, true, searchQuestionIds, searchQuestionTypeIds, registrantPersonType);

            return dq.ExecuteSelect(sql, msgs).Rows[0][0].ToString().ToInteger();
        }

        public static List<Registrant> LoadByAnswered(int accountId, int? formId, List<int> activityIds, string search, DateTime? startDate, DateTime? endDate, int? start, int? length,
            bool getFirstRegistrantsOnly, List<int> searchQuestionIds, List<QuestionDataType> searchQuestionTypeIds, RegistrantPersonType registrantPersonType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;

            var sql = LoadByAnsweredQuery(accountId, formId, activityIds, search, startDate, endDate, getFirstRegistrantsOnly, false, searchQuestionIds, searchQuestionTypeIds, registrantPersonType);

            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;

            return dq.ExecuteSelect(sql, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        private static string LoadByAnsweredQuery(int accountId, int? formId, List<int> activityIds, string search, DateTime? startDate, DateTime? endDate, 
            bool getFirstRegistrantsOnly, bool getCountOnly, List<int> searchQuestionIds, List<QuestionDataType> searchQuestionTypeIds, RegistrantPersonType registrantPersonType)
        {
            string sql = @$"
                select {(getCountOnly ? "count(distinct Registrant.ID)" : "distinct Registrant.*")} 
                from 
                    FormQuestionAnswer inner join 
                    FormQuestion on FormQuestionAnswer.FormQuestionId = FormQuestion.ID inner join 
                    Registrant on FormQuestionAnswer.RegistrantId = Registrant.ID";

            if (activityIds.Any())
                sql += @"
                    left outer join 
                    RegistrantActivity on Registrant.ID = RegistrantActivity.RegistrantId";

            sql += @$"
                where 
                    Registrant.AccountId = {accountId}";
            
            if (formId.HasValue) 
                sql += $" and FormQuestion.FormId = {formId.Value}";

            if (search.IsNotEmpty())
            {
                sql += $" and FormQuestionAnswer.Text like '%{search}%'";

                if (searchQuestionIds.Any())
                    sql += @$" and (
                                FormQuestion.ID in ({searchQuestionIds.ToStrings().ToStringList()}) or
                                FormQuestion.QuestionDataTypeId in ({searchQuestionTypeIds.Where(x => x != QuestionDataType.NotSet).Select(x => (int)x).ToStrings().ToStringList()})
                            )";
            }

            if (startDate.HasValue)
                sql += $" and Registrant.CreateDate >= '{startDate.ToShortDateTimeString()}'";

            if (endDate.HasValue)
                sql += $" and Registrant.CreateDate <= '{endDate.ToShortDateTimeString()}'";

            if (registrantPersonType != RegistrantPersonType.NotSet)
                sql += $" and Registrant.PersonTypeId = {(int)registrantPersonType}";

            if (getFirstRegistrantsOnly)
            {
                sql += " and Registrant.FirstRegistrantId is NULL";

                if (activityIds.Any())
                    sql += @$"
                    and 
                    (
                        (RegistrantActivity.ParticipationObjectId in ({activityIds.ToStrings().ToStringList()})
                        and RegistrantActivity.ParticipationObjectTypeId = {(int) ParticipationObjectType.Activity})
                        or 
                        Registrant.ID in 
                        (
                            select r2.FirstRegistrantId 
                            from 
                                Registrant r2 inner join RegistrantActivity ra2 on r2.ID = ra2.RegistrantId
                            where 
                                r2.FirstRegistrantId = Registrant.ID and
                                (ra2.ParticipationObjectId in ({activityIds.ToStrings().ToStringList()})
                                and ra2.ParticipationObjectTypeId = {(int) ParticipationObjectType.Activity})
                        )
                    )";
            }
            else
            {
                if (activityIds.Any())
                    sql += @$"
                    and RegistrantActivity.ParticipationObjectId in ({activityIds.ToStrings().ToStringList()})
                    and RegistrantActivity.ParticipationObjectTypeId = {(int) ParticipationObjectType.Activity}";
            }

            if (!getCountOnly)
                sql += @"
                    order by Registrant.CreateDate";

            return sql;
        }

        #endregion
    }
}