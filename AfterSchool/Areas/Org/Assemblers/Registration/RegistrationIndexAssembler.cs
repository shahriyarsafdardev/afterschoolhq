using System.Collections.Generic;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using BasicLibrary;

namespace AfterSchool.Areas.Org.Assemblers.Registration
{
    public class RegistrationIndexResult
    {
        public bool IsStudentCrmEnabled { get; set; }
        public Package StudentCrmPackage { get; set; }
        public List<AfterSchoolLibrary.Models.Registration> Registrations { get; set; }
        public SystemMessages Messages { get; set; }
        public PagingModel PagingModel { get; set; }
        public List<AfterSchoolLibrary.Models.Activity> Activities { get; set; }
        public List<ActivityGroup> Groups { get; set; }
        public List<FormQuestion> Questions { get; set; }
        public List<FormQuestionGroup> QuestionGroups { get; set; }
        public bool SearchBarReadOnly { get; set; }
        public Account CurrentAccount { get; set; }
    }

    public class RegistrationIndexAssembler
    {
        public RegistrationIndexResult AssembleRegistrationIndexResult(bool isStudentCrmEnabled, Package studentCrmPackage,
            List<AfterSchoolLibrary.Models.Registration> registrations, SystemMessages messages, PagingModel pagingModel,
            List<AfterSchoolLibrary.Models.Activity> activities, List<ActivityGroup> groups, bool searchBarReadOnly, Account currentAccount)
        {
            return new RegistrationIndexResult
            {
                IsStudentCrmEnabled = isStudentCrmEnabled,
                StudentCrmPackage = studentCrmPackage,
                Registrations = registrations,
                Messages = messages,
                PagingModel = pagingModel,
                Activities = activities,
                Groups = groups,
                Questions = new List<FormQuestion>(),
                QuestionGroups = new List<FormQuestionGroup>(),
                SearchBarReadOnly = searchBarReadOnly,
                CurrentAccount = currentAccount,
            };
        }
    }
}