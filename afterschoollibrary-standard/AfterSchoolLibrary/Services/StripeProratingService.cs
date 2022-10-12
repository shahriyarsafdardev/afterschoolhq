using AfterSchoolLibrary.Billing;
using AfterSchoolLibrary.Models;
using BasicLibrary;

namespace AfterSchoolLibrary.Services
{
    public interface IStripeProratingService
    {
        decimal? GetProratingValue(Account currentAccount, SubscriptionPlan plan, SystemMessages msgs);
    }

    public class StripeProratingService : IStripeProratingService
    {
        public decimal? GetProratingValue(Account currentAccount, SubscriptionPlan plan, SystemMessages msgs)
        {
            return currentAccount.GetStripeProratePreview(StripeAccountType.ConnectAccount, plan, msgs);
        }
    }
}