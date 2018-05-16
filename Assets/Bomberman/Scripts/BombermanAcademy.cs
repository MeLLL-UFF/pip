using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombermanAcademy : Academy {

    public override void AcademyReset()
    {
        ServiceLocator.GetLogManager().print("Academia foi resetada");
    }

    public override void AcademyStep()
    {
        base.AcademyStep();
        ServiceLocator.GetLogManager().stepPrint();
    }
}
