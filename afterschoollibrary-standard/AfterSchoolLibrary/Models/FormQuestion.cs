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
    /// One question that is on a form. A form question belongs to one Form Question Group.
    /// </summary>
	
    public class FormQuestion : BasicObjectWithModified
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required.
		/// </summary>
		public int? FormQuestionGroupId { get { return (int?)MDV["FormQuestionGroupId"].Value; } set { MDV["FormQuestionGroupId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int? FormId { get { return (int?)MDV["FormId"].Value; } set { MDV["FormId"].Value = value; } }
		public bool IsTemplate { get { return (bool)MDV["IsTemplate"].Value; } set { MDV["IsTemplate"].Value = value; } }
		public string Text { get { return (string)MDV["Text"].Value; } set { MDV["Text"].Value = value; } }
		public string Description { get { return (string)MDV["Description"].Value; } set { MDV["Description"].Value = value; } }
		public QuestionType QuestionTypeId { get { return (QuestionType)MDV["QuestionTypeId"].Value; } set { MDV["QuestionTypeId"].Value = value; } }
		public QuestionDataType QuestionDataTypeId { get { return (QuestionDataType)MDV["QuestionDataTypeId"].Value; } set { MDV["QuestionDataTypeId"].Value = value; } }
		public bool IsActive { get { return (bool)MDV["IsActive"].Value; } set { MDV["IsActive"].Value = value; } }
		public bool IsDeleted { get { return (bool)MDV["IsDeleted"].Value; } set { MDV["IsDeleted"].Value = value; } }
		public bool IsRequired { get { return (bool)MDV["IsRequired"].Value; } set { MDV["IsRequired"].Value = value; } }
		public bool IsLocked { get { return (bool)MDV["IsLocked"].Value; } set { MDV["IsLocked"].Value = value; } }
		public bool IsAnswerDescriptionEnabled { get { return (bool)MDV["IsAnswerDescriptionEnabled"].Value; } set { MDV["IsAnswerDescriptionEnabled"].Value = value; } }
		public string Options { get { return (string)MDV["Options"].Value; } set { MDV["Options"].Value = value; } }
		public string Link { get { return (string)MDV["Link"].Value; } set { MDV["Link"].Value = value; } }
		public int? DependsOnQuestionId { get { return (int?)MDV["DependsOnQuestionId"].Value; } set { MDV["DependsOnQuestionId"].Value = value; } }
		public string DependsOnValues { get { return (string)MDV["DependsOnValues"].Value; } set { MDV["DependsOnValues"].Value = value; } }
		public int? DisplayOrder { get { return (int?)MDV["DisplayOrder"].Value; } set { MDV["DisplayOrder"].Value = value; } }
		public string FormQuestionGroupName { get { return (string)MDV["FormQuestionGroupName"].Value; } }
		public int? FormQuestionGroupDisplayOrder { get { return (int?)MDV["FormQuestionGroupDisplayOrder"].Value; } }
		public RegistrantType FormQuestionGroupRegistrantTypeId { get { return (RegistrantType)MDV["FormQuestionGroupRegistrantTypeId"].Value; } }
		public QuestionGroupDataType FormQuestionGroupQuestionGroupDataTypeId { get { return (QuestionGroupDataType)MDV["FormQuestionGroupQuestionGroupDataTypeId"].Value; } }
		

        public FormQuestion()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(FormQuestion).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.IsTemplate = false;
			this.QuestionTypeId = QuestionType.NotSet;
			this.QuestionDataTypeId = QuestionDataType.NotSet;
			this.IsActive = false;
			this.IsDeleted = false;
			this.IsRequired = false;
			this.IsLocked = false;
			this.IsAnswerDescriptionEnabled = false;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(FormQuestion).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "FormQuestionGroupId", System.Data.SqlDbType.Int);
				_dp.Add(className, "FormId", System.Data.SqlDbType.Int);
				_dp.Add(className, "IsTemplate", System.Data.SqlDbType.Bit);
				_dp.Add(className, "Text", System.Data.SqlDbType.NVarChar , "400");
				_dp.Add(className, "Description", System.Data.SqlDbType.NVarChar , "4000");
				_dp.Add(className, "QuestionTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "QuestionDataTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "IsActive", System.Data.SqlDbType.Bit);
				_dp.Add(className, "IsDeleted", System.Data.SqlDbType.Bit);
				_dp.Add(className, "IsRequired", System.Data.SqlDbType.Bit);
				_dp.Add(className, "IsLocked", System.Data.SqlDbType.Bit);
				_dp.Add(className, "IsAnswerDescriptionEnabled", System.Data.SqlDbType.Bit);
				_dp.Add(className, "Options", System.Data.SqlDbType.NVarChar , "1000");
				_dp.Add(className, "Link", System.Data.SqlDbType.NVarChar , "1000");
				_dp.Add(className, "DependsOnQuestionId", System.Data.SqlDbType.Int);
				_dp.Add(className, "DependsOnValues", System.Data.SqlDbType.NVarChar , "400");
				_dp.Add(className, "DisplayOrder", System.Data.SqlDbType.Int);
				_dp.Add(className, "FormQuestionGroupName", typeof(FormQuestionGroup).Name, "Name", System.Data.SqlDbType.NVarChar, "FormQuestionGroup");
				_dp.Add(className, "FormQuestionGroupDisplayOrder", typeof(FormQuestionGroup).Name, "DisplayOrder", System.Data.SqlDbType.Int, "FormQuestionGroup");
				_dp.Add(className, "FormQuestionGroupRegistrantTypeId", typeof(FormQuestionGroup).Name, "RegistrantTypeId", System.Data.SqlDbType.Int, "FormQuestionGroup");
				_dp.Add(className, "FormQuestionGroupQuestionGroupDataTypeId", typeof(FormQuestionGroup).Name, "QuestionGroupDataTypeId", System.Data.SqlDbType.Int, "FormQuestionGroup");
				
                return _dp;
            }
        }

        public static FormQuestion LoadObject(System.Data.DataRow row)
        {
            FormQuestion obj = new FormQuestion();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(FormQuestion).Name);
				dq.Joins.Add(FormQuestionGroup.ObjectMap["ID"], FormQuestion.ObjectMap["FormQuestionGroupId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "FormQuestionGroup");
				
                return dq;
            }
        }

        public static FormQuestion Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<FormQuestion> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return FormQuestion.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return FormQuestion.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.FormQuestionGroupId.IsEmpty())
				msgs.Add("FormQuestionGroupId is required on FormQuestion object", MessageType.Error);
			if (this.FormId.IsEmpty())
				msgs.Add("FormId is required on FormQuestion object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(FormQuestion).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(FormQuestion).Name);
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
			dps.Add(ObjectMap["FormQuestionGroupId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["FormId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsTemplate"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Text"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Description"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["QuestionTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["QuestionDataTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsActive"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsDeleted"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsRequired"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsLocked"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsAnswerDescriptionEnabled"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Options"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Link"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["DependsOnQuestionId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["DependsOnValues"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["DisplayOrder"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public FormQuestion Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return FormQuestion.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<FormQuestion> LoadByFormId(int FormId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["FormId"], FormId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<FormQuestion> LoadByFormIdByIsActive(int FormId, bool IsActive, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["FormId"], FormId);
			dq.Criterias.Add(ObjectMap["IsActive"], IsActive);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<FormQuestion> LoadByFormQuestionGroupId(int FormQuestionGroupId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["FormQuestionGroupId"], FormQuestionGroupId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<FormQuestion> LoadByFormQuestionGroupIdByIsActive(int FormQuestionGroupId, bool IsActive, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["FormQuestionGroupId"], FormQuestionGroupId);
			dq.Criterias.Add(ObjectMap["IsActive"], IsActive);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<FormQuestion> LoadByFormQuestionGroupIdByIsDeleted(int FormQuestionGroupId, bool IsDeleted, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["FormQuestionGroupId"], FormQuestionGroupId);
			dq.Criterias.Add(ObjectMap["IsDeleted"], IsDeleted);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<FormQuestion> LoadByDependsOnQuestionId(int DependsOnQuestionId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["DependsOnQuestionId"], DependsOnQuestionId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static FormQuestion LoadByFormIdByQuestionDataTypeId(int FormId, QuestionDataType QuestionDataTypeId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["FormId"], FormId);
			dq.Criterias.Add(ObjectMap["QuestionDataTypeId"], QuestionDataTypeId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static FormQuestion LoadByFormQuestionGroupIdByQuestionDataTypeId(int FormQuestionGroupId, QuestionDataType QuestionDataTypeId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
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
            return new FormQuestionGroup()
            {
                ID = this.FormQuestionGroupId,
                Name = this.FormQuestionGroupName,
                RegistrantTypeId = this.FormQuestionGroupRegistrantTypeId,
                DisplayOrder = this.FormQuestionGroupDisplayOrder
            };
        }

        public static List<FormQuestion> Load(List<int> ids, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], ids, DatabaseCriteriaOperatorEnum.In);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<FormQuestion> LoadByFormIdsByIsActive(List<int> FormIds, bool IsActive, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["FormId"], FormIds, BasicLibrary.DataAccess.DatabaseCriteriaOperatorEnum.In);
            dq.Criterias.Add(ObjectMap["IsActive"], IsActive);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<FormQuestion> LoadList(List<int> ids, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], ids, BasicLibrary.DataAccess.DatabaseCriteriaOperatorEnum.In);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

		public FormQuestionGroup GetFormQuestionGroup(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return FormQuestionGroup.Load(this.FormQuestionGroupId.Value, msgs, dc);
        }

        public Form GetForm(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Form.Load(this.FormId.Value, msgs, dc);
        }

        public FormQuestionAnswer GetAnswer(int RegistrantId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return FormQuestionAnswer.LoadByFormQuestionIdByRegistrantId(this.ID.Value, RegistrantId, msgs, dc);
        }

        public FormQuestion Copy()
        {
            var newQuestion = new FormQuestion
            {
                IsRequired = this.IsRequired,
                Options = this.Options,
                DisplayOrder = this.DisplayOrder,
                QuestionTypeId = this.QuestionTypeId,
                Text = this.Text,
                IsActive = this.IsActive,
                IsLocked = this.IsLocked,
                QuestionDataTypeId = this.QuestionDataTypeId
            };

            return newQuestion;
        }

		public static bool Exists(Hashtable criterias = null, SystemMessages msgs = null, DatabaseConnection dc = null)
		{
			return GetCount(
				criterias: criterias,
				msgs: msgs,
				dc: dc) > 0;
		}
        
        public static List<FormQuestion> LoadByQuestionDataTypeId(List<QuestionDataType> questionDataTypeIds, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["QuestionDataTypeId"], questionDataTypeIds, DatabaseCriteriaOperatorEnum.In);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }


		#endregion
    }
}
