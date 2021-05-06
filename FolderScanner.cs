using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderTreeSize
{
    public class FolderEnumerationEndedEventArgs : EventArgs
    {
    }
    public class NewFolderAddedEventArgs : EventArgs
    {
        public FolderData? NewFolder { get; set; }
    }

    public delegate void FolderEnumerationEndedHandler(object sender, FolderEnumerationEndedEventArgs e);
    public delegate void NewFolderAddedHandler(object sender, NewFolderAddedEventArgs e);

    public class FileSystemAnalyzer
    {
        public event FolderEnumerationEndedHandler? OnFolderEnumerationEnded;
        public event NewFolderAddedHandler? OnNewFolderAdded;

        private object syncObj = new object();

        public int NumFolders { get; set; }
        private List<FolderData> rootfolders = new List<FolderData>();

        private void NotifyChange(FolderData d)
        {
            if (OnNewFolderAdded != null)
            {
                OnNewFolderAdded(this, new NewFolderAddedEventArgs() { NewFolder = d });
            }
        }

        private void addRootFolder(FolderData data)
        {
            lock (syncObj)
            {
                rootfolders.Add(data);
            }
        }

        public async Task CollectFolders()
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                await manageDrive(drive);
            }
            if (OnFolderEnumerationEnded != null)
                OnFolderEnumerationEnded(this, new FolderEnumerationEndedEventArgs());
        }

        public async Task CollectFolders(DriveInfo drive)
        {
            await manageDrive(drive);
        }

        private async Task manageDrive(DriveInfo drive)
        {
            if (drive.DriveType == DriveType.Fixed || drive.DriveType == DriveType.Removable)
            {
                FolderData rootfolder = CreateFolderData(drive.RootDirectory, null);
                addRootFolder(rootfolder);
                rootfolder.IsPopulating = true;
                rootfolder.IsDisk = true;
                NotifyChange(rootfolder);
                await Task.Run(() => CrawlFolder(rootfolder));
                rootfolder.IsPopulating = false;
            }
            if (OnFolderEnumerationEnded != null)
                OnFolderEnumerationEnded(this, new FolderEnumerationEndedEventArgs());
        }

        private FolderData CreateFolderData(DirectoryInfo di, FolderData? parent)
        {
            FolderData data = new FolderData();
            data.DirectoryInfo = di;
            data.FolderName = di.Name;
            data.FolderPath = di.FullName;
            if (parent != null)
                parent.AddSubdir(data);
            long s = di.EnumerateFiles("*.*").Sum(x => x.Length) / FolderData.Kb;
            data.IncreaseSize(s);

            return data;
        }

        private async Task CrawlFolder(FolderData dir)
        {
            try
            {
                if (dir.DirectoryInfo != null)
                {
                    DirectoryInfo[] directoryInfos = dir.DirectoryInfo.GetDirectories();
                    foreach (DirectoryInfo childInfo in directoryInfos)
                    {
                        try
                        {
                            // here may be dragons using enumeration variable as closure!!
                            DirectoryInfo di = childInfo;
                            FolderData data = CreateFolderData(di, dir);
                            NotifyChange(data);
                            data.IsDisk = false;
                            data.IsPopulating = true;
                            await Task.Run(() => CrawlFolder(data));
                            data.IsPopulating = false;
                        }
                        catch (Exception)
                        {
                        }
                    }
                    NumFolders++;
                }
            }
            catch (Exception? ex)
            {
                while (ex != null)
                {
                    Console.WriteLine($"{ex.GetType()} {ex.Message}\n{ex.StackTrace}");
                    ex = ex.InnerException;
                }
            }
        }
    }
}
