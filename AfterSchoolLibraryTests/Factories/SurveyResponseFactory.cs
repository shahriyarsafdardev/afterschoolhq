using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibraryTests.Factories
{
    public class SurveyResponseFactory
    {
        public static SurveyResponse Create(Registrant registrant, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new SurveyResponse()
            {
                RegistrantId = registrant.ID,
                SurveyFeedbackStatusTypeId = SurveyFeedbackStatus.Completed,
                IsDeleted = false,
                Uid = new System.Guid()
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
