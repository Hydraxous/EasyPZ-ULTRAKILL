using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EasyPZ
{
    public abstract class LevelSessionBehaviour : MonoBehaviour
    {
        protected bool started;
        protected bool stopped;

        private void Update()
        {
            if (stopped)
                return;

            if (!started)
            {
                if (StatsManager.Instance.seconds > 0)
                    StartSession();
                else
                    return;
            }

            if (started)
            {
                if (StatsManager.Instance.infoSent)
                {
                    StopSession();
                }
            }

            OnSessionUpdate();
        }

        protected abstract void OnSessionUpdate();

        private void StartSession()
        {
            started = true;
            OnStartSession();
        }

        protected abstract void OnStartSession();

        protected abstract void OnStopSession();


        private void StopSession()
        {
            stopped = true;
            OnStopSession();
        }
    }
}
