using System;
using System.Configuration;
using System.Windows;
using System.Collections.Generic;
using Arup.RestSharp;
using System.Diagnostics;

namespace ARUP.IssueTracker.Classes
{
    public class JiraAccount
    {
        public string jiraserver {get; set;}
        public string username {get; set;}
        public string password {get; set;}
        public string guidfield { get; set; }
        /// <summary>
        /// is current active account
        /// </summary>
        public bool active { get; set; }
        /// <summary>
        /// the time account is saved on machine
        /// </summary>
        public DateTime savedTime { get; set; }
    }

    public static class MySettings
    {
        //http://jira.arup.com
        //https://casedesigninc.atlassian.net

        private const string _jiraservercase = "https://casedesigninc.atlassian.net";
        private const string _jiraserverarup = "http://jira.arup.com";
        private const string defaultguidfield = "customfield_10900";
        public  static string Get(string key)
        {
            try
            {
                Configuration config = GetConfig();

                if (config == null)
                    return string.Empty;


                KeyValueConfigurationElement element = config.AppSettings.Settings[key];
                if (element != null)
                {
                    string value = element.Value;
                    if (!string.IsNullOrEmpty(value)) 
                    {
                        return value;
                    }
                    else if (key == "guidfield") // fallback if an empty string
                    {
                        return defaultguidfield;
                    }
                        
                }
                else
                {
                    string value = string.Empty;

                    // inject default Jira server address
                    if (key == "jiraserver") 
                    {
                        value = _jiraserverarup;
                    }
                    // inject default guid field id
                    if (key == "guidfield")
                    {
                        value = defaultguidfield;
                    }

                    config.AppSettings.Settings.Add(key, value);
                    config.Save(ConfigurationSaveMode.Modified);
                    return value;
                }
            }
            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1);
            }
            return string.Empty;
        }
        public static void Set(string key, string value)
        {
            try
            {
                Configuration config = GetConfig();
                if (config == null)
                    return;

                KeyValueConfigurationElement element = config.AppSettings.Settings[key];
                if (element != null)
                    element.Value = value;
                else
                    config.AppSettings.Settings.Add(key, value);

                config.Save(ConfigurationSaveMode.Modified);

            }
            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1);
            }
        }

        // For using in the Settings window only
        public static List<JiraAccount> GetAllJiraAccounts() 
        {
            try 
            {
                Configuration config = GetConfig();

                if (config == null)
                    return null;

                List<JiraAccount> allAccounts = new List<JiraAccount>();
                foreach (string key in config.AppSettings.Settings.AllKeys)
                {
                    if (key.StartsWith("jiraaccount_"))
                    {
                        try
                        {
                            JiraAccount ac = SimpleJson.DeserializeObject<JiraAccount>(config.AppSettings.Settings[key].Value);
                            allAccounts.Add(ac);
                        }
                        catch (Exception ex)
                        {
                            allAccounts.Add(new JiraAccount() { active = false, jiraserver = string.Empty, username = string.Empty, password = string.Empty, guidfield = string.Empty, savedTime = DateTime.Now });
                        }
                    }
                }

                // Compatibility for old version
                if (allAccounts.Count == 0)
                {
                    JiraAccount ac = new JiraAccount() { active = true, jiraserver = Get("jiraserver"), username = Get("username"), password = Get("password"), guidfield = Get("guidfield"), savedTime = DateTime.Now };
                    allAccounts.Add(ac);
                    string key = "jiraaccount_" + Convert.ToInt32(DateTime.UtcNow.AddHours(8).Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
                    string value = SimpleJson.SerializeObject(ac);
                    config.AppSettings.Settings.Add(key, value);
                    config.Save(ConfigurationSaveMode.Modified);
                }

                // Decrypt passwords
                allAccounts.ForEach(ac => ac.password = DataProtector.DecryptData(ac.password));

                return allAccounts;
            }
            catch(Exception ex)
            {
                MessageBox.Show("exception: " + ex);
                return null;
            }            
        }

        // For using in the Settings window only
        public static void SetAllJiraAccounts(List<JiraAccount> accounts)
        {
            try 
            {
                Configuration config = GetConfig();
                if (config == null)
                    return;

                // Clear all exisiting accounts
                foreach (string key in config.AppSettings.Settings.AllKeys)
                {
                    if (key.StartsWith("jiraaccount_"))
                    {
                        config.AppSettings.Settings.Remove(key);
                    }
                }

                // Store all new account data
                foreach (JiraAccount ac in accounts)
                {
                    if (!string.IsNullOrWhiteSpace(ac.jiraserver) || !string.IsNullOrWhiteSpace(ac.username) || !string.IsNullOrWhiteSpace(ac.password) || !string.IsNullOrWhiteSpace(ac.guidfield))
                    {
                        ac.password = DataProtector.EncryptData(ac.password);
                        ac.savedTime = DateTime.Now;
                        string key = "jiraaccount_" + Guid.NewGuid().ToString();
                        string value = SimpleJson.SerializeObject(ac);
                        config.AppSettings.Settings.Add(key, value);
                        config.Save(ConfigurationSaveMode.Modified);         
                    }                               
                }                

                // Set active account
                JiraAccount activeAccount = accounts.Find(ac => ac.active);
                if (activeAccount != null)
                {
                    Set("jiraserver", activeAccount.jiraserver);
                    Set("username", activeAccount.username);
                    Set("password", activeAccount.password);  // no need to encrypt here, already did
                    Set("guidfield", activeAccount.guidfield);
                }                
            }
            catch(Exception ex)
            {
                MessageBox.Show("exception: " + ex);
            }            
        }

        /// <summary>
        /// Arup accounts need to change password every 3 months. Remind users to change here otherwise an account would be locked out until login on Jira.
        /// </summary>
        /// <returns>Always return false if not an Arup account</returns>
        public static bool isActiveAccountPasswordChanged() 
        {
            DateTime now = DateTime.Now;
            if (MySettings.Get("jiraserver").Trim() == _jiraserverarup)
            {
                string username = MySettings.Get("username").Trim();
                // get last saved time on user machine
                Configuration config = GetConfig();
                if (config == null)
                    return false;
                DateTime lastSavedTime = now;
                foreach (string key in config.AppSettings.Settings.AllKeys)
                {
                    if (key.StartsWith("jiraaccount_"))
                    {
                        try
                        {
                            JiraAccount ac = SimpleJson.DeserializeObject<JiraAccount>(config.AppSettings.Settings[key].Value);
                            if (ac.username.Trim() == username && ac.jiraserver.Trim() == _jiraserverarup && ac.savedTime != DateTime.MinValue)
                            {
                                lastSavedTime = ac.savedTime;
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            return false;
                        }
                    }
                }
                // find last changed time on AD
                DateTime lastChangedTime = now;
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = "net.exe";
                startInfo.Arguments = string.Format("user {0} /domain", username);
                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;
                process.StartInfo = startInfo;
                process.Start();
                while (!process.StandardOutput.EndOfStream)
                {
                    string line = process.StandardOutput.ReadLine();
                    if (line.Contains("Password last set"))
                    {
                        lastChangedTime = DateTime.Parse(line.Substring(17).Trim());
                    }
                }
                // return true if lastChangedTime is later than lastSavedTime
                if (lastChangedTime > lastSavedTime)
                {
                    return true;
                }
            }
            return false;
        }

        public static string GetWindowsUsername()
        {
            string s = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            int stop = s.IndexOf("\\");
            return (stop > -1) ? s.Substring(stop + 1, s.Length - stop - 1) : "Unknown User";
        }

        private static Configuration GetConfig()
        {

            string _issuetracker = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CASE", "ARUP Issue Tracker", "ARUPIssueTracker.config");

            ExeConfigurationFileMap configMap = new ExeConfigurationFileMap();
            configMap.ExeConfigFilename = _issuetracker;
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);

            
            if (config == null)
                    MessageBox.Show("Error loading the Configuration file.", "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return config;
        }
    }
}
