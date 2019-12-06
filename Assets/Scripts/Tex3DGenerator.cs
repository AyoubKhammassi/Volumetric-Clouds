using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tex3DGenerator : MonoBehaviour
{
    [Header("Compute Shader used to create the Tex3D")]
    public ComputeShader cmpShader;

    [Header("Sprite Sheet Data")]
    [Tooltip("The used sprite sheet")]
    public Texture2D spriteSheet;
    [Tooltip("Number of Sprites in  X and Y dimensions")]
    public Vector2Int nSprites;
    [Tooltip("The distance in pixels between each sprite in the Z direction")]
    public int depthStep;
    [Tooltip("Set this if you want to impose the number of sprites to be used")]
    public int numberOfSprites = -1;
    private int depth;

    public Shader slicer;
    public Material mat;
    public bool testing = false;
    public RenderTexture finalTex;
    // Start is called before the first frame update
    void Start()
    {
        if(numberOfSprites == -1)
            numberOfSprites = nSprites.x * nSprites.y;

        depth = (numberOfSprites - 1) * depthStep;
    }

    // Update is called once per frame
    void Update()
    {
        //Testing
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CreateTex3D();
            mat = new Material(slicer);
        }
    }

    public RenderTexture CreateTex3D()
    {
        RenderTextureDescriptor desc = new RenderTextureDescriptor();
        desc.colorFormat = RenderTextureFormat.Default;
        desc.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        desc.enableRandomWrite = true;
        //Finding the supported Graphics format
        desc.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R16_UNorm;
        desc.msaaSamples = 4;
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
            cmpShader.SetInt("numberOfSprites", numberOfSprites);

            //Disoatch the compute shader with the specified number of thread groups in each dimension
            Vector3Int threadGroups = new Vector3Int(desc.width / 8, desc.height / 8, depth / 8);
            cmpShader.Dispatch(0, threadGroups.x, threadGroups.y, threadGroups.z);
        }

        return finalTex;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (mat == null)
        {
            mat = new Material(slicer);
        }

        if (testing)
        {

            mat.SetTexture("_VolumeTex", finalTex);
            //mat.SetFloat("_Slice", 0.7f);
            Graphics.Blit(source, destination, mat);
        }
    }
}
