# FrameCast Protocol

FrameCast uses a lightweight binary protocol to transmit screen frames between the server and clients over TCP.

The protocol is intentionally simple to minimize latency and reduce overhead during real-time streaming.

---

## Frame Message Structure

Each frame consists of a **12-byte header** followed by the **JPEG frame payload**.

```
+------------+--------------+------------------+
| DataLength | Timestamp    | Frame Data       |
| (4 bytes)  | (8 bytes)    | (variable size)  |
+------------+--------------+------------------+
```

Header size: **12 bytes**

---

## Field Description

### DataLength

```
Size: 4 bytes
Type: Int32
```

Specifies the size of the compressed frame payload in bytes.

The client reads this value to know how many bytes must be read from the TCP stream.

---

### Timestamp

```
Size: 8 bytes
Type: Int64
```

Unix timestamp representing when the frame was captured.

Used for:

- latency measurement
- debugging
- frame ordering

---

### Frame Data

```
Size: Variable
Type: Byte[]
```

Contains the **JPEG-compressed image** of the captured screen.

The client decodes this JPEG image and renders it to the viewer window.

---

## Example Frame Transmission

```
Header
--------------------------------
DataLength: 53240
Timestamp : 1700000000000

Payload
--------------------------------
JPEG Image Bytes
```

---

## Transmission Process

For every frame:

1. The server sends the 12-byte header.
2. The server sends the JPEG frame data.
3. The client reads the header.
4. The client reads the payload based on `DataLength`.
5. The client decodes the JPEG image.
6. The frame is rendered on the screen.

---

## Broadcast Model

The server broadcasts frames to **all connected clients**.

```
Capture Server
      |
      v
+-------------+
| TCP Server  |
+------+------+ 
       |
------------------------
|        |        |
v        v        v
Client   Client   Client
Viewer   Viewer   Viewer
```

Multiple viewers can watch the same stream simultaneously.

---

## Error Handling

Possible network issues include:

- Partial frame reads
- Client disconnections
- Network interruptions

The server automatically removes disconnected clients if writing to their stream fails.

---

## Protocol Advantages

The protocol is designed to provide:

- Low overhead
- Fast parsing
- Low latency streaming
- Efficient frame transmission

---

## Possible Future Extensions

The protocol can be extended to support:

- Frame type identifiers
- Delta frame compression
- Adaptive bitrate streaming
- Audio streaming
- Input control messages (mouse/keyboard)