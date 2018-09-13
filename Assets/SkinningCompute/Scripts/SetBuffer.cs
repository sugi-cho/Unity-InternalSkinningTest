using UnityEngine;

public class SetBuffer : MonoBehaviour {

    public string propName = "_VertIn";

    public void Set(ComputeBuffer buffer)
    {
        var r = GetComponent<Renderer>();
        var mpb = new MaterialPropertyBlock();
        r.GetPropertyBlock(mpb);
        mpb.SetBuffer(propName, buffer);
        r.SetPropertyBlock(mpb);
    }

    public void SetSkinAndBones(ComputeBuffer skin, ComputeBuffer bones)
    {
        var r = GetComponent<Renderer>();
        var mpb = new MaterialPropertyBlock();
        r.GetPropertyBlock(mpb);
        mpb.SetBuffer("_Skin", skin);
        mpb.SetBuffer("_Bones", bones);
        r.SetPropertyBlock(mpb);
    }
}
