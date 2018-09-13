using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

public class Skinning : MonoBehaviour
{

    public ComputeShader cs;
    public ComputeBufferEvent onCreateOutBuffer;
    public DoubleComputeBufferEvent setSkinAndBone;
    [SerializeField] Transform[] bones;
    struct SVertInVBO
    {
        public Vector3 pos;
        public Vector3 norm;
        public Vector4 tang;
    }

    struct SVertInSkin
    {
        public float weight0, weight1, weight2, weight3;
        public int index0, index1, index2, index3;
    }

    struct SVertOut
    {
        Vector3 pos;
        Vector3 norm;
        Vector4 tang;
    }

    Mesh mesh;
    int vertCount;
    ComputeBuffer sourceVBO;
    ComputeBuffer sourceSkin;
    ComputeBuffer meshVertsOut;
    ComputeBuffer mBones;

    Matrix4x4[] boneMatrices;

    // Use this for initialization
    void Start()
    {
        Initialize();
    }

    private void OnDestroy()
    {
        new[] { sourceVBO, sourceSkin, meshVertsOut, mBones }.ToList().ForEach(b => b.Dispose());
    }

    // Update is called once per frame
    void Update()
    {
        SetBoneMatrices();
        ComputeSkinning();
    }

    void SetBoneMatrices()
    {
        for (var i = 0; i < boneMatrices.Length; i++)
            boneMatrices[i] = transform.worldToLocalMatrix * bones[i].localToWorldMatrix * mesh.bindposes[i];
        mBones.SetData(boneMatrices);
    }

    void ComputeSkinning()
    {
        var kernel = cs.FindKernel("main");
        cs.SetBuffer(kernel, "g_SourceVBO", sourceVBO);
        cs.SetBuffer(kernel, "g_SourceSkin", sourceSkin);
        cs.SetBuffer(kernel, "g_MeshVertsOut", meshVertsOut);
        cs.SetBuffer(kernel, "g_mBones", mBones);
        cs.SetInt("g_VertCount", vertCount);
        cs.Dispatch(kernel, vertCount / 64 + 1, 1, 1);
    }

    void Initialize()
    {
        var skin = GetComponentInChildren<SkinnedMeshRenderer>();
        mesh = skin.sharedMesh;
        vertCount = mesh.vertexCount;

        var inVBO = Enumerable.Range(0, vertCount).Select(
            idx => new SVertInVBO()
            {
                pos = mesh.vertices[idx],
                norm = mesh.normals[idx],
                tang = mesh.tangents[idx],
            }).ToArray();
        sourceVBO = new ComputeBuffer(vertCount, Marshal.SizeOf(typeof(SVertInVBO)));
        sourceVBO.SetData(inVBO);

        var inSkin = mesh.boneWeights.Select(
            w => new SVertInSkin()
            {
                weight0 = w.weight0,
                weight1 = w.weight1,
                weight2 = w.weight2,
                weight3 = w.weight3,
                index0 = w.boneIndex0,
                index1 = w.boneIndex1,
                index2 = w.boneIndex2,
                index3 = w.boneIndex3,
            }).ToArray();
        sourceSkin = new ComputeBuffer(vertCount, Marshal.SizeOf(typeof(SVertInSkin)));
        sourceSkin.SetData(inSkin);

        meshVertsOut = new ComputeBuffer(vertCount, Marshal.SizeOf(typeof(SVertOut)));
        onCreateOutBuffer.Invoke(meshVertsOut);

        bones = skin.bones;
        mBones = new ComputeBuffer(bones.Length, Marshal.SizeOf(typeof(Matrix4x4)));
        boneMatrices = bones.Select((b, idx) => transform.worldToLocalMatrix * b.localToWorldMatrix * mesh.bindposes[idx]).ToArray();

        setSkinAndBone.Invoke(sourceSkin, mBones);
    }

    [System.Serializable]
    public class ComputeBufferEvent : UnityEvent<ComputeBuffer> { }
    [System.Serializable]
    public class DoubleComputeBufferEvent : UnityEvent<ComputeBuffer, ComputeBuffer> { }
}
