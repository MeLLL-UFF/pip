using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayersManager {

    static bool initialized = false;

    internal PlayersManager()
    {
        if (!initialized)
        {
            initialized = true;
        }
    }
}
