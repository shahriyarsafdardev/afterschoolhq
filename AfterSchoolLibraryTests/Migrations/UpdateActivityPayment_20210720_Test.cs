using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace AfterSchoolLibraryTests.Migrations
{
    public class UpdateActivityPayment_20210720_Test : BaseTest
    {
        private WebUser ParentWebUser { get; set; }
        private WebUser ProviderWebUser { get; set; }
        private Account ParentAccount { get; set; }
        private Account ProviderAccount { get; set; }

        public UpdateActivityPayment_20210720_Test(ITestOutputHelper output) : base(output)
        {
            ParentWebUser = WebUserFactory.Create(msgs, dc);
            ParentAccount = AccountFactory.CreateWithFreePackage(ParentWebUser, AccountType.School, msgs, dc);
            ProviderWebUser = WebUserFactory.Create(msgs, dc);
            ProviderAccount = AccountFactory.CreateWithFreePackage(ProviderWebUser, AccountType.ActivityProvider, msgs, dc);
            
        }

        [Fact]
        public void RunMigration()
        {    
            try
            {
                // Arrange
                if (msgs.IsEmpty()) msgs = new SystemMessages();

                AfterSchoolLibrary.Helpers.Utilities.AdditionalData(dc);
                var registrant = RegistrantFactory.Create(ParentAccount, false, msgs, dc);
                var activity = ActivityFactory.Create(ProviderAccount, msgs, dc);
                var transaction = TransactionFactory.Create(ProviderAccount, ParentAccount, PaymentType.ACH, msgs, dc);
                var activityPayment = ActivityPaymentFactory.Create(ProviderAccount, activity, registrant, transaction, msgs, dc);

                // Act                   
                AfterSchoolLibrary.Migrations.UpdateActivityPayment_20210720.Do(msgs, dc);

                var expectedActivityPayment = ActivityPayment.Load(activityPayment.ID.Value, msgs, dc);
                
                // Assert
                Assert.True(msgs.HasNoErrors());
                Assert.NotNull(expectedActivityPayment);
                Assert.Equal(expected: transaction.Amount.Value, actual: expectedActivityPayment.AmountPaidByParent.Value);
                Assert.Equal(expected: transaction.GetBillerAmount(msgs, dc), actual: expectedActivityPayment.Amount.Value);
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }
    }
}
       