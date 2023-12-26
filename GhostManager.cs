using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;

namespace EasyPZ
{
    public class GhostManager : LevelSessionBehaviour
    {
        private List<GhostPlayer> ghostPlayers = new List<GhostPlayer>();

        private static GameObject GhostPlayerPrefab
        {
            get
            {
                if (ghostPlayerPrefab == null)
                {
                    ghostPlayerPrefab = BuildGhostPlayerPrefab();
                }
                return ghostPlayerPrefab;
            }
        }

        private static GameObject ghostPlayerPrefab;

        protected override void OnSessionUpdate()
        {
        }

        public static void PreloadGhostPrefab()
        {
            if (ghostPlayerPrefab != null)
                return;

            ghostPlayerPrefab = BuildGhostPlayerPrefab();
        }

        private void Start()
        {
            string sceneName = SceneHelper.CurrentScene;
            string folderPath = HydraDynamics.DataPersistence.DataManager.GetDataPath("Recordings", sceneName);

            DirectoryInfo info = new DirectoryInfo(folderPath);

            List<SessionRecording> recordings = new List<SessionRecording>();
            foreach (var x in info.GetFiles("*.ukrun", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    SessionRecording recording = SessionRecording.LoadFromBytes(File.ReadAllBytes(x.FullName));
                    Debug.Log($"Loaded recording {x.Name} with {recording.frames.Count} frames");
                    recordings.Add(SessionRecording.LoadFromBytes(File.ReadAllBytes(x.FullName)));
                }catch(Exception e)
                {
                    Debug.LogError(e);
                }
            }

            foreach (var x in recordings)
            {
                GameObject go = GameObject.Instantiate(GhostPlayerPrefab);
                go.SetActive(true);

                GhostPlayer player = go.AddComponent<GhostPlayer>();

                player.SetRecording(x);
                ghostPlayers.Add(player);
            }
        }

        protected override void OnStartSession()
        {
            foreach (var gp in ghostPlayers)
            {
                gp.Play();
            }
        }

        protected override void OnStopSession()
        {
            foreach (var gp in ghostPlayers)
            {
                if(gp != null)
                    gp.Dispose();
            }
        }

        private static GameObject BuildGhostPlayerPrefab()
        {
            //Load prefab
            GameObject v2Prefab = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Enemies/V2.prefab").WaitForCompletion();

            if (v2Prefab == null)
            {
                Debug.LogError("Could not find V2 prefab");
                return null;
            }

            try
            {
                GameObject newPrefab = GameObject.Instantiate(v2Prefab);

                RemoveAllComponent<V2>(newPrefab);
                RemoveAllComponent<Rigidbody>(newPrefab);
                RemoveAllComponent<Collider>(newPrefab);
                RemoveAllComponent<Machine>(newPrefab);
                RemoveAllComponent<NavMeshAgent>(newPrefab);
                RemoveAllComponent<EnemyIdentifier>(newPrefab);
                RemoveAllComponent<EnemyIdentifierIdentifier>(newPrefab);
                RemoveAllComponent<GroundCheckEnemy>(newPrefab);
                RemoveAllComponent<BulletCheck>(newPrefab);
                RemoveAllComponent<EnemySimplifier>(newPrefab);
                RemoveAllComponent<EnemyShotgun>(newPrefab);
                RemoveAllComponent<EnemyNailgun>(newPrefab);
                RemoveAllComponent<EnemyRevolver>(newPrefab);
                RemoveAllComponent<V2AnimationController>(newPrefab);

                SkinnedMeshRenderer[] meshRenderers = newPrefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                Shader ghostShader = Addressables.LoadAssetAsync<Shader>("Assets/Shaders/Transparent/ULTRAKILL-unlit-transparent-zwrite-fresnel.shader").WaitForCompletion();

                for(int j = 0; j < meshRenderers.Length; j++)
                {

                    for (int i = 0; i < meshRenderers[j].materials.Length; i++)
                    {
                        Material newMat = new Material(ghostShader);

                        newMat.CopyPropertiesFromMaterial(meshRenderers[j].materials[i]);
                        newMat.SetFloat("_OpacScale", 0.3f);
                        newMat.SetColor("_Color", Color.blue);//Test

                        meshRenderers[j].materials[i] = newMat;
                    }
                }
                

                newPrefab.name = "PlayerGhost";
                newPrefab.hideFlags = HideFlags.HideAndDontSave;
                newPrefab.SetActive(false);
                DontDestroyOnLoad(newPrefab);

                return newPrefab;
            }catch (Exception e)
            {
                Debug.LogError(e);
                return null;
            }
        }

        private static void RemoveAllComponent<T>(GameObject go) where T : Component
        {
            T[] components = go.GetComponentsInChildren<T>(true);

            for(int i = 0; i < components.Length; i++)
            {
                T c = components[i];
                components[i] = null;
                UnityEngine.Object.Destroy(c);
            }
        }
    }
}
