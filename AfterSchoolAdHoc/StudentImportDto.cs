using BasicLibrary;

namespace AfterSchoolAdHoc;

public class StudentImportDto
{
    public int ID { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string DOB { get; set; }
    public int Gender { get; set; }
    public string Address{ get; set; }
    public string ZipCode{ get; set; }
    public int RegistrantId { get; set; }

    public static GenderType GetGenderType(string genderType)
    {
        switch (genderType)
        {
            case "m":
                return GenderType.Male;
            case "f":
                return GenderType.Female;
            case "o":
                return GenderType.Other;
            default:
                return GenderType.NotSet;
        }
    }
}