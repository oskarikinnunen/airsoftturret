Airsoft turret built for the TSTOS 2021 airsoft event. Features:
  - 360 degree rotation turret, bolted on top of a fabulous Lada Samara combat vehicle
  - Night vision (Infrared camera and lights)
  - Wireless control of fully automatic airsoft weapon using a laptop and an xbox 360 controller

For the X axis movement it's using a car windshield wiper, with a chain drive mechanism. For the Y axis it's using a smaller continuous servo motor with a 3D printed worm gear mechanism.

Uses my own software which comprises of a Windows client, a Raspberry Pi server, and an Arduino.
The Windows client, built using Unity/C#, displays video from a camera at a URL and gathers input data from controller/keyboard inputs. It then constructs a control string that is sent to the Raspberry Pi over a local network.

The Raspberry Pi server software, built using .NET Core 3.1/C#, receives the control string and sends it to the Arduino via USB. The Raspberry Pi also has USB cameras attached to it, and it uses a free software called "Motion" to stream the video from these cameras back to the Windows client.

The Arduino, programmed in C/C++, receives the control string and interprets it. It then communicates with multiple motor controllers and relays using digitalwrite, analogwrite, and some functions from the Servo.h header.

## Images:
![view](https://user-images.githubusercontent.com/45420297/219242728-62cbb14f-009e-40f1-b8f8-b266e7d0f40b.gif)
<img src="https://user-images.githubusercontent.com/45420297/182593473-d3c663e6-0622-4838-8d1d-447931a8da50.png" alt="drawing" width="500"/>
<img src="https://user-images.githubusercontent.com/45420297/182592581-a92fcc81-68ac-4bcb-899a-2feb51ec9567.gif" alt="drawing" width="500"/>
<img src="https://user-images.githubusercontent.com/45420297/182594155-0909a873-9fb5-4136-91cd-f186e8420faf.png" alt="drawing" width="500"/>
