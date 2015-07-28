using Curt.shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curt.shared
{
    public static class ExtensionConverter
    {
        private static Dictionary<string, int> ExtensionID = new Dictionary<string, int>();

        public static int ConvertFromString(string Name)
        {
            if (ExtensionID.Keys.Count == 0)
                ExtensionID = BuildDictionary();
            if(ExtensionID.Keys.Contains(Name))
            return ExtensionID[Name];
            return -1;
        }

        private static Dictionary<string,int> BuildDictionary()
        {
            Dictionary<string, int> tmp = new Dictionary<string, int>();
            try {              
                List<Extension> list = DatabaseManager.GetAllFromWhiteList();
                foreach (var e in list)
                {
                    
                     tmp.Add(e.Type, e.ID);
                }
            }
            catch (Exception e)
            {
            }
            return tmp;
        }
        
        public static bool IsExtensionInDictionary(string extension)
        {
            return ExtensionID.Keys.Contains(extension);
        }
    }
}
