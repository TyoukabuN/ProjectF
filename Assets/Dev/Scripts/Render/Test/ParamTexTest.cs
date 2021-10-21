using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TPPM;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ParamTexTest : MonoBehaviour
{
	public PixelParamMap PixelParamMap;

	public Color color = Color.red;

	public int rowLength = 2;
	public int columnLength = 2;

    private void OnEnable()
    {
		PixelParamMap = new PixelParamMap(rowLength, columnLength);
		PixelParamMap.SetParam(0, color);
		PixelParamMap.Apply();

	}
}

#if UNITY_EDITOR

[CustomEditor(typeof(ParamTexTest))]
public class ParamTexTestEditor : Editor
{
	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		base.DrawDefaultInspector();

		var target = this.target as ParamTexTest;
		if (target == null)
			return;

		if (GUILayout.Button("设置大小"))
		{
			target.PixelParamMap.SetSize(target.rowLength, target.columnLength);
		}

		if (GUILayout.Button("设置颜色"))
		{
			target.PixelParamMap.SetParam(0, target.color);
			target.PixelParamMap.Apply();
		}		
		if (GUILayout.Button("全部填充"))
		{
            for (int y = 0; y < target.columnLength; y++)
            {
                target.PixelParamMap.SetParam(y, Color.red, Color.red);
            }
            target.PixelParamMap.Apply();
		}


		if (GUILayout.Button("自定义"))
		{
			target.PixelParamMap.SetParam(1, Color.blue, Color.yellow);
			target.PixelParamMap.SetParam(0, Color.red, Color.green);

			target.PixelParamMap.Apply();
		}

		if (GUILayout.Button("输出图片"))
		{
			target.PixelParamMap.OutputToPNG();
		}
	}
}
#endif
