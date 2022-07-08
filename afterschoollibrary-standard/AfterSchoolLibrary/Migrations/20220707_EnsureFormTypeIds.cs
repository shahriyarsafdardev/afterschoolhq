using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections;
using System.Collections.Generic;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20220707_EnsureFormTypeIds
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var forms = Form.Load(
                                            criterias: new Hashtable() { {"FormTypeId", (int)FormType.NotSet} }, 
                                            msgs: amsgs, 
                                            dc: adc);

                    forms.ForEach(form =>
                    {
                        form.FormTypeId = FormType.StudentForm;
                        amsgs.AddRange(form.Save(adc));
                    });
                },
                uniqueIdenfier: "Migration_20220707_EnsureFormTypeIds",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
