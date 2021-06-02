using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace AfterSchoolUIAutomationTests.Parameters
{
    public static class Parameter
    {
        public static string Get(string feature, string element)
        {
            string value = "";

            //create XMLDocument object
            XmlDocument xmlDoc = new XmlDocument();

            //returns url of main directory which contains "/bin/Debug"
            var url = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);

            //correction in path to point it in Root directory
            var mainpath = url.Replace("\\bin\\Debug\\netcoreapp3.1", "") + "\\Parameters\\parameters_users.xml";
            //load xml file
            xmlDoc.Load(mainpath);

            //save all nodes in XMLnodelist
            XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/PARAMETERS/" + feature);

            //loop through each node and save it in value

            foreach (XmlNode node in nodeList)
            {
                value = node.SelectSingleNode(element).InnerText;
            }

            return value;
        }
    }
}
