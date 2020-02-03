using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Use this structure to initialize the Tex3DGenerator before creating the 3D texture
[System.Serializable]
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

public class Tex3DGenerator
{
    private ComputeShader cmpShader;
    const string cmpShaderName = "SpriteSheetToTex3D";
    public RenderTexture finalTex;
    private SSToTex3DDesc descriptor;

    private void print(object text)
    {
        MonoBehaviour.print(text);
    }

    public Tex3DGenerator(SSToTex3DDesc desc)
    {
        //Load needed resources
        cmpShader = Resources.Load<ComputeShader>(cmpShaderName);
        //Initialize descriptor
        SetTex3DDescriptor(desc);
    }

    public void SetTex3DDescriptor(SSToTex3DDesc desc)
    {
        print("Setting the descriptor for the texture3D Generator");
        descriptor = desc;


    }

    public RenderTexture CreateTex3D()
    {
        finalTex = new RenderTexture(descriptor.textureDesc);
        finalTex.name = "test";
        finalTex.descriptor = descriptor.textureDesc;
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
            print(descriptor.maxNumberOfSprites);
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

