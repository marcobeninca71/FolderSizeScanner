using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace FolderTreeSize
{
    public class FolderData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private void RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public FolderData()
        {
        }
        public FolderData(FolderData? data)
        {
            if (data != null)
            {
                isDisk = data.isDisk;
                size = data.size;
                FolderName = data.FolderName;
                FolderPath = data.FolderPath;
                DirectoryInfo = data.DirectoryInfo;
                Parent = data.Parent;
                IsPopulating = data.IsPopulating;
                lock (syncObj)
                {
                    subDirectories = new List<FolderData>(data.subDirectories);
                }
            }
        }

        private bool isPopulating = false;
        public bool IsPopulating { get { return isPopulating; } set { isPopulating = value; RaisePropertyChanged("IsPopulating"); } }

        public FolderData? Parent { get; set; }

        public DirectoryInfo? DirectoryInfo { get; set; }

        private string? folderName;
        public string? FolderName { get { return folderName; } set { folderName = value; RaisePropertyChanged("FolderName"); } }
        private string? folderPath;
        public string? FolderPath { get { return folderPath; } set { folderPath = value; RaisePropertyChanged("FolderPath"); } }
        private bool isDisk = false;
        public bool IsDisk { get { return isDisk; } set { isDisk = value; RaisePropertyChanged("IsDisk"); } }
        private long size = 0;
        public long Size 
        {
            get
            {
                lock (syncObj)
                {
                    return size;
                }
            }
        }

        public void IncreaseSize(long s)
        {
            lock (syncObj)
            {
                size += s;
            }
            if (Parent != null)
            {
                Parent.IncreaseSize(s);
            }
            RaisePropertyChanged("Size");
            RaisePropertyChanged("SizeString");
        }

        public static long Kb { get { return 1024; } }
        public static long Mb { get { return Kb * 1024; } }
        public static long Gb { get { return Mb * 1024; } }

        public string SizeString
        {
            get
            {
                long s = Size;
                string sizeDesc = "";

                if (s < Kb)
                {
                    sizeDesc = $"{((double)s).ToString("0.00")} Kb";
                }
                else if (s >= Kb && s < Mb)
                {
                    sizeDesc = $"{((double)s / Kb).ToString("0.00")} Mb";
                }
                else
                {
                    sizeDesc = $"{((double)s / Mb).ToString("0.00")} Gb";
                }
                return sizeDesc;
            }
        }

        private object syncObj = new object();
        private List<FolderData> subDirectories = new List<FolderData>();

        public ObservableCollection<FolderData> SubDirectories
        {
            get
            {
                lock (syncObj)
                {
                    return new ObservableCollection<FolderData>(subDirectories.OrderByDescending(x=>x.Size));
                }  
            } 
        }
        public void AddSubdir(FolderData data)
        {
            lock (syncObj)
            {
                data.Parent = this;
                subDirectories.Add(data);
                RaisePropertyChanged("SubDirectories");
            }
        }
    }
}
