using Arup.RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARUP.IssueTracker.Classes
{
    public class JsonUtils
    {

        // Known issue: SimpleJson is not able to parse enum
        public static T Deserialize<T>(string serializedJson) 
        {
            try
            {
                return SimpleJson.DeserializeObject<T>(serializedJson.Trim());
            }
            catch
            {
                return default(T);
            }            
        }

        public static string Serialize(object o)
        {
            try
            {
                return SimpleJson.SerializeObject(o);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
