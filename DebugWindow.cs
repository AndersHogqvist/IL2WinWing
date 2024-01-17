using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IL2WinWing
{
    public partial class DebugWindow : Form
    {
        public DebugWindow()
        {
            InitializeComponent();
        }

        public void AddText(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(AddText), text);
                return;
            }

            string[] lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            foreach (string line in lines)
            {
                debugTextBox.AppendText(line + Environment.NewLine);
            }
        }

        public void ClearText()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(ClearText));
                return;
            }

            debugTextBox.Clear();
        }
    }
}
