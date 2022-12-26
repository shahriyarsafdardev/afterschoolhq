namespace AfterSchoolLibrary.Services
{
    public interface IStripeProratingService
    {
        decimal GetProratingValue();
    }

    public class StripeProratingService : IStripeProratingService
    {
        public decimal GetProratingValue()
        {
            return 3;
        }
    }
}