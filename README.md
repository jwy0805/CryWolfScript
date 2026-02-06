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
<img width="210" height="380" alt="Image" src="https://github.com/user-attachments/assets/9060a177-9146-4d3e-b7b2-c3ab709fd96f" />
<img width="210" height="380" alt="Image" src="https://github.com/user-attachments/assets/1f80099f-3634-42c1-b4dc-6ce9aad641cc" />
<img width="180" height="380" alt="Image" src="https://github.com/user-attachments/assets/7b877155-c75e-4c25-990d-9969ced6231b" />
<img width="180" height="380" alt="Image" src="https://github.com/user-attachments/assets/94798c2d-f131-4a4d-a1fe-bcbaf86d9522" />
<img width="180" height="380" alt="Image" src="https://github.com/user-attachments/assets/cfd990f6-a635-4370-87f3-ee99e4ec136f" />
<img width="180" height="380" alt="Image" src="https://github.com/user-attachments/assets/826cfedd-2593-43de-85bf-363cb0c66ace" />
<img width="180" height="380" alt="Image" src="https://github.com/user-attachments/assets/10dcf078-fe9f-4c29-82c1-efba7d464cb6" />
<img width="180" height="380" alt="Image" src="https://github.com/user-attachments/assets/d25d2e4d-9296-40a2-9b66-9e1304afe132" />
<img width="180" height="380" alt="Image" src="https://github.com/user-attachments/assets/dd54a4ac-c686-4cfd-8635-aeace4d9792f" />
<img width="180" height="380" alt="Image" src="https://github.com/user-attachments/assets/42f3d509-7ae0-4ec0-b386-7155b0f44a95" />
<img width="180" height="380" alt="Image" src="https://github.com/user-attachments/assets/6d981ee7-b239-4293-afa7-6d9db42806b9" />
<img width="180" height="380" alt="Image" src="https://github.com/user-attachments/assets/0d94dfd4-322d-4a8d-84b3-35a1bf014670" />
<img width="180" height="380" alt="Image" src="https://github.com/user-attachments/assets/6f541fd3-5653-4fcc-9961-10a66e8291fe" />

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
