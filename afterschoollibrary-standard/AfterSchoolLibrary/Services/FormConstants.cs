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
        
        public static List<QuestionDataType> QuestionsToIncludeParentPanel()
        {
            return new List<QuestionDataType>
            {
                QuestionDataType.RegistrantFirstName,
                QuestionDataType.RegistrantMiddleName,
                QuestionDataType.RegistrantLastName,
                QuestionDataType.RegistrantEmail,
                QuestionDataType.RegistrantMobilePhone,
                QuestionDataType.RegistrantHomePhone,
                QuestionDataType.RegistrantWorkPhone,
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
                QuestionDataType.RegistrantAddress1,
                QuestionDataType.RegistrantAddress2,
                QuestionDataType.RegistrantCity,
                QuestionDataType.RegistrantState,
                QuestionDataType.RegistrantPostalCode,
                QuestionDataType.Parent1FirstName,
                QuestionDataType.Parent1MiddleName,
                QuestionDataType.Parent1LastName,
                QuestionDataType.Parent1Email,
                QuestionDataType.Parent1MobilePhone,
                QuestionDataType.Parent1HomePhone,
                QuestionDataType.Parent1WorkPhone,
                QuestionDataType.Parent1RelationshipToRegistrant,
                QuestionDataType.Parent1Employer,
                QuestionDataType.Parent1Address1,
                QuestionDataType.Parent1Address2,
                QuestionDataType.Parent1City,
                QuestionDataType.Parent1State,
                QuestionDataType.Parent1PostalCode,
                QuestionDataType.Parent2FirstName,
                QuestionDataType.Parent2MiddleName,
                QuestionDataType.Parent2LastName,
                QuestionDataType.Parent2Email,
                QuestionDataType.Parent2HomePhone,
                QuestionDataType.Parent2WorkPhone,
                QuestionDataType.Parent2RelationshipToRegistrant,
                QuestionDataType.Parent2Employer,
                QuestionDataType.Parent2Address1,
                QuestionDataType.Parent2Address2,
                QuestionDataType.Parent2City,
                QuestionDataType.Parent2State,
                QuestionDataType.Parent2PostalCode,
                QuestionDataType.EmergencyContact1FirstName,
                QuestionDataType.EmergencyContact1LastName,
                QuestionDataType.EmergencyContact1Phone,
                QuestionDataType.EmergencyContact1Phone2,
                QuestionDataType.EmergencyContact1Phone3,
                QuestionDataType.EmergencyContact1RelationshipToRegistrant,
                QuestionDataType.EmergencyContact2FirstName,
                QuestionDataType.EmergencyContact2LastName,
                QuestionDataType.EmergencyContact2Phone,
                QuestionDataType.EmergencyContact2Phone2,
                QuestionDataType.EmergencyContact2Phone3,
                QuestionDataType.EmergencyContact2RelationshipToRegistrant,
                QuestionDataType.EmergencyContact3FirstName,
                QuestionDataType.EmergencyContact3LastName,
                QuestionDataType.EmergencyContact3Phone,
                QuestionDataType.EmergencyContact3Phone2,
                QuestionDataType.EmergencyContact3Phone3,
                QuestionDataType.EmergencyContact3RelationshipToRegistrant,
                QuestionDataType.RegistrantDoctorName,
                QuestionDataType.RegistrantDoctorPhone,
                QuestionDataType.RegistrantPreferredHospital,
                QuestionDataType.RegistrantAllergies,
                QuestionDataType.RegistrantMedicalConditions,
                QuestionDataType.RegistrantImmunizations,
            };
        }

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