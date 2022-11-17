using BasicLibrary;

namespace AfterSchoolLibrary.Models
{
    public class VolunteerTask : BaseObject
    {
        public int? WebUserId
        {
            get => (int) MDV["WebUserId"].Value;
            set => MDV["WebUserId"].Value = value;
        }
    }
}