using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace EasyPZ
{
    public class TrackerManager : MonoBehaviour
    {
        [SerializeField] private Image blocker;
        [SerializeField] private GameObject container;

        private RectTransform content;
        private GameObject tracker;

        private bool editMode;

        private void Awake()
        {
            instance = this;

            blocker.gameObject.SetActive(false);
            content = container.GetComponent<RectTransform>();
            InstanceTracker();

            if(EasyPZ.PModeDefault)
                EasyPZ.PModeEnabled = true;
        }

        private void InstanceTracker()
        {
            tracker = GameObject.Instantiate(Prefabs.ClassicTrackerPrefab, content);
        }

        private void Update()
        {
            if (editMode)
            {
                if(Input.GetKeyDown(KeyCode.Escape))
                    EndEditMode();

                return;
            }

            if (EasyPZ.Key_PModeToggle.WasPerformedThisFrame)
            {
                EasyPZ.PModeEnabled = !EasyPZ.PModeEnabled;
            }

            if (EasyPZ.Key_RestartMission.WasPerformedThisFrame)
            {
                OptionsManager.Instance.RestartMission();
            }
        }

        [Configgy.Configgable(displayName:"Edit Tracker")]
        private static void EnterEditMode()
        {
            if (instance == null)
                return;

            instance.EnterEditModeInternal();
        }

        private void EnterEditModeInternal()
        {
            editMode = true;

            if (tracker.TryGetComponent<IUIEditable>(out IUIEditable editable))
            {
                editable.StartEditMode();
            }
        }

        private void EndEditMode()
        {
            if(tracker.TryGetComponent<IUIEditable>(out IUIEditable editable))
            {
                editable.EndEditMode();
            }

            editMode = false;
        }


        private static TrackerManager instance;
    }
}
