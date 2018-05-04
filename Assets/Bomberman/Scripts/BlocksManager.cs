using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlocksManager {

    static bool initialized = false;

    private List<Destructable> blocks = new List<Destructable>();

    internal BlocksManager()
    {
        if (!initialized)
        {
            initialized = true;
        }
    }

    public void addBlock(Destructable block)
    {
        blocks.Add(block);
    }

    public void resetBlocks()
    {
        for(int i = 0; i < blocks.Count; i++)
        {
            blocks[i].reset();
        }
    }

    public void clear()
    {
        blocks.Clear();
    }
}
