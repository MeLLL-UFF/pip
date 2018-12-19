using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers
{
    public int id;

    private BombManager sBombManager;
    private BlocksManager sBlocksManager;
    private LogManager sLogManager;
    private PlayerManager myPlayerManager;

    public Managers(int _id)
    {
        id = _id;
        sLogManager = new LogManager(id);
        sBombManager = new BombManager();
        sBlocksManager = new BlocksManager();
        myPlayerManager = new PlayerManager();
    }

    public LogManager GetLogManager()
    {
        Debug.Assert(sLogManager != null, "sLogManager is null.");

        return sLogManager;
    }

    public BombManager GetBombManager()
    {
        Debug.Assert(sBombManager != null, "sBombManager is null.");

        return sBombManager;
    }

    public BlocksManager GetBlocksManager()
    {
        Debug.Assert(sBlocksManager != null, "sBlocksManager is null.");

        return sBlocksManager;
    }

    public PlayerManager GetPlayerManager()
    {
        Debug.Assert(myPlayerManager != null, "myPlayerManager is null.");

        return myPlayerManager;
    }
}

public class ServiceLocator : Singleton<ServiceLocator>
{
    //bool hasImitation = true;
    static Dictionary<int, Managers> dictManagers = new Dictionary<int, Managers>();
    //static Managers manager1 = null;
    //static Managers manager2 = null;

    protected override void Init()
    {
        DontDestroyOnLoad(this);

        /*if (hasImitation)
        {
            manager1 = new Managers(1);
            manager2 = new Managers(2);
        }
        else
        {
            manager1 = new Managers(1);
        }*/
    }

    public static Managers getManager(int id)
    {
        if (!dictManagers.ContainsKey(id))
        {
            Managers m = new Managers(id);
            dictManagers.Add(id, m);
            //Debug.Log("Managers " + id.ToString() + " foi criado");
        }

        return dictManagers[id];
    }

    void OnApplicationQuit()
    {
        foreach(KeyValuePair<int, Managers> m in dictManagers)
        {
            m.Value.GetLogManager().finish();
        }
    }
}
