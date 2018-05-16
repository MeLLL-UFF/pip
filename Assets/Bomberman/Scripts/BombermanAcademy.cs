using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombermanAcademy : Academy {

    public override void AcademyReset()
    {
        ServiceLocator.GetLogManager().episodePrint(GetEpisodeCount());
    }

    public override void AcademyStep()
    {
        ServiceLocator.GetLogManager().globalStepPrint(GetStepCount());
    }
}
