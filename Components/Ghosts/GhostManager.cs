using Configgy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using EasyPZ.Ghosts;

namespace EasyPZ.Components
{
    public class GhostManager : LevelSessionBehaviour
    {
        [Configgable("Ghosts/Playback", "Max Ghosts", description:"Hard cap on ghosts to spawn regardless of selected amount.")]
        private static ConfigInputField<int> maxGhosts = new ConfigInputField<int>(3);

        [Configgable("Ghosts/Playback", "Ghosts Enabled")]
        public static ConfigToggle GhostsEnabled = new ConfigToggle(false);

        [Configgable("Ghosts/Playback", "Ghost Opacity")]
        private static FloatSlider ghostOpacity = new FloatSlider(1f, 0f, 1f);

        [Configgable("Ghosts/Playback", "Ghost Spawn Order", description:ghostSpawnPatternDescription)]
        private static ConfigDropdown<GhostSpawnOrderType> ghostSpawningPattern = new ConfigDropdown<GhostSpawnOrderType>((GhostSpawnOrderType[])Enum.GetValues(typeof(GhostSpawnOrderType)));
        const string ghostSpawnPatternDescription = "Changes how the selection of max ghosts are spawned.";

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
        private List<GhostRecording> loadedRecordings;

        private bool spawnedGhosts => ghostPlayers != null;
        private float timeStarted;

        private static GhostManager instance;
        private static Dictionary<string, bool> enabledGhosts = new Dictionary<string, bool>();

        private void Awake()
        {
            instance = this;
        }

        public static bool IsGhostEnabled(string id)
        {
            if (!enabledGhosts.ContainsKey(id))
            {
                enabledGhosts.Add(id, true);
                return true;
            }

            return enabledGhosts[id];
        }

        public static void SetGhostEnabled(string id, bool enabled)
        {
            if (!enabledGhosts.ContainsKey(id))
            {
                enabledGhosts.Add(id, enabled);
                return;
            }

            enabledGhosts[id] = enabled;

            if(instance != null)
            {
                instance.ResetGhosts();
            }
        }


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
            GhostsEnabled.OnValueChanged += SetGhostsEnabled;
            maxGhosts.OnValueChanged += ResetGhostsFromEvent<int>;
            ghostSpawningPattern.OnValueChanged += ResetGhostsFromEvent<GhostSpawnOrderType>;

            SetGhostsEnabled(GhostsEnabled.Value);
        }

        private void LoadRecordings()
        {
            string sceneName = SceneHelper.CurrentScene;

            string folderPath = Path.Combine(Paths.ghostRecordingPath.Value, sceneName);

            if (!Directory.Exists(folderPath))
            {
                loadedRecordings = new List<GhostRecording>();
                return;
            }

            DirectoryInfo info = new DirectoryInfo(folderPath);

            List<GhostRecording> recordings = new List<GhostRecording>();
            foreach (var x in info.GetFiles("*.ukrun", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    GhostRecording recording = GhostRecording.LoadFromBytes(File.ReadAllBytes(x.FullName));
                    recording.Metadata.LocatedFilePath = x.FullName;
                    Debug.Log($"Loaded recording {x.Name} with {recording.frames.Count} frames");
                    recordings.Add(recording);
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

            if (GhostsEnabled.Value)
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

        private void ResetGhostsFromEvent<T>(T _)
        {
            ResetGhosts();
        }

        private void ResetGhosts()
        {
            if (!spawnedGhosts || !GhostsEnabled.Value)
                return;

            DisposeGhosts();
            SpawnGhosts();

            if (started)
                PlayGhosts();
        }

        private void SpawnGhosts()
        {
            ghostPlayers = new List<GhostPlayer>();

            if (loadedRecordings == null)
                LoadRecordings();

            //Sort by lowest time
            foreach (var x in SelectRecordings())
            {
                SpawnGhost(x);
            }
        }

        private IEnumerable<GhostRecording> SelectRecordings()
        {
            if(loadedRecordings.Count == 0)
                return Enumerable.Empty<GhostRecording>();

            IEnumerable<GhostRecording> recordings = loadedRecordings.Where(x => enabledGhosts.ContainsKey(x.Metadata.LocatedFilePath) && enabledGhosts[x.Metadata.LocatedFilePath]);

            if(recordings.Count() == 0)
                return Enumerable.Empty<GhostRecording>();

            switch (ghostSpawningPattern.Value)
            {
                case GhostSpawnOrderType.Fastest:
                    recordings = recordings.OrderBy(x => x.GetTotalTime());
                    break;
                case GhostSpawnOrderType.Slowest:
                    recordings = recordings.OrderByDescending(x => x.GetTotalTime());
                    break;
                case GhostSpawnOrderType.MostRecent:
                    recordings = recordings.OrderByDescending(x => x.Metadata.DateCreated.Ticks);
                    break;
            }

            int takeCount = Mathf.Min(maxGhosts.Value, recordings.Count());
            return recordings.Take(takeCount);
        }

        private void SpawnGhost(GhostRecording recording)
        {
            GameObject go = Instantiate(GhostPlayerPrefab);
            go.SetActive(true);

            GhostPlayer player = go.AddComponent<GhostPlayer>();

            player.SetRecording(recording);
            ghostPlayers.Add(player);
        }

        private void DisposeGhosts()
        {
            if (ghostPlayers == null)
                return;

            foreach (var gp in ghostPlayers)
                gp?.Dispose();

            ghostPlayers = null;
        }

        private void OnDestroy()
        {
            GhostsEnabled.OnValueChanged -= SetGhostsEnabled;
            maxGhosts.OnValueChanged -= ResetGhostsFromEvent<int>;
            ghostSpawningPattern.OnValueChanged -= ResetGhostsFromEvent<GhostSpawnOrderType>;
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

    public enum GhostSpawnOrderType
    {
        Fastest,
        Slowest,
        MostRecent,
    }
}
