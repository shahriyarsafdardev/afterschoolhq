using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibraryTests
{
    public class BaseTest : IDisposable
    {
        internal DatabaseConnection dc = null;
        internal SystemMessages msgs = null;
        private readonly ITestOutputHelper _output;

        public BaseTest(ITestOutputHelper output)
        {
            AfterSchoolLibrary.Utilities.InitializeApp();

            _output = output;

            dc = new DatabaseConnection(Database.TestCredentials.ConnectionString);
            msgs = new SystemMessages();
            dc.Open();
            Database.CreateTables(dc);
            Database.AlterTables(dc);
            Database.UpdateData(dc);
            dc.BeginTransaction();

            // need some default data in the AfterSchool admin account in order to run certain tests
            msgs.AddRange(AccountService.CreateAfterSchoolAccount(null, null, null, null, dc));
            msgs.AddRange(GradeLevel.CreateDefaultGradeLevels(dc));
        }

        public void Dispose()
        {
            if (msgs.Count > 0)
                _output.WriteLine(msgs.RenderMessageList(MessageOutputType.Text));

            dc.RollbackTransaction();
            dc.Close();
            dc.Dispose();
        }
    }
}
