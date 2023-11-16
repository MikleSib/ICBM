using UnityEngine;
using System;
using System.Collections;

public class CloudLayerAnimator : MonoBehaviour {

	[NonSerialized]
	public float speed;

	[NonSerialized]
	public Vector2 cloudMainTextureOffset;

	[NonSerialized]
	public Material earthMat;

	[NonSerialized]
	Material cloudMat;

	Vector2 tdisp;

	void Awake () {
		cloudMat = GetComponent<Renderer>().sharedMaterial;
	}
	
	public void Update () {
		if (cloudMat==null) return;
		tdisp.x += Time.deltaTime * speed * 0.001f;
		Vector2 offset = cloudMainTextureOffset + tdisp;
		cloudMat.mainTextureOffset = offset;
        cloudMat.SetVector("_TextureOffset", offset);
		if (earthMat!=null) earthMat.SetVector("_CloudMapOffset", tdisp);
	}
}
