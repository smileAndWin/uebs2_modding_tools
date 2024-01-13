using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class CharacterModSetup : MonoBehaviour
{
    public string subFolderExport;

    public MeshProps[] AllMeshes;
    public CollisionSettings collisionSettings;
    [Tooltip("Used for animation. The root bone/transform is usually the first child bone of the character skeleton")]
    public Transform RootBone;
    [Tooltip("Used for animation. The head bone is usually far down the hierarchy children of bones often the child of the neck bone.")]
    public Transform HeadBone;
   // public bool IsHumanoid = true;
    public AnimSets humanoidAnimationSet = AnimSets.SwordOneHanded;
    public CustomAnimationSettings customAnimationSettings;
    public SoundSets SoundVoicePresets = SoundSets.None;
    public ArmyStats DefaultArmyStats;
    [HideInInspector]
    public int LastArraycount;
    [Tooltip("This is a tool we provided that can help optimization. If a mesh has multple mesh groups, it helps the LOD system if they are seperated into individual meshes.")]
    public bool SeperateSubMeshes;
    [Tooltip("The sub folder the new generated meshes from submeshes will save to in your Unity project.")]
    public string SaveMeshFolder;

    [Tooltip("Highly recommened for human characters! An ultra low poly human will be placed in as the last lod which inherits the textures of your character, which gives huge performance gains.")]
    public bool UseUltraLowPolyHumanLod = true;

    [System.Serializable]
    public class CollisionSettings
    {
        public float CollisionWidth = 0.5f;
        public float CollisionHeight = 1.9f;
    }

    [System.Serializable]
    public class CustomAnimationSettings
    {
        [Tooltip("Standing idle(required)")]
        public AnimationClip IdleNormal;
        [Tooltip("Standing animation when in combat(optional if normal idle is used in place)")]
        public AnimationClip IdleCombat;
        [Tooltip("Alternate idle animations that random play(optional)")]
        public AnimationClip[] IdleOther;
        [Tooltip("Speed multiplayer on the play speed of animation")]
        public float IdleAnimSpeedMultiplier = 1;
        [Tooltip("Walk animation(required)")]
        public AnimationClip Walk;
        [Tooltip("Speed multiplayer on the play speed of animation")]
        public float WalkAnimSpeedMultiplier = 1;
        [Tooltip("Jog animation(required, but you can use run in place if no jog)")]
        public AnimationClip Jog;
        [Tooltip("Speed multiplayer on the play speed of animation")]
        public float JogAnimSpeedMultiplier = 1;
        [Tooltip("Run animation(required)")]
        public AnimationClip Run;
        [Tooltip("Speed multiplayer on the play speed of animation")]
        public float RunAnimSpeedMultiplier = 1;
        [Tooltip("(Required)Attack animations for standing while attacking")]
        public AnimationClip[] Attacks;
        [Tooltip("(Optional)Attack animations for attacking while running, for melee units first approaching a target")]
        public AnimationClip[] AttackRunCharge;
        public AnimationClip[] Deaths;
        [Tooltip("Speed multiplayer on the play speed of animation")]
        public float DeathAnimSpeedMultiplier = 1;

        [Tooltip("(optional)Block animation.  Animation shouldn't deviate too much from combat idle stance.")]
        public AnimationClip Block;
        [Tooltip("(optional)Strafing right animation.")]
        public AnimationClip StrafeRight;
        [Tooltip("(optional)Strafing left animation.")]
        public AnimationClip StrafeLeft;


    }

    [System.Serializable]
    public class ArmyStats
    {
        public int Health = 250;
        public int Damage = 40;
        public float AttackRange=2;
        public RangedStats RangedProjectile;
       
        public float AttackTimeSeconds = 1.2f;
        public float AttackBreakSeconds = 0.5f;
        public float ImpactForce=0;
        public int SplashDamage = 0;
        public int SplashDamageRadius = 0;

        public float MeleeArmor;
        public float RangeArmor;
        public float MeleeBlockChance = 0;
        public float RangeBlockChance = 0;
        public float MaxRangedBlockForce = 0;
        public float MaxRangedArmorForce;
    }

    [System.Serializable]
    public class RangedStats
    {
        public Projectile FireProjectile;
        public float ProjectileSpeed = 50;
        public int BurstFireAmount = 1;
        public float BurstRate = 0.1f;
        public int AccuracyPercent = 84;
    }



        public enum Priority
    {
        VeryLow,
        Low,
        Med,
        High
    }

    public enum Projectile
    {
        None,
        Arrow,
        BulletNormal,
        Rock,
        BulletSmall,
        MusketBall,
        TankArtillary,
        LightningBolt,
        SuperBullet,
        FireBall
    }

    public enum AnimSets
    {
        Custom,
        SwordOneHanded,
        SwordTwoHanded,
        SpearShield,
        Archer,
        Troll,
        AssaultRifle,
        DualPistol,
        Karate,
        Zombie,
        OneHandPistol

    }

    public enum SoundSets
    {
        None,
        Custom,
        Spartan,
        Englishman,
        Duck,
        AmericanSoldier,
        Zombie,
        Catapult,
        MantisShrimp,
        Troll,
        Chicken,
        GermanSoldier,
        TRex,
        WereWolf,
        Ghost,
        JuiceMan
    }



    [ContextMenu("Export Character")]
    void ExportCharacter()
    {
        Debug.Log("Exporting..");


        string error = CheckList();

        if (error != "")
        {
            Debug.LogError("Error during export: " + error);
        }
        else
        {

#if UNITY_EDITOR

            // Check if the current game object is a prefab

            var prefabStatus = PrefabUtility.GetPrefabInstanceStatus(gameObject);

            if (prefabStatus != PrefabInstanceStatus.NotAPrefab)
            {
                string prefabPath = AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(gameObject));

                PrefabUtility.ReplacePrefab(gameObject, PrefabUtility.GetCorrespondingObjectFromSource(gameObject));

                string bundleName = gameObject.name.ToLower();

                // Set the asset bundle name for the prefab
                AssetImporter.GetAtPath(prefabPath).SetAssetBundleNameAndVariant(bundleName, "");

                // Build a single asset bundle for the prefab
                string bundlePath = Application.dataPath + "/" + subFolderExport + "/" + bundleName;
                Object prefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(Object));
                BuildPipeline.BuildAssetBundle(prefab, null, bundlePath, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.StandaloneWindows);


            }
            else
            {
                Debug.Log(gameObject.name + " is not a prefab, and cannot be exported as an asset bundle.");
            }



            //WRITE STATS
            string content = "";
            // attrange
            content += "animset=" + (int)humanoidAnimationSet;
            content += "\n";

            content += "hp=" + DefaultArmyStats.Health;
            content += "\n";
            content += "dmg=" + DefaultArmyStats.Damage;
            content += "\n";
            content += "attrange=" + DefaultArmyStats.AttackRange;
            content += "\n";
            content += "splashdmg=" + DefaultArmyStats.SplashDamage;
            content += "\n";
            content += "splashdmgrng=" + DefaultArmyStats.SplashDamageRadius;
            content += "\n";
            content += "impactforce=" + DefaultArmyStats.ImpactForce;
            content += "\n";
            content += "meleeblock=" + DefaultArmyStats.MeleeBlockChance;
            content += "\n";
            content += "rangeblock=" + DefaultArmyStats.RangeBlockChance;
            content += "\n";
            content += "rangeblockforce=" + DefaultArmyStats.MaxRangedBlockForce;
            content += "\n";
            content += "meleearmor=" + DefaultArmyStats.MeleeArmor;
            content += "\n";
            content += "rangearmor=" + DefaultArmyStats.RangeArmor;
            content += "\n";
            content += "rangearmorforce=" + DefaultArmyStats.MaxRangedArmorForce;



            content += "\n";
            content += "attacktime=" + DefaultArmyStats.AttackTimeSeconds;
            content += "\n";
            content += "attackbreak=" + DefaultArmyStats.AttackBreakSeconds;

            content += "\n";
            content += "projectile=" + ((int)(DefaultArmyStats.RangedProjectile.FireProjectile) - 1);
            content += "\n";
            content += "projectilespeed=" + DefaultArmyStats.RangedProjectile.ProjectileSpeed;
            content += "\n";
            content += "burstamount=" + DefaultArmyStats.RangedProjectile.BurstFireAmount;
            content += "\n";
            content += "burstrate=" + DefaultArmyStats.RangedProjectile.BurstRate;




            //PHYSICS

            content += "\n";
            content += "unitheight=" + collisionSettings.CollisionHeight;
            content += "\n";
            content += "unitwidth=" + collisionSettings.CollisionWidth;
            content += "\n";
            content += "soundpres=" + (int)SoundVoicePresets;

            content += "\n";
            if (gameObject.GetComponent<Animator>().isHuman || (int)humanoidAnimationSet > 0) content += "human=1";
            else content += "human=0";

            content += "\n";

            content += "accuracy=" + (int)DefaultArmyStats.RangedProjectile.AccuracyPercent;

            File.WriteAllText(Application.dataPath + "/" + subFolderExport + "/" + "MODDATA-" + gameObject.name.ToLower() + ".bgmod", content);

#endif

            Debug.Log("Export Complete");

        }
    }



    string CheckList()
    {
        string error = "";


        if((int)humanoidAnimationSet == 0)
        {
            if (customAnimationSettings.IdleNormal == null) error += " Idle Normal has not been assigned for custom animations!";
            if (customAnimationSettings.IdleCombat == null) error += " Idle combat has not been assigned for custom animations! If you don't have one, assign normal idle in place.";
            if (customAnimationSettings.Walk == null) error += " Walk has not been assigned for custom animations!";
            if (customAnimationSettings.Jog == null) error += " Jog has not been assigned for custom animations! If you don't have one, try assigning run animation or walk.";
            if (customAnimationSettings.Run == null) error += " Run has not been assigned for custom animations!";
            if (customAnimationSettings.Attacks.Length <= 0) error += " No attack animations assigned!";
            if (customAnimationSettings.Deaths.Length <= 0) error += " No death animations assigned!";

        }

        if (RootBone == null) error += " Root bone has not been assigned!";
        if (HeadBone == null) error += " Head bone has not been assigned!";

     //   if(humanoidAnimationSet == AnimSets.Custom) error += " Custom animation sets are not yet supported in this version!";

        for (int i = 0; i < AllMeshes.Length; i++)
        {
            if (AllMeshes[i].Skinned == null) error += " Mesh element " + i + " is missing a skinned mesh!";
            else
            {
                if(!AllMeshes[i].Skinned.sharedMesh.isReadable) error += " Mesh element " + i + " is not read/write enabled.  Please enable read/write on import settings for model file located in your project folder.";

                for (int m = 0; m < AllMeshes[i].Skinned.sharedMaterials.Length; m++)
                {
                    if (AllMeshes[i].Skinned.sharedMaterials[m] == null)
                    {
                        error += " Mesh element " + i + " has an empty material slot in the skinned mesh settings, slot: " + m;
                    }
                }
            }


        }



        return error;
    }





    [System.Serializable]
    public class MeshProps
    {
        public SkinnedMeshRenderer Skinned;
       // public Renderer MeshRenderer;
      //  [HideInInspector]
        public Material[] mats;
        [Tooltip("Lower priroity makes it more likely to cull at closer render distance. Materials such as fur or small details/decals should be low or very low")]
        public Priority RenderPriority = Priority.Med;
   
    }
   

    private void Update()
    {
        for (int i = 0; i < AllMeshes.Length; i++)
        {
            if (AllMeshes[i].Skinned != null/* || AllMeshes[i].MeshRenderer != null*/)
            {
                if (AllMeshes[i].Skinned != null) AllMeshes[i].mats = AllMeshes[i].Skinned.sharedMaterials;
               // if (AllMeshes[i].MeshRenderer != null) AllMeshes[i].mats = AllMeshes[i].MeshRenderer.sharedMaterials;
            }

        }

        if(AllMeshes.Length != LastArraycount)
        {
            if(AllMeshes.Length > LastArraycount)
            {
                for (int i = AllMeshes.Length - (AllMeshes.Length - LastArraycount); i < AllMeshes.Length; i++)
                {
                    AllMeshes[i].RenderPriority = Priority.Med;
                }
            }

            LastArraycount = AllMeshes.Length;
        }

        

        if (SeperateSubMeshes)
        {
            SeperateSubMeshes = false;
            List<MeshProps> nm = new List<MeshProps>();
            for (int i = 0; i < AllMeshes.Length; i++)
            {
                GameObject[] newobs = SeparateSubmeshes(AllMeshes[i].Skinned.gameObject);

                for (int s = 0; s < newobs.Length; s++)
                {
                    MeshProps nmm = new MeshProps();
                    nmm.Skinned = newobs[s].GetComponent<SkinnedMeshRenderer>();
                    nm.Add(nmm);
                }

            }

            AllMeshes = nm.ToArray();

        }
    }



    public GameObject[] SeparateSubmeshes(GameObject sourceObject)
    {
        SkinnedMeshRenderer sourceRenderer = sourceObject.GetComponent<SkinnedMeshRenderer>();

        if (sourceRenderer == null)
        {
            Debug.LogWarning("No SkinnedMeshRenderer found on the source object.");
            return null;
        }

        Mesh sourceMesh = sourceRenderer.sharedMesh;
        Material[] sourceMaterials = sourceRenderer.sharedMaterials;

        if (sourceMesh == null || sourceMesh.subMeshCount <= 1)
        {
            Debug.LogWarning("No sub-meshes found to separate.");
            return null;
        }

        List<GameObject> createdObjects = new List<GameObject>();

        for (int i = 0; i < sourceMesh.subMeshCount; i++)
        {
            GameObject newObject = new GameObject(sourceMesh.name + " Submesh "+i, typeof(SkinnedMeshRenderer));
            newObject.transform.SetParent(sourceObject.transform.parent, false);
            newObject.transform.localPosition = Vector3.zero;
            newObject.transform.localRotation = Quaternion.identity;
            newObject.transform.localScale = Vector3.one;

            SkinnedMeshRenderer newRenderer = newObject.GetComponent<SkinnedMeshRenderer>();

            Mesh newMesh = CreateMeshFromSubmesh(sourceMesh, i);
            newRenderer.sharedMesh = newMesh;

          

            newRenderer.sharedMaterials = new Material[] { sourceMaterials[i] };
            newRenderer.bones = sourceRenderer.bones;
            newRenderer.rootBone = sourceRenderer.rootBone;

            createdObjects.Add(newObject);

            //SAVE MESH
            newMesh.name = transform.name + "-"+ newObject.name;

#if UNITY_EDITOR
            AssetDatabase.CreateAsset(newMesh, "Assets"+"/"+SaveMeshFolder+"/"+ newMesh.name);
            AssetDatabase.SaveAssets();
#endif
        }

        return createdObjects.ToArray();
    }

    private Mesh CreateMeshFromSubmesh(Mesh sourceMesh, int submeshIndex)
    {
        if (submeshIndex < 0 || submeshIndex >= sourceMesh.subMeshCount)
        {
            Debug.LogError("Invalid submesh index.");
            return null;
        }

        Mesh newMesh = new Mesh();
        if (sourceMesh.triangles.Length > 150000) newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        int[] sourceTriangles = sourceMesh.GetTriangles(submeshIndex);

        // Collect unique vertex indices from the submesh
        HashSet<int> uniqueIndices = new HashSet<int>(sourceTriangles);
        int[] newIndexRemap = new int[sourceMesh.vertexCount];
        Vector3[] sourceVertices = sourceMesh.vertices;
        Vector2[] sourceUv = sourceMesh.uv;
        Vector3[] sourceNormals = sourceMesh.normals;
        Vector4[] sourceTangents = sourceMesh.tangents;
        BoneWeight[] sourceBoneWeights = sourceMesh.boneWeights;

        // Create new vertex attribute arrays
        Vector3[] vertices = new Vector3[uniqueIndices.Count];
        Vector2[] uv = new Vector2[uniqueIndices.Count];
        Vector3[] normals = new Vector3[uniqueIndices.Count];
        Vector4[] tangents = new Vector4[uniqueIndices.Count];
        BoneWeight[] boneWeights = new BoneWeight[uniqueIndices.Count];

        int newIndex = 0;
        foreach (int vertexIndex in uniqueIndices)
        {
            newIndexRemap[vertexIndex] = newIndex;
            vertices[newIndex] = sourceVertices[vertexIndex];
            uv[newIndex] = sourceUv[vertexIndex];
            normals[newIndex] = sourceNormals[vertexIndex];
            tangents[newIndex] = sourceTangents[vertexIndex];
            boneWeights[newIndex] = sourceBoneWeights[vertexIndex];
            newIndex++;
        }

        // Remap the triangles to the new vertex indices
        int[] triangles = new int[sourceTriangles.Length];
        for (int i = 0; i < sourceTriangles.Length; i++)
        {
            triangles[i] = newIndexRemap[sourceTriangles[i]];
        }

        newMesh.vertices = vertices;
        newMesh.uv = uv;
        newMesh.normals = normals;
        newMesh.tangents = tangents;
        newMesh.boneWeights = boneWeights;
        newMesh.triangles = triangles;
        newMesh.bindposes = sourceMesh.bindposes;

        newMesh.RecalculateBounds();

        return newMesh;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position + (Vector3.up * collisionSettings.CollisionHeight * 0.5f), new Vector3(collisionSettings.CollisionWidth, collisionSettings.CollisionHeight, collisionSettings.CollisionWidth));
    }

    /* SMILEANDWIN CODE START */
    [Tooltip("Path to your game installation without trailing slash. Example: E:/Games/steamapps/common/UEBS2")]
    public string GameInstallationPath;

#if UNITY_EDITOR
    public string GetSourcePath => Application.dataPath + "/" + subFolderExport + "/";
    public string GetBgModFilePath => "MODDATA-" + name.ToLower() + ".bgmod";
    public string GetModFilePath => name.ToLower();
    public string GetTargetPath => GameInstallationPath + "/UEBS2_Data/StreamingAssets/ModCharacterTest/";

    public void CheckForErrors()
    {
        var animator = GetComponent<Animator>();
        var otherErrors = CheckList();
        if (otherErrors != "" && otherErrors != null)
            throw new UnityException(otherErrors);

        if (name.Contains("-"))
            throw new UnityException("Detected disallowed character in name: " + name);

        if (GameInstallationPath is null || GameInstallationPath == "")
            throw new UnityException("Game installation path not set. Example: E:/Games/steamapps/common/UEBS2");

        if(AllMeshes == null || AllMeshes.Length == 0)
            throw new UnityException("AllMeshes is empty. This would result in an invisible unit in UEBS2.");

        foreach (var camera in gameObject.GetComponentsInChildren<Camera>())
            if(camera.isActiveAndEnabled)
                throw new UnityException($"Your model contains an active camera. This will render the game unplayable. Please remove or disable the camera in {camera.name}.");

        foreach (var light in gameObject.GetComponentsInChildren<Light>())
            if(light.type == LightType.Directional)
                throw new UnityException($"Your model contains a directional light, e.g. a sun. Please remove the light in {light.name}.");

        if (UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null)
            throw new UnityException("You are currently not viewing a scene but a prefab. This is not supported at the moment. Open a .unity file and retry from there.");

        if (humanoidAnimationSet == AnimSets.Troll)
            Debug.LogWarning("Using the Troll AnimSet might not work as intended. Troll is a non-humanoid animation set and might be incompatible with the rig you are using. Consider using custom animations instead.");

        if (humanoidAnimationSet == AnimSets.Custom)
        {
            if (animator == null || animator.runtimeAnimatorController == null)
                throw new UnityException("You specified the animations to be 'custom' but provided no Animator with Controller so far. This would result in an unanimated unit. Attach an Animator with a Controller containing all animation clips.");
        }

        if (animator.avatar != null && !animator.avatar.isHuman && humanoidAnimationSet != AnimSets.Custom)
            throw new UnityException("You are using a humanoid animation set with a character that uses a generic rig. Please either change the rig to Humanoid or change humanoidAnimationSet to Custom.");

        foreach (var curr in gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
            if (curr.sharedMaterials.Length > 1)
                throw new UnityException("Your SkinnedMeshRenderer in " + curr.name + " uses more than 1 material. This is not allowed in UEBS2 and would lead to the unit falling through the map. Use a 3D modelling tool to separate the meshes by material, e.g. 'Mesh>Separate>By Material' in Blender.");
    }

    [ContextMenu("Exchange MeshRenderers")]
    void ExchangeMeshRenderers()
    {
        if (UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() == null)
        {
            EditorUtility.DisplayDialog("Error", "You are currently viewing a scene and not a prefab. For safety reasons, open the prefab and retry.", "OK");
            return;
        }

        var MeshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>(); // Note: SkinnedMeshRenderers are Renderers but are not MeshRenderers.
        foreach(var curr in MeshRenderers)
        {
            var sharedMaterials = curr.sharedMaterials;
            var go = curr.gameObject;
            var newRenderer = go.AddComponent<SkinnedMeshRenderer>();
            newRenderer.sharedMaterials = sharedMaterials;
        }
    }

    [ContextMenu("Reset AllMeshes")]
    void ResetAllMeshes()
    {
        AllMeshes = GetComponentsInChildren<SkinnedMeshRenderer>().Select(i => new MeshProps
        {
            Skinned = i,
            mats = i.sharedMaterials
        }).ToArray();
        PrefabUtility.RecordPrefabInstancePropertyModifications(this);
    }


    [ContextMenu("Export + Test in UEBS2")]
    void TestCharacter()
    {
        try
        {
            CheckForErrors();
        }
        catch(UnityException e)
        {
            EditorUtility.DisplayDialog("Error", e.Message, "OK");
            return;
        }
        
        ExportCharacter();

        File.Copy(GetSourcePath + GetModFilePath, GetTargetPath + GetModFilePath, true);
        File.Copy(GetSourcePath + GetBgModFilePath, GetTargetPath + GetBgModFilePath, true);
        Debug.Log("Copied. Starting game now.");

        System.Diagnostics.Process.Start("steam://run/1468720");
    }

#endif
    /* SMILEANDWIN CODE END */
}
