using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlocksManager {

    bool initialized = false;

    private List<Destructable> blocks = new List<Destructable>();
    private Dictionary<int, Destructable> blocksMap = new Dictionary<int, Destructable>();
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

        if (block.myID != 0)
        {
            blocksMap.Add(block.myID, block);
        }
    }

    public void loadReplaySetup(Dictionary<int, bool> blockEnableMap)
    {
        foreach (KeyValuePair<int, bool> entry in blockEnableMap)
        {
            if (blocksMap.ContainsKey(entry.Key))
            {
                blocksMap[entry.Key].SetVisible(entry.Value);
            }
        }
    }

    public void resetBlocks()
    {
        for (int i = 0; i < blocks.Count; i++)
        {
            blocks[i].reset();
        }

        clearDestroyList();
    }

    public string generateBlocksStatusList()
    {
        string result = "";
        string suffix = ";";

        for (int i = 0; i < blocks.Count; i++)
        {
            if (i >= blocks.Count - 1)
                suffix = "";

            result += blocks[i].myID + "," + (blocks[i].IsVisible() ? 1 : 0) + suffix;
        }

        return result;
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
