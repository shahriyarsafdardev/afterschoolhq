using AfterSchoolLibrary.Models;

namespace AfterSchoolLibrary.Integrations
{
    public interface IBackgroundCheckRepository
    {
        void Save(BackgroundCheck backgroundCheck);
    }

    public class BackgroundCheckRepository : IBackgroundCheckRepository
    {
        public void Save(BackgroundCheck backgroundCheck)
        {
            backgroundCheck.Save();
        }
    }
}