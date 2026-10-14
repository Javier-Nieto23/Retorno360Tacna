using System;
using System.Windows.Forms;

namespace Retorno360Tacna.HELPERS
{
    public static class WinFormsHelper
    {
        public static DialogResult ShowSaveFileDialog(IWin32Window owner, SaveFileDialog dlg)
        {
            if (dlg == null) throw new ArgumentNullException(nameof(dlg));
            try
            {
                if (owner is Control c && c.InvokeRequired)
                {
                    return (DialogResult)c.Invoke(new Func<DialogResult>(() => dlg.ShowDialog(owner)));
                }
                return dlg.ShowDialog(owner);
            }
            catch
            {
                // Fallback simple
                return dlg.ShowDialog();
            }
        }

        public static DialogResult ShowOpenFileDialog(IWin32Window owner, OpenFileDialog dlg)
        {
            if (dlg == null) throw new ArgumentNullException(nameof(dlg));
            try
            {
                if (owner is Control c && c.InvokeRequired)
                {
                    return (DialogResult)c.Invoke(new Func<DialogResult>(() => dlg.ShowDialog(owner)));
                }
                return dlg.ShowDialog(owner);
            }
            catch
            {
                return dlg.ShowDialog();
            }
        }
    }
}
