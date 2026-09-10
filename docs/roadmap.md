# ION Development Roadmap

Development roadmap for **ION - Is Online Now**.

The roadmap prioritizes building a stable cross-platform foundation before expanding streaming platform support.

---

# Milestone 0 - Project Foundation

**Goal:** Establish the development environment and project structure.

* [x] Define project name: ION - Is Online Now
* [x] Define initial architecture
* [x] Define streaming platform priorities
* [x] Define Android, Windows and Discord as notification targets
* [x] Create Git repository
* [x] Create GitHub repository
* [x] Create .NET solution
* [x] Create ION.App
* [x] Create ION.Core
* [x] Create ION.Server
* [x] Reference ION.Core from App and Server
* [ ] Add `.gitignore`
* [ ] Add `.editorconfig`
* [x] Add README
* [x] Add architecture documentation
* [x] Add roadmap
* [x] Build entire solution successfully
* [x] Create initial Git commit
* [x] Push initial repository to GitHub

---

# Milestone 1 - Cross-platform Application

**Goal:** Prove that ION runs as the same application on Windows and Android.

* [ ] Create initial ION interface
* [ ] Display ION branding
* [ ] Implement offline logo state
* [ ] Implement online logo state
* [x] Run ION.App on Windows
* [x] Configure Android emulator
* [x] Run ION.App on Android emulator
* [x] Test ION.App on a physical Android device

Expected result:

```text
ION.App
│
├── Windows ✅
└── Android ✅
```

---

# Milestone 2 - Channel Management

**Goal:** Allow channels to be configured dynamically.

* [ ] Create `StreamingPlatform` enum
* [ ] Create `StreamChannel` model
* [ ] Create channel repository
* [ ] Create Add Channel interface
* [ ] Accept channel URLs
* [ ] Automatically detect platform from URL
* [ ] Normalize channel URLs
* [ ] Display configured channels
* [ ] Remove channels
* [ ] Enable/disable channels
* [ ] Enable/disable notifications per channel
* [ ] Store channels locally

Recognized platforms:

* [ ] Twitch
* [ ] Kick
* [ ] YouTube
* [ ] Picarto
* [ ] Piczel.tv
* [ ] TikTok
* [ ] Instagram

---

# Milestone 3 - Notification Pipeline

**Goal:** Send a test notification to every initial notification target without requiring a real livestream.

## Android

* [ ] Create Firebase project
* [ ] Configure Firebase Cloud Messaging
* [ ] Register Android application
* [ ] Obtain device FCM token
* [ ] Register device with ION.Server
* [ ] Send test push notification
* [ ] Open ION from notification

## Windows

* [ ] Implement native Windows notification
* [ ] Send test notification
* [ ] Open ION from notification

## Discord

* [ ] Create ION Discord application
* [ ] Create ION bot
* [ ] Implement user opt-in
* [ ] Send test DM
* [ ] Add stream link button

Expected result:

```text
             TEST EVENT
                 │
          NotificationDispatcher
                 │
        ┌────────┼────────┐
        ▼        ▼        ▼
     Android   Windows  Discord
        🔔        🔔       💬
```

---

# Milestone 4 - Twitch Integration

**Goal:** Detect a configurable Twitch channel going live.

* [ ] Create Twitch developer application
* [ ] Implement `TwitchPlatform`
* [ ] Accept Twitch channel URL
* [ ] Resolve username
* [ ] Resolve broadcaster ID
* [ ] Retrieve channel information
* [ ] Retrieve avatar
* [ ] Retrieve livestream state
* [ ] Implement EventSub
* [ ] Handle `stream.online`
* [ ] Handle `stream.offline`
* [ ] Convert Twitch event to `LiveEvent`
* [ ] Prevent duplicate events
* [ ] Dispatch notification

## First major ION test

```text
Twitch test channel
       │
       ▼
     OFFLINE
       │
   Start stream
       │
       ▼
     ONLINE
       │
       ▼
    LiveEvent
       │
 ┌─────┼─────┐
 ▼     ▼     ▼
📱    🖥️     💬
```

Success requires the same live event to reach:

* [ ] Android
* [ ] Windows
* [ ] Discord

---

# Milestone 5 - Kick Integration

* [ ] Implement `KickPlatform`
* [ ] Resolve channel URL
* [ ] Resolve channel ID
* [ ] Retrieve channel metadata
* [ ] Implement livestream status events
* [ ] Implement fallback status checking if necessary
* [ ] Produce `LiveEvent`
* [ ] Test online transition
* [ ] Test notifications

---

# Milestone 6 - YouTube Integration

* [ ] Implement `YouTubePlatform`
* [ ] Resolve YouTube channel URLs
* [ ] Support `@handle`
* [ ] Resolve stable channel ID
* [ ] Configure YouTube Data API
* [ ] Implement WebSub
* [ ] Identify livestream content
* [ ] Detect active livestream
* [ ] Produce `LiveEvent`
* [ ] Monitor API quota usage
* [ ] Test notifications

---

# Milestone 7 - Picarto Integration

* [ ] Research current Picarto API
* [ ] Implement `PicartoPlatform`
* [ ] Resolve channel URL
* [ ] Retrieve channel state
* [ ] Determine webhook/polling strategy
* [ ] Detect online transition
* [ ] Produce `LiveEvent`
* [ ] Test notifications

---

# Milestone 8 - Piczel.tv Integration

* [ ] Research current Piczel API/status endpoints
* [ ] Document available API behavior
* [ ] Implement `PiczelPlatform`
* [ ] Resolve `/watch/{username}` URLs
* [ ] Retrieve channel state
* [ ] Determine monitoring strategy
* [ ] Detect online transition
* [ ] Produce `LiveEvent`
* [ ] Test notifications

---

# Milestone 9 - TikTok Experimental Integration

**Status:** Experimental.

* [ ] Research current official LIVE capabilities
* [ ] Document API limitations
* [ ] Implement `TikTokPlatform`
* [ ] Resolve TikTok profile URL
* [ ] Investigate reliable LIVE detection
* [ ] Avoid depending on unstable mechanisms where possible
* [ ] Produce `LiveEvent` if reliable detection is possible
* [ ] Mark integration experimental if necessary

TikTok support must not compromise stable ION integrations.

---

# Milestone 10 - Instagram Experimental Integration

**Status:** Experimental.

* [ ] Research current official Instagram capabilities
* [ ] Document API limitations
* [ ] Implement `InstagramPlatform`
* [ ] Resolve Instagram profile URL
* [ ] Investigate reliable LIVE detection
* [ ] Produce `LiveEvent` if reliable detection is possible
* [ ] Mark integration experimental if necessary

Instagram support must not compromise stable ION integrations.

---

# Milestone 11 - Persistent Settings

* [ ] Notification preferences
* [ ] Per-channel notification targets
* [ ] Android enabled/disabled
* [ ] Windows enabled/disabled
* [ ] Discord enabled/disabled
* [ ] Quiet hours
* [ ] Application startup preferences
* [ ] Monitoring interval preferences where applicable

---

# Milestone 12 - Live Dashboard

* [ ] Display all monitored channels
* [ ] Separate online and offline channels
* [ ] Display channel avatars
* [ ] Display livestream title
* [ ] Display category/game when available
* [ ] Display platform
* [ ] Display livestream start time
* [ ] Open stream directly
* [ ] Refresh channel state

---

# Milestone 13 - Online Backend

**Goal:** Move from personal/local operation to an always-online service.

* [ ] Select hosting provider
* [ ] Configure production database
* [ ] Deploy ION.Server
* [ ] Configure HTTPS
* [ ] Move platform webhooks to production endpoint
* [ ] Implement user accounts
* [ ] Synchronize channels between devices
* [ ] Synchronize notification preferences
* [ ] Secure API
* [ ] Implement rate limiting
* [ ] Implement logging and monitoring

---

# Milestone 14 - Public Release Preparation

* [ ] Application icon
* [ ] Final ION visual identity
* [ ] Android adaptive icon
* [ ] Windows icon
* [ ] Light theme
* [ ] Dark theme
* [ ] Portuguese localization
* [ ] English localization
* [ ] Privacy policy
* [ ] Terms where required
* [ ] Setup documentation
* [ ] Contributor documentation
* [ ] Release build pipeline
* [ ] Android release build
* [ ] Windows release build

---

# Future Ideas

These are not requirements for the initial release.

* [ ] Scheduled livestream notifications
* [ ] Upcoming livestream dashboard
* [ ] Notification history
* [ ] Live session history
* [ ] Category/game filters
* [ ] Notification sounds per platform
* [ ] Custom notification sounds
* [ ] Import/export followed channels
* [ ] OPML-like channel list format
* [ ] Discord server notifications in addition to DMs
* [ ] Multiple Discord accounts
* [ ] Stream thumbnails
* [ ] Viewer count
* [ ] Favorite channels
* [ ] Priority notifications
* [ ] Notification grouping
* [ ] Statistics
* [ ] Multi-device synchronization
* [ ] Additional streaming platforms

---

# Current Target

The immediate target is:

> **Run ION.App successfully on both Windows and Android before implementing any streaming API.**

After that:

> **Send one simulated LiveEvent to Android, Windows and Discord.**

Then:

> **Replace the simulated event with a real Twitch `OFFLINE → ONLINE` event.**

That will mark the first fully functional version of ION.
