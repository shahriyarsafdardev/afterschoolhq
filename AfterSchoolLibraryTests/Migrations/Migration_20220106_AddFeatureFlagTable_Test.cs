using Microsoft.Data.SqlClient;
using Dapper;
using Xunit;
using Xunit.Abstractions;

namespace AfterSchoolLibraryTests.Migrations
{
    public class Migration_20220106_AddFeatureFlagTable_Test : BaseTest
    {
        public Migration_20220106_AddFeatureFlagTable_Test(ITestOutputHelper output, bool useDatabase = true) : base(output, useDatabase)
        {
        }

        [Fact(Skip = "Use for debugging")]
        public void Test()
        {
            AfterSchoolLibrary.Migrations.Migration_20220106_AddFeatureFlagTable.Do(msgs, dc);
            int result;
            using (var connection = new SqlConnection(dc.ConnectionString))
            {
                result = connection.ExecuteScalar<int>(@"if object_id('FeatureFlag', 'U') is not null
                                                                  declare @result int
                                                                  set @result=1
                                                                  select @result as result");
            }
            Assert.Equal(1, result);
        }
    }
}