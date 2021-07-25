using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class RenderDepthTexture : MonoBehaviour {

	[SerializeField]
	private Camera camera;
	// Use this for initialization
	void OnEnable() {
		if (camera == null) {
			camera = GetComponent<Camera>();
		}
		camera.depthTextureMode = DepthTextureMode.Depth;
	}
}
