using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using COM3D2.Lilly.Plugin.Utill;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace COM3D2.MaidAnmLoop.Plugin
{
    [BepInPlugin("COM3D2.PhotoModeAutoLoop.Plugin", "COM3D2.PhotoModeAutoLoop.Plugin", "21.6.05")]// 버전 규칙 잇음. 반드시 2~4개의 숫자구성으로 해야함. 미준수시 못읽어들임
    [BepInProcess("COM3D2x64.exe")]
    public class MaidAnmLoop : BaseUnityPlugin, interfaceUnity
    {
        public static ConfigEntryUtill configEntryUtill;
        public static ConfigEntryUtill configEntryUtillScene;


        public MaidAnmLoop()
        {
        }

        string[] wrapModes;

        public void Awake()
        {
            wrapModes = Enum.GetNames(typeof(WrapMode));
            ConfigEntryUtill.init(Config);
            configEntryUtill = ConfigEntryUtill.Create(
                "PhotoModeAutoLoop"
            );
            configEntryUtillScene = ConfigEntryUtill.Create(
                "PhotoModeAutoLoop"
            );
        }

        public void OnEnable()
        {
            SceneManager.sceneLoaded += this.OnSceneLoaded;
        }

        public void Start()
        {

        }

        string scene_name = string.Empty;

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            scene_name = scene.name;
            if (isOn = configEntryUtillScene[scene.name, false])
            {
                Apply();
            }
        }

        public void FixedUpdate()
        {
        }

        public bool isOn;
        public bool isRepeat;
        public WrapMode wrapMode = WrapMode.Default;

        public void Update()
        {
            // maid.GetAnimation().isPlaying 
            if (isRepeat)
            {
                Apply();
            }
            // 
        }

        private void Apply()
        {
            foreach (var item in GameMain.Instance.CharacterMgr.GetStockMaidList())
            {
                item.GetAnimation().wrapMode = wrapMode;
            }
        }

        #region OnGUI

        private Rect windowRect = new Rect(windowSpace, windowSpace, 400f, 400f);
        private int windowId = new System.Random().Next();
        private const float windowSpace = 40.0f;

        public void OnGUI()
        {
            windowRect.x = Mathf.Clamp(windowRect.x, -windowRect.width + windowSpace, Screen.width - windowSpace);
            windowRect.y = Mathf.Clamp(windowRect.y, -windowRect.height + windowSpace, Screen.height - windowSpace);
            windowRect = GUILayout.Window(windowId, windowRect, WindowFunction, "PhotoModeAutoLoop" + windowId);
        }

        #endregion


        private Vector2 scrollPosition;
        private int selected;

        public void WindowFunction(int id)
        {
            GUI.enabled = true;
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label(scene_name);
            if (GUILayout.Button("Apply" + isOn)) { Apply(); configEntryUtillScene[scene_name, false] = isOn = !isOn; }
            if (GUILayout.Button("Apply Repeat" + " , " + isRepeat)) isRepeat = !isRepeat;

            GUILayout.Label("Mode");
            selected = GUILayout.SelectionGrid(selected, wrapModes, 2);
            if (GUI.changed)
            {
                wrapMode = (WrapMode)Enum.Parse(typeof(WrapMode), wrapModes[selected]);
                Apply();
            }

            GUILayout.Label("Auto Apply Scene");
            foreach (var item in configEntryUtillScene)
            {
                if (GUILayout.Button(item.Key + " , " + item.Value.Value)) item.Value.Value = !item.Value.Value;
            }

            GUILayout.EndScrollView();
            GUI.DragWindow();
            GUI.enabled = true;
        }

        /*
Once	재생시간이 애니메이션 클립의 끝부분에 다다르는 경우에, 클립이 자동으로 재생을 멈추고 재생시간은 클립의 첫부분으로 리셋됩니다.
Loop	재생시간이 애니메이션 클립의 끝부분에 다다르면, 재생시간이 처음으로 되돌아가서 재생을 반복합니다.
PingPong	재생시간이 애니메이션 클립의 끝부분에 다다르면, 재생시간은 시작과 끝부분 사이에서 반복적으로 바뀌게 됩니다.
Default	기본 반복 모드(repeat mode)를 읽어옵니다.
ClampForever	애니메이션을 재생합니다. 재생의 끝부분에 다다르면, 마지막 프레임의 재생을 계속 유지하고 재생을 멈추지 않습니다.
         */

        public void OnDisable()
        {
            SceneManager.sceneLoaded -= this.OnSceneLoaded;
        }






        public void LateUpdate()
        {
        }


        public void Pause()
        {
        }

        public void Resume()
        {
        }
    }
}
