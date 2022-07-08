namespace AfterSchool.Helpers
{
    public class IconForActivityType
    {
        public static string GetIconClassForActivityType(ActivityType activityType)
        {
            switch (activityType)
            {
                case ActivityType.FieldTrip:
                    return "s7-compass";
                case ActivityType.PermissionSlip:
                    return "s7-file";
                case ActivityType.LearningPod:
                    return "s7-box2";
                case ActivityType.VolunteerOpportunity:
                    return "s7-smile";
                case ActivityType.NotSet:
                case ActivityType.Activity:
                case ActivityType.Team:
                default:
                    return "s7-gleam";
            }
        }
    }
}