using ARUP.IssueTracker.Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;

namespace ARUP.IssueTracker.IPC
{
    public class IpcMessageStore
    {
        /// <summary>
        /// the type of AIT UI actions
        /// </summary>
        public IpcOperationType type { get; set; }

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
                tempFilePath  = Path.GetTempFileName();
                using (FileStream fs = new FileStream(tempFilePath, FileMode.Create)) 
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    try
                    {
                        formatter.Serialize(fs, data);
                    }
                    catch (SerializationException ex)
                    {
                        MessageBox.Show("Failed to serialize. Reason: " + ex.ToString());
                    }
                }
            }                        

            IpcMessageStore msg = new IpcMessageStore() { type = type, tempFilePath = tempFilePath };
            return JsonConvert.SerializeObject(msg);
        }

        public static IpcOperationType getIpcOperationType(string jsonMsg) 
        {
            try
            {
                return JsonConvert.DeserializeObject<IpcMessageStore>(jsonMsg).type;
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

                using (FileStream fs = new FileStream(msg.tempFilePath, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    T obj = (T)formatter.Deserialize(fs);
                    return obj;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return default(T);
            }
        }
    }
}
