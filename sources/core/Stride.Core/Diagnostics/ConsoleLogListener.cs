// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Diagnostics;
#if STRIDE_PLATFORM_ANDROID
using Android.Util;
#endif
#if STRIDE_PLATFORM_DESKTOP
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
#endif

namespace Stride.Core.Diagnostics;

/// <summary>
/// A <see cref="LogListener"/> implementation redirecting its output to the default OS console. If console is not supported message are output to <see cref="Debug"/>
/// </summary>
public partial class ConsoleLogListener : LogListener
{
    /// <summary>
    /// Gets or sets the minimum log level handled by this listener.
    /// </summary>
    /// <value>The minimum log level.</value>
    public LogMessageType LogLevel { get; set; }

    /// <summary>
    /// Gets or sets the log mode.
    /// </summary>
    /// <value>The log mode.</value>
    public ConsoleLogMode LogMode { get; set; }

    protected override void OnLog(ILogMessage logMessage)
    {
        // filter logs with lower level
        if (!Debugger.IsAttached && // Always log when debugger is attached
            (logMessage.Type < LogLevel || LogMode == ConsoleLogMode.None
            || (!(LogMode == ConsoleLogMode.Auto && Platform.IsRunningDebugAssembly) && LogMode != ConsoleLogMode.Always)))
        {
            return;
        }

        // Make sure the console is opened when the debugger is not attached
        EnsureConsole();

#if STRIDE_PLATFORM_ANDROID
        const string appliName = "Stride";
        var exceptionMsg = GetExceptionText(logMessage);
        var messageText = GetDefaultText(logMessage);
        if (!string.IsNullOrEmpty(exceptionMsg))
            messageText += exceptionMsg;

        // set the color depending on the message log level
        switch (logMessage.Type)
        {
            case LogMessageType.Debug:
                Log.Debug(appliName, messageText);
                break;
            case LogMessageType.Verbose:
                Log.Verbose(appliName, messageText);
                break;
            case LogMessageType.Info:
                Log.Info(appliName, messageText);
                break;
            case LogMessageType.Warning:
                Log.Warn(appliName, messageText);
                break;
            case LogMessageType.Error:
            case LogMessageType.Fatal:
                Log.Error(appliName, messageText);
                break;
        }
        return;
#else // STRIDE_PLATFORM_ANDROID

        var exceptionMsg = GetExceptionText(logMessage);

#if STRIDE_PLATFORM_DESKTOP
        // save initial console color
        var initialColor = Console.ForegroundColor;

        // set the color depending on the message log level
        switch (logMessage.Type)
        {
            case LogMessageType.Debug:
                Console.ForegroundColor = ConsoleColor.DarkGray;
                break;
            case LogMessageType.Verbose:
                Console.ForegroundColor = ConsoleColor.Gray;
                break;
            case LogMessageType.Info:
                Console.ForegroundColor = ConsoleColor.Green;
                break;
            case LogMessageType.Warning:
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            case LogMessageType.Error:
            case LogMessageType.Fatal:
                Console.ForegroundColor = ConsoleColor.Red;
                break;
        }
#endif

        if (Debugger.IsAttached)
        {
            // Log the actual message
            Debug.WriteLine(GetDefaultText(logMessage));
            if (!string.IsNullOrEmpty(exceptionMsg))
            {
                Debug.WriteLine(exceptionMsg);
            }
        }

#if !STRIDE_PLATFORM_UWP
        // Log the actual message
        Console.WriteLine(GetDefaultText(logMessage));
        if (!string.IsNullOrEmpty(exceptionMsg))
        {
            Console.WriteLine(exceptionMsg);
        }
#endif

#if STRIDE_PLATFORM_DESKTOP

        // revert console initial color
        Console.ForegroundColor = initialColor;
#endif
#endif // !STRIDE_PLATFORM_ANDROID
    }

#if STRIDE_PLATFORM_DESKTOP

    // TODO: MOVE THIS CODE OUT IN A SEPARATE CLASS

    private bool isConsoleActive;
    private void EnsureConsole()
    {
        if (isConsoleActive || !Platform.IsWindowsDesktop)
        {
            return;
        }

        // try to attach to the parent console, if the program is run directly from a console
        var attachedToConsole = AttachConsole(-1);
        if (!attachedToConsole)
        {
            // Else open a new console
            ShowConsole();
        }

        isConsoleActive = true;
    }

    public static void ShowConsole()
    {
        var handle = GetConsoleWindow();

        var outputRedirected = IsHandleRedirected((IntPtr)StdOutConsoleHandle);

        // If we are outputting somewhere unexpected, add an additional console window
        if (outputRedirected)
        {
            var originalStream = Console.OpenStandardOutput();

            // Free before trying to allocate
            FreeConsole();
            AllocConsole();

            var outputStream = Console.OpenStandardOutput();
            if (originalStream != null)
            {
                outputStream = new DualStream(originalStream, outputStream);
            }

            TextWriter writer = new StreamWriter(outputStream) { AutoFlush = true };
            Console.SetOut(writer);
        }
        else if (handle != IntPtr.Zero)
        {
            const int SW_SHOW = 5;
            ShowWindow(handle, SW_SHOW);
        }
    }

    private class DualStream : Stream
    {
        private readonly Stream stream1;
        private readonly Stream stream2;

        public DualStream(Stream stream1, Stream stream2)
        {
            this.stream1 = stream1;
            this.stream2 = stream2;
        }

        public override void Flush()
        {
            stream1.Flush();
            stream2.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            stream1.Write(buffer, offset, count);
            stream2.Write(buffer, offset, count);
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override long Position { get; set; }
    }

    public static void HideConsole()
    {
        var handle = GetConsoleWindow();
        const int SW_HIDE = 0;
        ShowWindow(handle, SW_HIDE);
    }

    private const int StdOutConsoleHandle = -11;

#if NET7_0_OR_GREATER
    [LibraryImport("kernel32", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool AttachConsole(int dwProcessId);
#else
    [DllImport("kernel32", SetLastError = true)]
    private static extern bool AttachConsole(int dwProcessId);
#endif // NET7_0_OR_GREATER

#if NET7_0_OR_GREATER
    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool FreeConsole();
#else
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool FreeConsole();
#endif // NET7_0_OR_GREATER

#if NET7_0_OR_GREATER
    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool AllocConsole();
#else
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AllocConsole();
#endif // NET7_0_OR_GREATER

#if NET7_0_OR_GREATER
    [LibraryImport("kernel32.dll")]
    private static partial IntPtr GetConsoleWindow();
#else
    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();
#endif // NET7_0_OR_GREATER

#if NET7_0_OR_GREATER
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);
#else
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
#endif // NET7_0_OR_GREATER

#if NET7_0_OR_GREATER
    [LibraryImport("kernel32.dll")]
    private static partial IntPtr GetStdHandle(uint nStdHandle);
#else
    [DllImport("kernel32.dll")]
    private static extern IntPtr GetStdHandle(uint nStdHandle);
#endif // NET7_0_OR_GREATER

#if NET7_0_OR_GREATER
    [LibraryImport("kernel32.dll")]
    private static partial void SetStdHandle(uint nStdHandle, IntPtr handle);
#else
    [DllImport("kernel32.dll")]
    private static extern void SetStdHandle(uint nStdHandle, IntPtr handle);
#endif // NET7_0_OR_GREATER

#if NET7_0_OR_GREATER
    [LibraryImport("kernel32.dll")]
    private static partial int GetFileType(SafeFileHandle handle);
#else
    [DllImport("kernel32.dll")]
    private static extern int GetFileType(SafeFileHandle handle);
#endif // NET7_0_OR_GREATER

#if NET7_0_OR_GREATER
    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetConsoleMode(IntPtr hConsoleHandle, out int mode);
#else
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out int mode);
#endif // NET7_0_OR_GREATER

    private static bool IsHandleRedirected(IntPtr ioHandle)
    {
        if ((GetFileType(new SafeFileHandle(ioHandle, false)) & 2) != 2)
        {
            return true;
        }

        // We are fine with being attached to non-consoles
        return false;

        //int mode;
        //return !GetConsoleMode(ioHandle, out mode);
    }

#else
    private void EnsureConsole()
    {
    }
#endif
}
