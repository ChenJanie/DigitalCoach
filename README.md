# DigitalCoach-TaskManager

This repository contains the **TaskManager** flow control implementation described in the paper *“Integrating Lean Thinking into Safety Training: A Digital Coach for Enhancing Operation Safety and Productivity in Virtual Environments”*. The core code resides in the `Scripts/` folder, and all Unity Package Manager dependencies are specified in `Packages/manifest.json` and locked in `Packages/packages-lock.json`.

## File Structure

```
/
├─ Packages/
│  ├─ manifest.json           # Direct dependency list
│  └─ packages-lock.json      # Locked versions of all dependencies
├─ Scripts/
│  ├─ Controller/             # Control logic scripts
│  ├─ Equipment/              # Equipment-related scripts
│  ├─ ForceSeat/              # Force seat handling scripts
│  ├─ GameObject/             # Game object utilities
│  ├─ Models/                 # Data models
│  ├─ Performance/            # Performance monitoring
│  ├─ Physics/                # Physics utilities
│  ├─ Recorder/               # Recording/logging scripts
│  ├─ Renderer/
│  │   └─ LineRenderer/       # Line rendering utilities
│  ├─ Tag/                    # Tagging utilities
│  ├─ Task/                   # Task management implementations
│  ├─ TextToSpeech/           # TTS integration scripts
│  ├─ Tool/                   # Tooling helpers
│  ├─ UI/                     # UI components
│  ├─ VRInteractable/         # VR interaction handlers
│  ├─ GlobalVariables.cs      # Global configuration and constants
│  └─ StartButton.cs          # Entry point for scene start
└─ README.md                  # This file
```
