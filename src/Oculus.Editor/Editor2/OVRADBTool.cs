/************************************************************************************

Copyright   :   Copyright (c) Facebook Technologies, LLC and its affiliates. All rights reserved.

Licensed under the Oculus SDK License Version 3.4.1 (the "License");
you may not use the Oculus SDK except in compliance with the License,
which is provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

You may obtain a copy of the License at

https://developer.oculus.com/licenses/sdk-3.4.1

Unless required by applicable law or agreed to in writing, the Oculus SDK
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

************************************************************************************/

using System;
using System.Diagnostics;
using System.IO;
using System.Text;

using Debug = UnityEngine.Debug;

public class OVRADBTool
{
    public bool isReady;

    public string androidSdkRoot;
    public string androidPlatformToolsPath;
    public string adbPath;

    public OVRADBTool(string androidSdkRoot)
    {
        this.androidSdkRoot = !string.IsNullOrEmpty(androidSdkRoot) ? androidSdkRoot : string.Empty;
        if (this.androidSdkRoot.EndsWith("\\") || this.androidSdkRoot.EndsWith("/"))
            this.androidSdkRoot = this.androidSdkRoot.Remove(this.androidSdkRoot.Length - 1);
        androidPlatformToolsPath = Path.Combine(this.androidSdkRoot, "platform-tools");
        adbPath = Path.Combine(androidPlatformToolsPath, "adb.exe");
        isReady = File.Exists(adbPath);
    }

    public static bool IsAndroidSdkRootValid(string androidSdkRoot) => new OVRADBTool(androidSdkRoot).isReady;
    public delegate void WaitingProcessToExitCallback();
    public int StartServer(WaitingProcessToExitCallback waitingProcessToExitCallback) => RunCommand(new[] { "start-server" }, waitingProcessToExitCallback, out var outputString, out var errorString);
    public int KillServer(WaitingProcessToExitCallback waitingProcessToExitCallback) => RunCommand(new[] { "kill-server" }, waitingProcessToExitCallback, out var outputString, out var errorString);
    public int ForwardPort(int port, WaitingProcessToExitCallback waitingProcessToExitCallback) => RunCommand(new[] { "forward", $"tcp:{port}", $"tcp:{port}" }, waitingProcessToExitCallback, out var outputString, out var errorString);

    public int ReleasePort(int port, WaitingProcessToExitCallback waitingProcessToExitCallback) => RunCommand(new[] { "forward", "--remove", $"tcp:{port}" }, waitingProcessToExitCallback, out var outputString, out var errorString);

    StringBuilder outputStringBuilder = null;
    StringBuilder errorStringBuilder = null;

    public int RunCommand(string[] arguments, WaitingProcessToExitCallback waitingProcessToExitCallback, out string outputString, out string errorString)
    {
        var exitCode = -1;
        if (!isReady)
        {
            Debug.LogWarning("OVRADBTool not ready");
            outputString = string.Empty;
            errorString = "OVRADBTool not ready";
            return exitCode;
        }
        var args = string.Join(" ", arguments);
        outputStringBuilder = new StringBuilder("");
        errorStringBuilder = new StringBuilder("");
        var process = Process.Start(new ProcessStartInfo(adbPath, args)
        {
            WorkingDirectory = androidSdkRoot,
            CreateNoWindow = true,
            UseShellExecute = false,
            WindowStyle = ProcessWindowStyle.Hidden,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        });
        process.OutputDataReceived += new DataReceivedEventHandler(OutputDataReceivedHandler);
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        try
        {
            do
            {
                waitingProcessToExitCallback?.Invoke();
            } while (!process.WaitForExit(100));
            process.WaitForExit();
        }
        catch (Exception e) { Debug.LogWarningFormat("[OVRADBTool.RunCommand] exception {0}", e.Message); }
        exitCode = process.ExitCode;
        process.Close();
        outputString = outputStringBuilder.ToString();
        errorString = errorStringBuilder.ToString();
        outputStringBuilder = null;
        errorStringBuilder = null;
        return exitCode;
    }

    void OutputDataReceivedHandler(object sendingProcess, DataReceivedEventArgs args)
    {
        // Collect the sort command output.
        if (!string.IsNullOrEmpty(args.Data))
        {
            // Add the text to the collected output.
            outputStringBuilder.Append(args.Data);
            outputStringBuilder.Append(Environment.NewLine);
        }
    }

    void ErrorDataReceivedHandler(object sendingProcess, DataReceivedEventArgs args)
    {
        // Collect the sort command output.
        if (!string.IsNullOrEmpty(args.Data))
        {
            // Add the text to the collected output.
            errorStringBuilder.Append(args.Data);
            errorStringBuilder.Append(Environment.NewLine);
        }
    }
}
