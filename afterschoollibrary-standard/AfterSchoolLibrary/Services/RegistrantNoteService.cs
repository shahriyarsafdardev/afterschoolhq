using System.Linq;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Services
{
    public static class RegistrantNoteService
    {
        public static RegistrantNoteVersions GetLatestVersion(this RegistrantNote registrantNote, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return RegistrantNoteVersions.LoadByNoteIdByIsDeleted(registrantNote.ID.Value, false, msgs, dc)
                .OrderByDescending(rnv => rnv.CreateDate).FirstOrDefault();
        }

        public static WebUser GetCreator(this RegistrantNote registrantNote, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return WebUser.Load(registrantNote.CreatedBy.Value, msgs, dc);
        }
    }
}