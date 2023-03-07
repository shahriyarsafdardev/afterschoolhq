using System;
using System.Collections.Generic;
using System.Linq;
using BasicLibrary;

namespace AfterSchoolLibrary.Services
{
    public static class FormConstants
    {
        public static IReadOnlyList<UserType> AdminUserTypes =>
            new[] {UserType.Administrator, UserType.Owner, UserType.SuperAdmin, UserType.Treasurer, UserType.TechSupport};

        public static AccountType[] ProviderAccountTypes => new[]
            {AccountType.Teacher, AccountType.ActivityProvider, AccountType.Business, AccountType.CommunityCenter, AccountType.School, AccountType.Church};

        public static List<AccountType> OrganizationAccountTypes => new List<AccountType>(ProviderAccountTypes) {AccountType.Directory};

        public static List<AccountType> OrganizationAccountTypesAndNotSet => new List<AccountType>(OrganizationAccountTypes) {AccountType.NotSet};

        public static AccountType[] IndividualProviderAccountTypes => new[] {AccountType.Teacher, AccountType.ActivityProvider, AccountType.Business};

        public static AccountType[] LocationProviderAccountTypes =>
            new[] {AccountType.CommunityCenter, AccountType.School, AccountType.Church, AccountType.Directory};

        public static AccountType[] CustomerAccountTypes => new[]
        {
            AccountType.Teacher, AccountType.ActivityProvider, AccountType.Business, AccountType.CommunityCenter, AccountType.School, AccountType.Church,
            AccountType.Registrant, AccountType.SchoolDistrict,AccountType.Volunteer,
        };

        public static List<QuestionDataType> QuestionsToNotIncludeInParentPanel()
        {
            return new List<QuestionDataType>
            {
                QuestionDataType.RegistrantOrganization,
                QuestionDataType.RegistrantGrade,
                QuestionDataType.RegistrantGender,
                QuestionDataType.RegistrantDateOfBirth,
                QuestionDataType.RegistrantRaceUsFed,
                QuestionDataType.RegistrantEthnicityUsFed,
                QuestionDataType.RegistrantTShirtSize,
                QuestionDataType.RegistrantWorkPhone,
                QuestionDataType.RegistrantHomePhone,
                QuestionDataType.RegistrantParentRelationship,
                QuestionDataType.RegistrantEmployer,
                QuestionDataType.StudentId,
            };
        }

        public static List<QuestionDataType> QuestionsToNotIncludeInAllAgeSignUpPanel()
        {
            return new List<QuestionDataType>
            {
                QuestionDataType.RegistrantOrganization,
                QuestionDataType.RegistrantGrade,
                QuestionDataType.RegistrantGender,
                QuestionDataType.RegistrantRaceUsFed,
                QuestionDataType.RegistrantEthnicityUsFed,
                QuestionDataType.RegistrantTShirtSize,
                QuestionDataType.RegistrantWorkPhone,
                QuestionDataType.RegistrantHomePhone,
                QuestionDataType.RegistrantParentRelationship,
                QuestionDataType.RegistrantEmployer,
                QuestionDataType.StudentId,
            };
        }

        public static List<QuestionGroupDataType> QuestionsGroupToNotIncludeInAdultPanel()
        {
            return new List<QuestionGroupDataType>
            {
                QuestionGroupDataType.Parent1Address,
                QuestionGroupDataType.Parent1Information,
                QuestionGroupDataType.Parent2Address,
                QuestionGroupDataType.Parent2Information
            };
        }

        public static List<QuestionDataType> QuestionsToNotIncludeInVolunteerPanel()
        {
            return new List<QuestionDataType>
            {
                QuestionDataType.RegistrantOrganization,
                QuestionDataType.RegistrantGrade,
                QuestionDataType.RegistrantDateOfBirth,
                QuestionDataType.RegistrantRaceUsFed,
                QuestionDataType.RegistrantEthnicityUsFed,
                QuestionDataType.RegistrantTShirtSize,
                QuestionDataType.RegistrantWorkPhone,
                QuestionDataType.RegistrantHomePhone,
                QuestionDataType.RegistrantParentRelationship,
                QuestionDataType.RegistrantEmployer,
                QuestionDataType.StudentId,
            };
        }

        public static List<QuestionDataType> QuestionsToNotIncludeInStudentRegistrationPanel()
        {
            return new List<QuestionDataType>
            {
                QuestionDataType.RegistrantHomePhone,
                QuestionDataType.RegistrantWorkPhone,
                QuestionDataType.RegistrantEthnicityUsFed,
                QuestionDataType.RegistrantRaceUsFed,
                QuestionDataType.RegistrantTShirtSize,
                QuestionDataType.RegistrantOrganization,
                QuestionDataType.RegistrantParentRelationship,
                QuestionDataType.RegistrantEmployer,
            };
        }

        public static List<QuestionDataType> QuestionsToNotIncludeInStudentRegistrationParentPanel()
        {
            return new List<QuestionDataType>
            {
                QuestionDataType.RegistrantMiddleName,
                QuestionDataType.RegistrantHomePhone,
                QuestionDataType.RegistrantWorkPhone,
                QuestionDataType.RegistrantMobilePhone,
                QuestionDataType.RegistrantDateOfBirth,
                QuestionDataType.RegistrantGrade,
                QuestionDataType.RegistrantGender,
                QuestionDataType.RegistrantEthnicityUsFed,
                QuestionDataType.RegistrantRaceUsFed,
                QuestionDataType.RegistrantTShirtSize,
                QuestionDataType.RegistrantOrganization,
                QuestionDataType.RegistrantParentRelationship,
                QuestionDataType.RegistrantEmployer,
                QuestionDataType.StudentId,
            };
        }

        public static List<QuestionDataType> QuestionsToNotIncludeInAdditionalParentPanel()
        {
            return new List<QuestionDataType>
            {
                QuestionDataType.RegistrantOrganization,
                QuestionDataType.RegistrantGrade,
                QuestionDataType.RegistrantGender,
                QuestionDataType.RegistrantDateOfBirth,
                QuestionDataType.RegistrantRaceUsFed,
                QuestionDataType.RegistrantEthnicityUsFed,
                QuestionDataType.RegistrantTShirtSize,
                QuestionDataType.StudentId,
            };
        }

        public static List<QuestionDataType> QuestionsToNotIncludeInStudentPanel(bool isGradeIncluded)
        {
            var list = new List<QuestionDataType>
            {
                QuestionDataType.RegistrantOrganization,
                QuestionDataType.RegistrantGender,
                QuestionDataType.RegistrantDateOfBirth,
                QuestionDataType.RegistrantRaceUsFed,
                QuestionDataType.RegistrantEthnicityUsFed,
                QuestionDataType.RegistrantTShirtSize,
                QuestionDataType.RegistrantHomePhone,
                QuestionDataType.RegistrantWorkPhone,
                QuestionDataType.RegistrantParentRelationship,
                QuestionDataType.RegistrantEmployer,
            };

            if (isGradeIncluded)
                list.Add(QuestionDataType.RegistrantGrade);

            return list;
        }

        public static List<ActivityMessageReason> StudentMessageReasons()
        {
            return Enum.GetValues(typeof(ActivityMessageReason)).Cast<ActivityMessageReason>().Except(ActivityMessageReason.NotSet.AsList()).OrderBy(x => x.ToString()).ToList();
        }

        public static List<ActivityMessageReason> VolunteerMessageReasons()
        {
            return new List<ActivityMessageReason>
            {
                ActivityMessageReason.ActivityCancellationNotice,
                ActivityMessageReason.GeneralUpdate,
                ActivityMessageReason.UrgentUpdate
            };
        }
    }
}