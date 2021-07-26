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
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;

namespace AfterSchoolLibraryTests
{
    public class BaseTest : IDisposable
    {
        internal DatabaseConnection dc = null;
        internal SystemMessages msgs = null;
        private bool UseDatabase;

        private readonly ITestOutputHelper _output;

        public BaseTest(ITestOutputHelper output, bool useDatabase = true)
        {
            _output = output;
            UseDatabase = useDatabase;

            if (UseDatabase)
            {
                AfterSchoolLibrary.Helpers.Utilities.InitializeApp();

                dc = new DatabaseConnection(Database.LocalTestCredentials.ConnectionString);
                msgs = new SystemMessages();
                dc.Open();
                Database.CreateTables(dc);
                Database.AlterTables(dc);
                Database.UpdateData(dc);
                dc.BeginTransaction();

                // clean up data, sometimes tests run and data is stored
                // temp work around
                var dq = new DatabaseQuery();
                dq.Connection = dc;
                var sql = "EXEC sys.sp_msforeachtable 'DELETE FROM ?'";
                dq.ExecuteSql(sql);

                // need some default data in the AfterSchool admin account in order to run certain tests
                msgs.AddRange(AccountService.CreateAfterSchoolAccount(null, null, null, null, dc));
                msgs.AddRange(GradeLevel.CreateDefaultGradeLevels(dc));
            }
        }

        public void Dispose()
        {
            if (UseDatabase)
            {
                if (msgs.Count > 0)
                    _output.WriteLine(msgs.RenderMessageList(MessageOutputType.Text));

                dc.RollbackTransaction();
                dc.Close();
                dc.Dispose();
            }
        }
    }
}
