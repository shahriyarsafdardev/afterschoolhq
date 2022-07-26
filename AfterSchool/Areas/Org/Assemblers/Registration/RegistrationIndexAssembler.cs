using System.Collections.Generic;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using BasicLibrary;
namespace AfterSchool.Areas.Org.Assemblers.Registration
{
    public class RegistrationIndexResult
    {
        public bool IsStudentCRMEnabled;
        public Package StudentCRMPackage;
        public List<AfterSchoolLibrary.Models.Registration> Registrations { get; set; }
        public SystemMessages Messages { get; set; }
        public PagingModel PagingModel { get; set; }
        public bool IsFullScreen { get; set; }
        public List<AfterSchoolLibrary.Models.Activity> Activities { get; set; }
        public List<ActivityGroup> Groups { get; set; }
        public List<FormQuestion> Questions { get; set; }
        public List<FormQuestionGroup> QuestionGroups { get; set; }
        public bool SearchBarReadOnly { get; set; }
        public Account CurrentAccount { get; set; }
    }
    public class RegistrationIndexAssembler
    {
        public RegistrationIndexResult AssembleRegistrationIndexResult(bool isStudentCRMEnabled, Package studentCRMPackage,
            List<AfterSchoolLibrary.Models.Registration> registrations, SystemMessages messages, PagingModel pagingModel, bool isFullScreen,
            List<AfterSchoolLibrary.Models.Activity> activities, List<ActivityGroup> groups, List<FormQuestion> questions,
            List<FormQuestionGroup> questionGroups, bool searchBarReadOnly, Account currentAccount)
        {
            return new RegistrationIndexResult
            {
                IsStudentCRMEnabled = isStudentCRMEnabled,
                StudentCRMPackage = studentCRMPackage,
                Registrations = registrations,
                Messages = messages,
                PagingModel = pagingModel,
                IsFullScreen = isFullScreen,
                Activities = activities,
                Groups = groups,
                Questions = questions,
                QuestionGroups = questionGroups,
                SearchBarReadOnly = searchBarReadOnly,
                CurrentAccount = currentAccount,
            };
        }
    }
}