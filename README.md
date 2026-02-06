# CryWolfScript — Production Unity Client for the Live Mobile PvP Game "Cry Wolf"

## Real-time 1v1 PvP Stretegy client (deck-based RTS defence/offense) featuring UI flows, packet-driven networking, and a maintaining project structure.

### Product Proof (Live)
- iOS (App Store): https://apps.apple.com/kr/app/id6745862935  
- Android (Google Play): https://play.google.com/store/apps/detailsid=com.hamonstudio.crywolf&hl=ko

### Introductions
- Cry Wolf Game Play Demo:
- Cry Wolf Lobby Demo:

## Technologies
### Language
- Unity C#
### Skills
- MVVM-inspired UI architecture(Zenject), Addressables, IAP/Ads integration

## Features
- End-to-end real-time PvP client flow: login -> lobby -> matchmaking -> in-game state updates
- UI complexity control: clear boundaries between presentation, flow coordination, and shared services
- Network separation: packet definitions/serialization are isolated from gameplay/UI features

## Game overview
Cry Wolf is a live mobile real-time PvP stretegy game combining RTS and idle-style automation.
Player choose a faction (defence-oriented Sheep or offense-oriented Wolf), build a 6-card deck, and win through resource management and unit evolution timing.
