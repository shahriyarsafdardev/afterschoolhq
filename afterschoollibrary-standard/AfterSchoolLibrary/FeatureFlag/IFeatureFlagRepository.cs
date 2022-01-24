namespace AfterSchoolLibrary.FeatureFlag
{
    public interface IFeatureFlagRepository
    {
        bool IsEnabled(string flagName);
    }
}