using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tex3DRenderer : MonoBehaviour
{
    [Header("The descriptor structure for Tex3D generation")]
    public SSToTex3DDesc ss2texDesc;

    public Shader slicer;
    public Material mat;
    public bool testing = false;

    private Tex3DGenerator tex3DGenerator;
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
        desc.msaaSamples = 0;  //NO MSAA
        desc.height = ss2texDesc.spriteSheet.height / ss2texDesc.nSprites.y;
        desc.width = ss2texDesc.spriteSheet.width / ss2texDesc.nSprites.x;
        //Calculating Depth
        ss2texDesc.depth = ss2texDesc.
        desc.volumeDepth = ss2texDesc.depth;


        tex3DGenerator = new Tex3DGenerator(ss2texDesc);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
