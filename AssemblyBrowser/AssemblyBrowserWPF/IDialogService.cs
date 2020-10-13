using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyBrowserWPF
{
    public interface IDialogService
    {
        string FilePath { get; set; }
        bool OpenFileDialog();
    }
}
