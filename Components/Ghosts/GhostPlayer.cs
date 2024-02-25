using Configgy;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using EasyPZ.Ghosts;

namespace EasyPZ.Components
{
    public class GhostPlayer : MonoBehaviour, IDisposable
    {
        GhostRecording recording;
        private Animator animator;
        private Transform[] rotationTransforms;

        public void SetRecording(GhostRecording recording)
        {
            this.recording = recording;
        }

        private float timeStarted;
        private bool isPlaying;

        private void Awake()
        {
            animator = GetComponentsInChildren<Animator>(true).Where(x => x.name == "v2_combined").FirstOrDefault();
            rotationTransforms = new Transform[2];
            rotationTransforms[0] = GetComponentsInChildren<Transform>(true).Where(x => x.name == "spine.006").FirstOrDefault();
            rotationTransforms[1] = GetComponentsInChildren<Transform>(true).Where(x => x.name == "upper_arm.R").FirstOrDefault();
            TrailRenderer trailRenderer = GetComponentsInChildren<TrailRenderer>(true).FirstOrDefault();
            if (trailRenderer != null)
                trailRenderer.enabled = false;

           
        }

        private void Start()
        {
            InstanceNamePlate();
        }

        private void InstanceNamePlate()
        {
            string playerName = "";
            try
            {
                if (SteamClient.IsValid)
                {
                    playerName = new Friend(recording.Metadata.SteamID).Name;
                    Debug.Log($"Created Ghost {playerName}");
                    gameObject.name += $"({playerName})";
                }
                else
                {
                    throw new Exception("Steam not valid or not connected!");
                }
                
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError("Unable to name ghost :(");
                return;
            }

            GameObject nameplate = GameObject.Instantiate(Prefabs.PlayerNameplate, transform);
            nameplate.transform.localPosition = new Vector3(0f, 3.854f, 0f);
            nameplate.transform.localEulerAngles = new Vector3(0f, 0f, 0f);

            Text nameText = nameplate.GetComponentInChildren<Text>();

            nameText.text = playerName;
        }

        private void Update()
        {
            if (!isPlaying)
                return;

            if (recording == null)
                return;

            float timeElapsed = Time.time - timeStarted;
            GhostRecordingFrame[] frames = recording.GetNearestTwoFrames(timeElapsed);

            if (frames == null)
                return;

            if (frames[1].time < timeElapsed)
            {
                Dispose();
                return;
            }

            float t = Mathf.InverseLerp(frames[0].time, frames[1].time, timeElapsed);
            int animation = t > 0.5f ? frames[1].animation : frames[0].animation;


            Vector3 position = Vector3.Lerp(frames[0].position, frames[1].position, t);

            //No idea why but v2 has a weird offset only when not sliding.
            if (animation == 1 || animation == 0)
                position += -Vector3.up * 1.15f;

            float yRot = Mathf.LerpAngle(frames[0].rotation.y, frames[1].rotation.y, t);
            float xRot = Mathf.LerpAngle(frames[0].rotation.x, frames[1].rotation.x, t);

            transform.position = position;
            transform.eulerAngles = new Vector3(0f, yRot, 0);

            for (int i = 0; i < rotationTransforms.Length; i++)
            {
                if (rotationTransforms[i] == null)
                    continue;

                rotationTransforms[i].eulerAngles = new Vector3(xRot, 0f, 0f);
            }

            SetAnimation(animation);
        }

        public void Play(float timeStarted)
        {
            this.timeStarted = timeStarted;
            isPlaying = true;
        }

        public void SetCurrentTime(float time)
        {
            timeStarted = Time.time - time;
        }

        public void Stop()
        {
            isPlaying = false;
        }

        int lastAnimationState = 0;

        private void SetAnimation(int animationState)
        {
            if (lastAnimationState == animationState)
                return;

            if (animator == null)
                return;

            switch (animationState)
            {
                case 0: //Idle
                    animator.SetLayerWeight(1, 0f);
                    animator.SetBool("RunningForward", false);
                    animator.SetBool("InAir", false);
                    animator.SetBool("Sliding", false);
                    break;
                case 1: //Running
                    animator.SetLayerWeight(1, 1f);
                    animator.SetBool("RunningForward", true);
                    animator.SetBool("InAir", false);
                    animator.SetBool("Sliding", false);
                    break;
                case 2: //Sliding
                    animator.SetLayerWeight(1, 0f);
                    animator.Play("Slide", 0, 0f);
                    animator.SetBool("RunningForward", false);
                    animator.SetBool("InAir", false);
                    animator.SetBool("Sliding", true);
                    break;
                case 3: //Air
                    animator.SetLayerWeight(1, 0f);
                    animator.SetBool("RunningForward", false);
                    animator.SetBool("InAir", true);
                    animator.SetBool("Sliding", false);
                    break;
            }
        }

        public void Dispose()
        {
            Destroy(gameObject);
        }
    }


}
