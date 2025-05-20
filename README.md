# DigitalCoach-Scripts

This repository shows the implementation of the digital coach in our research, which mainly contains the scripts used in Virtual Scene, IO Module, Task Manager, and Data Recorder.

## Overview
The digital coach prototype contains two core components: 1) an accident-enabled virtual training scenario embedded with crane operation knowledge and employing roguelike game mechanics (e.g., permanent death), guided by the lean concept of autonomation; and 2) a lean-oriented training protocol based on VSM, using lean indicators such as waste time to enable automatic performance analysis and real-time personalized coaching. This repository provides comprehensive resources to support the future refinement and application of this digital coaching approach across other domains.
## System Architecture
Development of the digital coach prototype primarily integrates four modules: Virtual Scene, Input/Output (IO) Module, Task Manager, and Data Recorder. 
![image](https://github.com/user-attachments/assets/49df488c-1bec-43df-9ccb-fdee4f1e2ba7)
### Virtual Scene
Contains the following four sets of objects: (1) buildings, (2) two joysticks, a walkie-talkie, and a switch button for direct interaction, (3) a tower crane, (4) the site environment, ground crew, and other construction machinery.\
Contains the core scene definition and is implemented with object-level scripts: `Equipment/`, `GameObject/`, `Performance/`, `Physics/`, `Renderer/`, `VRInteractable/`, and `UI/`.
### IO Module
Utilizes ForceSeatMI, ForceSeatPM, XRI toolkit, Unity UI toolkit, and Google Cloud API for developing interactive functions in Unity3D game engine.\
Implemented with external-interface scripts: `Controller/`, `ForceSeat/`, and `TextToSpeech/`.
### Task Manager
Monitors the virtual scene and updates it based on the training progress in completing a lifting task.\
Implemented with scripts in the folder `Task/`, enabling the flow control in the coaching process.
### Data Recorder
Records, analyzes, and visualizes training profiles for providing real-time personalized coaching.\
Implemented with scripts in the folder `Recorder/` and the script `analyseSingleExp`.
## File Structure

- Packages/
  - manifest.json  
    - Direct dependency list  
  - packages-lock.json  
    - Locked versions of all dependencies  
- Scripts/
  - Controller/  
    - Scripts for **Controllers** to control **Equipment** in the **Virtual Scene**  
  - Equipment/  
    - Scripts for **Equipment** in the **Virtual Scene** such as **Tower Crane**  
  - ForceSeat/  
    - Scripts to handle the **Motion Platform**  
  - GameObject/  
    - Scripts for the functionality of **GameObjects** placed in the **Virtual Scene**  
  - Models/  
    - Data models  
  - Performance/  
    - Scripts for **Performances** (e.g., wind start-up, prompting workers to step away from a lifted object) in the **Virtual Scene**  
  - Physics/  
    - Implementation of some **Physical** behaviors  
  - Recorder/  
    - **Recorder** for experiments  
  - Renderer/  
    - Render utilities  
  - Tag/  
    - Tagging utilities  
  - Task/  
    - Scripts for **Task** management implementations  
  - TextToSpeech/  
    - TextToSpeech utilities  
  - Tool/  
    - Auxiliary tools  
  - UI/  
    - UI components  
  - VRInteractable/  
    - Scripts for **GameObjects** that can be interacted with VR devices  
  - GlobalVariables.cs  
    - Auxiliary global variables  
  - StartButton.cs  
    - Start button logic
  - analyseSingleExp.py
    - Data analysis and visualization
- visio_0416.png  
  - Figure base for partial data visualization
- README.md  
  - This file
