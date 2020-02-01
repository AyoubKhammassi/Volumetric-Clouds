using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditorInternal;
using UnityEditor;
[ExecuteInEditMode]
public class EditorModeHandler : MonoBehaviour
{
    public GameObject rendererHolder;
    public Tex3DRenderer editorRenderer;
    private void OnEnable()
    {
        
    }

    public bool AddRendererToEditorCamera()
    {
        //Get The editor cam
        Camera editorCam = EditorWindow.GetWindow<SceneView>().camera;
        //see the number of renderers attached to it
        int nbOfRenderers = editorCam.gameObject.GetComponents<Tex3DRenderer>().Length;
        print(nbOfRenderers);

        //No renderers are attached
        if (nbOfRenderers == 0)
        {
            //add a renderer with default params
            editorRenderer = editorCam.gameObject.AddComponent<Tex3DRenderer>();
        }

        //Get the renderer with the needed params
        Component mainRenderer = rendererHolder.GetComponent<Tex3DRenderer>();
        //copy its values
        if (ComponentUtility.CopyComponent(mainRenderer))
        {
            print("Component copied!");
            if (ComponentUtility.PasteComponentValues(editorRenderer))
            {
                print("Component values pasted!");
                print("Added Tex3DRenderer to editor camera");
            }
            else
                return false;
        }
        else
            return false;


        this.editorRenderer = editorCam.GetComponent<Tex3DRenderer>();
        return true;
    }

    public bool RemoveRendererFromEditorCamera()
    {
        //Get The editor cam
        Camera editorCam = EditorWindow.GetWindow<SceneView>().camera;
        //see the number of renderers attached to it
        int nbOfRenderers = editorCam.gameObject.GetComponents<Tex3DRenderer>().Length;
        print(nbOfRenderers);

        if(nbOfRenderers > 0)
        {
            ComponentUtility.DestroyComponentsMatching(rendererHolder, (Component o) => { return (o.GetType() == typeof(Tex3DRenderer)); });
            print("Removed renderers from editor camera");
        }
        return true;
    }
}
