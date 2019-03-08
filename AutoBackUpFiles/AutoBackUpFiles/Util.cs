using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Configuration;
using System.Security.AccessControl;

namespace AutoBackUpFiles
{
    public class Util
    {

        private List<string> lstSourcePaths { get; set; }

        private string DestPath { get; set; }

        public void BackupFiles()
        {
            //folder name as month/day/year
            string currentfoldername = string.Format("{0}_{1}_{2}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Year);            

            //read the config file and store it in properties
            ReadConfigFile();

            //copy folders
            CopyFolders(lstSourcePaths, DestPath + currentfoldername);

            //delete older folders which are greater than 180 days
            DeleteOldFolders(DestPath);

        }

        //copy directories
        public void CopyFolders(List<string> lstSourcePaths, string destinationPath)
        {
            for (int i = 0; i < lstSourcePaths.Count; i++)
            {
                try
                {
                    var DirectoryName = Path.GetDirectoryName(lstSourcePaths[i]);

                    var machinename = System.Environment.MachineName;

                    Console.WriteLine("Adding access control entry for " + DirectoryName);

                    // Add the access control entry to the directory.
                    AddDirectorySecurity(DirectoryName, machinename, FileSystemRights.FullControl, AccessControlType.Allow);

                    Console.WriteLine("Copying Files to Folder in  " + destinationPath);

                    //copy the files to destination folder
                    File.Copy(lstSourcePaths[i], destinationPath);

                    Console.WriteLine("Done.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error Occured in CopyFiles. ErrorText: =>" + ex.Message);
                }

                Console.ReadLine();
            }
        }

        //delete folder which are greater than 180 days
        public void DeleteOldFolders(string destpath)
        {
            foreach (var folder in Directory.GetDirectories(destpath))
            {
                if (folder.Contains("20") && Directory.GetLastWriteTime(folder) <= DateTime.Now.AddDays(-180))
                {
                    ClearReadOnly(new DirectoryInfo(folder));

                    try
                    {
                        Directory.Delete(folder, true);
                    }
                    catch (Exception ex)
                    {
                        //do nothing
                        Console.WriteLine("Error Occured in DeleteOldFolders. ErrorText: =>" + ex.Message);
                    }
                }
            }
        }
        
        // Adds an ACL entry on the specified directory for the specified account.
        public void AddDirectorySecurity(string FileName, string Account, FileSystemRights Rights, AccessControlType ControlType)
        {
            try
            {
                // Create a new DirectoryInfo object.
                DirectoryInfo dInfo = new DirectoryInfo(FileName);

                // Get a DirectorySecurity object that represents the current security settings.
                DirectorySecurity dSecurity = dInfo.GetAccessControl();

                //Assing access rule with full rights
                var fullAccessRule = new FileSystemAccessRule("everyone", FileSystemRights.FullControl, InheritanceFlags.ContainerInherit, PropagationFlags.None, AccessControlType.Allow);
                var authUsersAccessRule = new FileSystemAccessRule("Authenticated Users", FileSystemRights.FullControl, InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow);

                // Add the FileSystemAccessRule to the security settings. 
                dSecurity.AddAccessRule(fullAccessRule);
                dSecurity.AddAccessRule(authUsersAccessRule);


                // Set the new access settings.
                dInfo.SetAccessControl(dSecurity);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error Occured in AddDirectorySecurity. ErrorText: =>" + ex.Message);
            }

        }

        // Removes an ACL entry on the specified directory for the specified account.
        public static void RemoveDirectorySecurity(string FileName, string Account, FileSystemRights Rights, AccessControlType ControlType)
        {
            // Create a new DirectoryInfo object.
            DirectoryInfo dInfo = new DirectoryInfo(FileName);

            // Get a DirectorySecurity object that represents the current security settings.
            DirectorySecurity dSecurity = dInfo.GetAccessControl();

            // Add the FileSystemAccessRule to the security settings. 
            dSecurity.RemoveAccessRule(new FileSystemAccessRule(Account, Rights, ControlType));

            // Set the new access settings.
            dInfo.SetAccessControl(dSecurity);

        }       

        public void ReadConfigFile()
        {
            lstSourcePaths = new List<string>();
            lstSourcePaths.Add(ConfigurationManager.AppSettings["SourcePath"]);
            DestPath = ConfigurationManager.AppSettings["DestinationPath"];            
        }

        public void ClearReadOnly(DirectoryInfo parentDir)
        {
            if (parentDir != null)
            {
                parentDir.Attributes = FileAttributes.Normal;

                foreach (var fi in parentDir.GetFiles())
                {
                    fi.Attributes = FileAttributes.Normal;
                }

                foreach (var di in parentDir.GetDirectories())
                {
                    ClearReadOnly(di);
                }
            }
        }

        public void ReadXMLFile(string filepath)
        {

            XmlDocument xmlDoc = new XmlDocument();
            XmlNode xmlNode;
            XDocument doc = new XDocument();

            doc = XDocument.Load(filepath);

            foreach (var xnode in doc.Descendants())
            {
            }

            doc = null;

            xmlDoc.Load(filepath);
            xmlNode = xmlDoc.SelectSingleNode("/Root/DestinationDrive/DriveAddress");
            DestPath = xmlNode.InnerText;
            xmlDoc = null;

        }

    }
}
