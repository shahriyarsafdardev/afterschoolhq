using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchoolLibraryTests.Factories;
using System.Linq;

namespace AfterSchoolLibraryTests.Tests
{
    public class AccountTest : BaseTest
    {
        public AccountTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact(DisplayName = "Load 1 account")]
        public void LoadTest()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            Assert.NotNull(Account.Load(account.ID.Value, msgs, dc));
        }

        [Theory(DisplayName = "Load accounts with paging")]
        [InlineData(8, 10)]
        [InlineData(15, 10)]
        public void LoadPagingTest(int numberToCreate, int numberPerPage)
        {
            var originalCount = Account.GetCount(msgs: msgs, dc: dc);
            for( var i = 0; i < numberToCreate; i++)
            {
                var accountOwner = WebUserFactory.Create(msgs, dc);
                var account = AccountFactory.Create(accountOwner, msgs, dc);
            }

            Assert.Equal(
                Math.Min(numberPerPage, numberToCreate + originalCount), 
                Account.Load(null, 0, numberPerPage, msgs, dc).Count
            );
        }

        //Gets the Number of accounts in the DB
        [Theory(DisplayName = "Get # of accounts in db")]
        [InlineData(8)]
        [InlineData(2)]
        public void GetCountTest(int numberToCreate)
        {
            var originalCount = Account.GetCount(msgs: msgs, dc: dc);
            for (var i = 0; i < numberToCreate; i++)
            {
                var accountOwner = WebUserFactory.Create(msgs, dc);
                var account = AccountFactory.Create(accountOwner, msgs, dc);
            }

            Assert.Equal(originalCount + numberToCreate, Account.GetCount(msgs: msgs, dc: dc));
        }

        /// <summary>
        /// saves the account to the DB
        /// </summary>
        [Fact(DisplayName = "Save account")]
        public void SaveTest()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            account.AccountTypeId = AccountType.ActivityProvider;
            msgs.AddRange(account.Save(dc));

            Assert.True(msgs.HasNoErrors());
        }

        /// <summary>
        /// removes the account from the DB
        /// </summary>
        [Fact(DisplayName = "Delete an account")]
        public void DeleteTest()
        {
            var originalCount = Account.GetCount(msgs: msgs, dc: dc);
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);

            msgs.AddRange(account.Delete(dc));

            Assert.Equal(originalCount, Account.GetCount(msgs: msgs, dc: dc));
        }

        /// <summary>
        /// loads the account using its own ID
        /// </summary>
        [Fact(DisplayName = "Reload account")]
        public void ReloadTest()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            Assert.Equal(account.ID, account.Reload(msgs, dc).ID);
        }

        [Fact(DisplayName = "Load account by Owner")]
        public void LoadByOwnerIdTest()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);

            Assert.Single(Account.LoadByOwnerId(accountOwner.ID.Value, msgs, dc));
        }

        [Fact(DisplayName = "Load account by unique name")]
        public void LoadByUniqueNameTest()
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, msgs, dc);
            account.UniqueName = AccountService.GenerateUniqueName(account.Name, account.ID, msgs, dc);
            msgs.AddRange(account.Save(dc));

            Assert.Equal(account.ID, Account.LoadByUniqueName(account.UniqueName, msgs, dc).ID);
        }

        /// <summary>
        /// testing for admin account which is changing to use AccountType in the future
        /// </summary>
        [Theory(DisplayName = "Load accounts by account type")]
        [InlineData(AccountType.ActivityProvider)]
        [InlineData(AccountType.School)]
        public void LoadByAccountTypeId(AccountType accountType)
        {
            var accountOwner = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(accountOwner, accountType, msgs, dc);

            Assert.NotNull(Account.LoadByAccountTypeId(accountType, msgs, dc));
        }

        [Fact(DisplayName = "Load account by UID")]
        public void LoadByUidTest()
        {
            var user = WebUserFactory.Create(msgs, dc);
            var account = AccountFactory.Create(user, msgs, dc);
            Assert.NotNull(Account.LoadByUid(account.Uid, msgs, dc));
        }

        [Fact]
        public void CheckIfRegistrantTShirtSizeOptionsExistsTest()
        {
            var administratorAccountForm = Account.LoadByAccountTypeId(AccountType.AfterSchool, msgs, dc).First().GetDefaultForm(msgs, dc);
            var personalInformationQuestionGroup = administratorAccountForm.GetQuestionGroups(msgs, dc).First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.PersonalInformation);
            var questions = personalInformationQuestionGroup.GetAllQuestions(msgs, dc);

            Assert.Contains(questions, f => f.QuestionDataTypeId == QuestionDataType.RegistrantTShirtSize);
        }

        [Fact]
        public void CheckNewRegistrantTShirtSizeOptionsTest()
        {
            var administratorAccountForm = Account.LoadByAccountTypeId(AccountType.AfterSchool, msgs, dc).First().GetDefaultForm(msgs, dc);
            var personalInformationQuestionGroup = administratorAccountForm.GetQuestionGroups(msgs, dc).First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.PersonalInformation);
            var questions = personalInformationQuestionGroup.GetAllQuestions(msgs, dc);
            var question = questions.Where(f => f.QuestionDataTypeId == QuestionDataType.RegistrantTShirtSize).FirstOrDefault();

            Assert.Contains("Youth XS,Youth S,Youth M,Youth L,Youth XL,Adult S,Adult M,Adult L,Adult XL,Adult 2XL, Adult 3XL", question.Options);
        }

        [Fact]
        public void RegistrantMiddleNameExistsTest()
        {
            var administratorAccountForm = Account.LoadByAccountTypeId(AccountType.AfterSchool, msgs, dc).First().GetDefaultForm(msgs, dc);
            var personalInformationQuestionGroup = administratorAccountForm.GetQuestionGroups(msgs, dc).First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.PersonalInformation);
            var questions = personalInformationQuestionGroup.GetAllQuestions(msgs, dc);

            Assert.Contains(questions, f => f.QuestionDataTypeId == QuestionDataType.RegistrantMiddleName);
        }

        [Fact]
        public void Parent1InformationMiddleNameExistsTest()
        {
            var administratorAccountForm = Account.LoadByAccountTypeId(AccountType.AfterSchool, msgs, dc).First().GetDefaultForm(msgs, dc);
            var personalInformationQuestionGroup = administratorAccountForm.GetQuestionGroups(msgs, dc).First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.Parent1Information);
            var questions = personalInformationQuestionGroup.GetAllQuestions(msgs, dc);

            Assert.Contains(questions, f => f.QuestionDataTypeId == QuestionDataType.Parent1MiddleName);
        }

        [Fact]
        public void Parent2InformationMiddleNameExistsTest()
        {
            var administratorAccountForm = Account.LoadByAccountTypeId(AccountType.AfterSchool, msgs, dc).First().GetDefaultForm(msgs, dc);
            var personalInformationQuestionGroup = administratorAccountForm.GetQuestionGroups(msgs, dc).First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.Parent2Information);
            var questions = personalInformationQuestionGroup.GetAllQuestions(msgs, dc);

            Assert.Contains(questions, f => f.QuestionDataTypeId == QuestionDataType.Parent2MiddleName);
        }
        [Fact]
        public void GetQuestionsParentRegistrationStep2Test()
        {
            var administratorAccountForm = Account.LoadByAccountTypeId(AccountType.AfterSchool, msgs, dc).First().GetDefaultForm(msgs, dc);
            var homeAddressQuestionGroup = administratorAccountForm.GetQuestionGroups(msgs, dc).First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.HomeAddress);
            var questions = homeAddressQuestionGroup.GetAllQuestions(msgs, dc);

            Assert.Contains(questions, f => f.QuestionDataTypeId == QuestionDataType.RegistrantWorkPhone);
            Assert.Contains(questions, f => f.QuestionDataTypeId == QuestionDataType.RegistrantHomePhone);
        }

        [Fact]
        public void Parent1InformationEmployerExistsTest()
        {
            var administratorAccountForm = Account.LoadByAccountTypeId(AccountType.AfterSchool, msgs, dc).First().GetDefaultForm(msgs, dc);
            var personalInformationQuestionGroup = administratorAccountForm.GetQuestionGroups(msgs, dc).First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.Parent1Information);
            var questions = personalInformationQuestionGroup.GetAllQuestions(msgs, dc);

            Assert.Contains(questions, f => f.QuestionDataTypeId == QuestionDataType.Parent1Employer);
        }

        [Fact]
        public void Parent1InformationRelationshipToRegistrantExistsTest()
        {
            var administratorAccountForm = Account.LoadByAccountTypeId(AccountType.AfterSchool, msgs, dc).First().GetDefaultForm(msgs, dc);
            var personalInformationQuestionGroup = administratorAccountForm.GetQuestionGroups(msgs, dc).First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.Parent1Information);
            var questions = personalInformationQuestionGroup.GetAllQuestions(msgs, dc);

            Assert.Contains(questions, f => f.QuestionDataTypeId == QuestionDataType.Parent1RelationshipToRegistrant);
        }

        [Fact]
        public void Parent2InformationEmployerExistsTest()
        {
            var administratorAccountForm = Account.LoadByAccountTypeId(AccountType.AfterSchool, msgs, dc).First().GetDefaultForm(msgs, dc);
            var personalInformationQuestionGroup = administratorAccountForm.GetQuestionGroups(msgs, dc).First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.Parent2Information);
            var questions = personalInformationQuestionGroup.GetAllQuestions(msgs, dc);

            Assert.Contains(questions, f => f.QuestionDataTypeId == QuestionDataType.Parent2Employer);
        }

        [Fact]
        public void Parent2InformationRelationshipToRegistrantExistsTest()
        {
            var administratorAccountForm = Account.LoadByAccountTypeId(AccountType.AfterSchool, msgs, dc).First().GetDefaultForm(msgs, dc);
            var personalInformationQuestionGroup = administratorAccountForm.GetQuestionGroups(msgs, dc).First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.Parent2Information);
            var questions = personalInformationQuestionGroup.GetAllQuestions(msgs, dc);

            Assert.Contains(questions, f => f.QuestionDataTypeId == QuestionDataType.Parent2RelationshipToRegistrant);
        }
    }
}
