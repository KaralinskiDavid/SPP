using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using AssemblyBrowserLib;

namespace AssemblyBrowserWPF
{
    class ViewModel : INotifyPropertyChanged
    {
        private RelayCommand openFileCommand;
        public RelayCommand OpenFileCommand
        {
            get
            {
                return openFileCommand ?? (openFileCommand = new RelayCommand(obj =>
                {
                    IDialogService dialogService = new DialogService();
                    if (dialogService.OpenFileDialog())
                    {
                        PathToAssembly = dialogService.FilePath;
                        BrowseAssembly();
                    }
                }));
            }
        }

        private RelayCommand closeWindowCommand;
        public RelayCommand CloseWindowCommand
        {
            get
            {
                return closeWindowCommand ??
                    (closeWindowCommand = new RelayCommand(obj =>
                    {
                        Window wnd = obj as Window;
                        if (wnd != null)
                        {
                            wnd.Close();
                        }
                    }));
            }
        }

        private string assemblyName;
        public string AssemblyName
        {
            get { return assemblyName; }
            set
            {
                assemblyName = value;
                OnPropertyChanged("AssemblyName");
            }
        }

        private List<AssemblyNamespace> assemblyInformation;
        public List<AssemblyNamespace> AssemblyInformation
        {
            get { return assemblyInformation; }
            set
            {
                assemblyInformation = value;
                OnPropertyChanged("AssemblyInformation");
            }
        }

        private string pathToAssembly;
        public string PathToAssembly
        {
            get { return pathToAssembly; }
            set
            {
                pathToAssembly = value;
                OnPropertyChanged("PathToAssembly");
            }
        }

        private void BrowseAssembly()
        {
            AssemblyAnalyser analyser = new AssemblyAnalyser();
            AssemblyAnalysisResult result = analyser.GetAnalysysResult(PathToAssembly);
            AssemblyName = result.AssemblyName;
            AssemblyInformation = result.namespaces;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

    }
}
