# ION - Is Online Now

ION is a cross-platform livestream notification application for Android and Windows.

The goal of ION is simple: allow users to follow creators across different streaming platforms and receive a notification when they go live, regardless of where they stream.

## Planned platforms

Initial priority:

* Twitch
* Kick
* YouTube
* Picarto
* Piczel.tv

Experimental / later integrations:

* TikTok
* Instagram

## Notification targets

ION is designed to deliver notifications through:

* Android push notifications
* Windows notifications
* Discord direct messages through an optional bot integration

## Channel configuration

Channels are not hardcoded.

Users will be able to add channels using their URL or username, allowing any supported channel to be monitored.

Examples:

```text
https://twitch.tv/channel
https://youtube.com/@channel
https://kick.com/channel
https://picarto.tv/channel
https://piczel.tv/watch/channel
https://tiktok.com/@channel
https://instagram.com/channel
```

ION will identify the platform, resolve the channel information and monitor its livestream status.

## Architecture

The project is divided into three main components:

### ION.App

.NET MAUI application targeting:

* Android
* Windows

Responsible for the user interface, channel management, settings and local notification handling.

### ION.Core

Shared domain layer containing:

* Streaming platform definitions
* Channel models
* Live event models
* Notification contracts
* Shared interfaces

This project contains no platform-specific implementation.

### ION.Server

ASP.NET Core backend responsible for:

* Monitoring streaming platforms
* Receiving webhooks
* Polling platforms when required
* Detecting online/offline transitions
* Dispatching notifications
* Firebase Cloud Messaging
* Discord integration
* Future synchronization between devices

During early development, ION.Server can run locally on the user's computer.

The architecture will allow it to be moved to an online server later without redesigning the applications.

## Initial development goal

The first functional milestone is:

```text
Configured Twitch channel
          ↓
OFFLINE → ONLINE
          ↓
       ION.Server
          ↓
 ┌────────┼────────┐
 ↓        ↓        ↓
Android  Windows  Discord
 Push     Alert      DM
```

The same notification pipeline will later be reused by every supported streaming platform.

## Development roadmap

### Milestone 0 - Foundation

* Create Git repository
* Create .NET solution
* Create MAUI app
* Create shared Core project
* Create ASP.NET Core server
* Run application on Windows
* Run application on Android

### Milestone 1 - Channel management

* Add channels by URL
* Detect platform automatically
* Store configured channels
* Remove and disable channels
* Show online/offline status

### Milestone 2 - Notification pipeline

* Android push through Firebase Cloud Messaging
* Windows native notification
* Discord DM
* Test notification command

### Milestone 3 - Twitch

* Resolve Twitch channel
* Check stream state
* Implement EventSub
* Detect offline → online transition
* Generate LiveEvent
* Send notification to all configured destinations

### Milestone 4 - Additional platforms

Planned implementation order:

```text
Twitch
  ↓
Kick
  ↓
YouTube
  ↓
Picarto
  ↓
Piczel.tv
  ↓
TikTok
  ↓
Instagram
```

TikTok and Instagram may require alternative or experimental detection strategies because their public APIs do not currently expose livestream-online events as conveniently as platforms such as Twitch.

## Project status

🚧 Early development

Current objective: build the cross-platform foundation and notification pipeline.

## Name

**ION** stands for:

> **Is Online Now**

The visual identity uses the `O` as a status indicator:

```text
I ○ N    Offline

I ● N    Online
```
