using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchoolLibrary.Models
{
    /// <summary>
    ///     Answers to a FormQuestion as submitted by a registrant/student.
    /// </summary>
    public class FormQuestionAnswer : BasicObjectWithModified
    {
        #region "ClassGenerator"

        /// <summary>
        ///     Required.
        /// </summary>
        public int? FormQuestionId
        {
            get => (int?) MDV["FormQuestionId"].Value;
            set => MDV["FormQuestionId"].Value = value;
        }

        public int? RegistrantId
        {
            get => (int?) MDV["RegistrantId"].Value;
            set => MDV["RegistrantId"].Value = value;
        }

        public int? ActivityId
        {
            get => (int?) MDV["ActivityId"].Value;
            set => MDV["ActivityId"].Value = value;
        }

        public int? SurveyResponseId
        {
            get => (int?) MDV["SurveyResponseId"].Value;
            set => MDV["SurveyResponseId"].Value = value;
        }

        public int? RegistrationId
        {
            get => (int?) MDV["RegistrationId"].Value;
            set => MDV["RegistrationId"].Value = value;
        }

        public string Text
        {
            get => (string) MDV["Text"].Value;
            set => MDV["Text"].Value = value;
        }

        public string Description
        {
            get => (string) MDV["Description"].Value;
            set => MDV["Description"].Value = value;
        }

        public QuestionDataType QuestionDataTypeId
        {
            get => (QuestionDataType) MDV["QuestionDataTypeId"].Value;
            set => MDV["QuestionDataTypeId"].Value = value;
        }

        /// <summary>
        ///     Used to keep the question text just in case the question text changes on the original
        /// </summary>
        public string QuestionText
        {
            get => (string) MDV["QuestionText"].Value;
            set => MDV["QuestionText"].Value = value;
        }

        public Guid? Uid
        {
            get => (Guid?) MDV["Uid"].Value;
            set => MDV["Uid"].Value = value;
        }

        public QuestionDataType FormQuestionQuestionDataTypeId => (QuestionDataType) MDV["FormQuestionQuestionDataTypeId"].Value;
        public QuestionType FormQuestionQuestionTypeId => (QuestionType) MDV["FormQuestionQuestionTypeId"].Value;
        public int? FormQuestionFormId => (int?) MDV["FormQuestionFormId"].Value;
        public int? FormQuestionFormQuestionGroupId => (int?) MDV["FormQuestionFormQuestionGroupId"].Value;
        public int? FormQuestionDisplayOrder => (int?) MDV["FormQuestionDisplayOrder"].Value;
        public string FormQuestionText => (string) MDV["FormQuestionText"].Value;
        public string FormQuestionGroupName => (string) MDV["FormQuestionGroupName"].Value;
        public int? FormQuestionGroupDisplayOrder => (int?) MDV["FormQuestionGroupDisplayOrder"].Value;
        public QuestionGroupDataType FormQuestionGroupQuestionGroupDataTypeId => (QuestionGroupDataType) MDV["FormQuestionGroupQuestionGroupDataTypeId"].Value;
        public int? RegistrationAccountId => (int?) MDV["RegistrationAccountId"].Value;


        public FormQuestionAnswer()
        {
            Config(ObjectMap, DatabaseQuery, typeof(FormQuestionAnswer).Name, "ID");
            LoadObjectValues(ObjectMap);
            QuestionDataTypeId = QuestionDataType.NotSet;
        }

        public static DatabaseParameters ObjectMap
        {
            get
            {
                var _dp = new DatabaseParameters();
                var className = typeof(FormQuestionAnswer).Name;
                _dp = get_BaseObjectMap(className);
                _dp.Add(className, "FormQuestionId", SqlDbType.Int);
                _dp.Add(className, "RegistrantId", SqlDbType.Int);
                _dp.Add(className, "ActivityId", SqlDbType.Int);
                _dp.Add(className, "SurveyResponseId", SqlDbType.Int);
                _dp.Add(className, "RegistrationId", SqlDbType.Int);
                _dp.Add(className, "Text", SqlDbType.NVarChar, "2000");
                _dp.Add(className, "Description", SqlDbType.NVarChar, "4000");
                _dp.Add(className, "QuestionDataTypeId", SqlDbType.Int);
                _dp.Add(className, "QuestionText", SqlDbType.NVarChar, "400");
                _dp.Add(className, "Uid", SqlDbType.UniqueIdentifier);
                _dp.Add(className, "FormQuestionQuestionDataTypeId", typeof(FormQuestion).Name, "QuestionDataTypeId", SqlDbType.Int, "FormQuestion");
                _dp.Add(className, "FormQuestionQuestionTypeId", typeof(FormQuestion).Name, "QuestionTypeId", SqlDbType.Int, "FormQuestion");
                _dp.Add(className, "FormQuestionFormId", typeof(FormQuestion).Name, "FormId", SqlDbType.Int, "FormQuestion");
                _dp.Add(className, "FormQuestionFormQuestionGroupId", typeof(FormQuestion).Name, "FormQuestionGroupId", SqlDbType.Int, "FormQuestion");
                _dp.Add(className, "FormQuestionDisplayOrder", typeof(FormQuestion).Name, "DisplayOrder", SqlDbType.Int, "FormQuestion");
                _dp.Add(className, "FormQuestionText", typeof(FormQuestion).Name, "Text", SqlDbType.NVarChar, "FormQuestion");
                _dp.Add(className, "FormQuestionGroupName", typeof(FormQuestionGroup).Name, "Name", SqlDbType.NVarChar, "FormQuestionGroup");
                _dp.Add(className, "FormQuestionGroupDisplayOrder", typeof(FormQuestionGroup).Name, "DisplayOrder", SqlDbType.Int, "FormQuestionGroup");
                _dp.Add(className, "FormQuestionGroupQuestionGroupDataTypeId", typeof(FormQuestionGroup).Name, "QuestionGroupDataTypeId", SqlDbType.Int,
                    "FormQuestionGroup");
                _dp.Add(className, "RegistrationAccountId", typeof(Registration).Name, "AccountId", SqlDbType.Int, "Registration");

                return _dp;
            }
        }

        public static FormQuestionAnswer LoadObject(DataRow row)
        {
            var obj = new FormQuestionAnswer();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static DatabaseQuery DatabaseQuery
        {
            get
            {
                var dq = get_DatabaseQuery(ObjectMap, typeof(FormQuestionAnswer).Name);
                dq.Joins.Add(FormQuestion.ObjectMap["ID"], ObjectMap["FormQuestionId"], DatabaseJoinTypeEnum.LeftOuterJoin, "FormQuestion");
                dq.Joins.Add(Registration.ObjectMap["ID"], ObjectMap["RegistrationId"], DatabaseJoinTypeEnum.LeftOuterJoin, "Registration");
                dq.Joins.Add(SurveyResponse.ObjectMap["ID"], ObjectMap["SurveyResponseId"], DatabaseJoinTypeEnum.LeftOuterJoin, "SurveyResponse");
                dq.Joins.Add(FormQuestionGroup.ObjectMap["ID"], FormQuestion.ObjectMap["FormQuestionGroupId"], DatabaseJoinTypeEnum.LeftOuterJoin,
                    "FormQuestionGroup");

                return dq;
            }
        }

        public static FormQuestionAnswer Load(int id, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<FormQuestionAnswer> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null,
            DatabaseConnection dc = null)
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

            if (FormQuestionId.IsEmpty())
                msgs.Add("FormQuestionId is required on FormQuestionAnswer object", MessageType.Error);

            if (msgs.HasNoErrors())
                msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(FormQuestionAnswer).Name));

            return msgs;
        }

        public SystemMessages Delete(DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(FormQuestionAnswer).Name);
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
            dps.Add(ObjectMap["FormQuestionId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["RegistrantId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["ActivityId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["SurveyResponseId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["RegistrationId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["Text"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["Description"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["QuestionDataTypeId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["QuestionText"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["Uid"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));


            return msgs;
        }

        public FormQuestionAnswer Reload(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Load(ID.Value, msgs, dc);
        }

        public static List<FormQuestionAnswer> LoadByRegistrantId(int RegistrantId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["RegistrantId"], RegistrantId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static FormQuestionAnswer LoadByRegistrantIdByFormQuestionIdByRegistrationId(int RegistrantId, int FormQuestionId, int RegistrationId,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["RegistrantId"], RegistrantId);
            dq.Criterias.Add(ObjectMap["FormQuestionId"], FormQuestionId);
            dq.Criterias.Add(ObjectMap["RegistrationId"], RegistrationId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<FormQuestionAnswer> LoadByRegistrantIdByActivityId(int RegistrantId, int ActivityId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["RegistrantId"], RegistrantId);
            dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<FormQuestionAnswer> LoadByFormQuestionId(int FormQuestionId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["FormQuestionId"], FormQuestionId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static FormQuestionAnswer LoadByFormQuestionIdByRegistrantId(int FormQuestionId, int RegistrantId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["FormQuestionId"], FormQuestionId);
            dq.Criterias.Add(ObjectMap["RegistrantId"], RegistrantId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<FormQuestionAnswer> LoadByRegistrationId(int RegistrationId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["RegistrationId"], RegistrationId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<FormQuestionAnswer> LoadBySurveyResponseId(int SurveyResponseId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["SurveyResponseId"], SurveyResponseId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        #endregion

        #region "UserDefined"

        public FormQuestionGroup ToFormQuestionGroup()
        {
            return new FormQuestionGroup
            {
                ID = FormQuestionFormQuestionGroupId,
                Name = FormQuestionGroupName,
                DisplayOrder = FormQuestionGroupDisplayOrder,
                QuestionGroupDataTypeId = FormQuestionGroupQuestionGroupDataTypeId,
            };
        }

        public FormQuestion ToFormQuestion(bool loadFromDb = false, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (loadFromDb)
                return FormQuestion.Load(FormQuestionId.Value, msgs, dc);
            return new FormQuestion
            {
                ID = FormQuestionId,
                Text = FormQuestionText,
                QuestionDataTypeId = FormQuestionQuestionDataTypeId,
                QuestionTypeId = FormQuestionQuestionTypeId,
                DisplayOrder = FormQuestionDisplayOrder,
                FormId = FormQuestionFormId,
                FormQuestionGroupId = FormQuestionFormQuestionGroupId,
            };
        }

        public SystemMessages UpdateAnswer(string answerText, Registrant registrant, int? loggedInUserId = null, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            Text = answerText;
            LastModifiedDate = DateTime.UtcNow;
            LastModifiedBy = 0;
            LastModifiedBy = loggedInUserId;
            msgs.AddRange(Save(dc));

            UpdateRegistrantInfo(registrant, msgs, dc);

            return msgs;
        }

        public void UpdateRegistrantInfo(Registrant registrant, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (QuestionDataTypeId != QuestionDataType.NotSet)
            {
                switch (QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantFirstName:
                        registrant.FirstName = Text;
                        break;
                    case QuestionDataType.RegistrantLastName:
                        registrant.LastName = Text;
                        break;
                    case QuestionDataType.RegistrantEmail:
                        registrant.Email = Text;
                        break;
                    case QuestionDataType.RegistrantMobilePhone:
                        registrant.Phone = Text;
                        break;
                    case QuestionDataType.RegistrantOrganization:
                        registrant.Company = Text;
                        break;
                    case QuestionDataType.RegistrantGender:
                        registrant.GenderId = FormService.GetGenderFromAnswer(Text);
                        break;
                }

                msgs.AddRange(registrant.Save(dc));
            }
        }

        public static List<FormQuestionAnswer> LoadByRegistrationIds(List<int> RegistrationIds, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["RegistrationId"], RegistrationIds, DatabaseCriteriaOperatorEnum.In);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static FormQuestionAnswer LoadByRegistrantIdByQuestionDataTypeId(int registrantId, QuestionDataType qt, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.Criterias.Add(ObjectMap["RegistrantId"], registrantId);
            dq.Criterias.Add(ObjectMap["QuestionDataTypeId"], (int) qt);
            // dq.Criterias.Add(ObjectMap["RegistrationId"], DBNull.Value, DatabaseCriteriaOperatorEnum.Is);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public string GetFileDownloadLink()
        {
            if (FormQuestionQuestionTypeId == QuestionType.FileUpload)
                return GetFileDownloadLink(Text);

            return null;
        }

        public static string GetFileDownloadLink(string amazonFileLocation)
        {
            var ah = Utilities.GetNewAmazonHelper();
            return ah.GetFileLink(amazonFileLocation, DateTime.UtcNow.AddMinutes(30), false);
        }

        #endregion
    }
}