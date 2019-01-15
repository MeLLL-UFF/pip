using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MLAgents
{

    /// <summary>
    /// Behavioral Cloning Helper script. Attach to teacher agent to enable 
    /// resetting the experience buffer, as well as toggling session recording.
    /// </summary>
    public class BCTeacherHelper : MonoBehaviour
    {

        bool recordExperiences;
        bool resetBuffer;
        Agent myAgent;
        float bufferResetTime;
        public Transform myTransform;

        //public KeyCode recordKey = KeyCode.R;
        //public KeyCode resetKey = KeyCode.C;

        private void Awake()
        {
            myTransform = null;
            recordExperiences = true;
            resetBuffer = false;
        }

        // Use this for initialization
        void Start()
        {
            
            myAgent = GetComponent<Agent>();
            bufferResetTime = Time.time;
        }

        public void setMyMonitorTransform(Transform _transform)
        {
            myTransform = _transform;
        }

        public void forceStopRecord()
        {
            recordExperiences = false;
        }

        // Update is called once per frame
        void Update()
        {
            /*if (Input.GetKeyDown(recordKey))
            {
                recordExperiences = !recordExperiences;
            }

            if (Input.GetKeyDown(resetKey))
            {
                resetBuffer = true;
                bufferResetTime = Time.time;
            }
            else
            {
                resetBuffer = false;
            }*/

            Monitor.Log("Recording experiences " /*+ recordKey*/, recordExperiences.ToString(), myTransform);
            float timeSinceBufferReset = Time.time - bufferResetTime;

            Monitor.Log("Seconds since buffer reset " /*+ resetKey*/, 
                Mathf.FloorToInt(timeSinceBufferReset).ToString(),
                myTransform);
        }

        void FixedUpdate()
        {
            // Convert both bools into single comma separated string. Python makes
            // assumption that this structure is preserved. 
            myAgent.SetTextObs(recordExperiences + "," + resetBuffer);
        }
    }
}
