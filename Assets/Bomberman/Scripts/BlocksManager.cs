using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlocksManager {

    bool initialized = false;
    // primeiro reset é chamado uma vez na inicialização do agente. Logo isso é um hack para corrigir isso.
    bool firstReset;

    private List<Destructable> blocks = new List<Destructable>();

    internal BlocksManager()
    {
        if (!initialized)
        {
            initialized = true;
            firstReset = false;
        }
    }

    public void addBlock(Destructable block)
    {
        blocks.Add(block);
    }

    public void resetBlocks()
    {
        if (firstReset)
        {
            for (int i = 0; i < blocks.Count; i++)
            {
                blocks[i].reset();
            }
        }

        firstReset = true;
    }

    public void clear()
    {
        blocks.Clear();
    }
}
