﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tex3DRenderer : MonoBehaviour
{
    [Header("The descriptor structure for Tex3D generation")]
    public SSToTex3DDesc ss2texDesc;
    private RenderTexture tex3D;

    [Header("The transform for the Volume Container")]
    public Transform container;

    [Header("Density Sampling Params")]
    [Range(0.0001f,0.1f)]
    public float step = 0.1f;

    [Header("Volume Properties")]
    public Vector3 volumeOffset = new Vector3(0, 0, 0);
    public float volumeScale = 1.0f;
    [Range(0.0f, 100.0f)]
    public float density;
    public Color volumeColor;
    public float minDensity;

    [Header("Testing configuration")]
    public Shader slicer;
    public Material slicingMat;
    public bool testing = false;

    private Tex3DGenerator tex3DGenerator;

    private Shader renderingShader;
    private Material renderingMat;

    // Start is called before the first frame update
    void Start()
    {
        //Creating the render texture descriptor, this can be overwritten later
        RenderTextureDescriptor desc = new RenderTextureDescriptor();
        desc.colorFormat = RenderTextureFormat.Default;
        desc.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        desc.enableRandomWrite = true;
        //Finding the supported Graphics format
        desc.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R16_UNorm;
        desc.msaaSamples = 1;  //NO MSAA
        desc.height = ss2texDesc.spriteSheet.height / ss2texDesc.nSprites.y;
        desc.width = ss2texDesc.spriteSheet.width / ss2texDesc.nSprites.x;
        //max number of sprites
        if (ss2texDesc.maxNumberOfSprites <= 0)
            ss2texDesc.maxNumberOfSprites = ss2texDesc.nSprites.x * ss2texDesc.nSprites.y;
        //Calculating Depth
        ss2texDesc.depth = ss2texDesc.maxNumberOfSprites * ss2texDesc.depthStep;
        desc.volumeDepth = ss2texDesc.depth;
        ss2texDesc.textureDesc = desc;

        //Creating the 3D texture
        tex3DGenerator = new Tex3DGenerator(ss2texDesc);
        tex3D = tex3DGenerator.CreateTex3D();
        tex3D.wrapMode = TextureWrapMode.Repeat;
        tex3D.filterMode = FilterMode.Trilinear;

        //Loading the rendering shader and creating the rendering material
        renderingShader = Shader.Find("Custom/VolumetricRenderer");
        renderingMat = new Material(renderingShader);

        //Setting the camera to render depth
        Camera.main.depthTextureMode = DepthTextureMode.Depth;
    }


    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //When testing just slice  the 3D texture and display it
        if (testing)
        {
            if (slicingMat == null)
                slicingMat = new Material(slicer);

            slicingMat.SetTexture("_VolumeTex", tex3D);
            //slicingMat.SetFloat("_Slice", 0.7f);
            Graphics.Blit(source, destination, slicingMat);
        }
        else
        {
            if (renderingMat == null)
                renderingMat = new Material(renderingShader);

            //container position and localScale to determine its furthest/nearest point in each direction (x,y,z)
            renderingMat.SetVector("_ContainerMaxBounds", container.position + container.localScale / 2);
            renderingMat.SetVector("_ContainerMinBounds", container.position - container.localScale / 2);
            renderingMat.SetTexture("SSVolume", tex3D);
            renderingMat.SetMatrix("_ContainerMatrix", container.worldToLocalMatrix);
            renderingMat.SetFloat("_Step", step);
            renderingMat.SetFloat("_VolumeScale", volumeScale);
            renderingMat.SetVector("_VolumeOffset", volumeOffset);
            renderingMat.SetColor("_VolumeColor", volumeColor);
            renderingMat.SetFloat("_Density", density);
            renderingMat.SetFloat("_MinDensity", minDensity);

            Graphics.Blit(source, destination, renderingMat);
        }
    }

}
