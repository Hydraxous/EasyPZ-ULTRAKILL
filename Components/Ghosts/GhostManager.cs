using Configgy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;

namespace EasyPZ.Components
{
    public class GhostManager : LevelSessionBehaviour
    {
        [Configgable("Ghosts/Playback", "Max Ghosts")]
        private static ConfigInputField<int> maxGhosts = new ConfigInputField<int>(3);

        [Configgable("Ghosts/Playback", "Ghosts Enabled")]
        private static ConfigToggle ghostsEnabled = new ConfigToggle(true);

        [Configgable("Ghosts/Playback", "Ghost Opacity")]
        private static FloatSlider ghostOpacity = new FloatSlider(0f, 0f, 1f);

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


        private List<GhostPlayer> ghostPlayers;
        private List<SessionRecording> loadedRecordings;

        private bool spawnedGhosts => ghostPlayers != null;
        private float timeStarted;


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
            ghostsEnabled.OnValueChanged += SetGhostsEnabled;
            maxGhosts.OnValueChanged += OnMaxGhostsChanged;

            SetGhostsEnabled(ghostsEnabled.Value);
        }

        private void LoadRecordings()
        {
            string sceneName = SceneHelper.CurrentScene;

            string folderPath = Path.Combine(Paths.ghostRecordingPath.Value, sceneName);

            if (!Directory.Exists(folderPath))
                return;

            DirectoryInfo info = new DirectoryInfo(folderPath);

            List<SessionRecording> recordings = new List<SessionRecording>();
            foreach (var x in info.GetFiles("*.ukrun", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    SessionRecording recording = SessionRecording.LoadFromBytes(File.ReadAllBytes(x.FullName));
                    Debug.Log($"Loaded recording {x.Name} with {recording.frames.Count} frames");
                    recordings.Add(SessionRecording.LoadFromBytes(File.ReadAllBytes(x.FullName)));
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            loadedRecordings = recordings;
        }


        protected override void OnStartSession()
        {
            timeStarted = Time.time;

            if (ghostsEnabled.Value)
                PlayGhosts();
        }

        protected override void OnStopSession()
        {
            DisposeGhosts();
        }

        private void SetGhostsEnabled(bool enabled)
        {
            if (!enabled)
            {
                if (spawnedGhosts)
                    DisposeGhosts();

                return;
            }

            if (!spawnedGhosts)
                SpawnGhosts();

            if (started)
                PlayGhosts();
        }

        private void PlayGhosts()
        {
            foreach (var gp in ghostPlayers)
            {
                gp.Play(timeStarted);
            }
        }

        private void OnMaxGhostsChanged(int maxGhosts)
        {
            if (!spawnedGhosts || !ghostsEnabled.Value)
                return;

            DisposeGhosts();
            SpawnGhosts();
        }

        private void SpawnGhosts()
        {
            ghostPlayers = new List<GhostPlayer>();

            if (loadedRecordings == null)
                LoadRecordings();

            //Sort by lowest time
            foreach (var x in loadedRecordings.OrderBy(x => x.GetTotalTime()).Take(maxGhosts.Value))
            {
                GameObject go = Instantiate(GhostPlayerPrefab);
                go.SetActive(true);

                GhostPlayer player = go.AddComponent<GhostPlayer>();

                player.SetRecording(x);
                ghostPlayers.Add(player);
            }
        }

        private void DisposeGhosts()
        {
            foreach (var gp in ghostPlayers)
            {
                if (gp != null)
                    gp.Dispose();
            }

            ghostPlayers = null;
        }

        private void OnDestroy()
        {
            ghostsEnabled.OnValueChanged -= SetGhostsEnabled;
            maxGhosts.OnValueChanged -= OnMaxGhostsChanged;
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
                GameObject newPrefab = Instantiate(v2Prefab);

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

                List<Renderer> renderers = new List<Renderer>();

                SkinnedMeshRenderer[] skinnedMeshRenderers = newPrefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                MeshRenderer[] meshRenderers = newPrefab.GetComponentsInChildren<MeshRenderer>(true);

                renderers.AddRange(skinnedMeshRenderers);
                renderers.AddRange(meshRenderers);

                Material ghostMaterial = Prefabs.GhostMaterial;

                List<Material> createdMaterials = new List<Material>();

                float opacity = ghostOpacity.Value;

                for (int j = 0; j < renderers.Count; j++)
                {
                    Material[] newMaterials = new Material[renderers[j].sharedMaterials.Length];

                    for (int i = 0; i < renderers[j].sharedMaterials.Length; i++)
                    {
                        Material newMaterial = new Material(ghostMaterial);
                        createdMaterials.Add(newMaterial);

                        Color color = newMaterial.color;
                        color.a = opacity;
                        newMaterial.color = color;

                        newMaterials[i] = newMaterial;
                        Texture mainTex = renderers[j].sharedMaterials[i].GetTexture("_MainTex");
                        newMaterials[i].SetTexture("_MainTex", mainTex);
                    }

                    renderers[j].sharedMaterials = newMaterials;
                }

                ghostOpacity.OnValueChanged += (v) =>
                {
                    Debug.Log($"Setting ghost opac {v}");
                    foreach (var material in createdMaterials)
                    {
                        Color color = material.color;
                        color.a = v;
                        material.color = color;
                    }
                };

                newPrefab.name = "PlayerGhost";
                newPrefab.hideFlags = HideFlags.HideAndDontSave;
                newPrefab.SetActive(false);
                DontDestroyOnLoad(newPrefab);

                return newPrefab;
            }
            catch (Exception e)
            {
                Debug.LogError("Error creating Player Ghost Prefab.");
                Debug.LogException(e);
                return null;
            }
        }

        private static void RemoveAllComponent<T>(GameObject go) where T : Component
        {
            T[] components = go.GetComponentsInChildren<T>(true);

            for (int i = 0; i < components.Length; i++)
            {
                T c = components[i];
                components[i] = null;
                Destroy(c);
            }
        }
    }
}
