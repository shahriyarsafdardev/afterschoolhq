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
    /// Like a section of a form. A FormQuestionGroup contains many FormQuestions
    /// </summary>
	
    public class FormQuestionGroup : BasicLookUpObject
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required.
		/// </summary>
		public int? FormId { get { return (int?)MDV["FormId"].Value; } set { MDV["FormId"].Value = value; } }
		public int? DisplayOrder { get { return (int?)MDV["DisplayOrder"].Value; } set { MDV["DisplayOrder"].Value = value; } }
		public RegistrantType RegistrantTypeId { get { return (RegistrantType)MDV["RegistrantTypeId"].Value; } set { MDV["RegistrantTypeId"].Value = value; } }
		public QuestionGroupDataType QuestionGroupDataTypeId { get { return (QuestionGroupDataType)MDV["QuestionGroupDataTypeId"].Value; } set { MDV["QuestionGroupDataTypeId"].Value = value; } }
		public bool IsDeleted { get { return (bool)MDV["IsDeleted"].Value; } set { MDV["IsDeleted"].Value = value; } }
		

        public FormQuestionGroup()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(FormQuestionGroup).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.RegistrantTypeId = RegistrantType.NotSet;
			this.QuestionGroupDataTypeId = QuestionGroupDataType.NotSet;
			this.IsDeleted = false;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(FormQuestionGroup).Name;
                _dp = BasicLookUpObject.get_BaseObjectMap(className);
				_dp.Add(className, "FormId", System.Data.SqlDbType.Int);
				_dp.Add(className, "DisplayOrder", System.Data.SqlDbType.Int);
				_dp.Add(className, "RegistrantTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "QuestionGroupDataTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "IsDeleted", System.Data.SqlDbType.Bit);
				
                return _dp;
            }
        }

        public static FormQuestionGroup LoadObject(System.Data.DataRow row)
        {
            FormQuestionGroup obj = new FormQuestionGroup();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(FormQuestionGroup).Name);
				
                return dq;
            }
        }

        public static FormQuestionGroup Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<FormQuestionGroup> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return FormQuestionGroup.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return FormQuestionGroup.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.FormId.IsEmpty())
				msgs.Add("FormId is required on FormQuestionGroup object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(FormQuestionGroup).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(FormQuestionGroup).Name);
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
			dps.Add(ObjectMap["FormId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["DisplayOrder"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["RegistrantTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["QuestionGroupDataTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsDeleted"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public FormQuestionGroup Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return FormQuestionGroup.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<FormQuestionGroup> LoadByFormId(int FormId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["FormId"], FormId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<FormQuestionGroup> LoadByFormIdByRegistrantTypeId(int FormId, RegistrantType RegistrantTypeId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["FormId"], FormId);
			dq.Criterias.Add(ObjectMap["RegistrantTypeId"], RegistrantTypeId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<FormQuestionGroup> LoadByFormIdByIsDeleted(int FormId, bool IsDeleted, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
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

            var questions = this.GetAllQuestions(dc: dc).Where(x => x.DisplayOrder >= atIndex).OrderBy(x => x.DisplayOrder).ToList();
            foreach(var item in questions)
            {
                item.DisplayOrder = item.DisplayOrder + 1;
                msgs.AddRange(item.Save(dc: dc));
            }

            question.FormQuestionGroupId = this.ID;
            question.DisplayOrder = atIndex;
            msgs.AddRange(question.Save(dc: dc));

            return msgs;
        }

        public SystemMessages RemoveQuestion(FormQuestion question, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            question.FormQuestionGroupId = null;
            question.DisplayOrder = null;
            msgs.AddRange(question.Save(dc: dc));

            var questions = this.GetAllQuestions(dc: dc).OrderBy(x => x.DisplayOrder).ToList();
            for (var i = 0; i < questions.Count; i++)
            {
                questions[i].DisplayOrder = i;
                msgs.AddRange(questions[i].Save(dc: dc));
            }

            return msgs;
        }

        public List<FormQuestion> GetAllQuestions(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (this.ID.HasValue)
                return FormQuestion.LoadByFormQuestionGroupId(this.ID.Value, msgs, dc).OrderBy(x => x.DisplayOrder).ToList();

            return new List<FormQuestion>();
        }
        public List<FormQuestion> GetActiveQuestions(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return FormQuestion.LoadByFormQuestionGroupIdByIsActive(this.ID.Value, true, msgs, dc).OrderBy(x => x.DisplayOrder).ToList();
        }
        public int GetActiveQuestionCount(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return FormQuestion.GetCount(new Hashtable() { { "FormQuestionGroupId", this.ID.Value }, { "IsActive", true } }, msgs, dc);
        }
        public int GetQuestionCount(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return FormQuestion.GetCount(new Hashtable() { { "FormQuestionGroupId", this.ID.Value } }, msgs, dc);
        }

        public static List<FormQuestionGroup> LoadActiveFormQuestionGroupByFormId(int FormId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["FormId"], FormId);
            dq.Criterias.Add(ObjectMap["IsActive"], true);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public FormQuestionGroup Copy()
        {
            FormQuestionGroup newGroup = new FormQuestionGroup();
            newGroup.Name = this.Name;
            newGroup.Description = this.Description;
            newGroup.DisplayOrder = this.DisplayOrder;
            newGroup.RegistrantTypeId = this.RegistrantTypeId;
            newGroup.QuestionGroupDataTypeId = this.QuestionGroupDataTypeId;
            return newGroup;
        }

        #endregion
    }
}
