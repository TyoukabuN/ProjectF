using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TMesh
{
    [TSaveDuringPlay]
    [HelpURL("https://docs.qq.com/doc/DY0JqTVFyWGRFSGdi")]
    public class TCameraVertex : TVertex
    {
        public Vector3 EularAngle = Vector3.zero;
        public Vector3 PivotPosition = Vector3.zero;
    }
}
