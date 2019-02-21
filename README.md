This project creates a WPF application which can detect system DPI, per-Monitor DPI.

# Pre-requisites

- .Net Framework 4.6.2 or later

Note: Per-Monitor DPI awareness is a windows feature, it requires Windows 10 1607 and later.

# How to Use

Just run the [compiled executable file](bin/Release/DPIMonitor.exe), it will show you:

- System DPI
- Current window DPI (for this application)
- per-Monitor DPI for every monitor

At same time, it writes output to `%TEMP%\DPIMonitor.json` file in below format:

```json
{
   "CurrentWindowDpi": 96,
   "Monitors": [
      {
         "DisplayName": "\\\\.\\DISPLAY1",
         "Dpi": 96
      },
      {
         "DisplayName": "\\\\.\\DISPLAY2",
         "Dpi": 96
      }
   ],
   "SystemDpi": 96
}
```

You can read this file by any other programming language for your automation testing.

DPI results on the application window and the `%TEMP%\DPIMonitor.json` file will be updated immedately on below events:

- **DPI of the screen which this application is running on changed.** For example, you change the DPI from Settings manually; you drag and drop the window of this application to another screen which has different DPI.

- **Window size of this application changed.**

So it's also useful for manual testing.

# Known Issues

## Issue 1

**Problem:** In multi-monitor environment, if this application is running on Monitor 1, you changed DPI of Monitor 2. The DPI results will not be updated.

**Workaround:** Resize the window of this application or restart this application.
