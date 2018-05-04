using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServiceLocator : Singleton<ServiceLocator>
{
    static BombManager sBombManager;
    static BlocksManager sBlocksManager;

    protected override void Init()
    {
        DontDestroyOnLoad(this);

        sBombManager = new BombManager();
        sBlocksManager = new BlocksManager();
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
}
