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

# Videos
PPO Agents with Binary Flag State Representation: https://youtu.be/qM8n9tmvBdw

PPO Agents with Normalized Binary Flag State Representation: https://youtu.be/_YlMBlMhHL8

PPO Agents with ICAART State Representation: https://youtu.be/eIjo7Vat-aE

PPO Agents with ZerOrOne State Representation: https://youtu.be/SC2vUTtlmSA

PPO Agents with Hybrid State Representation: https://youtu.be/w7noFJ_w2GQ


PPO Agents Tournament Example Hybrid x ZeroOrOne x ICAART x Normalized Binary Flag: https://youtu.be/oRKZfGiBlqs


PPO+LSTM Agents with Hybrid State Representation: https://youtu.be/sJ79-OHrFGM

PPO versus PPO+LSTM: https://youtu.be/QQ4kThziFa8

BC+PPO versus PPO+LSTM: https://youtu.be/2ccxyoHbI4o
