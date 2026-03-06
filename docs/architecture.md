# FrameCast Architecture

FrameCast is a modular real-time LAN screen streaming system built with .NET.  
The system captures the screen, compresses frames using JPEG, and streams them through a TCP server to clients on the same network.

The architecture separates responsibilities into independent modules to keep the system simple, maintainable, and extensible.

---

## System Overview

The streaming pipeline follows this flow:

Screen Capture → Frame Encoding → TCP Transport → Frame Broadcast → Client Rendering

```
+--------------------+
| Screen Capture     |
| (Windows API)      |
+---------+----------+
          |
          v
+--------------------+
| JPEG Encoder       |
| (FrameCast.Encoding)
+---------+----------+
          |
          v
+--------------------+
| TCP Transport      |
| (FrameCast.Transport)
+---------+----------+
          |
          v
+--------------------+
| LAN Clients        |
| (FrameCast.App)    |
+--------------------+
```

---

## Project Structure

```
FrameCast
│
├── FrameCast.App               # Desktop viewer (UI client)
├── FrameCast.Capture.Windows   # Windows screen capture
├── FrameCast.Core              # Shared core abstractions
├── FrameCast.Encoding          # JPEG frame compression
├── FrameCast.Protocol          # Frame message structure
├── FrameCast.Transport         # TCP networking layer
└── FrameCast.DebugTest         # Streaming server
```

---

## Module Responsibilities

### FrameCast.Core

Contains shared abstractions used across the project.

Responsibilities:

- Core interfaces
- Shared types
- Base contracts between modules

This module keeps other components loosely coupled.

---

### FrameCast.Capture.Windows

Responsible for capturing the screen on Windows systems.

Responsibilities:

- Capturing the full desktop
- Producing bitmap frames
- Supplying frames to the encoding pipeline

---

### FrameCast.Encoding

Handles frame compression before transmission.

Responsibilities:

- Converting bitmap frames into JPEG images
- Reducing frame size
- Improving network efficiency

JPEG compression significantly reduces the amount of data transmitted across the network.

---

### FrameCast.Protocol

Defines the structure of messages exchanged between the server and clients.

Responsibilities:

- Frame message structure
- Serialization and deserialization
- Protocol consistency between sender and receiver

---

### FrameCast.Transport

Responsible for network communication.

Responsibilities:

- TCP server implementation
- Client connection management
- Frame broadcasting
- Network stream handling

The server listens on the LAN interface and distributes frames to all connected clients.

---

### FrameCast.App

The desktop viewer application.

Responsibilities:

- Connecting to the streaming server
- Receiving frame messages
- Decoding JPEG frames
- Rendering frames to the screen

---

### FrameCast.DebugTest

Used for development and testing.

Responsibilities:

- Running the screen capture pipeline
- Encoding frames
- Sending frames through the transport layer

---

## Runtime Data Flow

At runtime the system behaves as follows:

1. The screen is captured using the Windows capture module.
2. The captured bitmap is passed to the JPEG encoder.
3. The encoded frame is wrapped inside a `FrameMessage`.
4. The frame is sent through the TCP transport layer.
5. The server broadcasts the frame to connected clients.
6. Clients receive and decode the frame.
7. The frame is rendered on the viewer.

---

## Network Model

FrameCast uses a **local TCP server broadcasting model**.

```
+------------+
| Server     |
| (Capture)  |
+-----+------+
      |
-------------------------
|          |            |
v          v            v
Client     Client       Client
Viewer     Viewer       Viewer
```

A single machine captures the screen and streams frames to multiple clients on the same LAN.

---

## Design Goals

FrameCast was designed with the following goals:

- Simple architecture
- Modular components
- Efficient LAN streaming
- Low latency frame transmission
- Easy extensibility

---

## Current Limitations

FrameCast currently focuses only on **screen streaming**.

Not included:

- Remote keyboard/mouse control
- Authentication
- Internet streaming
- Audio transmission