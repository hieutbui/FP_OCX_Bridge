# FP_OCX_Bridge

***This is a bridge to use Ronald Jack SDK 2020 in JavaScript***

## Installation

1. Build the solution.
2. Copy `Interop.FP_CLOCKLib.dll` inside `lib` folder to the built output folder or where you place your executable file.
3. Using `execFile` or any other method in JavaScript to run the `FP_OCX_Bridge.exe` file.

## Compatible devices

***This library aims to support any Ronald Jack device with default port is 5005***

### Tested devices
- **Ronald Jack 3800Pro**: This device is tested and works with the bridge.

## Current functionality
- **Connect**: Connect to the device.
- **Disconnect**: Disconnect from the device.
- **EnableDevice**: Enable the device.
- **DisableDevice**: Disable the device.
- **GetAllGeneralLog**: Get all general logs from the device.

## Attention:
- This library is not an official library from Ronald Jack and is not responsible for any damage.
- Time accuracy of a general log is up to minutes, not seconds. ***(hh:mm:00)***
