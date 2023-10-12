using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AfterSchoolLibrary.Enums;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Twilio.Rest.Preview.TrustedComms;

namespace AfterSchoolLibrary.Models
{
    [Serializable]
    public class FormQuestionSettings : BasicObjectWithModified
    {
        public FormQuestionSettings()
        {
            Config(ObjectMap, DatabaseQuery, nameof(FormQuestionSettings), "ID");
            LoadObjectValues(ObjectMap);
            Audience = AudienceType.NotSet;
            Show = false;
            Required = false;
        }
        public int QuestionId
        {
            get => (int) MDV["QuestionId"].Value;
            set => MDV["QuestionId"].Value = value;
        }

        public AudienceType Audience
        {
            get => (AudienceType) MDV["Audience"].Value;
            set => MDV["Audience"].Value = value;
        }

        public bool Show
        {
            get => (bool) MDV["Show"].Value;
            set => MDV["Show"].Value = value;
        }

        public bool Required
        {
            get => (bool) MDV["Required"].Value;
            set => MDV["Required"].Value = value;
        }

        public static DatabaseParameters ObjectMap
        {
            get
            {
                const string className = nameof(FormQuestionSettings);
                var dp = get_BaseObjectMap(className);
                dp.Add(className, "QuestionId", SqlDbType.Int);
                dp.Add(className, "Audience", SqlDbType.Int);
                dp.Add(className, "Show", SqlDbType.Bit);
                dp.Add(className, "Required", SqlDbType.Bit);
                return dp;
            }
        }

        protected static DatabaseQuery DatabaseQuery
        {
            get
            {
                var dq = get_DatabaseQuery(ObjectMap, nameof(FormQuestionSettings));
                return dq;
            }
        }

        public static FormQuestionSettings LoadObject(DataRow row)
        {
            var obj = new FormQuestionSettings();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        public static FormQuestionSettings Load(int id, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }
        
        public static FormQuestionSettings LoadByQuestionIdByAudienceType(int? id, AudienceType type, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["QuestionId"], id);
            dq.Criterias.Add(ObjectMap["Audience"], type);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public SystemMessages Delete(DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(FormQuestionSettings).Name);
        }

        public static List<FormQuestionSettings> Load(Hashtable criteria = null, int? start = null, int? length = null, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return BaseLoad(criteria, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<FormQuestionSettings> LoadByQuestionIds(int? QuestionId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.Criterias.Add(ObjectMap["QuestionId"], QuestionId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<FormQuestionSettings> Copy(int questionId, int newQuestionId, int loggedInUserId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var formQuestionSettings = FormQuestionSettings.LoadByQuestionIds(questionId, msgs, dc);

            List<FormQuestionSettings> newFormQuestionSettings = new List<FormQuestionSettings>();
           
            formQuestionSettings.ForEach(questionSetting =>
            {
                var newQuestionSetting = new FormQuestionSettings
                {
                    CreatedBy = loggedInUserId,
                    QuestionId = newQuestionId,
                    Show = questionSetting.Show,
                    Audience = questionSetting.Audience,
                    Required = questionSetting.Required
                };

                newFormQuestionSettings.Add(
                   newQuestionSetting
                );

                msgs.AddRange(newQuestionSetting.Save(dc: dc));
            });

            return newFormQuestionSettings;
        }

        public static SystemMessages Create(DatabaseConnection dc = null)
        {
            return _Create(DatabaseQuery, dc, ObjectMap);
        }

        public static SystemMessages Alter(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters { ObjectMap["QuestionId"] }));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters { ObjectMap["Audience"] }));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters { ObjectMap["Show"] }));
            msgs.AddRange(_Alter(DatabaseQuery, dc, new DatabaseParameters { ObjectMap["Required"] }));

            return msgs;
        }

        public SystemMessages Save(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, nameof(FormQuestionSettings)));
            return msgs;
        }
    }
}