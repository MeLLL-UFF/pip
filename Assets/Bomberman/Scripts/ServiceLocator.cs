using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServiceLocator : Singleton<ServiceLocator>
{
    static BombManager sBombManager;

    protected override void Init()
    {
        DontDestroyOnLoad(this);

        sBombManager = new BombManager();
    }

    public static BombManager GetBombManager()
    {
        Debug.Assert(sBombManager != null, "sBombManager is null.");

        return sBombManager;
    }
}
