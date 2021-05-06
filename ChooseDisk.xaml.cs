using Dws.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FolderTreeSize
{
    /// <summary>
    /// Interaction logic for ChooseDisk.xaml
    /// </summary>
    public partial class ChooseDisk : Window
    {
        public ChooseDisk()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public ObservableCollection<DriveInfo> Disks
        {
            get { return new ObservableCollection<DriveInfo>(DriveInfo.GetDrives().Where(x => x.DriveType == DriveType.Fixed || x.DriveType == DriveType.Removable)); }
        }

        public DriveInfo? SelectedDrive { get; set; }

        private DelegateCommand? _selectDiskCommand = null;
        public ICommand SelectDiskCommand
        {
            get
            {
                if (_selectDiskCommand == null)
                    _selectDiskCommand = new DelegateCommand(new Action<object>(SelectDisk));
                return _selectDiskCommand;
            }
        }

        private void SelectDisk(object obj)
        {
            if (obj is DriveInfo)
            {
                SelectedDrive = obj as DriveInfo;
                this.DialogResult = true;
                this.Close();
            }
        }

        private DelegateCommand? _cancelCommand = null;
        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                    _cancelCommand = new DelegateCommand(new Action<object>(Cancel));
                return _cancelCommand;
            }
        }

        private void Cancel(object obj)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
