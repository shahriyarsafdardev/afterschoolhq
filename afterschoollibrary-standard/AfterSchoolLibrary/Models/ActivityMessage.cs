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
	/// <summary>
    /// Used to store a message that is sent in regards to an session/league, activity/team, or even on the whole account
    /// </summary>
	
    public class ActivityMessage : BasicObject
    {
		#region "ClassGenerator"

		public int? ActivityId { get { return (int?)MDV["ActivityId"].Value; } set { MDV["ActivityId"].Value = value; } }
		public int? ActivityGroupId { get { return (int?)MDV["ActivityGroupId"].Value; } set { MDV["ActivityGroupId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int? AccountId { get { return (int?)MDV["AccountId"].Value; } set { MDV["AccountId"].Value = value; } }
		public DateTime? DateSent { get { return (DateTime?)MDV["DateSent"].Value; } set { MDV["DateSent"].Value = value; } }
		public string Subject { get { return (string)MDV["Subject"].Value; } set { MDV["Subject"].Value = value; } }
		public string Body { get { return (string)MDV["Body"].Value; } set { MDV["Body"].Value = value; } }
		public string ActivityName { get { return (string)MDV["ActivityName"].Value; } }
		public string ActivityGroupName { get { return (string)MDV["ActivityGroupName"].Value; } }
		public string AccountName { get { return (string)MDV["AccountName"].Value; } }
		

        public ActivityMessage()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(ActivityMessage).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(ActivityMessage).Name;
                _dp = BasicObject.get_BaseObjectMap(className);
				_dp.Add(className, "ActivityId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ActivityGroupId", System.Data.SqlDbType.Int);
				_dp.Add(className, "AccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "DateSent", System.Data.SqlDbType.DateTime);
				_dp.Add(className, "Subject", System.Data.SqlDbType.NVarChar , "150");
				_dp.Add(className, "Body", System.Data.SqlDbType.Text);
				_dp.Add(className, "ActivityName", typeof(Activity).Name, "Name", System.Data.SqlDbType.NVarChar, "Activity");
				_dp.Add(className, "ActivityGroupName", typeof(ActivityGroup).Name, "Name", System.Data.SqlDbType.NVarChar, "ActivityGroup");
				_dp.Add(className, "AccountName", typeof(Account).Name, "Name", System.Data.SqlDbType.NVarChar, "Account");
				
                return _dp;
            }
        }

        protected static ActivityMessage LoadObject(System.Data.DataRow row)
        {
            ActivityMessage obj = new ActivityMessage();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(ActivityMessage).Name);
				dq.Joins.Add(Activity.ObjectMap["ID"], ActivityMessage.ObjectMap["ActivityId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.LeftOuterJoin, "Activity");
				dq.Joins.Add(ActivityGroup.ObjectMap["ID"], ActivityMessage.ObjectMap["ActivityGroupId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.LeftOuterJoin, "ActivityGroup");
				dq.Joins.Add(Account.ObjectMap["ID"], ActivityMessage.ObjectMap["AccountId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "Account");
				
                return dq;
            }
        }

        public static ActivityMessage Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<ActivityMessage> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return ActivityMessage.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return ActivityMessage.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.AccountId.IsEmpty())
				msgs.Add("AccountId is required on ActivityMessage object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(ActivityMessage).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(ActivityMessage).Name);
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
			dps.Add(ObjectMap["ActivityId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ActivityGroupId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AccountId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["DateSent"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Subject"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Body"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public ActivityMessage Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return ActivityMessage.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<ActivityMessage> LoadByActivityId(int ActivityId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<ActivityMessage> LoadByActivityGroupId(int ActivityGroupId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityGroupId"], ActivityGroupId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		

        #endregion

		#region "UserDefined"

        private List<ActivityMessageRegistrant> _ActivityMessageRegistrants = null;
        public List<ActivityMessageRegistrant> GetActivityMessageRegistrants(DatabaseConnection dc = null)
        {
            if (_ActivityMessageRegistrants.IsEmpty())
            {
                _ActivityMessageRegistrants = ActivityMessageRegistrant.LoadByActivityMessageId(this.ID.Value, dc: dc);
            }
            return _ActivityMessageRegistrants;
        }

        public int GetRegistrantCount(DatabaseConnection dc = null)
        {
            return ActivityMessageRegistrant.GetCount(
                new Hashtable()
                {
                    { "ActivityMessageId", this.ID.Value }
                }
                );
        }

        public int GetUnsentCount(DatabaseConnection dc = null)
        {
            return ActivityMessageRegistrant.GetCount(
                   new Hashtable()
                   {
                    { "ActivityMessageId", this.ID.Value },
                    { "DateSent", DBNull.Value }
                   }
               );
        }

        public int GetSentCount(DatabaseConnection dc = null)
        {
            return this.GetRegistrantCount(dc: dc) - this.GetUnsentCount(dc: dc);
        }

        private Activity _Activity = null;
        public Activity GetActivity(DatabaseConnection dc = null)
        {
            if (_Activity.IsEmpty())
            {
                _Activity = Activity.Load(this.ActivityId.Value, dc: dc);
            }
            return _Activity;
        }

        private ActivityGroup _ActivityGroup = null;
        public ActivityGroup GetActivityGroup(DatabaseConnection dc = null)
        {
            if (_ActivityGroup.IsEmpty())
            {
                _ActivityGroup = ActivityGroup.Load(this.ActivityGroupId.Value, dc: dc);
            }
            return _ActivityGroup;
        }

        private static SystemMessages SendForActivity(ActivityMessage am, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var activity = am.GetActivity(dc: dc);
            var account = activity.GetAccount(dc: dc);

            msgs.AddRange(am.SendActivityMessage(account, activity.ContactName, activity.ContactEmail, activity.ContactName, dc: dc));

            if (msgs.HasNoErrors())
            {
                var members = activity.GetTeamMembers(dc: dc);
                foreach (var member in members)
                {
                    var body = am.Body;
                    body = body.Replace("[firstname]", member.WebUserFirstName.IfBlank(""));
                    body = body.Replace("[lastname]", member.WebUserLastName.IfBlank(""));
                    body = body.Replace("[name]", member.WebUserDisplayName.IfBlank(""));

                    //send confirmation to team leaders
                    SendGridHelper.SendMail(
                        fromEmail: Email.SendGridAppEmailAddress(account.IsAfterSchool),
                        toEmail: new SendGrid.Helpers.Mail.EmailAddress(member.WebUserEmail),
                        subject: am.Subject,
                        body: string.Format("{0}{1}", body, Email.GetAppSignature(isAfterSchool: account.IsAfterSchool, utm_campaign: "activity_message")),
                        replyTo: new SendGrid.Helpers.Mail.EmailAddress(
                            activity.ContactEmail, 
                            activity.ContactName),
                        emailTemplateId: SendGridHelper.GetGenericTemplateId(account.IsAfterSchool)
                    );
                }
            }

            return msgs;
        }

        private static SystemMessages SendForGroup(ActivityMessage am, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var group = am.GetActivityGroup(dc: dc);
            var account = group.GetAccount(dc: dc);

            msgs.AddRange(am.SendActivityMessage(account, account.Name, account.Email, account.Name, dc: dc));

            if (msgs.HasNoErrors())
            {
                // send to account admin
                SendGridHelper.SendMail(
                    fromEmail: new SendGrid.Helpers.Mail.EmailAddress(
                        Email.AppAdminEmailAddress, 
                        $"CC: {account.Name} via {Utilities.AppName(account.IsAfterSchool)}"),
                    toEmail: new SendGrid.Helpers.Mail.EmailAddress(account.Email),
                    subject: am.Subject,
                    body: $"{am.Body}{(Email.GetAppSignature(isAfterSchool: account.IsAfterSchool, utm_campaign: "activitygroup_message"))}",
                    emailTemplateId: SendGridHelper.GetGenericTemplateId(account.IsAfterSchool)
                );
            }

            return msgs;
        }

        private SystemMessages SendActivityMessage(Account account, string fromName, string replyToEmail, string replyToName, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var amrs = this.GetActivityMessageRegistrants(dc: dc);

            foreach (var amr in amrs)
            {
                var body = this.Body;

                //a person
                if (amr.RegistrantFirstName.IsNotBlank() || amr.RegistrantLastName.IsNotBlank())
                {
                    body = body.Replace("[firstname]", amr.RegistrantFirstName.IfBlank(""));
                    body = body.Replace("[lastname]", amr.RegistrantLastName.IfBlank(""));
                    body = body.Replace("[name]", amr.RegistrantDisplayName.IfBlank(""));
                }
                //an organization
                else if (amr.RegistrantCompany.IsNotBlank())
                {
                    body = body.Replace("[firstname]", amr.RegistrantCompany);
                    body = body.Replace("[name]", amr.RegistrantCompany);
                    body = body.Replace("[lastname]", "");
                }

                var sendMsgs = SendGridHelper.SendMail(
                    fromEmail: new SendGrid.Helpers.Mail.EmailAddress(
                        Email.AppAdminEmailAddress, 
                        $"{fromName} via {Utilities.AppName(account.IsAfterSchool)}"),
                    toEmail: new SendGrid.Helpers.Mail.EmailAddress(amr.RegistrantEmail, amr.RegistrantDisplayName),
                    subject: this.Subject,
                    body: $"{body}{(Email.GetAppSignature(isAfterSchool: account.IsAfterSchool, utm_campaign: "activity_message"))}",
                    replyTo: new SendGrid.Helpers.Mail.EmailAddress(replyToEmail, replyToName),
                    emailTemplateId: SendGridHelper.GetGenericTemplateId(account.IsAfterSchool)
                );

                if (sendMsgs.HasNoErrors())
                {
                    amr.DateSent = DateTime.UtcNow;
                    msgs.AddRange(amr.Save(dc: dc));
                }
                else
                {
                    amr.SendErrors = sendMsgs.RenderMessageList(MessageOutputType.Text).MaxChars(1000);
                    msgs.AddRange(amr.Save(dc: dc));
                }
            }

            if (msgs.HasNoErrors())
            {
                this.DateSent = DateTime.UtcNow;
                msgs.AddRange(this.Save(dc: dc));
            }
            else
            {
                // TODO: Fix Rollbar reporting
                // RollbarLocator.RollbarInstance.Configure(new RollbarConfig(Utilities.RollbarAccessToken));
                // RollbarLocator.RollbarInstance.Log(ErrorLevel.Error, msgs.RenderMessageList(MessageOutputType.Text));
            }

            return msgs;
        }

        public static SystemMessages Send(int id, DatabaseConnection dc = null)
        {
            var am = ActivityMessage.Load(id, dc: dc);

            if (am.ActivityId.HasValue)
                return ActivityMessage.SendForActivity(am, dc: dc);
            else if(am.ActivityGroupId.HasValue)
                return ActivityMessage.SendForGroup(am, dc: dc);
            else
                return new SystemMessages() { new SystemMessage() { Text = "Error", MessageType = MessageType.Error } };
        }

        public ActivityMessageRegistrant AddRegistrant(int registrantId, int loggedInUserId, SystemMessages msgs = null, DatabaseConnection dc= null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            var amr = new ActivityMessageRegistrant
            {
                RegistrantId = registrantId,
                ActivityMessageId = this.ID,
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUserId
            };
            msgs.AddRange(amr.Save(dc: dc));
            return amr;
        }

        #endregion
    }
}
