using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EasyPZ.Components
{
    public class GhostNotifier : MonoBehaviour
    {

        private static GhostNotifier instance;
        private Animator animator;
        private void Awake()
        {
            instance = this;
            animator = GetComponentInChildren<Animator>();
        }

        private void ShowNotif()
        {
            animator.Play("Show");
        }

        public static void Notify()
        {
            if (instance == null)
                return;

            instance.ShowNotif();
        }
    }
}
