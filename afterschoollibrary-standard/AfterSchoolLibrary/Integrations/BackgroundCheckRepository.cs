using AfterSchoolLibrary.Models;
using BasicLibrary;

namespace AfterSchoolLibrary.Integrations
{
    public interface IBackgroundCheckRepository
    {
        SystemMessages Save(BackgroundCheck backgroundCheck);
    }

    public class BackgroundCheckRepository : IBackgroundCheckRepository
    {
        public SystemMessages Save(BackgroundCheck backgroundCheck)
        {
            return backgroundCheck.Save();
        }
    }
}