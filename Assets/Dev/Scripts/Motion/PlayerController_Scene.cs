using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController
{
    private int scanLineAppearCenter = Shader.PropertyToID("_ScanLineAppearCenter");
    private Vector4 tempVector4 = Vector4.zero;

    [SerializeField]private bool m_CircleScanLine = false;
    private bool CircleScanLine
    {
        get { return m_CircleScanLine; }
        set {
            if (m_CircleScanLine == value)
                return;

            m_CircleScanLine = value;
            if (!m_CircleScanLine)
            {
                Shader.SetGlobalVector(scanLineAppearCenter, Vector4.zero);
            }
        }
    }
    public void Update_CircleScanLine()
    {
        if (!m_CircleScanLine)
        { 
            Shader.SetGlobalVector(scanLineAppearCenter, Vector4.zero);
            return;
        }

        tempVector4.x = transform.position.x;
        tempVector4.y = transform.position.y;
        tempVector4.z = transform.position.z;
        tempVector4.w = 1;
        Shader.SetGlobalVector(scanLineAppearCenter, tempVector4);
    }
}
