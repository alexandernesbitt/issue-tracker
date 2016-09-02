using System;
using System.IO;
using System.Reflection;

namespace ARUP.IssueTracker.Navisworks
{
    public class Utils
    {
        public static readonly string issueTrackerDllPath = Path.Combine(ProgramFilesx86(), "CASE", "ARUP Issue Tracker", "ARUP.IssueTracker.dll");

        public static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string dllFileName = new AssemblyName(args.Name).Name + ".dll";
            string dllPath = Path.Combine(ProgramFilesx86(), "CASE", "ARUP Issue Tracker", dllFileName);

            Assembly theAsm = null;
            if (!string.IsNullOrEmpty(dllPath) && File.Exists(dllPath))
            {
                theAsm = Assembly.LoadFrom(dllPath);
            }
            return theAsm;
        }

        public static string ProgramFilesx86()
        {
            if (8 == IntPtr.Size
                 || (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
            {
                return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            }

            return Environment.GetEnvironmentVariable("ProgramFiles");
        }
    }
}
