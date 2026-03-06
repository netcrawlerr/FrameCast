# FrameCast

![.NET](https://img.shields.io/badge/.NET-9.0-purple)
![Platform](https://img.shields.io/badge/platform-Windows-blue)
![License](https://img.shields.io/badge/license-MIT-green)
![Status](https://img.shields.io/badge/status-experimental-orange)

**FrameCast** is a real-time LAN screen streaming system built with **.NET**.  
It captures the screen, compresses frames using **JPEG**, and streams them through a **local TCP server** to clients connected on the same network.

FrameCast works in a similar spirit to remote desktop tools like **AnyDesk** and **TeamViewer**, but it focuses purely on **screen streaming over a local network** rather than full remote control.

---

##### Demo (click the image to play the video)

[![FrameCast Demo](assets/demo-thumbnail.png)](assets/framecast-demo.mp4)

---

## Features

- Real-time screen capture
- JPEG frame compression
- TCP-based streaming
- Local network (LAN) support
- Multiple client connections
- Desktop viewer

---

## Architecture

```text
FrameCast
│
├── FrameCast.App               # Desktop viewer (UI client)
├── FrameCast.Capture.Windows   # Windows screen capture
├── FrameCast.Core              # Shared core abstractions
├── FrameCast.Encoding          # JPEG frame compression
├── FrameCast.Protocol          # Frame message structure
├── FrameCast.Transport         # TCP networking layer
└── FrameCast.DebugTest         # Streaming server