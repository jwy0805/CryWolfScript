# CryWolfScript — Production Unity Client for the Live Mobile PvP Game "Cry Wolf"

## Real-time 1v1 PvP Strategy client (deck-based RTS defense/offense) featuring UI flows, packet-driven networking, and a maintainable project structure.

> This README is intentionally focused on **code navigation and reviewability** (not player installation).
> For a full product write-up, see the portfolio: https://www.notion.so/Cry-Wolf-Portfolio-2f52f5b151de80529d24c00c87a685fa?source=copy_link

## Product Proof (Live)
- iOS (App Store): https://apps.apple.com/kr/app/id6745862935  
- Android (Google Play): https://play.google.com/store/apps/details?id=com.hamonstudio.crywolf&hl=ko

## Introductions
### Video
- Cry Wolf Game Play Demo:
- Cry Wolf Lobby Demo:
### ScreenShots

## Skills
- Unity C#
- MVVM-inspired UI architecture(Zenject), Addressables, IAP/Ads integration

## Start Here (Review Guide)
> If you only have 5 minutes, follow these entry points.

## Repository Map


## Game overview
Cry Wolf is a live mobile real-time PvP strategy game combining RTS and idle-style automation.
Player choose a faction (defense-oriented Sheep or offense-oriented Wolf), build a 6-card deck, and win through resource management and unit evolution timing.

## Architecture at a glance
```mermaid
flowchart LR
  subgraph Presentation[UI / Presentation]
    Scenes[Scenes]
    UI[UI]
  end

  subgraph Flow[Flow / Coordination]
    Controllers[Controllers]
  end

  subgraph Services[Services / Shared State]
    Managers[Managers]
    Contents[Contents]
  end

  subgraph Network[Networking]
    Web[Web]
    ServerCore[ServerCore]
    Packet[Packet]
  end

  Scenes --> UI --> Controllers --> Managers
  Managers --> Web --> ServerCore --> Packet
  Packet --> ServerCore --> Web --> Managers --> UI
