# ION Architecture

This document describes the technical architecture of **ION - Is Online Now**.

ION is designed as a cross-platform livestream monitoring and notification system supporting Android, Windows and Discord.

## Overview

ION is divided into three primary projects:

```text
ION
│
├── ION.App
│   └── Cross-platform client
│       ├── Android
│       └── Windows
│
├── ION.Core
│   └── Shared domain models and contracts
│
└── ION.Server
    └── Monitoring and notification backend
```

The separation between client, core and server allows the backend to initially run locally while remaining portable to an online hosting environment later.

---

## High-level data flow

```text
Streaming Platforms
│
├── Twitch
├── Kick
├── YouTube
├── Picarto
├── Piczel.tv
├── TikTok
└── Instagram
        │
        ▼
┌───────────────────────┐
│      ION.Server       │
│                       │
│ Platform integrations │
│ Webhooks / Polling    │
│ State detection       │
└───────────┬───────────┘
            │
            ▼
        LiveEvent
            │
            ▼
 Notification Dispatcher
            │
      ┌─────┼─────┐
      ▼     ▼     ▼
   Android Windows Discord
      │     │       │
     FCM   Native    Bot
      │    Alert     DM
      ▼     ▼       ▼
     📱     🖥️       💬
```

---

# ION.App

`ION.App` is the user-facing application built with .NET MAUI.

Supported targets:

* Android
* Windows

Its responsibilities include:

* Displaying monitored channels
* Adding and removing channels
* Automatically identifying platforms from URLs
* Displaying online/offline state
* Managing notification preferences
* Managing application settings
* Registering devices for notifications
* Communicating with ION.Server

The application must not contain platform-specific livestream monitoring logic.

For example, `ION.App` should not need to understand how Twitch EventSub works.

It only understands common ION models such as `StreamChannel` and `LiveEvent`.

---

# ION.Core

`ION.Core` contains shared domain models, enums, interfaces and contracts.

It must remain independent of:

* Twitch
* Firebase
* Discord
* Android
* Windows
* ASP.NET Core
* MAUI UI implementation

Example domain objects:

```text
StreamChannel
LiveEvent
Notification
NotificationTarget
StreamingPlatform
```

Example platform enumeration:

```csharp
public enum StreamingPlatform
{
    Twitch,
    Kick,
    YouTube,
    Picarto,
    Piczel,
    TikTok,
    Instagram
}
```

A configured channel should contain information such as:

```text
Internal ID
Platform
Platform Channel ID
Username
Display Name
Channel URL
Avatar URL
Online State
Notification State
```

---

# ION.Server

`ION.Server` is an ASP.NET Core application responsible for monitoring livestream platforms and dispatching notifications.

During early development it will run locally on the development computer.

Future versions may move the same server application to online infrastructure.

Responsibilities include:

* Resolving channel URLs
* Monitoring configured channels
* Receiving platform webhooks
* Performing polling when necessary
* Detecting livestream state transitions
* Preventing duplicate notifications
* Creating LiveEvent objects
* Sending Android push notifications
* Communicating with Windows clients
* Sending Discord DMs

---

# Streaming Platform Architecture

Each streaming platform must be implemented independently.

Conceptually:

```text
IStreamingPlatform
│
├── TwitchPlatform
├── KickPlatform
├── YouTubePlatform
├── PicartoPlatform
├── PiczelPlatform
├── TikTokPlatform
└── InstagramPlatform
```

A platform implementation is responsible for converting platform-specific information into ION domain objects.

For example:

```text
Twitch EventSub
       │
       ▼
 TwitchPlatform
       │
       ▼
    LiveEvent
```

and:

```text
Kick Event
    │
    ▼
KickPlatform
    │
    ▼
 LiveEvent
```

Everything after `LiveEvent` is platform-independent.

---

# Platform Monitoring Strategies

Not every streaming service exposes the same capabilities.

ION therefore supports different monitoring strategies.

## Webhook

The platform actively informs ION about a state change.

Example:

```text
Streamer starts
      ↓
Platform
      ↓
Webhook
      ↓
ION.Server
```

Preferred whenever available.

## Polling

ION periodically checks the current state.

Example:

```text
ION.Server
    ↓
Check channel
    ↓
Offline

30 seconds later

ION.Server
    ↓
Check channel
    ↓
Online
```

## Hybrid

Uses webhooks as the primary mechanism with occasional polling as a fallback.

## Experimental

Used when a platform does not provide a stable public mechanism for livestream status detection.

---

# Initial Platform Priority

Current planned implementation order:

```text
1. Twitch
2. Kick
3. YouTube
4. Picarto
5. Piczel.tv
6. TikTok
7. Instagram
```

TikTok and Instagram are considered lower priority because their public APIs currently provide fewer convenient mechanisms for detecting arbitrary livestream start events.

Their implementations must not delay or compromise support for the other platforms.

---

# Notification Architecture

Livestream detection and notification delivery are separate systems.

A streaming platform produces:

```text
LiveEvent
```

The notification dispatcher determines where that event should be delivered.

Conceptually:

```text
LiveEvent
    │
    ▼
NotificationDispatcher
    │
    ├── AndroidNotificationProvider
    ├── WindowsNotificationProvider
    └── DiscordNotificationProvider
```

This prevents platform integrations from depending on notification technology.

For example, `TwitchPlatform` must never send Firebase messages directly.

---

# Android Notifications

Android push notifications will use Firebase Cloud Messaging (FCM).

Expected flow:

```text
ION Android
     ↓
FCM registration
     ↓
Device Token
     ↓
ION.Server
     ↓
Firebase
     ↓
Android Device
```

This allows notifications to arrive while the application is not open.

---

# Windows Notifications

The Windows application will support native Windows notifications.

During early development the Windows client may maintain communication with the locally running ION.Server.

The notification implementation must remain replaceable so that future versions can use a cloud push mechanism without affecting livestream integrations.

---

# Discord Notifications

Discord support will use an ION bot.

Users will explicitly opt in to Discord notifications.

Expected flow:

```text
LiveEvent
    ↓
ION.Server
    ↓
ION Discord Bot
    ↓
Direct Message
```

Discord support is a notification destination, not a livestream platform integration.

---

# Channel Configuration

ION must never require channels to be hardcoded.

Users can configure channels using URLs or supported usernames.

Examples:

```text
https://twitch.tv/example
https://youtube.com/@example
https://kick.com/example
https://picarto.tv/example
https://piczel.tv/watch/example
https://tiktok.com/@example
https://instagram.com/example
```

ION should:

1. Detect the platform.
2. Normalize the URL.
3. Resolve the platform-specific channel identifier.
4. Store the normalized channel.
5. Begin monitoring it.

Whenever possible, ION should monitor channels using stable platform IDs rather than usernames.

---

# State Transitions

ION notifications are based on transitions, not simply online state.

For example:

```text
OFFLINE
   ↓
ONLINE
   ↓
SEND NOTIFICATION
```

Repeated checks returning `ONLINE` must not produce repeated notifications.

Example:

```text
10:00 OFFLINE

10:01 ONLINE → notify

10:02 ONLINE → ignore

10:03 ONLINE → ignore

11:30 OFFLINE

14:00 ONLINE → notify
```

---

# Duplicate Protection

ION must track livestream sessions to prevent duplicate notifications caused by repeated webhook delivery or polling.

A live session may contain:

```text
Channel ID
Platform
Platform Stream ID
Started At
Detected At
Notification Sent
```

---

# Local Development

Initially:

```text
Development PC

ION.App (Windows)
ION.Server
        │
        ├── Internet APIs
        ├── Firebase
        └── Discord
```

An Android device communicates with the same system through the appropriate network and notification services.

The server architecture must not assume that it will always run locally.

---

# Future Hosting

Eventually:

```text
Android ──┐
          │
Windows ──┼──► ION.Server (Cloud)
          │
Discord ◄─┘
```

Moving ION.Server online should not require redesigning the platform integrations or ION.Core.

---

# Architectural Principles

ION follows these principles:

1. Channels are user-configurable.
2. Streaming platforms are independent adapters.
3. Notification destinations are independent providers.
4. Platform-specific APIs do not leak into ION.Core.
5. Livestream detection produces common LiveEvent objects.
6. Notifications are triggered by state transitions.
7. Duplicate notifications must be prevented.
8. Android and Windows are first-class targets from the beginning.
9. Local development and future cloud hosting use the same backend architecture.
10. Experimental integrations must not compromise stable integrations.
