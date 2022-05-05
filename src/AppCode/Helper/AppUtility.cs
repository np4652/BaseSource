using System;
using System.Collections.Generic;
using System.Text;

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

        // Generates a random string with a given size.    
        public string RandomString(int size, bool lowerCase = true)
        {
            Random _random = new Random();
            var builder = new StringBuilder(size);
            char offset = lowerCase ? 'a' : 'A';
            const int lettersOffset = 26;
            for (var i = 0; i < size; i++)
            {
                var @char = (char)_random.Next(offset, offset + lettersOffset);
                builder.Append(@char);
            }
            return lowerCase ? builder.ToString().ToLower() : builder.ToString();
        }
    }
}
