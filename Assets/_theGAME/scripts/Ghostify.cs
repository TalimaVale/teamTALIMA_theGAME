using UnityEngine;

[ExecuteInEditMode]
public class Ghostify : MonoBehaviour {
    Material material;
    int prevZWrite;
    int prevRenderQueue;

    [SerializeField]
    int renderQueue = 3000;

    void OnEnable() {
        if(GetComponent<MeshRenderer>() != null) material = Application.isPlaying ? GetComponent<MeshRenderer>().material : GetComponent<MeshRenderer>().sharedMaterial;
        else if(GetComponent<SkinnedMeshRenderer>() != null) material = Application.isPlaying ? GetComponent<SkinnedMeshRenderer>().material : GetComponent<SkinnedMeshRenderer>().sharedMaterial;
        else material = null;

        if(material != null) {
            prevZWrite = material.GetInt("_ZWrite");
            prevRenderQueue = material.renderQueue;

            material.SetInt("_ZWrite", 1);
            material.renderQueue = renderQueue;
            // ^ These two are the only lines you need to get the standard shader to 
            //   write to the z-buffer regardless of the material being Transparent or Fade.
            // - Setting the renderQueue is more or less to help you stop z-buffer problems 
            //   with multiple ghost objects. 3000 for the Player, 3001 for the Visor, etc...
        }
    }

    void OnValidate() {
        if(enabled && material != null) {
            material.SetInt("_ZWrite", 1);
            material.renderQueue = renderQueue;
        } else {
            OnDisable();
        }
    }

    void OnDisable() {
        if(material != null) {
            material.SetInt("_ZWrite", prevZWrite);
            material.renderQueue = prevRenderQueue;
            material = null;
        }
    }
}