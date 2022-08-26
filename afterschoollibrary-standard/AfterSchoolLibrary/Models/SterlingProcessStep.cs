namespace AfterSchoolLibrary.Models
{
    public enum SterlingProcessStep
    {
        NotStarted = 0,
        FailedToAuthenticate = 1,
        FailedToCreateCandidate = 2,
        FailedToCreateScreening = 3,
        SuccessfullyCreatedScreening = 4,
    }
}