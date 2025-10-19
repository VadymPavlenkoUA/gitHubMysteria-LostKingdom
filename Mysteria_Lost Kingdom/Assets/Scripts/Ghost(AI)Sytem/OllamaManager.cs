using System.Diagnostics;
using UnityEngine;

public static class OllamaManager
{
    private static Process ollamaProcess;

    public static void StartOllama()
    {
        if (ollamaProcess == null || ollamaProcess.HasExited)
        {
            ollamaProcess = new Process();
            ollamaProcess.StartInfo.FileName = "ollama";
            ollamaProcess.StartInfo.Arguments = "serve";
            ollamaProcess.StartInfo.UseShellExecute = false;
            ollamaProcess.StartInfo.CreateNoWindow = true;
            ollamaProcess.StartInfo.RedirectStandardOutput = true;
            ollamaProcess.StartInfo.RedirectStandardError = true;

            ollamaProcess.Start();

            ollamaProcess.PriorityClass = ProcessPriorityClass.High;
            ollamaProcess.ProcessorAffinity = (System.IntPtr)0x0F;
        }
    }

    public static void StopOllama()
    {
        if (ollamaProcess != null && !ollamaProcess.HasExited)
        {
            ollamaProcess.Kill();
            ollamaProcess = null;
        }
    }
}
