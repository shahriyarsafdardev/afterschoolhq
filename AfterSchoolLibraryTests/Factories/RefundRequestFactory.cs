using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibraryTests.Factories
{
    public class RegistrantActivityFactory
    {
        public static RegistrantActivity Create(Activity activity, Registrant registrant, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new RegistrantActivity()
            {
                ParticipationObjectId = activity.ID,
                ParticipationObjectTypeId = ParticipationObjectType.Activity,
                RegistrantId = registrant.ID
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

     
}
