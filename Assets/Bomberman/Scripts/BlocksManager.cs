using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlocksManager {

    bool initialized = false;

    private List<Destructable> blocks = new List<Destructable>();
    private List<Destructable> blocksWillBeDestroyed = new List<Destructable>();

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
        for (int i = 0; i < blocks.Count; i++)
        {
            blocks[i].reset();
        }

        clearDestroyList();
    }

    public void clear()
    {
        blocks.Clear();
    }

    public void addBlockToDestroy(Destructable block)
    {
        blocksWillBeDestroyed.Add(block);
    }

    public void clearDestroyList()
    {
        blocksWillBeDestroyed.Clear();
    }

    public void checkBlocksAndDestroy()
    {
        for (int i = 0; i < blocksWillBeDestroyed.Count; )
        {
            if (blocksWillBeDestroyed[i].destroyMethod())
            {
                blocksWillBeDestroyed.RemoveAt(i);
            }
            else
            {
                ++i;
            }
        }
    }
}
