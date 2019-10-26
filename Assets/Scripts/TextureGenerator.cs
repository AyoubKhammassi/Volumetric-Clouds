using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[ExecuteAlways]
public class TextureGenerator : MonoBehaviour
{
    [Header("Texture Sheet")]
    public Texture2D texSheet;
    public int nRows;
    public int nCols;

    public Texture3D finalTexture; //final 3D texture interpolted from the 2D slices


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
