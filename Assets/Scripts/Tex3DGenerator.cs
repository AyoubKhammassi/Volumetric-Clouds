using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Tex3DGenerator
{
    /*
    [Header("Compute Shader used to create the Tex3D")]
    public ComputeShader cmpShader;
    const string cmpShaderName = "SpriteSheetToTex3D.compute";
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
    */
    private ComputeShader cmpShader;
    const string cmpShaderName = "SpriteSheetToTex3D.compute";
    public RenderTexture finalTex;
    private SSToTex3DDesc descriptor;

    //Use this structure to initialize the Tex3DGenerator before creating the 3D texture
    public struct SSToTex3DDesc
    {
        //The actual sprite sheet that will be used
        public Texture2D spriteSheet;
        //The number of sprites in the X and Y dimensions on the spritesheet
        public Vector2Int nSprites;
        //The distance in pixels between each sprite in the Z direction
        public int depthStep;
        //Maximum Number of sprites used. If not filled all available sprites will be used
        public int maxNumberOfSprites;
        //The descriptor for the created Tex3D 
        public RenderTextureDescriptor textureDesc;
        //depth of the tex3D in pixels
        public int depth;
    }


    private void print(string text)
    {
        System.Console.WriteLine(text);
    }

    public Tex3DGenerator(SSToTex3DDesc desc)
    {
        //Load needed resources
        cmpShader = Resources.Load<ComputeShader>("");
        //Initialize descriptor
        SetTex3DDescriptor(desc);
    }

    public void SetTex3DDescriptor(SSToTex3DDesc desc)
    {
        System.Console.WriteLine("Setting the descriptor for the texture3D Generator");
        descriptor = desc;
        finalTex.descriptor = desc.textureDesc;
        if (descriptor.maxNumberOfSprites <= 0)
            descriptor.maxNumberOfSprites = descriptor.nSprites.x * descriptor.nSprites.y;
    }

    public RenderTexture CreateTex3D()
    {
        //Creating the render texture descriptor, this can be overwritten later
        /*RenderTextureDescriptor desc = new RenderTextureDescriptor();
        desc.colorFormat = RenderTextureFormat.Default;
        desc.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        desc.enableRandomWrite = true;
        //Finding the supported Graphics format
        desc.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R16_UNorm;
        desc.msaaSamples = 0;  //NO MSAA
        desc.height = spriteSheet.height / nSprites.y;
        desc.width = spriteSheet.width / nSprites.x;
        desc.volumeDepth = depth;*/

        finalTex = new RenderTexture(descriptor.textureDesc);
        finalTex.name = "test";
        finalTex.Create();

        print("3D Render Texture is Created");

        if (finalTex.IsCreated())
        {
            //setting shader properties
            cmpShader.SetTexture(0, "result", finalTex);
            cmpShader.SetTexture(0, "spriteSheet", descriptor.spriteSheet);

            cmpShader.SetInt("depthStep", descriptor.depthStep);
            cmpShader.SetInt("depth", descriptor.depth);
            cmpShader.SetVector("data", new Vector4(descriptor.nSprites.x, descriptor.nSprites.y, descriptor.spriteSheet.width, descriptor.spriteSheet.height));
            cmpShader.SetInt("numberOfSprites", descriptor.maxNumberOfSprites);

            //Disoatch the compute shader with the specified number of thread groups in each dimension
            Vector3Int threadGroups = new Vector3Int(descriptor.textureDesc.width , descriptor.textureDesc.height, descriptor.depth);
            cmpShader.Dispatch(0, threadGroups.x, threadGroups.y, threadGroups.z);
            print("3D Render Texture is filled");
        }

        return finalTex;
    }


    #region Monobehaviour related Code
    /*

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

    // Start is called before the first frame update

private void Awake()
{
}
     void Start()
     {
         if(numberOfSprites == -1)
             numberOfSprites = nSprites.x * nSprites.y;

         depth = (numberOfSprites - 1) * depthStep;
         print("Total number of Sprites is " + numberOfSprites);
         print("Depth is" + depth);

         CreateTex3D();
         mat = new Material(slicer);
         testing = true;
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
     */
    #endregion
    //This is used for testing 

}

