using System;
using System.Collections.Generic;

namespace BaseSource.AppCode.Helper
{
    public class AppUtility
    {
        private static Lazy<AppUtility> Instance = new Lazy<AppUtility>(() => new AppUtility());
        public static AppUtility O => Instance.Value;
        private AppUtility() { }
        public string DictionaryToString(Dictionary<string, string> dictionary)
        {
            string dictionaryString = "{";
            foreach (KeyValuePair<string, string> keyValues in dictionary)
            {
                dictionaryString += keyValues.Key + " : " + keyValues.Value + ", ";
            }
            return dictionaryString.TrimEnd(',', ' ') + "}";
        }
    }
}
