IL2Winwing
==========

This application is a simple proxy between IL-2 GB and WinWing SimApp Pro, enabling vibration feedback for owners of the WinWing sticks and
throttles equiped with their "Dynamic Vibration Motor".

Installation and usage
======================
1. Download the latest release from the [releases page](https://github.com/AndersHogqvist/IL2WinWing/releases)
2. Unzip the archive to a folder of your choice
3. Make sure SimApp Pro is running and that you're using the default settings on your devices under "Dynamic Vibration Motor"
4. Make sure you have the following section in your `startup.cfg` file located in your IL-2 GB installation folder:
```
[KEY = telemetrydevice]
      addr = "127.0.0.1"
      decimation = 2
      enable = true
      port = 4322
[END]
```
5. Start IL2WinWing.exe (the first time you run it, you will be asked to allow it to communicate through the firewall)
6. Start IL-2 GB
7. Enjoy!

If telemetry is already used
============================
If you have SimHaptic or some other application that is reading telemetry on port 4322 you need to add the following to `startup.cfg` in
order for IL2WinWing to work:
```
[KEY = telemetrydevice]
      addr = "127.0.0.1"
      addr1 = "127.0.0.1:29373"
      decimation = 2
      enable = true
      port = 4322
[END]
```
You can choose whatever port number you want for IL2WinWing, in this case it's 29373. Just make sure that the port number in `startup.cfg`
matches with the number in `IL2WinWing.dll.config`.

Changing port number
====================
If you for some reason need to change the port number, you can do so by editing the `IL2WinWing.dll.config` file in the same folder as the executable.
