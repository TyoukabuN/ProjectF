using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TMesh
{ 
    public class TVertex : MonoBehaviour
    {
#if !TYOU_LAB
        [HideInInspector]
#endif
        public TTrangle trangle;

        public float x
        {
            get
            {
                return transform.position.x;
            }
        }

        public float y
        {
            get
            {
                return transform.position.y;
            }
        }

        public float z
        {
            get
            {
                return transform.position.z;
            }
        }

        public float this[int index]
        {
            get
            {
                return transform.position[index];
            }
        }
    }
}
