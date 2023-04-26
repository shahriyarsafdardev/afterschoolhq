using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Models
{
    /// <summary>
    ///     One question that is on a form. A form question belongs to one Form Question Group.
    /// </summary>
    public class FormQuestion : BasicObjectWithModified
    {
        #region "ClassGenerator"

        /// <summary>
        ///     Required.
        /// </summary>
        public int? FormQuestionGroupId
        {
            get => (int?) MDV["FormQuestionGroupId"].Value;
            set => MDV["FormQuestionGroupId"].Value = value;
        }

        /// <summary>
        ///     Required.
        /// </summary>
        public int? FormId
        {
            get => (int?) MDV["FormId"].Value;
            set => MDV["FormId"].Value = value;
        }

        public bool IsTemplate
        {
            get => (bool) MDV["IsTemplate"].Value;
            set => MDV["IsTemplate"].Value = value;
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

        public QuestionType QuestionTypeId
        {
            get => (QuestionType) MDV["QuestionTypeId"].Value;
            set => MDV["QuestionTypeId"].Value = value;
        }

        public QuestionDataType QuestionDataTypeId
        {
            get => (QuestionDataType) MDV["QuestionDataTypeId"].Value;
            set => MDV["QuestionDataTypeId"].Value = value;
        }

        public bool IsActive
        {
            get => (bool) MDV["IsActive"].Value;
            set => MDV["IsActive"].Value = value;
        }

        public bool IsDeleted
        {
            get => (bool) MDV["IsDeleted"].Value;
            set => MDV["IsDeleted"].Value = value;
        }

        public bool IsRequired
        {
            get => (bool) MDV["IsRequired"].Value;
            set => MDV["IsRequired"].Value = value;
        }

        public bool IsLocked
        {
            get => (bool) MDV["IsLocked"].Value;
            set => MDV["IsLocked"].Value = value;
        }

        public bool IsAnswerDescriptionEnabled
        {
            get => (bool) MDV["IsAnswerDescriptionEnabled"].Value;
            set => MDV["IsAnswerDescriptionEnabled"].Value = value;
        }

        public string Options
        {
            get => (string) MDV["Options"].Value;
            set => MDV["Options"].Value = value;
        }

        public string OptionsJson
        {
            get => (string) MDV["OptionsJson"].Value;
            set => MDV["OptionsJson"].Value = value;
        }

        public string Link
        {
            get => (string) MDV["Link"].Value;
            set => MDV["Link"].Value = value;
        }

        public int? DependsOnQuestionId
        {
            get => (int?) MDV["DependsOnQuestionId"].Value;
            set => MDV["DependsOnQuestionId"].Value = value;
        }

        public string DependsOnValues
        {
            get => (string) MDV["DependsOnValues"].Value;
            set => MDV["DependsOnValues"].Value = value;
        }

        public int? DisplayOrder
        {
            get => (int?) MDV["DisplayOrder"].Value;
            set => MDV["DisplayOrder"].Value = value;
        }

        public string FormQuestionGroupName => (string) MDV["FormQuestionGroupName"].Value;
        public int? FormQuestionGroupDisplayOrder => (int?) MDV["FormQuestionGroupDisplayOrder"].Value;
        public RegistrantType FormQuestionGroupRegistrantTypeId => (RegistrantType) MDV["FormQuestionGroupRegistrantTypeId"].Value;
        public QuestionGroupDataType FormQuestionGroupQuestionGroupDataTypeId => (QuestionGroupDataType) MDV["FormQuestionGroupQuestionGroupDataTypeId"].Value;


        public FormQuestion()
        {
            Config(ObjectMap, DatabaseQuery, typeof(FormQuestion).Name, "ID");
            LoadObjectValues(ObjectMap);
            IsTemplate = false;
            QuestionTypeId = QuestionType.NotSet;
            QuestionDataTypeId = QuestionDataType.NotSet;
            IsActive = false;
            IsDeleted = false;
            IsRequired = false;
            IsLocked = false;
            IsAnswerDescriptionEnabled = false;
        }

        public static DatabaseParameters ObjectMap
        {
            get
            {
                var _dp = new DatabaseParameters();
                var className = typeof(FormQuestion).Name;
                _dp = get_BaseObjectMap(className);
                _dp.Add(className, "FormQuestionGroupId", SqlDbType.Int);
                _dp.Add(className, "FormId", SqlDbType.Int);
                _dp.Add(className, "IsTemplate", SqlDbType.Bit);
                _dp.Add(className, "Text", SqlDbType.NVarChar, "400");
                _dp.Add(className, "Description", SqlDbType.NVarChar, "4000");
                _dp.Add(className, "QuestionTypeId", SqlDbType.Int);
                _dp.Add(className, "QuestionDataTypeId", SqlDbType.Int);
                _dp.Add(className, "IsActive", SqlDbType.Bit);
                _dp.Add(className, "IsDeleted", SqlDbType.Bit);
                _dp.Add(className, "IsRequired", SqlDbType.Bit);
                _dp.Add(className, "IsLocked", SqlDbType.Bit);
                _dp.Add(className, "IsAnswerDescriptionEnabled", SqlDbType.Bit);
                _dp.Add(className, "Options", SqlDbType.NVarChar, "1000");
                _dp.Add(className, "OptionsJson", SqlDbType.NVarChar, "4000");
                _dp.Add(className, "Link", SqlDbType.NVarChar, "1000");
                _dp.Add(className, "DependsOnQuestionId", SqlDbType.Int);
                _dp.Add(className, "DependsOnValues", SqlDbType.NVarChar, "400");
                _dp.Add(className, "DisplayOrder", SqlDbType.Int);
                _dp.Add(className, "FormQuestionGroupName", typeof(FormQuestionGroup).Name, "Name", SqlDbType.NVarChar, "FormQuestionGroup");
                _dp.Add(className, "FormQuestionGroupDisplayOrder", typeof(FormQuestionGroup).Name, "DisplayOrder", SqlDbType.Int, "FormQuestionGroup");
                _dp.Add(className, "FormQuestionGroupRegistrantTypeId", typeof(FormQuestionGroup).Name, "RegistrantTypeId", SqlDbType.Int, "FormQuestionGroup");
                _dp.Add(className, "FormQuestionGroupQuestionGroupDataTypeId", typeof(FormQuestionGroup).Name, "QuestionGroupDataTypeId", SqlDbType.Int,
                    "FormQuestionGroup");

                return _dp;
            }
        }

        public static FormQuestion LoadObject(DataRow row)
        {
            var obj = new FormQuestion();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static DatabaseQuery DatabaseQuery
        {
            get
            {
                var dq = get_DatabaseQuery(ObjectMap, typeof(FormQuestion).Name);
                dq.Joins.Add(FormQuestionGroup.ObjectMap["ID"], ObjectMap["FormQuestionGroupId"], DatabaseJoinTypeEnum.InnerJoin, "FormQuestionGroup");

                return dq;
            }
        }

        public static FormQuestion Load(int id, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<FormQuestion> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null,
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

            if (FormQuestionGroupId.IsEmpty())
                msgs.Add("FormQuestionGroupId is required on FormQuestion object", MessageType.Error);
            if (FormId.IsEmpty())
                msgs.Add("FormId is required on FormQuestion object", MessageType.Error);

            if (msgs.HasNoErrors())
                msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(FormQuestion).Name));

            return msgs;
        }

        public SystemMessages Delete(DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(FormQuestion).Name);
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
            dps.Add(ObjectMap["FormQuestionGroupId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["FormId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["IsTemplate"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["Text"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["Description"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["QuestionTypeId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["QuestionDataTypeId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["IsActive"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["IsDeleted"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["IsRequired"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["IsLocked"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["IsAnswerDescriptionEnabled"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["Options"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["OptionsJson"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["Link"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["DependsOnQuestionId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["DependsOnValues"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["DisplayOrder"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));
            
            return msgs;
        }

        public FormQuestion Reload(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Load(ID.Value, msgs, dc);
        }

        public static List<FormQuestion> LoadByFormId(int FormId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["FormId"], FormId);
            dq.Criterias.Add(ObjectMap["IsDeleted"], false);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<FormQuestion> LoadByFormIdByIsActive(int FormId, bool IsActive, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["FormId"], FormId);
            dq.Criterias.Add(ObjectMap["IsActive"], IsActive);
            dq.Criterias.Add(ObjectMap["IsDeleted"], false);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<FormQuestion> LoadByFormQuestionGroupId(int FormQuestionGroupId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["FormQuestionGroupId"], FormQuestionGroupId);
            dq.Criterias.Add(ObjectMap["IsDeleted"], false);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<FormQuestion> LoadByFormQuestionGroupIdByIsActive(int FormQuestionGroupId, bool IsActive, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["FormQuestionGroupId"], FormQuestionGroupId);
            dq.Criterias.Add(ObjectMap["IsActive"], IsActive);
            dq.Criterias.Add(ObjectMap["IsDeleted"], false);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<FormQuestion> LoadByFormQuestionGroupIdByIsDeleted(int FormQuestionGroupId, bool IsDeleted, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["FormQuestionGroupId"], FormQuestionGroupId);
            dq.Criterias.Add(ObjectMap["IsDeleted"], IsDeleted);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<FormQuestion> LoadByDependsOnQuestionId(int DependsOnQuestionId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["DependsOnQuestionId"], DependsOnQuestionId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<FormQuestion> LoadByFormIdByQuestionDataTypeIds(int formId, IEnumerable<QuestionDataType> questionDataTypeIds, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["FormId"], formId);
            dq.Criterias.Add(ObjectMap["QuestionDataTypeId"], questionDataTypeIds, DatabaseCriteriaOperatorEnum.In);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static FormQuestion LoadByFormQuestionGroupIdByQuestionDataTypeId(int FormQuestionGroupId, QuestionDataType QuestionDataTypeId,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["FormQuestionGroupId"], FormQuestionGroupId);
            dq.Criterias.Add(ObjectMap["QuestionDataTypeId"], QuestionDataTypeId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        #endregion

        #region "UserDefined"

        public FormQuestionGroup ToFormQuestionGroup()
        {
            return new FormQuestionGroup
            {
                ID = FormQuestionGroupId,
                Name = FormQuestionGroupName,
                RegistrantTypeId = FormQuestionGroupRegistrantTypeId,
                DisplayOrder = FormQuestionGroupDisplayOrder,
            };
        }

        public static List<FormQuestion> Load(List<int> ids, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], ids, DatabaseCriteriaOperatorEnum.In);
            dq.Criterias.Add(ObjectMap["IsDeleted"], false);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<FormQuestion> LoadByFormIdsByIsActive(List<int> FormIds, bool IsActive, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["FormId"], FormIds, DatabaseCriteriaOperatorEnum.In);
            dq.Criterias.Add(ObjectMap["IsActive"], IsActive);
            dq.Criterias.Add(ObjectMap["IsDeleted"], false);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<FormQuestion> LoadList(List<int> ids, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], ids, DatabaseCriteriaOperatorEnum.In);
            dq.Criterias.Add(ObjectMap["IsDeleted"], false);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public FormQuestionGroup GetFormQuestionGroup(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return FormQuestionGroup.Load(FormQuestionGroupId.Value, msgs, dc);
        }

        public Form GetForm(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Form.Load(FormId.Value, msgs, dc);
        }

        public FormQuestionAnswer GetAnswer(int RegistrantId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return FormQuestionAnswer.LoadByFormQuestionIdByRegistrantId(ID.Value, RegistrantId, msgs, dc);
        }

        public FormQuestion Copy()
        {
            var newQuestion = new FormQuestion
            {
                IsRequired = IsRequired,
                Options = Options,
                DisplayOrder = DisplayOrder,
                QuestionTypeId = QuestionTypeId,
                Text = Text,
                IsActive = IsActive,
                IsLocked = IsLocked,
                IsDeleted = IsDeleted,
                QuestionDataTypeId = QuestionDataTypeId,
            };

            return newQuestion;
        }

        public static List<FormQuestion> LoadByQuestionDataTypeId(List<QuestionDataType> questionDataTypeIds, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["QuestionDataTypeId"], questionDataTypeIds, DatabaseCriteriaOperatorEnum.In);
            dq.Criterias.Add(ObjectMap["IsDeleted"], false);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        #endregion
    }
}