using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public static class NativeFolderPicker
{
    private const uint SIGDN_FILESYSPATH = 0x80058000;

    public static string Pick(string title, string initialDirectory)
    {
        try
        {
            return PickVista(title, initialDirectory);
        }
        catch
        {
            return PickLegacy(title, initialDirectory);
        }
    }

    private static string PickVista(string title, string initialDirectory)
    {
        var ofd = new OpenFileDialog
        {
            Title = title ?? "Select folder",
            InitialDirectory = DirectoryOrDesktop(initialDirectory),
            AddExtension = false,
            CheckFileExists = false,
            CheckPathExists = true,
            DereferenceLinks = true,
            Filter = "Folders|\n",
            FileName = "Select folder",
            Multiselect = false,
            ValidateNames = false
        };

        var dialogType = typeof(FileDialog);
        var assembly = dialogType.Assembly;
        var native = assembly.GetType("System.Windows.Forms.FileDialogNative+IFileDialog", true);
        var shellItem = assembly.GetType("System.Windows.Forms.FileDialogNative+IShellItem", true);
        var fosType = assembly.GetType("System.Windows.Forms.FileDialogNative+FOS", true);
        var create = typeof(OpenFileDialog).GetMethod("CreateVistaDialog", BindingFlags.Instance | BindingFlags.NonPublic);
        var before = typeof(OpenFileDialog).GetMethod("OnBeforeVistaDialog", BindingFlags.Instance | BindingFlags.NonPublic);
        var getOptions = dialogType.GetMethod("GetOptions", BindingFlags.Instance | BindingFlags.NonPublic);
        if (create == null || before == null || getOptions == null)
            throw new InvalidOperationException("Windows Forms Vista folder dialog is unavailable.");

        var dialog = create.Invoke(ofd, null);
        before.Invoke(ofd, new[] { dialog });
        var options = (uint)getOptions.Invoke(ofd, null);
        var pickFolders = (uint)fosType.GetField("FOS_PICKFOLDERS").GetValue(null);
        native.GetMethod("SetOptions").Invoke(dialog, new object[] { options | pickFolders });

        var hr = ToHResult(native.GetMethod("Show").Invoke(dialog, new object[] { GetActiveWindow() }));
        if (hr != 0) return null;

        var resultArgs = new object[] { null };
        native.GetMethod("GetResult").Invoke(dialog, resultArgs);
        var nameArgs = new object[] { SIGDN_FILESYSPATH, null };
        shellItem.GetMethod("GetDisplayName").Invoke(resultArgs[0], nameArgs);
        return nameArgs[1] as string;
    }

    private static string PickLegacy(string title, string initialDirectory)
    {
        using (var dialog = new FolderBrowserDialog())
        {
            dialog.Description = title ?? "Select folder";
            dialog.ShowNewFolderButton = false;
            var start = DirectoryOrDesktop(initialDirectory);
            if (!string.IsNullOrEmpty(start)) dialog.SelectedPath = start;

            using (var owner = new Form())
            {
                owner.ShowInTaskbar = false;
                owner.WindowState = FormWindowState.Minimized;
                owner.Opacity = 0;
                owner.Show();
                owner.BringToFront();
                return dialog.ShowDialog(owner) == DialogResult.OK ? dialog.SelectedPath : null;
            }
        }
    }

    private static string DirectoryOrDesktop(string path)
    {
        if (!string.IsNullOrEmpty(path) && System.IO.Directory.Exists(path)) return path;
        return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    }

    private static int ToHResult(object value)
    {
        if (value is int) return (int)value;
        if (value is uint) return unchecked((int)(uint)value);
        return -1;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();
}
