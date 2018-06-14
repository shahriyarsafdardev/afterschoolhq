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

            dc = new DatabaseConnection(Database.Credentials.ConnectionString);
            msgs = new SystemMessages();
            dc.Open();
            Database.AlterTables(dc);
            dc.BeginTransaction();
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
