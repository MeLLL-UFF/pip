# pip
Player Imitation Project

This project uses ML-Agent plugin for Unity3D, so the prerequisites are:

Unity3d Game engine > 5.0.0 (https://unity3d.com/pt/get-unity/download)

ML-Agents Plugin (https://github.com/Unity-Technologies/ml-agents)

It's important install the ML-Agents dependencies.

# Configuration
After we download the ML-Agents plugin for Unity3d, it's necessary copy the folder ml-agents\unity-environment\Assets\ML-Agents to our Unity Project. Moreover, we have to add the TFSharpPlugin.unitypackage to our project as custom package via menu Assets->Import Package->Custom Package...

Furthermore, we need change the .NetFramework to 4.6 and mark the Run Background checkbox.

Edit->Project Settings->Players

  Resolution and Presentation
  
    Run in Background
  
  Other Settings
  
    Configuration
    
      Script Runtime Version
      
        set 4.6


