using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GLTFast;
using System;

public class GLTFObject : MonoBehaviour
{
    // Start is called before the first frame update
    async void Start()
    {
      //  var gltf = gameObject.AddComponent<GLTFast.GltfAsset>();
      //  gltf.Url = "https://digitalworlds.github.io/CURE25_Test/models/test-model/scene.gltf";
    
        var gltfImport = new GltfImport();
    await gltfImport.Load("https://digitalworlds.github.io/CURE25_Test/models/Callithrix/Callithrix.glb");
    var instantiator = new GameObjectInstantiator(gltfImport,transform);
    var success=await gltfImport.InstantiateMainSceneAsync(instantiator);
    if (success) {
        Debug.Log("GLTF file is loaded.");
        
        // Assuming "gameObject" is your desired GameObject

       
        Renderer renderer = gameObject.GetComponentInChildren<Renderer>();

        Bounds boundingBox = renderer.bounds; 

        // Accessing specific values from the bounding box

        Vector3 center = boundingBox.center;

        Vector3 extents = boundingBox.extents;
        Debug.Log(extents);

        float size=Math.Max(Math.Max(extents.x,extents.y), extents.z);
        if(size==0)size=1;

        transform.localScale=new Vector3(1/size,1/size,1/size);

        // Get the SceneInstance to access the instance's properties
        //var sceneInstance = instantiator.SceneInstance;

        /*
        // Enable the first imported camera (which are disabled by default)
        if (sceneInstance.Cameras is { Count: > 0 }) {
            sceneInstance.Cameras[0].enabled = true;
        }

        // Decrease lights' ranges
        if (sceneInstance.Lights != null) {
            foreach (var glTFLight in sceneInstance.Lights) {
                glTFLight.range *= 0.1f;
            }
        }

        // Play the default (i.e. the first) animation clip
        var legacyAnimation = instantiator.SceneInstance.LegacyAnimation;
        if (legacyAnimation != null) {
            legacyAnimation.Play();
        }*/
    }else{
        Debug.Log("ERROR: GLTF file is NOT loaded!");
    }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
