namespace AfterSchoolLibrary.Models
{
    public enum BackgroundCheckStatus
    {
        NotSet = 0,
        ManuallyRejected = 1,
        ManuallyApproved = 2,
        BackgroundCheckRequested = 3,
        EmailSentToVolunteer = 4,
        CompletedByVolunteer = 5,
        RequiresReview = 6,
        AutomaticallyApproved = 7,
    }
}