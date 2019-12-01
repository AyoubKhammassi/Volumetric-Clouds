using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class CubeMapGenerator : MonoBehaviour
{
    [Header("The used compute shader")]
    public ComputeShader cmpShader;
    public Shader slicer;
    public Material mat;
    public bool testing = false;
    public RenderTexture finalTex;
    // Start is called before the first frame update
    void Start()
    {
     
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            CreateCubeMap();
            mat = new Material(slicer);
        }
    }

    public void CreateCubeMap()
    {
        RenderTextureDescriptor desc = new RenderTextureDescriptor();
        desc.colorFormat = RenderTextureFormat.ARGB32;
        desc.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        desc.enableRandomWrite = true;
        desc.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.B8G8R8A8_UNorm;
        desc.msaaSamples = 1;
        desc.height = Camera.main.pixelHeight;
        desc.width = Camera.main.pixelWidth;
        desc.volumeDepth = 512;

        finalTex = new RenderTexture(desc);
        finalTex.name = "test";
        finalTex.Create();

        if (finalTex.IsCreated())
        {
            cmpShader.SetTexture(0, "Result", finalTex);
            cmpShader.Dispatch(0, desc.width,desc.height,512);
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (mat == null)
        {
            mat = new Material(slicer);
        }

        if(testing)
        {
            
            mat.SetTexture("_VolumeTex", finalTex);
            //mat.SetFloat("_Slice", 0.7f);
            Graphics.Blit(source, destination, mat);
        }
    }
}
