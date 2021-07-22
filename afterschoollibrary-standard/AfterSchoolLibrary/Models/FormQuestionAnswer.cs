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
    /// Answers to a FormQuestion as submitted by a registrant/student.
    /// </summary>
	
    public class FormQuestionAnswer : BasicObjectWithModified
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required.
		/// </summary>
		public int? FormQuestionId { get { return (int?)MDV["FormQuestionId"].Value; } set { MDV["FormQuestionId"].Value = value; } }
		public int? RegistrantId { get { return (int?)MDV["RegistrantId"].Value; } set { MDV["RegistrantId"].Value = value; } }
		public int? ActivityId { get { return (int?)MDV["ActivityId"].Value; } set { MDV["ActivityId"].Value = value; } }
		public int? RegistrationId { get { return (int?)MDV["RegistrationId"].Value; } set { MDV["RegistrationId"].Value = value; } }
		public string Text { get { return (string)MDV["Text"].Value; } set { MDV["Text"].Value = value; } }
		public string Description { get { return (string)MDV["Description"].Value; } set { MDV["Description"].Value = value; } }
		public QuestionDataType QuestionDataTypeId { get { return (QuestionDataType)MDV["QuestionDataTypeId"].Value; } set { MDV["QuestionDataTypeId"].Value = value; } }
		/// <summary>
		/// Used to keep the question text just in case the question text changes on the original
		/// </summary>
		public string QuestionText { get { return (string)MDV["QuestionText"].Value; } set { MDV["QuestionText"].Value = value; } }
		public QuestionDataType FormQuestionQuestionDataTypeId { get { return (QuestionDataType)MDV["FormQuestionQuestionDataTypeId"].Value; } }
		public QuestionType FormQuestionQuestionTypeId { get { return (QuestionType)MDV["FormQuestionQuestionTypeId"].Value; } }
		public int? FormQuestionFormId { get { return (int?)MDV["FormQuestionFormId"].Value; } }
		public int? FormQuestionFormQuestionGroupId { get { return (int?)MDV["FormQuestionFormQuestionGroupId"].Value; } }
		public int? FormQuestionDisplayOrder { get { return (int?)MDV["FormQuestionDisplayOrder"].Value; } }
		public string FormQuestionText { get { return (string)MDV["FormQuestionText"].Value; } }
		public string FormQuestionGroupName { get { return (string)MDV["FormQuestionGroupName"].Value; } }
		public int? FormQuestionGroupDisplayOrder { get { return (int?)MDV["FormQuestionGroupDisplayOrder"].Value; } }
		public QuestionGroupDataType FormQuestionGroupQuestionGroupDataTypeId { get { return (QuestionGroupDataType)MDV["FormQuestionGroupQuestionGroupDataTypeId"].Value; } }
		public int? RegistrationAccountId { get { return (int?)MDV["RegistrationAccountId"].Value; } }
		

        public FormQuestionAnswer()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(FormQuestionAnswer).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.QuestionDataTypeId = QuestionDataType.NotSet;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(FormQuestionAnswer).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "FormQuestionId", System.Data.SqlDbType.Int);
				_dp.Add(className, "RegistrantId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ActivityId", System.Data.SqlDbType.Int);
				_dp.Add(className, "RegistrationId", System.Data.SqlDbType.Int);
				_dp.Add(className, "Text", System.Data.SqlDbType.NVarChar , "400");
				_dp.Add(className, "Description", System.Data.SqlDbType.NVarChar , "4000");
				_dp.Add(className, "QuestionDataTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "QuestionText", System.Data.SqlDbType.NVarChar , "400");
				_dp.Add(className, "FormQuestionQuestionDataTypeId", typeof(FormQuestion).Name, "QuestionDataTypeId", System.Data.SqlDbType.Int, "FormQuestion");
				_dp.Add(className, "FormQuestionQuestionTypeId", typeof(FormQuestion).Name, "QuestionTypeId", System.Data.SqlDbType.Int, "FormQuestion");
				_dp.Add(className, "FormQuestionFormId", typeof(FormQuestion).Name, "FormId", System.Data.SqlDbType.Int, "FormQuestion");
				_dp.Add(className, "FormQuestionFormQuestionGroupId", typeof(FormQuestion).Name, "FormQuestionGroupId", System.Data.SqlDbType.Int, "FormQuestion");
				_dp.Add(className, "FormQuestionDisplayOrder", typeof(FormQuestion).Name, "DisplayOrder", System.Data.SqlDbType.Int, "FormQuestion");
				_dp.Add(className, "FormQuestionText", typeof(FormQuestion).Name, "Text", System.Data.SqlDbType.NVarChar, "FormQuestion");
				_dp.Add(className, "FormQuestionGroupName", typeof(FormQuestionGroup).Name, "Name", System.Data.SqlDbType.NVarChar, "FormQuestionGroup");
				_dp.Add(className, "FormQuestionGroupDisplayOrder", typeof(FormQuestionGroup).Name, "DisplayOrder", System.Data.SqlDbType.Int, "FormQuestionGroup");
				_dp.Add(className, "FormQuestionGroupQuestionGroupDataTypeId", typeof(FormQuestionGroup).Name, "QuestionGroupDataTypeId", System.Data.SqlDbType.Int, "FormQuestionGroup");
				_dp.Add(className, "RegistrationAccountId", typeof(Registration).Name, "AccountId", System.Data.SqlDbType.Int, "Registration");
				
                return _dp;
            }
        }

        public static FormQuestionAnswer LoadObject(System.Data.DataRow row)
        {
            FormQuestionAnswer obj = new FormQuestionAnswer();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(FormQuestionAnswer).Name);
				dq.Joins.Add(FormQuestion.ObjectMap["ID"], FormQuestionAnswer.ObjectMap["FormQuestionId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.LeftOuterJoin, "FormQuestion");
				dq.Joins.Add(Registration.ObjectMap["ID"], FormQuestionAnswer.ObjectMap["RegistrationId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.LeftOuterJoin, "Registration");
				dq.Joins.Add(FormQuestionGroup.ObjectMap["ID"], FormQuestion.ObjectMap["FormQuestionGroupId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.LeftOuterJoin, "FormQuestionGroup");
				
                return dq;
            }
        }

        public static FormQuestionAnswer Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<FormQuestionAnswer> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return FormQuestionAnswer.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return FormQuestionAnswer.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.FormQuestionId.IsEmpty())
				msgs.Add("FormQuestionId is required on FormQuestionAnswer object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(FormQuestionAnswer).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(FormQuestionAnswer).Name);
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
			dps.Add(ObjectMap["FormQuestionId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["RegistrantId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ActivityId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["RegistrationId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Text"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Description"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["QuestionDataTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["QuestionText"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public FormQuestionAnswer Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return FormQuestionAnswer.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<FormQuestionAnswer> LoadByRegistrantId(int RegistrantId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["RegistrantId"], RegistrantId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static FormQuestionAnswer LoadByRegistrantIdByFormQuestionIdByRegistrationId(int RegistrantId, int FormQuestionId, int RegistrationId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["RegistrantId"], RegistrantId);
			dq.Criterias.Add(ObjectMap["FormQuestionId"], FormQuestionId);
			dq.Criterias.Add(ObjectMap["RegistrationId"], RegistrationId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static List<FormQuestionAnswer> LoadByRegistrantIdByActivityId(int RegistrantId, int ActivityId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["RegistrantId"], RegistrantId);
			dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<FormQuestionAnswer> LoadByFormQuestionId(int FormQuestionId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["FormQuestionId"], FormQuestionId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static FormQuestionAnswer LoadByFormQuestionIdByRegistrantId(int FormQuestionId, int RegistrantId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["FormQuestionId"], FormQuestionId);
			dq.Criterias.Add(ObjectMap["RegistrantId"], RegistrantId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static List<FormQuestionAnswer> LoadByRegistrationId(int RegistrationId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["RegistrationId"], RegistrationId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		

        #endregion

		#region "UserDefined"

        public FormQuestionGroup ToFormQuestionGroup()
        {
            return new FormQuestionGroup()
            {
                ID = this.FormQuestionFormQuestionGroupId,
                Name = this.FormQuestionGroupName,
                DisplayOrder = this.FormQuestionGroupDisplayOrder,
                QuestionGroupDataTypeId = this.FormQuestionGroupQuestionGroupDataTypeId
                
            };
        }

        public FormQuestion ToFormQuestion(bool loadFromDb = false, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (loadFromDb)
            {
                return FormQuestion.Load(this.FormQuestionId.Value, msgs, dc);
            }
            else
            {
                return new FormQuestion()
                {
                    ID = this.FormQuestionId,
                    Text = this.FormQuestionText,
                    QuestionDataTypeId = this.FormQuestionQuestionDataTypeId,
                    QuestionTypeId = this.FormQuestionQuestionTypeId,
                    DisplayOrder = this.FormQuestionDisplayOrder,
                    FormId = this.FormQuestionFormId,
                    FormQuestionGroupId = this.FormQuestionFormQuestionGroupId
                };
            }
        }

        public SystemMessages UpdateAnswer(string answerText, Registrant registrant, int? loggedInUserId = null, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            this.Text = answerText;
            this.LastModifiedDate = DateTime.UtcNow;
            this.LastModifiedBy = 0;
            this.LastModifiedBy = loggedInUserId;
            msgs.AddRange(this.Save(dc: dc));

            this.UpdateRegistrantInfo(registrant, msgs, dc);

            return msgs;
        }

        public void UpdateRegistrantInfo(Registrant registrant, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (this.QuestionDataTypeId != QuestionDataType.NotSet)
            {
                switch (this.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantFirstName:
                        registrant.FirstName = this.Text;
                        break;
                    case QuestionDataType.RegistrantLastName:
                        registrant.LastName = this.Text;
                        break;
                    case QuestionDataType.RegistrantEmail:
                        registrant.Email = this.Text;
                        break;
                    case QuestionDataType.RegistrantMobilePhone:
                        registrant.Phone = this.Text;
                        break;
                    case QuestionDataType.OrganizationName:
                        registrant.Company = this.Text;
                        break;
                    case QuestionDataType.RegistrantOrganization:
                        registrant.Company = this.Text;
                        break;
                    case QuestionDataType.OrganizationContactFirstName:
                        registrant.FirstName = this.Text;
                        break;
                    case QuestionDataType.OrganizationContactLastName:
                        registrant.LastName = this.Text;
                        break;
                    case QuestionDataType.OrganizationContactEmail:
                        registrant.Email = this.Text;
                        break;
                 
                }

                msgs.AddRange(registrant.Save(dc));
            }
        }

        public static List<FormQuestionAnswer> LoadByRegistrationIds(List<int> RegistrationIds, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["RegistrationId"], RegistrationIds, BasicLibrary.DataAccess.DatabaseCriteriaOperatorEnum.In);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static FormQuestionAnswer LoadByRegistrantIdByQuestionDataTypeId(int registrantId, QuestionDataType qt, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.Criterias.Add(ObjectMap["RegistrantId"], registrantId);
            dq.Criterias.Add(ObjectMap["QuestionDataTypeId"], (int)qt);
            // dq.Criterias.Add(ObjectMap["RegistrationId"], DBNull.Value, DatabaseCriteriaOperatorEnum.Is);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public string GetFileDownloadLink()
        {
            if (this.FormQuestionQuestionTypeId == QuestionType.FileUpload)
                return GetFileDownloadLink(this.Text);

            return null;
        }

        public static string GetFileDownloadLink(string amazonFileLocation)
        {
            var ah = Helpers.Utilities.GetNewAmazonHelper();
            return ah.GetFileLink(amazonFileLocation, true, DateTime.UtcNow.AddMinutes(30), false);
        }

        #endregion
    }
}
