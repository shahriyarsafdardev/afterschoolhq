using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Collections;
using AfterSchoolLibrary.Enums;
using System.Linq;
using Stripe;

namespace AfterSchoolAdHoc
{
    internal class FormDataFix
    {
        public static void Run(SystemMessages msgs, DatabaseConnection dc)
        {
            Registrant.Alter(dc);
            FormQuestionAnswer.Alter(dc);
            Registrant.Update(new Hashtable() { { "FormDataFixProcessed", 0 } }, new Hashtable() { { "FormDataFixProcessed", DBNull.Value } }, Registrant.ObjectMap, dc);

            var loop = true;
            var count = 0;
            var accountId = 0;

            while (loop)
            {
                Database.ExecuteTransaction
                (
                    action: (amsgs, adc) =>
                    {
                        var registrants = Registrant.Load
                        (
                            criterias: new System.Collections.Hashtable()
                            {
                                { "IsMaster", 0 },
                                { "PersonTypeId", $"{(int)RegistrantPersonType.Student}" },
                                { "FormDataFixProcessed", 0 },
                            },
                            orderBys: new List<string>() { "AccountId" },
                            start: 0,
                            length: 1,
                            msgs: amsgs,
                            dc: adc
                        );

                        var correctQuestionsCollection = new Dictionary<int, List<FormQuestion>>();
                        var i = 0;

                        foreach (var registrant in registrants)
                        {
                            i++;

                            // clear question cache for new accounts, save ram
                            if (accountId != registrant.AccountId!.Value)
                            {
                                correctQuestionsCollection = new Dictionary<int, List<FormQuestion>>();
                                accountId = registrant.AccountId.Value;
                            }

                            accountId = registrant.AccountId.Value;
                            var answers = registrant.GetAnswers(amsgs, adc);
                            var registrationIds = answers.Where(x => x.RegistrationId.HasValue).Select(x => x.RegistrationId!.Value).Distinct().ToList();

                            foreach (var registrationId in registrationIds)
                            {
                                var registrationAnswers = answers.Where(x => x.RegistrationId == registrationId).ToList();

                                if (registrationAnswers.Any(x => x.OldFormQuestionId.HasValue))
                                    continue;

                                var formIds = registrationAnswers.Select(x => x.FormQuestionFormId!.Value).Distinct().ToList();
                                
                                if (formIds.Count == 1)
                                {
                                    var form = Form.Load(formIds[0], amsgs, adc);
                                    var isIncorrectForm = form?.AccountId != registrant.AccountId;

                                    if (isIncorrectForm)
                                    {
                                        var account = registrant.GetAccount(amsgs, adc);
                                        var accountForms = account.GetForms(null, amsgs, adc).Where(x => x.FormTypeId == FormType.StudentForm).ToList();

                                        if (accountForms.Count == 1)
                                        {
                                            var correctForm = accountForms[0];

                                            UpdateAnswers(correctForm, registrationAnswers, ref correctQuestionsCollection, amsgs, adc);
                                        }
                                        else
                                        {
                                            // there's a chance that there isn't one form on the account,
                                            // so then we need to check the activity form
                                            // but that can also have more than one form,
                                            // so we need to see when the person registered in relationship to that form
                                            // they also could've registered for an activity group?

                                            var registrantActivities = registrant.GetRegistrantActivities(amsgs, adc);
                                            RegistrantActivity selectedRegistrantActivity = null;

                                            if (registrantActivities.Count == 1)
                                                selectedRegistrantActivity = registrantActivities[0];
                                            else if (registrantActivities.Count > 1)
                                                // get from first registration because that is what the user initially registered for
                                                selectedRegistrantActivity = registrantActivities.OrderBy(x => x.CreateDate!.Value).First();

                                            if (selectedRegistrantActivity.IsNotEmpty())
                                                GetFormFromRegistrantActivity
                                                (
                                                    registrant,
                                                    selectedRegistrantActivity!,
                                                    registrationAnswers,
                                                    ref correctQuestionsCollection,
                                                    amsgs,
                                                    adc
                                                );
                                        }
                                    }
                                }
                                else if (formIds.Count > 1)
                                {
                                    var forms = new List<Form>();

                                    foreach (var formId in formIds)
                                        forms.Add(Form.Load(formId, amsgs, adc));

                                    var correctForm = forms.Where(x => x.AccountId == registrant.AccountId).FirstOrDefault();

                                    if (correctForm.IsNotEmpty())
                                        UpdateAnswers(correctForm!, registrationAnswers, ref correctQuestionsCollection, amsgs, adc);
                                }
                            }

                            registrant.FormDataFixProcessed = true;
                            amsgs.AddRange(registrant.Save(adc));
                        }

                        if (registrants.Count == 0)
                            loop = false;                        

                    },
                    onSuccess: (smsgs) => msgs.AddRange(smsgs),
                    onFailure: (fmsgs) =>
                    {
                        msgs.AddRange(fmsgs);
                    },
                    onException: (emsgs) => 
                    {
                        AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs);
                    },
                    dc: dc
                );

                count++;
            }
        }

        private static void GetFormFromRegistrantActivity(Registrant registrant, RegistrantActivity registrantActivity, List<FormQuestionAnswer> registrationAnswers, 
            ref Dictionary<int, List<FormQuestion>> correctQuestionsCollection, SystemMessages amsgs, DatabaseConnection adc)
        {
            if (registrantActivity.ParticipationObjectTypeId == ParticipationObjectType.Activity)
            {
                var activity = registrantActivity.GetActivity(amsgs, adc);

                if (activity.IsNotEmpty())
                {
                    var forms = activity.GetForms(amsgs, adc);

                    if (forms.Count == 1)
                    {
                        var correctForm = forms[0];
                        UpdateAnswers(correctForm, registrationAnswers, ref correctQuestionsCollection, amsgs, adc);
                    }
                    else if (forms.Count > 1)
                    {
                        var activityForms = activity.GetActivityForms(amsgs, adc);
                        var correctActivityForm = activityForms.Where(x => x.CreateDate!.Value.IsBefore(registrant.CreateDate!.Value)).OrderByDescending(x => x.CreateDate!.Value).FirstOrDefault();

                        if (correctActivityForm.IsNotEmpty())
                        {
                            var correctForm = forms.Where(x => x.ID == correctActivityForm!.FormId).FirstOrDefault();
                            UpdateAnswers(correctForm!, registrationAnswers, ref correctQuestionsCollection, amsgs, adc);
                        }
                    }
                }
            }
            else if (registrantActivity.ParticipationObjectTypeId == ParticipationObjectType.ActivityGroup)
            {
                var activityGroup = registrantActivity.GetActivityGroup(amsgs, adc);

                if (activityGroup.IsNotEmpty())
                {
                    var correctForm = activityGroup.GetForm(amsgs, adc);
                    UpdateAnswers(correctForm, registrationAnswers, ref correctQuestionsCollection, amsgs, adc);
                }
            }
            else if (registrantActivity.ParticipationObjectTypeId == ParticipationObjectType.AccountActivityGroup)
            {
                var accountActivityGroup = registrantActivity.GetAccountActivityGroup(amsgs, adc);

                if (accountActivityGroup.IsNotEmpty())
                {
                    var correctForm = accountActivityGroup.GetForm(amsgs, adc);
                    UpdateAnswers(correctForm, registrationAnswers, ref correctQuestionsCollection, amsgs, adc);
                }
            }
        }

        private static void UpdateAnswers(Form correctForm, List<FormQuestionAnswer> answers, ref Dictionary<int, List<FormQuestion>> correctQuestionsCollection, SystemMessages amsgs, DatabaseConnection adc)
        {
            if (correctForm.IsNotEmpty())
            {
                var correctQuestions = GetCorrectQuestions(correctForm, ref correctQuestionsCollection, amsgs, adc);

                foreach (var answer in answers)
                {
                    if (answer.FormQuestionFormId != correctForm.ID)
                    {
                        var correctFormQuestion = correctQuestions.Where(x => x.QuestionDataTypeId == answer.QuestionDataTypeId).FirstOrDefault();

                        if (correctFormQuestion.IsNotEmpty())
                        {
                            answer.OldFormQuestionId = answer.FormQuestionId;
                            answer.FormQuestionId = correctFormQuestion!.ID;
                            amsgs.AddRange(answer.Save(adc));
                        }
                    }
                }

            }
        }

        private static List<FormQuestion> GetCorrectQuestions(Form correctForm, ref Dictionary<int, List<FormQuestion>> correctQuestionsCollection, SystemMessages amsgs, DatabaseConnection adc)
        {
            var correctQuestions = new List<FormQuestion>();

            if (correctQuestionsCollection.ContainsKey(correctForm.ID.Value))
            {
                correctQuestions = correctQuestionsCollection[correctForm.ID.Value];
            }
            else
            {
                correctQuestions = correctForm.GetAllQuestions(amsgs, adc);
                correctQuestionsCollection.Add(correctForm.ID.Value, correctQuestions);
            }

            return correctQuestions;
        }


    }
}
