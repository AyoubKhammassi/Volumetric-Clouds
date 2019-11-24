using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[ExecuteAlways]
public class TextureGenerator : MonoBehaviour
{
    /*[Header("Texture Sheet")]
    public Texture2D texSheet;
    public int nRows;
    public int nCols;*/

    [Header("Main Shader")]
    public Shader mainShader;

    [Header("Volume Container")]
    public Transform container; 

    private Material mainMat;
    //public Texture3D finalTexture; //final 3D texture interpolted from the 2D slices


    // Start is called before the first frame update
    void Start()
    {
        mainMat = new Material(mainShader);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if(!mainMat)
        {
            mainMat = new Material(mainShader);
        }
        //container position and localScale to determine its furthest/nearest point in each direction (x,y,z)
        mainMat.SetVector("_ContainerMaxBounds", container.position + (container.localScale/2));
        mainMat.SetVector("_ContainerMinBounds", container.position - (container.localScale/2));
        Graphics.Blit(source, destination, mainMat);
    }
}
