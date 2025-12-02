using System;
using UnityEngine;

namespace Splash
{
    public class SplashSystem : MonoBehaviour
    {
        public static SplashSystem Instance;

        private void Awake()
        {
            Instance = this;
        }
    }
}
