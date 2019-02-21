using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;

namespace DPIMonitor
{
    public partial class DpiMonitorWindow
    {
        private static string OutputFileName { get; } = "DPIMonitor.json";

        [DllImport("User32.dll")]
        public static extern uint GetDpiForWindow([In]IntPtr hmonitor);

        [DllImport("User32.dll")]
        public static extern uint GetDpiForSystem();

        public DpiMonitorWindow()
        {
            InitializeComponent();
            OutputPath.Text = $"Result saved to: {GetOutputFilePath()}";
        }

        private void WindowLayoutUpdated(object sender, EventArgs e)
        {
            UpdateResults();
        }

        private void WindowDpiChanged(object sender, EventArgs e)
        {
            UpdateResults();
        }

        private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateResults();
        }

        private void UpdateResults()
        {
            var systemDpi = GetDpiForSystem();
            var currentWindowDpi = GetCurrentWindowDpi();
            var monitorDpiData = GetMonitorDpi(DpiType.Effective);

            var dpiText = $"Current Window DPI: {currentWindowDpi}\n" +
                             $"System DPI: {systemDpi}\n" +
                             "Monitor DPIs:\n";

            var result = new DpiResult
            {
                SystemDpi = systemDpi,
                CurrentWindowDpi = currentWindowDpi,
                Monitors = new List<MonitorDpi>()
            };

            foreach (var displayName in monitorDpiData.Keys)
            {
                var monitorDpi = new MonitorDpi
                {
                    DisplayName = displayName,
                    Dpi = monitorDpiData[displayName]
                };
                result.Monitors.Add(monitorDpi);
                dpiText += $"    {displayName}: {monitorDpiData[displayName]}\n";
            }

            File.WriteAllText(GetOutputFilePath(), ConvertToJsonString(typeof(DpiResult), result));
            DPI.Text = dpiText;
        }

        private static string GetOutputFilePath()
        {
            var tempDir = Environment.GetEnvironmentVariable("temp");
            return $"{tempDir}\\{OutputFileName}";
        }

        private static string ConvertToJsonString(Type type, object item)
        {
            var stream1 = new MemoryStream();
            var ser = new DataContractJsonSerializer(type);
            ser.WriteObject(stream1, item);

            stream1.Position = 0;
            var sr = new StreamReader(stream1);
            return sr.ReadToEnd();
        }

        private uint GetCurrentWindowDpi()
        {
            var window = GetWindow(this);
            var wih = new WindowInteropHelper(window ?? throw new InvalidOperationException());
            var hWnd = wih.EnsureHandle();
            return GetDpiForWindow(hWnd);
        }

        private Dictionary<string, uint> GetMonitorDpi(DpiType type)
        {
            var result = new Dictionary<string, uint>();
            foreach (var screen in Screen.AllScreens)
            {
                screen.GetDpi(type, out var x, out _);
                result[screen.DeviceName] = x;
            }
            return result;
        }
    }

    public class MonitorDpi
    {
        public string DisplayName;
        public uint Dpi;
    }

    public class DpiResult
    {
        public uint SystemDpi;
        public uint CurrentWindowDpi;
        public List<MonitorDpi> Monitors;
    }

    public static class ScreenExtensions
    {
        [DllImport("User32.dll")]
        private static extern IntPtr MonitorFromPoint([In]System.Drawing.Point pt, [In]uint dwFlags);

        [DllImport("Shcore.dll")]
        private static extern IntPtr GetDpiForMonitor([In]IntPtr hmonitor, [In]DpiType dpiType, [Out]out uint dpiX, [Out]out uint dpiY);

        public static void GetDpi(this Screen screen, DpiType dpiType, out uint dpiX, out uint dpiY)
        {
            var pnt = new System.Drawing.Point(screen.Bounds.Left + 1, screen.Bounds.Top + 1);
            var mon = MonitorFromPoint(pnt, 2/*MONITOR_DEFAULTTONEAREST*/);
            GetDpiForMonitor(mon, dpiType, out dpiX, out dpiY);
        }
    }

    public enum DpiType
    {
        Effective = 0,
        Angular = 1,
        Raw = 2,
    }
}
