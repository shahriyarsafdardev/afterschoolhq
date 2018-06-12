using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Data;
using System.Collections;

namespace AfterSchoolLibrary
{
	
    public class Form : BasicLookUpObject
    {
		#region "ClassGenerator"

		public int? AccountId { get { return (int?)MDV["AccountId"].Value; } set { MDV["AccountId"].Value = value; } }
		public Guid Uid { get { return (Guid)MDV["Uid"].Value; } set { MDV["Uid"].Value = value; } }
		public bool IsActive { get { return (bool)MDV["IsActive"].Value; } set { MDV["IsActive"].Value = value; } }
		public string AccountName { get { return (string)MDV["AccountName"].Value; } }
		

        public Form()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(Form).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.Uid = Guid.NewGuid();
			this.IsActive = false;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(Form).Name;
                _dp = BasicLookUpObject.get_BaseObjectMap(className);
				_dp.Add(className, "AccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "Uid", System.Data.SqlDbType.UniqueIdentifier);
				_dp.Add(className, "IsActive", System.Data.SqlDbType.Bit);
				_dp.Add(className, "AccountName", typeof(Account).Name, "Name", System.Data.SqlDbType.NVarChar, "Account");
				
                return _dp;
            }
        }

        protected static Form LoadObject(System.Data.DataRow row)
        {
            Form obj = new Form();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(Form).Name);
				dq.Joins.Add(Account.ObjectMap["ID"], Form.ObjectMap["AccountId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "Account");
				
                return dq;
            }
        }

        public static Form Load(int id, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<Form> Load(Hashtable criterias = null, int? start = null, int? length = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return Form.BaseLoad(criterias, ObjectMap, dq).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return Form.BaseGetCount(criterias, ObjectMap, dq);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.AccountId.IsEmpty())
				msgs.Add("AccountId is required on Form object", MessageType.Error);
			if (this.Uid.IsEmpty())
				msgs.Add("Uid is required on Form object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(Form).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(Form).Name);
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
			dps.Add(ObjectMap["Uid"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsActive"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public Form Reload(BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return Form.Load(this.ID.Value, dc);
		}
		
		public static List<Form> LoadByAccountId(int AccountId, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static Form LoadByUid(Guid Uid, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		

        #endregion

		#region "UserDefined"

        public int GetQuestionCount(DatabaseConnection dc = null)
        {
            return FormQuestion.GetCount(
                new Hashtable() {
                    { "FormId", this.ID.Value }
                },
                dc
            );
        }

        public static Form GetLatestTemplateForm(int AccountId, DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.Criterias.Add(ObjectMap["IsActive"], true);
            dq.OrderBys.Add(ObjectMap["ID"], OrderByDirectionEnum.Descending);
            dq.SelectRange.Start = 0;
            dq.SelectRange.Length = 1;
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

        public List<FormQuestion> GetAllQuestions(DatabaseConnection dc = null)
        {
            return FormQuestion.LoadByFormId(this.ID.Value, dc);
        }

        public List<FormQuestion> GetActiveQuestions(DatabaseConnection dc = null)
        {
            return FormQuestion.LoadByFormIdByIsActive(this.ID.Value, true, dc);
        }

        public List<FormQuestionGroup> GetQuestionGroups(DatabaseConnection dc = null)
        {
            return FormQuestionGroup.LoadByFormId(this.ID.Value, dc).OrderBy(x => x.DisplayOrder).ToList();
        }

        public Form Copy(int loggedInUserId, int newAccountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var form = new Form
            {
                Name = this.Name,
                Description = this.Description,
                LastModifiedDate = DateTime.UtcNow,
                CreateDate = DateTime.UtcNow,
                LastModifiedBy = loggedInUserId,
                CreatedBy = loggedInUserId,
                AccountId = newAccountId,
                IsActive = true,
                Uid = Guid.NewGuid()
            };
            msgs.AddRange(form.Save(dc));

            var groups = FormQuestionGroup.LoadByFormId(this.ID.Value, dc);
            foreach (var group in groups)
            {
                var questions = FormQuestion.LoadByFormQuestionGroupId(group.ID.Value, dc);

                var newGroup = group.Copy();
                newGroup.LastModifiedDate = DateTime.UtcNow;
                newGroup.CreateDate = DateTime.UtcNow;
                newGroup.LastModifiedBy = newGroup.CreatedBy = loggedInUserId;
                newGroup.FormId = form.ID;
                msgs.AddRange(newGroup.Save(dc));

                questions.ForEach(question => {
                    var newQuestion = question.Copy();
                    newQuestion.LastModifiedDate = newQuestion.CreateDate = DateTime.UtcNow;
                    newQuestion.LastModifiedBy = newQuestion.CreatedBy = loggedInUserId;
                    newQuestion.FormQuestionGroupId = newGroup.ID;
                    newQuestion.FormId = form.ID;
                    newQuestion.IsTemplate = false;
                    msgs.AddRange(newQuestion.Save(dc));
                });
            }

            //check for default form existence
            var account = Account.Load(newAccountId, dc);
            if(account.IsNotEmpty() && !account.DefaultFormId.HasValue)
            {
                account.DefaultFormId = form.ID;
                msgs.AddRange(account.Save(dc));
            }

            return form;
        }

        #endregion
    }
}
