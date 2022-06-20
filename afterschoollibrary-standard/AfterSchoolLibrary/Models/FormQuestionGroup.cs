using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Models
{
    /// <summary>
    ///     Like a section of a form. A FormQuestionGroup contains many FormQuestions
    /// </summary>
    public class FormQuestionGroup : BasicLookUpObject
    {
        #region "ClassGenerator"

        /// <summary>
        ///     Required.
        /// </summary>
        public int? FormId
        {
            get => (int?) MDV["FormId"].Value;
            set => MDV["FormId"].Value = value;
        }

        public int? DisplayOrder
        {
            get => (int?) MDV["DisplayOrder"].Value;
            set => MDV["DisplayOrder"].Value = value;
        }

        public RegistrantType RegistrantTypeId
        {
            get => (RegistrantType) MDV["RegistrantTypeId"].Value;
            set => MDV["RegistrantTypeId"].Value = value;
        }

        public QuestionGroupDataType QuestionGroupDataTypeId
        {
            get => (QuestionGroupDataType) MDV["QuestionGroupDataTypeId"].Value;
            set => MDV["QuestionGroupDataTypeId"].Value = value;
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


        public FormQuestionGroup()
        {
            Config(ObjectMap, DatabaseQuery, typeof(FormQuestionGroup).Name, "ID");
            LoadObjectValues(ObjectMap);
            RegistrantTypeId = RegistrantType.NotSet;
            QuestionGroupDataTypeId = QuestionGroupDataType.NotSet;
            IsActive = false;
            IsDeleted = false;
        }

        public static DatabaseParameters ObjectMap
        {
            get
            {
                var _dp = new DatabaseParameters();
                var className = typeof(FormQuestionGroup).Name;
                _dp = get_BaseObjectMap(className);
                _dp.Add(className, "FormId", SqlDbType.Int);
                _dp.Add(className, "DisplayOrder", SqlDbType.Int);
                _dp.Add(className, "RegistrantTypeId", SqlDbType.Int);
                _dp.Add(className, "QuestionGroupDataTypeId", SqlDbType.Int);
                _dp.Add(className, "IsActive", SqlDbType.Bit);
                _dp.Add(className, "IsDeleted", SqlDbType.Bit);

                return _dp;
            }
        }

        public static FormQuestionGroup LoadObject(DataRow row)
        {
            var obj = new FormQuestionGroup();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static DatabaseQuery DatabaseQuery
        {
            get
            {
                var dq = get_DatabaseQuery(ObjectMap, typeof(FormQuestionGroup).Name);

                return dq;
            }
        }

        public static FormQuestionGroup Load(int id, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<FormQuestionGroup> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null,
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

            if (FormId.IsEmpty())
                msgs.Add("FormId is required on FormQuestionGroup object", MessageType.Error);

            if (msgs.HasNoErrors())
                msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(FormQuestionGroup).Name));

            return msgs;
        }

        public SystemMessages Delete(DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(FormQuestionGroup).Name);
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
            dps.Add(ObjectMap["FormId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["DisplayOrder"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["RegistrantTypeId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["QuestionGroupDataTypeId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["IsActive"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["IsDeleted"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));


            return msgs;
        }

        public FormQuestionGroup Reload(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Load(ID.Value, msgs, dc);
        }

        public static List<FormQuestionGroup> LoadByFormId(int FormId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["FormId"], FormId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<FormQuestionGroup> LoadByFormIdByRegistrantTypeId(int FormId, RegistrantType RegistrantTypeId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["FormId"], FormId);
            dq.Criterias.Add(ObjectMap["RegistrantTypeId"], RegistrantTypeId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<FormQuestionGroup> LoadByFormIdByIsDeleted(int FormId, bool IsDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["FormId"], FormId);
            dq.Criterias.Add(ObjectMap["IsDeleted"], IsDeleted);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        #endregion

        #region "UserDefined"

        public SystemMessages AddQuestion(FormQuestion question, int atIndex, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            var questions = GetAllQuestions(dc: dc).Where(x => x.DisplayOrder >= atIndex).OrderBy(x => x.DisplayOrder).ToList();
            foreach (var item in questions)
            {
                item.DisplayOrder += 1;
                msgs.AddRange(item.Save(dc));
            }

            question.FormQuestionGroupId = ID;
            question.DisplayOrder = atIndex;
            msgs.AddRange(question.Save(dc));

            return msgs;
        }

        public SystemMessages RemoveQuestion(FormQuestion question, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            question.FormQuestionGroupId = null;
            question.DisplayOrder = null;
            msgs.AddRange(question.Save(dc));

            var questions = GetAllQuestions(dc: dc).OrderBy(x => x.DisplayOrder).ToList();
            for (var i = 0; i < questions.Count; i++)
            {
                questions[i].DisplayOrder = i;
                msgs.AddRange(questions[i].Save(dc));
            }

            return msgs;
        }

        public List<FormQuestion> GetAllQuestions(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ID.HasValue
                ? FormQuestion.LoadByFormQuestionGroupIdByIsDeleted(ID.Value, false, msgs, dc).OrderBy(x => x.DisplayOrder).ToList()
                : new List<FormQuestion>();
        }

        public List<FormQuestion> GetActiveQuestions(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return FormQuestion.LoadByFormQuestionGroupIdByIsActive(ID.Value, true, msgs, dc).OrderBy(x => x.DisplayOrder).ToList();
        }

        public int GetActiveQuestionCount(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return FormQuestion.GetCount(new Hashtable {{"FormQuestionGroupId", ID.Value}, {"IsActive", true}}, msgs, dc);
        }

        public int GetQuestionCount(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return FormQuestion.GetCount(new Hashtable {{"FormQuestionGroupId", ID.Value}}, msgs, dc);
        }

        public FormQuestionGroup Copy()
        {
            var newGroup = new FormQuestionGroup();
            newGroup.Name = Name;
            newGroup.Description = Description;
            newGroup.DisplayOrder = DisplayOrder;
            newGroup.RegistrantTypeId = RegistrantTypeId;
            newGroup.QuestionGroupDataTypeId = QuestionGroupDataTypeId;
            return newGroup;
        }

        public static bool Exists(Hashtable criterias = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return GetCount(
                criterias,
                msgs,
                dc) > 0;
        }

        public static List<FormQuestionGroup> LoadByQuestionGroupDataTypeId(int questionGroupDataTypeId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["QuestionGroupDataTypeId"], questionGroupDataTypeId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        #endregion
    }
}