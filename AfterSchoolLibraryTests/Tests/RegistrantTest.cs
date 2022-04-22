using System;
using Xunit;
using Xunit.Abstractions;
using BasicLibrary;
using AfterSchoolLibraryTests.Factories;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using System.Linq;

namespace AfterSchoolLibraryTests.Tests
{
    public class RegistrantTest: BaseTest
    {
        public RegistrantTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void LoadTest()
        {
            var registrant = RegistrantFactory.Create(msgs, dc);

            Assert.True(Registrant.Load(registrant.ID.Value, msgs, dc).IsNotEmpty());
        }

        [Fact]
        public void SaveTest()
        {
            var registrant = RegistrantFactory.Create(msgs, dc);

            registrant.LastModifiedBy = 1;
            msgs.AddRange(registrant.Save(dc));

            Assert.True(msgs.HasNoErrors());
        }
        [Fact]
        public void DeleteTest()
        {
            var registrant = RegistrantFactory.Create(msgs, dc);

            msgs.AddRange(registrant.Delete(dc));

            Assert.Null(Coupon.Load(registrant.ID.Value));
        }

        [Fact]
        public void ReloadTest()
        {
            var registrant = RegistrantFactory.Create(msgs, dc);

            Assert.Equal(registrant.ID, registrant.Reload(msgs, dc).ID);
        }

        [Fact]
        public void LoadByUidByAccountIdTest()
        {
            try
            {
                // Arrange
                var user = WebUserFactory.Create(msgs: msgs, dc: dc);
                var account = AccountFactory.Create(user: user, msgs: msgs, dc: dc);
                var registrant = RegistrantFactory.Create(account, isMaster: false, msgs, dc);

                // Act                
                var expected = Registrant.LoadByUidByAccountId(Uid: registrant.Uid, AccountId: account.ID.Value, msgs: msgs, dc: dc);

                // Assert
                Assert.Equal(expected: expected.ID, actual: registrant.ID);
                Assert.Equal(expected: expected.Uid, actual: registrant.Uid);
                Assert.Equal(expected: expected.Email, actual: registrant.Email);                
            }
            catch (Exception ex)
            {
                //Assert
                Assert.False(false, ex.Message);
            }
        }

        [Fact]
        public void GetSubscriptionCount()
        {
            var providerUser = WebUserFactory.Create(msgs: msgs, dc: dc);
            var registrantUser = WebUserFactory.Create("Vikas", "Lalwani","vikas@gmail.com",true, msgs: msgs, dc: dc);

            var account = AccountFactory.Create(user: providerUser, msgs: msgs, dc: dc);
            var registrantAccount = AccountFactory.Create(registrantUser, AccountType.Registrant, msgs, dc);

            var registrant = RegistrantFactory.Create(registrantAccount, isMaster: false, msgs, dc);
            var activity = ActivityFactory.Create(account, msgs, dc);

            var subscriptionPlan = SubscriptionPlanFactory.Create(account, activity, SubscriptionPlanType.Subscription, msgs, dc);
            var purchase = PurchaseFactory.Create(registrantAccount, 100, msgs, dc);

            RegistrantActivityFactory.Create(activity, registrant, msgs,dc);

            RegistrantSubscriptionPlanFactory.Create(registrant, subscriptionPlan, purchase, msgs, dc);


            var registrants = activity.GetRegistrants(msgs, dc).FirstOrDefault();
            var participationObject = registrants.GetParticipationObjects(msgs, dc).FirstOrDefault();
            var subPlans = registrants.GetRegistrantSubscriptionPlans(msgs, dc);
            var createTransaction = TransactionFactory.Create(account, registrantAccount, PaymentType.CreditCard, msgs, dc);
            subPlans.FirstOrDefault().AddPayment(createTransaction, 50, registrantUser, msgs, dc);

            var subCount = registrant.GetSubscriptionCount(participationObject, msgs, dc);

            Assert.True(subCount > 0);
        }

        [Fact]
        public void GetValueTest()
        {
            //Arrage
            var user = Factories.WebUserFactory.Create(true, msgs, dc);
            var account = Factories.AccountFactory.Create(user, msgs, dc);
            var registrant = RegistrantFactory.Create(account, false, msgs, dc);
            var surveyResponse = SurveyResponseFactory.Create(registrant, msgs, dc);
            var form = Factories.FormFactory.Create(account, true, msgs, dc);

            var formQuestionGroup = FormQuestionGroupFactory.Create(form.ID.Value, QuestionGroupDataType.PersonalInformation, false, msgs, dc);

            var formQuestion = FormQuestionFactory.Create(form.ID.Value, formQuestionGroup.ID.Value, "Gender", "Male,Female,Other", QuestionType.RadioList, false, msgs, dc);

            var formQuestionAnswer = FormQuestionAnswerFactory.Create(formQuestion.ID.Value, registrant.ID.Value, surveyResponse.ID.Value, formQuestion.Text, "Male", formQuestion.QuestionDataTypeId, msgs, dc);

            //Act
            var expected = registrant.GetValue(formQuestionAnswer.QuestionDataTypeId, msgs, dc);

            //Assert
            Assert.Equal(expected, "Male");
        }

        [Fact]
        public void DeleteWithRelatedRecordsTest()
        {
                // Arrange
                var user = WebUserFactory.Create(msgs: msgs, dc: dc);
                var account = AccountFactory.Create(user: user, msgs: msgs, dc: dc);
                var registrant = RegistrantFactory.Create(account, isMaster: false, msgs, dc);

                // Act                
                registrant.DeleteWithRelatedRecords(msgs, dc);

                // Assert
                Assert.True(registrant.IsDeleted);
        }

        [Fact]
        public void AddOrUpdateParticipationObjectTest()
        {
                // Arrange
                var user = WebUserFactory.Create(msgs: msgs, dc: dc);
                var account = AccountFactory.Create(user: user, msgs: msgs, dc: dc);
                var registrant = RegistrantFactory.Create(account, isMaster: false, msgs, dc);
                var registration = RegistrationFactory.Create(account, registrant, msgs, dc);
                var activity = ActivityFactory.Create(account, 20, msgs, dc);
                var participationObject = new ParticipationObject(activity, msgs, dc);

                // Act                
                var expected = registrant.AddOrUpdateParticipationObject(registration.ID.Value,participationObject.ObjectID.Value, ParticipationObjectType.Activity,RegistrationAssignmentType.Applied,null,null,100,user.ID.Value, msgs, dc);

                // Assert
                Assert.NotNull(expected);
                Assert.True(expected.IsDeleted == false);
        }

    }

}
