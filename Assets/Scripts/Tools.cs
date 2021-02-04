using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

public static class Tools
{
    public static void MessageBoxInfo(string message, string title)
    {
        var file = $@"{Main.GetPath}\\msg.vbs";
        if (File.Exists(file))
            File.Delete(file);
        var stream = File.CreateText(file);
        stream.Write($"MsgBox \"{message}\", vbOk + vbInformation, \"{title}\"");
        stream.Close();
        stream.Dispose();
        Process.Start(file);
    }

    public static void RenameSaveInputBox(string message, string title, string def)
    {
        var file = $@"{Main.GetPath}\\msg.vbs";
        if (File.Exists(file))
            File.Delete(file);
        var stream = File.CreateText(file);
        stream.WriteLine("Set fs = CreateObject(\"Scripting.FileSystemObject\")");
        stream.WriteLine($"rep = InputBox(\"{message}\", \"{title}\", \"{def}\")");
        stream.WriteLine($"fs.MoveFile \"{Main.GetPath+@"\Saves\"+def}\",\"{Main.GetPath+@"\Saves\"}\" & rep");
        stream.Close();
        stream.Dispose();
        var task = Process.Start(file);
        task.WaitForExit();
    }
}