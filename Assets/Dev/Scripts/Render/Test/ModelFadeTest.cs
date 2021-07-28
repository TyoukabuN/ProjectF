using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


[ExecuteInEditMode]
public class ModelFadeTest : MonoBehaviour
{
	private static int _ModelFadeParam_ID = Shader.PropertyToID("_ModelFadeParam");
	private static int _XTime_ID = Shader.PropertyToID("_XTime");

	public GameObject EffectObject;

	public List<SkinnedMeshRenderer> renderers_up = new List<SkinnedMeshRenderer> ();
	public List<SkinnedMeshRenderer> renderers_down = new List<SkinnedMeshRenderer> ();

	[Range(0,1)]
	public float slide = 0;
	public float time = 2.5f;
	public float delay = 0f;
	public float posY = 0f;
	public float height = 2.5f;
	public int _OffsetFactor = 1;
	public int _OffsetUnit = 100;

	private static Vector4 _XTimeV4 = Vector4.zero;
	private static Vector4 s_temp_v4 = Vector4.zero;


	void Update()
	{
		float t = Time.realtimeSinceStartup;
		_XTimeV4.Set(t / 20, t, t * 2, t * 3);
		Shader.SetGlobalVector(_XTime_ID, _XTimeV4);
	}
}

[CustomEditor(typeof(ModelFadeTest))]
public class ModelFadeTestEditor : Editor
{
	private static Vector4 s_temp_v4 = Vector4.zero;
	private static int _ModelFadeParam_ID = Shader.PropertyToID("_ModelFadeParam");
	private static int _OffsetFactor = Shader.PropertyToID("_OffsetFactor");
	private static int _OffsetUnit = Shader.PropertyToID("_OffsetUnit");
	private static int _SrcBlend = Shader.PropertyToID("_SrcBlend");
	private static int _DstBlend = Shader.PropertyToID("_DstBlend");




	public GameObject modelGobj;
	private float modelHeight = 0;


	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		base.DrawDefaultInspector();

		if (GUILayout.Button("播放"))
		{
			var target = this.target as ModelFadeTest;

			if (target.EffectObject) {
				target.EffectObject.SetActive (false);
				target.EffectObject.SetActive (true);
			}

			foreach (var render in target.renderers_up) {
				//MaterialPropertyBlock mpb = new MaterialPropertyBlock();
				s_temp_v4.x = Time.realtimeSinceStartup ;//+ target.delay;
				s_temp_v4.y = s_temp_v4.x + target.time;
				s_temp_v4.z = target.height;
				s_temp_v4.w = target.posY;
				//mpb.SetVector(_ModelFadeParam_ID, s_temp_v4);

				//render.SetPropertyBlock(mpb);

				foreach (var material in render.materials) {
					material.EnableKeyword("_USE_CHARACTER_EFFECT");
					material.SetVector(_ModelFadeParam_ID, s_temp_v4);
					material.SetFloat (_OffsetFactor, target._OffsetFactor);
					material.SetFloat (_OffsetUnit, -target._OffsetUnit);
					material.SetInt (_SrcBlend,(int)UnityEngine.Rendering.BlendMode.SrcAlpha);
					material.SetInt (_DstBlend,(int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
					material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent+2;
				}
			}

			foreach (var render in target.renderers_down) {
				//MaterialPropertyBlock mpb = new MaterialPropertyBlock();
				s_temp_v4.x = Time.realtimeSinceStartup + target.delay;
				s_temp_v4.y = s_temp_v4.x + target.time;
				s_temp_v4.z = -1 *target.height;
				s_temp_v4.w = target.posY;
				//mpb.SetVector(_ModelFadeParam_ID, s_temp_v4);


				//render.SetPropertyBlock(mpb);
				foreach (var material in render.materials) {
					material.EnableKeyword("_USE_CHARACTER_EFFECT");
					material.SetVector(_ModelFadeParam_ID, s_temp_v4);
					material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
					material.SetFloat (_OffsetFactor, target._OffsetFactor);
					material.SetFloat (_OffsetUnit, target._OffsetUnit);
					material.SetInt (_SrcBlend, (int)UnityEngine.Rendering.BlendMode.One);
					material.SetInt (_DstBlend, (int)UnityEngine.Rendering.BlendMode.Zero);
				}
			}
		}
	}
}


