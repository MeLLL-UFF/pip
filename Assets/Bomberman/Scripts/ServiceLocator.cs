using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServiceLocator : Singleton<ServiceLocator>
{
    static BombManager sBombManager;
    static BlocksManager sBlocksManager;
    static PlayersManager sPlayersManager;
    static LogManager sLogManager;

    protected override void Init()
    {
        DontDestroyOnLoad(this);

        sLogManager = new LogManager();
        sBombManager = new BombManager();
        sBlocksManager = new BlocksManager();
        sPlayersManager = new PlayersManager();
    }

    public static BombManager GetBombManager()
    {
        Debug.Assert(sBombManager != null, "sBombManager is null.");

        return sBombManager;
    }

    public static BlocksManager GetBlocksManager()
    {
        Debug.Assert(sBlocksManager != null, "sBlocksManager is null.");

        return sBlocksManager;
    }

    public static PlayersManager GetPlayersManager()
    {
        Debug.Assert(sPlayersManager != null, "sPlayersManager is null.");

        return sPlayersManager;
    }

    public static LogManager GetLogManager()
    {
        Debug.Assert(sPlayersManager != null, "sPlayersManager is null.");

        return sLogManager;
    }
}
