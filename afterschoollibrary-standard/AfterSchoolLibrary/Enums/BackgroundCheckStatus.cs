namespace AfterSchoolLibrary.Models
{
    public enum BackgroundCheckStatus
    {
        NotSet = 0,
        ManuallyRejected = 1,
        ManuallyApproved = 2,
        BackgroundCheckRequested = 3,
        EmailSentToVolunteer = 4,
        Complete = 5,
        Review = 6,
        AutomaticallyApproved = 7,
        PreDraft = 8,
        Pending = 9,
        Hold = 10,
        AdverseAction = 11,
        Dispute = 12,
        Archive = 13,
    }
}