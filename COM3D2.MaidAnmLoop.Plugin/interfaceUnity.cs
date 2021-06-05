using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.SceneManagement;

namespace COM3D2.Lilly.Plugin.Utill
{
    public interface interfaceUnity
    {
        public void Awake();
        
        public void OnEnable();

        public void Start();

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode);

        public void Update();

        public void FixedUpdate();

        public void LateUpdate();

        public void OnGUI();

        public void WindowFunction(int id);

        public void OnDisable();

        public void Pause();

        public void Resume();
    }
}
