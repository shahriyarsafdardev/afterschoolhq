using System.Collections.Generic;

namespace AfterSchoolLibrary.Services
{
    public static class FormConstants
    {
        public static List<QuestionDataType> QuestionsToNotIncludeInParentPanel() => new List<QuestionDataType>
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

        public static List<QuestionDataType> QuestionsToNotIncludeInStudentRegistrationPanel() =>
            new List<QuestionDataType>
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

        public static List<QuestionDataType> QuestionsToNotIncludeInStudentRegistrationParentPanel() =>
            new List<QuestionDataType>
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

        public static List<QuestionDataType> QuestionsToNotIncludeInAdditionalParentPanel() =>
            new List<QuestionDataType>
            {
                QuestionDataType.RegistrantOrganization,
                QuestionDataType.RegistrantGrade,
                QuestionDataType.RegistrantGender,
                QuestionDataType.RegistrantDateOfBirth,
                QuestionDataType.RegistrantRaceUsFed,
                QuestionDataType.RegistrantEthnicityUsFed,
                QuestionDataType.RegistrantTShirtSize,
            };

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
    }
}