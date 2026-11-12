using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public static class ConceptRoomBuilder
{
    static Material Gray(string name, float value, float gloss = 0)
    {
        string path = "Assets/Whitebox/" + name + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!m) { m = new Material(Shader.Find("Standard")); AssetDatabase.CreateAsset(m, path); }
        m.color = new Color(value,value,value); m.SetFloat("_Glossiness",gloss);
        EditorUtility.SetDirty(m); return m;
    }
    static GameObject Shape(string name, PrimitiveType type, Vector3 pos, Vector3 scale, Material mat, Transform parent, bool shadows = false)
    {
        var g = GameObject.CreatePrimitive(type); g.name=name; g.transform.SetParent(parent);
        g.transform.position=pos; g.transform.localScale=scale;
        var r=g.GetComponent<Renderer>(); r.sharedMaterial=mat;
        r.shadowCastingMode=shadows?UnityEngine.Rendering.ShadowCastingMode.On:UnityEngine.Rendering.ShadowCastingMode.Off;
        return g;
    }
    [MenuItem("Tools/Build Concept Room")]
    public static void Build()
    {
        Directory.CreateDirectory("Artifacts");
        var active=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if(active.isDirty) EditorSceneManager.SaveScene(active,"Assets/Scenes/BeforeConceptRoom.unity",true);
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        // Rebuild only the original whitebox, retaining scene-level settings.
        foreach(var root in scene.GetRootGameObjects()) Object.DestroyImmediate(root);
        var room=new GameObject("01_Room_8x18x4.5").transform;
        var puzzle=new GameObject("02_FloatingFrame_And_Geometry").transform;
        var projector=new GameObject("03_FilmProjector").transform;
        var setup=new GameObject("04_Lighting_And_References").transform;
        var wall=Gray("RoomGray",.32f); var floor=Gray("FloorGray",.23f,.32f);
        var dark=Gray("FrameCharcoal",.075f,.25f); var silver=Gray("BrushedSilver",.48f,.35f);
        var ivory=Gray("PuzzleGray",.78f,.12f); var screen=Gray("ScreenWhite",.58f);
        Shape("Floor",PrimitiveType.Cube,new Vector3(0,-.1f,0),new Vector3(8.4f,.2f,18.4f),floor,room);
        Shape("Ceiling",PrimitiveType.Cube,new Vector3(0,4.6f,0),new Vector3(8.4f,.2f,18.4f),wall,room);
        Shape("Wall_Back",PrimitiveType.Cube,new Vector3(0,2.25f,-9.1f),new Vector3(8,4.5f,.2f),wall,room);
        Shape("Wall_Front_ProjectionReceiver",PrimitiveType.Cube,new Vector3(0,2.25f,9.1f),new Vector3(8,4.5f,.2f),screen,room);
        foreach(float x in new[]{-4.1f,4.1f}) Shape("SideWall",PrimitiveType.Cube,new Vector3(x,2.25f,0),new Vector3(.2f,4.5f,18.4f),wall,room);
        foreach(float x in new[]{-3.8f,3.8f}) {
            Shape("WallPilaster",PrimitiveType.Cube,new Vector3(x,2.25f,2),new Vector3(.28f,4.5f,.35f),wall,room);
            Shape("CeilingBeam",PrimitiveType.Cube,new Vector3(x,4.28f,0),new Vector3(.3f,.44f,18),wall,room);
        }
        // The physical frame is in front of the receiving wall, so its shadow also reads in the composition.
        const float z=0f;
        foreach(float x in new[]{-1.44f,1.44f}) Shape("FloatingFrame_Vertical",PrimitiveType.Cube,new Vector3(x,2.3f,z),new Vector3(.08f,1.56f,.08f),silver,puzzle,true);
        foreach(float y in new[]{1.56f,3.04f}) Shape("FloatingFrame_Horizontal",PrimitiveType.Cube,new Vector3(0,y,z),new Vector3(2.8f,.08f,.08f),silver,puzzle,true);
        var cube=Shape("Puzzle_Cube",PrimitiveType.Cube,new Vector3(-.45f,2.5f,z),Vector3.one*.2925f,ivory,puzzle,true);
        cube.transform.rotation=Quaternion.Euler(8,16,5);
        Shape("Puzzle_Cylinder",PrimitiveType.Cylinder,new Vector3(.45f,2.5f,z),new Vector3(.27f,.216f,.27f),ivory,puzzle,true);
        var mesh=new Mesh(); mesh.name="Cone_48Sides";
        const int n=48; var vertices=new Vector3[n*6];var indices=new int[n*6];
        for(int i=0;i<n;i++){
            float a=2*Mathf.PI*i/n,b=2*Mathf.PI*(i+1)/n;
            var p=new Vector3(Mathf.Cos(a)*.43f,-.5f,Mathf.Sin(a)*.43f);
            var q=new Vector3(Mathf.Cos(b)*.43f,-.5f,Mathf.Sin(b)*.43f);
            int k=i*6;vertices[k]=new Vector3(0,.5f,0);vertices[k+1]=q;vertices[k+2]=p;
            vertices[k+3]=new Vector3(0,-.5f,0);vertices[k+4]=p;vertices[k+5]=q;
            for(int j=0;j<6;j++)indices[k+j]=k+j;
        }
        mesh.vertices=vertices;mesh.triangles=indices;mesh.RecalculateNormals();mesh.RecalculateBounds();
        const string meshPath="Assets/Whitebox/Cone.asset";
        var old=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
        if(old){EditorUtility.CopySerialized(mesh,old);Object.DestroyImmediate(mesh);mesh=old;}else AssetDatabase.CreateAsset(mesh,meshPath);
        var cone=new GameObject("Puzzle_Cone");cone.transform.SetParent(puzzle);cone.transform.position=new Vector3(0,2.08f,z); cone.transform.localScale=Vector3.one*.4f;
        cone.AddComponent<MeshFilter>().sharedMesh=mesh;cone.AddComponent<MeshRenderer>().sharedMaterial=ivory;
        cone.AddComponent<MeshCollider>().sharedMesh=mesh;
        // 6 m to the operation plane and 15 m to the wall give 2.5x magnification.
        Vector3 basePos=new Vector3(0,.5f,-6.24f);
        Shape("Pedestal",PrimitiveType.Cube,basePos+new Vector3(0,.4f,-.55f),new Vector3(1.05f,1.8f,.9f),dark,projector);
        Shape("ProjectorBody",PrimitiveType.Cube,basePos+new Vector3(0,1.75f,-.5f),new Vector3(.65f,.65f,.85f),dark,projector);
        Shape("ProjectorFoot",PrimitiveType.Cube,basePos+new Vector3(0,1.36f,-.5f),new Vector3(.8f,.12f,.9f),silver,projector);
        var lens=Shape("LensBarrel",PrimitiveType.Cylinder,basePos+new Vector3(0,1.8f,.04f),new Vector3(.28f,.18f,.28f),dark,projector);
        lens.transform.rotation=Quaternion.Euler(90,0,0);
        for(int reel=0;reel<2;reel++){
            float rz=-.85f+reel*.65f, radius=reel==0?.43f:.3f;
            var center=basePos+new Vector3(0,2.35f,rz);
            for(int j=0;j<32;j++){
                float a=j*360f/32,rad=a*Mathf.Deg2Rad;
                var bar=Shape("Reel_Rim",PrimitiveType.Cube,center+new Vector3(0,Mathf.Cos(rad)*radius,Mathf.Sin(rad)*radius),new Vector3(.09f,.045f,radius*.21f),silver,projector);
                bar.transform.rotation=Quaternion.Euler(a,0,0);
            }
            for(int j=0;j<6;j++){
                var spoke=Shape("Reel_Spoke",PrimitiveType.Cube,center,new Vector3(.075f,radius*1.9f,.055f),silver,projector);
                spoke.transform.rotation=Quaternion.Euler(j*30,0,0);
            }
        }
        for(int j=0;j<7;j++)Shape("Vent",PrimitiveType.Cube,basePos+new Vector3(-.331f,1.75f,-.8f+j*.09f),new Vector3(.012f,.4f,.025f),silver,projector);
        var lightObj=new GameObject("PuzzleLight_Fixed");lightObj.transform.SetParent(setup);lightObj.transform.position=basePos+new Vector3(0,1.8f,.24f);
        lightObj.transform.LookAt(new Vector3(0,2.3f,0));var light=lightObj.AddComponent<Light>();
        light.type=LightType.Spot;light.range=18;light.spotAngle=58;light.innerSpotAngle=54;light.intensity=3.5f;
        lens.transform.position=lightObj.transform.position-lightObj.transform.forward*.26f;
        lens.transform.rotation=lightObj.transform.rotation*Quaternion.Euler(90,0,0);
        light.shadows=LightShadows.Soft;light.shadowBias=.005f;light.shadowNormalBias=.015f;light.shadowCustomResolution=2048;
        var fillObj=new GameObject("SoftRoomFill");fillObj.transform.SetParent(setup);fillObj.transform.position=new Vector3(0,4.25f,-4f);
        fillObj.transform.rotation=Quaternion.Euler(90,0,0);
        var fill=fillObj.AddComponent<Light>();fill.type=LightType.Spot;fill.range=6.2f;fill.spotAngle=110;fill.innerSpotAngle=75;fill.intensity=1.25f;fill.shadows=LightShadows.None;
        Shape("CeilingLamp_Housing",PrimitiveType.Cube,new Vector3(0,4.43f,-4f),new Vector3(1.2f,.14f,.5f),dark,setup);
        Shape("CeilingLamp_Diffuser",PrimitiveType.Cube,new Vector3(0,4.35f,-4f),new Vector3(1.08f,.025f,.38f),screen,setup);
        RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=new Color(.22f,.22f,.22f);RenderSettings.skybox=null;
        foreach(var pair in new[]{new[]{"PlayerSpawn","0","0","-2.5"},new[]{"OperationPlane","0","2.3","0"}}){
            var g=new GameObject(pair[0]);g.transform.SetParent(setup);g.transform.position=new Vector3(float.Parse(pair[1],System.Globalization.CultureInfo.InvariantCulture),float.Parse(pair[2],System.Globalization.CultureInfo.InvariantCulture),float.Parse(pair[3],System.Globalization.CultureInfo.InvariantCulture));}
        var camObj=new GameObject("Main Camera");camObj.tag="MainCamera";camObj.transform.SetParent(setup);
        camObj.transform.position=new Vector3(3f,2.25f,-4.6f);camObj.transform.LookAt(new Vector3(-.8f,2.15f,1.5f));
        var cam=camObj.AddComponent<Camera>();cam.fieldOfView=65;cam.nearClipPlane=.1f;cam.farClipPlane=50;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=Color.black;camObj.AddComponent<AudioListener>();
        QualitySettings.shadows=ShadowQuality.All;QualitySettings.shadowDistance=30;
        var interaction=new GameObject("PuzzleInteraction").AddComponent<PuzzleInteraction>();
        var playerObject=new GameObject("Player"); playerObject.tag="Player"; playerObject.layer=2;
        playerObject.transform.position=new Vector3(0,.05f,-2.5f);
        var controller=playerObject.AddComponent<CharacterController>();
        controller.height=1.8f; controller.radius=.3f; controller.center=new Vector3(0,.9f,0);
        var player=playerObject.AddComponent<FirstPersonPlayer>(); player.view=cam; player.interaction=interaction;
        interaction.player=player; interaction.view=cam;
        cam.transform.SetParent(playerObject.transform,false); cam.transform.localPosition=new Vector3(0,1.65f,0);
        cam.transform.localRotation=Quaternion.identity; cam.fieldOfView=60;
        var rightArm=new GameObject("RightArm"); rightArm.layer=2;
        rightArm.transform.SetParent(cam.transform,false);
        rightArm.transform.localPosition=new Vector3(.28f,-.27f,.48f);
        var forearm=GameObject.CreatePrimitive(PrimitiveType.Cylinder); forearm.name="RightForearm_Cylinder";
        forearm.transform.SetParent(rightArm.transform,false);
        forearm.transform.localRotation=Quaternion.Euler(90,0,0);
        forearm.transform.localScale=new Vector3(.12f,.23f,.12f);
        var hand=GameObject.CreatePrimitive(PrimitiveType.Sphere); hand.name="RightHand_Sphere";
        hand.transform.SetParent(rightArm.transform,false);
        hand.transform.localPosition=new Vector3(0,0,.23f); hand.transform.localScale=Vector3.one*.17f;
        foreach(var part in new[]{forearm,hand})
        {
            part.layer=2; Object.DestroyImmediate(part.GetComponent<Collider>());
            var renderer=part.GetComponent<Renderer>(); renderer.sharedMaterial=ivory;
            renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows=false;
        }
        var rules=interaction.gameObject.AddComponent<PuzzlePlacementRules>();
        rules.operationPlane=GameObject.Find("OperationPlane").transform;
        rules.puzzleLight=light; rules.frameHalfSize=new Vector2(1.4f,.7f);
        var slotList=new System.Collections.Generic.List<PuzzleSlot>();
        foreach(var pieceName in new[]{"Puzzle_Cube","Puzzle_Cylinder","Puzzle_Cone"})
        {
            var pieceObject=GameObject.Find(pieceName);
            var piece=pieceObject.AddComponent<PuzzlePiece>(); piece.pieceType=pieceName;
            var slotObject=new GameObject("Slot_"+pieceName);
            slotObject.transform.SetParent(puzzle,false);
            slotObject.transform.SetPositionAndRotation(piece.transform.position,piece.transform.rotation);
            var slot=slotObject.AddComponent<PuzzleSlot>(); slot.allowedPiece=piece; slotList.Add(slot);
        }
        rules.slots=slotList.ToArray(); interaction.rules=rules;
        var presentation=interaction.gameObject.AddComponent<HeldItemPresentation>();
        presentation.view=cam; presentation.arm=rightArm.transform; interaction.presentation=presentation;
        interaction.gameObject.AddComponent<InteractionHUD>().interaction=interaction;
        player.look=player.GetComponent<PlayerLook>();
        EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        var rt=new RenderTexture(1280,720,24);cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;
        var img=new Texture2D(1280,720,TextureFormat.RGB24,false);img.ReadPixels(new Rect(0,0,1280,720),0,0);img.Apply();
        Directory.CreateDirectory("Artifacts");File.WriteAllBytes("Artifacts/ConceptRoom.png",img.EncodeToPNG());
        cam.targetTexture=null;RenderTexture.active=null;Object.DestroyImmediate(rt);Object.DestroyImmediate(img);
        Debug.Log("CONCEPT_ROOM_BUILD_OK");
    }
}

