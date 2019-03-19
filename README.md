# BLE
Bomberman Learning Environment

This project uses ML-Agent plugin for Unity3D version 0.5, so the prerequisites are:

Unity3d Game engine > 5.0.0 (https://unity3d.com/pt/get-unity/download)

ML-Agents 0.5v Plugin (https://github.com/icaro56/ml-agents   (fork with some changes))

It's important install the ML-Agents dependencies.

# Configuration
we need change the .NetFramework to 4.6 and mark the Run Background checkbox case this is not configured.

Edit->Project Settings->Players

  Resolution and Presentation
  
    Run in Background
  
  Other Settings
  
    Configuration
    
      Script Runtime Version
      
        set 4.6
        
# Experiments Settings

We created a tag named bomberman_experiment both in this repository and in our ml-agent fork respository to expose which Bomberman Learning Environment version was used to run the experiments that were done in the paper. This tag is to PPO and LSTM trainings. The Imitation Learning uses the most up-to-date version.

All statistics and all templates generated in agent training in this environment can be found in Google Drive: https://drive.google.com/file/d/15ZEPz4j3FvPfEAhPd-ZrUN5Hj5EE9ajQ/view?usp=sharing


