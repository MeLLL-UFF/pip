using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollerAgentDiscrete : Agent {

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

    List<float> observation = new List<float>();
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

    //0-cima 1-baixo 2-direita 3-esquerda
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        int action = (int)vectorAction[0];

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

        if (action != -1)
        {
            //Actions, size = 2
            Vector3 controlSignal = Vector3.zero;


            if (action == 0 || action == 1)
            {
                action = action == 0 ? 1 : -1;
                controlSignal.z = Mathf.Clamp(action, -1, 1);
            }
            else if (action == 2 || action == 3)
            {
                action = action == 2 ? 1 : -1;
                controlSignal.x = Mathf.Clamp(action, -1, 1);
            }

            rBody.AddForce(controlSignal * speed);
        }

        
    }
}
