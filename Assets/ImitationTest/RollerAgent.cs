using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class RollerAgent : Agent {

    Rigidbody rBody;
	// Use this for initialization
	void Start () {
        rBody = GetComponent<Rigidbody>();
	}

    public Transform Target;
    public override void AgentReset()
    {
        if (this.transform.position.y < -1.0)
        {
            //o agente caiu
            this.transform.position = Vector3.zero;
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;
        }
        else
        {
            //move o Target para uma nova posição
            Target.position = new Vector3(Random.value * 8 - 4,
                                          0.5f,
                                          Random.value * 8 - 4);
        }
    }

    public override void CollectObservations()
    {
        Vector3 relativePosition = Target.position - this.transform.position;

        //posição relativa
        AddVectorObs(relativePosition.x / 5);
        AddVectorObs(relativePosition.z / 5);

        //distancia para cada borda da plataforma
        AddVectorObs((this.transform.position.x + 5) / 5);
        AddVectorObs((this.transform.position.x - 5) / 5);
        AddVectorObs((this.transform.position.z + 5) / 5);
        AddVectorObs((this.transform.position.z - 5) / 5);

        //velocidade do agente
        AddVectorObs(rBody.velocity.x / 5);
        AddVectorObs(rBody.velocity.z / 5);

        //All the values are divided by 5 to normalize the inputs to the neural network to the range [-1,1]. (The number five is used because the platform is 10 units across.)

    }

    public float speed = 10;
    private float previousDistance = float.MaxValue;

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        //recompensas
        float distanceToTarget = Vector3.Distance(this.transform.position, Target.position);

        //alcançou o objetivo
        if (distanceToTarget < 1.42f)
        {
            Done();
            AddReward(1.0f);
        }

        //se aproximando
        if (distanceToTarget < previousDistance)
        {
            AddReward(0.5f);
        }

        //penalidade de tempo
        AddReward(-0.05f);

        //caiu da plataforma
        if (this.transform.position.y < -1.0)
        {
            Done();
            AddReward(-1.0f);
        }
        previousDistance = distanceToTarget;

        //Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = Mathf.Clamp(vectorAction[0], -1, 1);
        controlSignal.z = Mathf.Clamp(vectorAction[1], -1, 1);
        rBody.AddForce(controlSignal * speed);
    }
}
