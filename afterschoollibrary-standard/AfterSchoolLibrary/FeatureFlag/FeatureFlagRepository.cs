using System.Data.SqlClient;
using AfterSchoolLibrary.Models;
using Dapper;

namespace AfterSchoolLibrary.FeatureFlag
{
    public class FeatureFlagRepository : IFeatureFlagRepository
    {
        private readonly DatabaseCredentials databaseCredentials;

        public FeatureFlagRepository(DatabaseCredentials databaseCredentials)
        {
            this.databaseCredentials = databaseCredentials;
        }
        public bool IsEnabled(string flagName)
        {
            using (var connection = new SqlConnection(databaseCredentials.ConnectionString))
            {
                return connection.QueryFirstOrDefault<bool>(@"select IsEnabled from FeatureFlag where FlagName = @FlagName", new {FlagName = flagName});
            }
        }

        public void Enable(string flagName)
        {
            using (var connection = new SqlConnection(databaseCredentials.ConnectionString))
            {
                connection.Execute(@"if not exists (select * from FeatureFlag where FlagName = @FlagName)
                                        insert into FeatureFlag(FlagName, IsEnabled)
                                        values(@FlagName, 1)
                                    else
                                        update dbo.FeatureFlag
                                        set IsEnabled = 1
                                        where FlagName = @FlagName", new {FlagName = flagName});
            }
        }
    }
}