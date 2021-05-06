using Dws.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FolderTreeSize
{
    public class FolderTreeModelView : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private void RaisePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        private FileSystemAnalyzer analyzer = new FileSystemAnalyzer();
        public FolderTreeModelView()
        {
            analyzer.OnFolderEnumerationEnded += Analyzer_OnFolderEnumerationEnded;
            analyzer.OnNewFolderAdded += Analyzer_OnNewFolderAdded;
        }


        private List<FolderData> rootFolders = new List<FolderData>();
        public ObservableCollection<FolderData> Folders { get { return new ObservableCollection<FolderData>(rootFolders); } }

        private DelegateCommandAsync<object>? _refreshCommand = null;
        public ICommand RefreshCommand
        {
            get
            {
                if (_refreshCommand == null)
                    _refreshCommand = new DelegateCommandAsync<object>(RefreshFolders);
                return _refreshCommand;
            }
        }

        private async Task RefreshFolders(object obj)
        {
            rootFolders.Clear();
            if(SelectedDrive == null)
                await analyzer.CollectFolders();
            else
                await analyzer.CollectFolders(SelectedDrive);
            RaisePropertyChanged("Folders");

        }

        private void Analyzer_OnFolderEnumerationEnded(object sender, FolderEnumerationEndedEventArgs e)
        {
            RaisePropertyChanged("Folders");
        }
        private void Analyzer_OnNewFolderAdded(object sender, NewFolderAddedEventArgs e)
        {

            if (e.NewFolder != null)
            {
                if (e.NewFolder.Parent == null)
                {
                    rootFolders.Add(e.NewFolder);
                    RaisePropertyChanged("Folders");
                }
            }
        }

        private void addFolderToTree(List<FolderData> parents, FolderData data, out bool found)
        {
            found = false;
            if (data.Parent != null)
            {
                foreach (FolderData folder in parents)
                {
                    if (folder.FolderPath == data.Parent.FolderPath)
                    {
                        folder.AddSubdir(data);
                        found = true;
                        break;
                    }
                    else
                    {
                        addFolderToTree(folder.SubDirectories.ToList(), data, out found);
                        if (found)
                            break;
                    }
                }
            }
        }

        private DelegateCommand? _chooseDiskCommand = null;
        public ICommand ChooseDiskCommand
        {
            get
            {
                if (_chooseDiskCommand == null)
                    _chooseDiskCommand = new DelegateCommand(new Action<object>(ChooseDiskAction));
                return _chooseDiskCommand;
            }
        }

        private void ChooseDiskAction(object obj)
        {
            ChooseDisk wnd = new ChooseDisk();
            bool? ok = wnd.ShowDialog();
            if (ok.GetValueOrDefault())
            {
                SelectedDrive = wnd.SelectedDrive;
                RaisePropertyChanged("SelectedDrive");
            }
        }

        public DriveInfo? SelectedDrive { get; set; }
    }
}
