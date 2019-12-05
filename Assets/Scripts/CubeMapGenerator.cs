using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class CubeMapGenerator : MonoBehaviour
{
    [Header("The used compute shader")]
    public ComputeShader cmpShader;
    public Texture2D spriteSheet;

    [Header("Number of sprites in each direction")]
    public Vector2Int nSprites;

    public int depthStep;
    private int numberOfSprites;
    private int depth;

    public Shader slicer;
    public Material mat;
    public bool testing = false;
    public RenderTexture finalTex;
    // Start is called before the first frame update
    void Start()
    {
        numberOfSprites = nSprites.x * nSprites.y;
        depth = (numberOfSprites-1) * depthStep;
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
        desc.height = spriteSheet.height / nSprites.y;
        desc.width = spriteSheet.width / nSprites.x;
        desc.volumeDepth = depth;

        finalTex = new RenderTexture(desc);
        finalTex.name = "test";
        finalTex.Create();

        if (finalTex.IsCreated())
        {
            //setting shader properties
            cmpShader.SetTexture(0, "result", finalTex);
            cmpShader.SetTexture(0, "spriteSheet", spriteSheet);

            cmpShader.SetFloat("depthStep", depthStep);
            cmpShader.SetFloat("depth", depth);
            cmpShader.SetVector("data", new Vector4(nSprites.x, nSprites.y, spriteSheet.width, spriteSheet.height));
            cmpShader.Dispatch(0, desc.width, desc.height, depth);
            //cmpShader.Dispatch(0, Camera.main.pixelWidth, Camera.main.pixelHeight, 512);

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
