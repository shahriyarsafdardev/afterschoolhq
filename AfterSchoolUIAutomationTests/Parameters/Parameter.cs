using System.IO;
using System.Reflection;
using System.Xml;

namespace AfterSchoolUIAutomationTests.Parameters
{
    public static class Parameter
    {
        public static string Get(string element, string parameterType, string feature = null)
        {
            string value = "";

            //create XMLDocument object
            XmlDocument xmlDoc = new XmlDocument();

            //returns url of main directory which contains "/bin/Debug"
            var url = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location.Substring(0, Assembly.GetEntryAssembly().Location.IndexOf("bin\\")));

            //correction in path to point it in Root directory
            var mainpath = url + $@"\Parameters\parameters_{parameterType}.xml";

            //load xml file
            xmlDoc.Load(mainpath);

            //save all nodes in XMLnodelist
            XmlNodeList nodeList = parameterType.Equals("urls") && feature is null ? xmlDoc.DocumentElement.SelectNodes("/PARAMETERS") : xmlDoc.DocumentElement.SelectNodes("/PARAMETERS/" + feature);

            //loop through each node and save it in value

            foreach (XmlNode node in nodeList)
            {
                value = node.SelectSingleNode(element).InnerText;
            }

            return value;
        }
    }
}