namespace AfterSchool.Models
{
    public class Utilities
    {
        public static bool IsAfterSchool()
        {
            SessionVariables.IsAfterSchool = true;
            return SessionVariables.IsAfterSchool.Value;
        }

        public static void DetectAfterSchool()
        {
            if (!SessionVariables.IsAfterSchool.HasValue)
            {
                IsAfterSchool();
            }
        }

        public static void SetAfterSchool(bool isAfterSchool)
        {
            SessionVariables.IsAfterSchool = isAfterSchool;
        }
    }
}
