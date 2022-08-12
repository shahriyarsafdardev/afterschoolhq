using AfterSchoolLibrary.Models;

namespace AfterSchoolLibrary.Integrations
{
    public interface IWebUserRepository
    {
        WebUser LoadById(int id);
    }

    public class WebUserRepository : IWebUserRepository
    {
        public WebUser LoadById(int id)
        {
            return WebUser.Load(id);
        }
    }
}