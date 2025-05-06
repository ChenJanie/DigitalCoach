using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag
{
    public class SelfDestroy : MonoBehaviour
    {
        private float curDur = 0;
        private float maxDur = 10;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (maxDur <= curDur) Destroy(this.gameObject);
        }

        public void SetDur(float dur)
        {
            maxDur = dur;
        }
    }
}

