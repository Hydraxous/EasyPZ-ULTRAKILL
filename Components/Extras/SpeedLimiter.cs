using Configgy;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EasyPZ.Components
{
    public class SpeedLimiter : LevelSessionBehaviour
    {
        [Configgable("Extras/Speed Limiter", "Speed Limit Enabled")]
        private static ConfigToggle enableSpeedLimit = new ConfigToggle(false);

        [Configgable("Extras/Speed Limiter", "Min Speed")]
        private static ConfigInputField<float> minSpeed = new ConfigInputField<float>(10f);

        [Configgable("Extras/Speed Limiter", "Max Speed")]
        private static ConfigInputField<float> maxSpeed = new ConfigInputField<float>(500f);

        [Configgable("Extras/Speed Limiter", "Forgiveness Time")]
        private static ConfigInputField<float> forgivenessTime = new ConfigInputField<float>(1.5f);

        private float timeSpeedLimitBroken;

        protected override void OnSessionUpdate()
        {
            if (!enableSpeedLimit.Value)
                return;

            float speed = NewMovement.Instance.rb.velocity.magnitude;



            if(speed > maxSpeed.Value || speed < minSpeed.Value)
            {
                timeSpeedLimitBroken += Time.deltaTime;
                if(timeSpeedLimitBroken > forgivenessTime.Value)
                {
                    OptionsManager.Instance.RestartMission();
                }
            }
            else
            {
                timeSpeedLimitBroken = 0f;
            }
        }

        protected override void OnStartSession()
        {
        }

        protected override void OnStopSession()
        {
        }
    }
}
