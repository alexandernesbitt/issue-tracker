using ARUP.IssueTracker.Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace ARUP.IssueTracker.IPC
{
    public class IpcMessageStore
    {
        /// <summary>
        /// the type of AIT UI actions
        /// </summary>
        public int type { get; set; }

        /// <summary>
        /// the temp file for object serialization
        /// </summary>
        public string tempFilePath { get; set; }

        /// <summary>
        /// serialize object in a temp file and return a JSON message for data exchange in named pipes
        /// </summary>
        public static string savePayload(IpcOperationType type, object data)
        {
            string tempFilePath = null;
            
            if(data != null)
            {
                tempFilePath = Path.GetTempFileName();
                File.WriteAllText(tempFilePath, JsonConvert.SerializeObject(data));
            }                        

            IpcMessageStore msg = new IpcMessageStore() { type = (int) type, tempFilePath = tempFilePath };
            return JsonConvert.SerializeObject(msg);
        }

        public static IpcOperationType getIpcOperationType(string jsonMsg) 
        {
            try
            {
                return (IpcOperationType) JsonConvert.DeserializeObject<IpcMessageStore>(jsonMsg).type;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return IpcOperationType.Invalid;
            }
        }

        /// <summary>
        /// deserialize object based on JSON message
        /// </summary>
        public static T getPayload<T>(string jsonMsg) 
        {
            try
            {
                IpcMessageStore msg = JsonConvert.DeserializeObject<IpcMessageStore>(jsonMsg);

                if (string.IsNullOrEmpty(msg.tempFilePath))
                {
                    return default(T);
                }

                var obj = JsonConvert.DeserializeObject<T>(File.ReadAllText(msg.tempFilePath));

                File.Delete(msg.tempFilePath);

                return obj;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return default(T);
            }
        }
    }
}
