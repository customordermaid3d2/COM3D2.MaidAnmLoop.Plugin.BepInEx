using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using BepInEx.Configuration;

using COM3D2.LillyUtill;
using COM3D2API;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace COM3D2.MaidAnmLoop.Plugin
{
    [BepInPlugin("COM3D2.MaidAnmLoop.Plugin", "COM3D2.MaidAnmLoop.Plugin", "22.01.18.15")]// 버전 규칙 잇음. 반드시 2~4개의 숫자구성으로 해야함. 미준수시 못읽어들임
    [BepInProcess("COM3D2x64.exe")]
    public class MaidAnmLoop : BaseUnityPlugin
    {
     
        private static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> ShowCounter;
        
        public static MyWindowRect myWindowRect;
        //private Rect windowRect = new Rect(windowSpace, windowSpace, 250f, 400f);
        private int windowId = new System.Random().Next();
        private const float windowSpace = 40.0f;

        ConfigEntryUtill2<bool> configScene;//= new ConfigEntryUtill2();
        ConfigEntryUtill2<int> configMode;//= new ConfigEntryUtill2();
        
        public MaidAnmLoop()
        {
            ConfigFile c = Config;
        }

        string[] wrapModes;

        public static byte[] ExtractResource(Bitmap image)
        {
            using (MemoryStream ms = new MemoryStream())
            {                
                image.Save(ms, ImageFormat.Png);
                return ms.ToArray(); 
            }
        }

        public void Awake()
        {
            Logger.LogMessage("Awake");
            configScene = new ConfigEntryUtill2<bool>( Config,false);
            configMode = new ConfigEntryUtill2<int>( Config,0);
            ShowCounter = Config.Bind("GUI", "isGUIOnKey", new BepInEx.Configuration.KeyboardShortcut(KeyCode.Alpha6, KeyCode.LeftControl));
            myWindowRect = new MyWindowRect(Config, "COM3D2.MaidAnmLoop.Plugin", "MaidAnmLoop", "MAL");
            //IsGUIOn = Config.Bind("GUI", "isGUIOn", false);
            wrapModes = Enum.GetNames(typeof(WrapMode));
            //ConfigEntryUtill.init(Config);
            //ConfigEntryUtill<int>.init(Config);

            SystemShortcutAPI.AddButton("COM3D2.MaidAnmLoop.Plugin", new Action(delegate() { myWindowRect.IsGUIOn = !myWindowRect.IsGUIOn; }), "COM3D2.MaidAnmLoop.Plugin " + ShowCounter.Value.ToString(), ExtractResource(Properties.Resources.AnmLoop));
        }
                
        public void OnEnable()
        {
            SceneManager.sceneLoaded += this.OnSceneLoaded;
            myWindowRect?.load();
        }

        public void Start()
        {

        }

        string scene_name = string.Empty;

        /// <summary>
        /// Start() 이전에 실행됨
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="mode"></param>
        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            scene_name = scene.name;
            try
            {
                if (isOn = configScene[scene.name, "isOn"])
                {
                    selected = configMode[scene.name, "WrapMode"];
                    Apply();
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }

        public void Update()
        {
            // maid.GetAnimation().isPlaying 
            if (isOn && isRepeat)
            {
                Apply();
            }            
        }


        public void OnGUI()
        {
            if (!myWindowRect.IsGUIOn)
            {
                return;
            }
            myWindowRect.WindowRect = GUILayout.Window(windowId, myWindowRect.WindowRect, WindowFunction, "", GUI.skin.box);
        }

        private Vector2 scrollPosition;
        private int selected;
        private int selectedf;

        public bool isOn;
        public bool isRepeat;
        public WrapMode wrapMode = WrapMode.Default;



        public void WindowFunction(int id)
        {
            GUI.enabled = true;

            GUILayout.BeginHorizontal();
            //GUILayout.Label("MaidAnmLoop " + ShowCounter.Value.ToString(), GUILayout.Height(20));
            GUILayout.Label(myWindowRect.windowName, GUILayout.Height(20));

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { myWindowRect.IsOpen = !myWindowRect.IsOpen; }
            if (GUILayout.Button("x", GUILayout.Width(20), GUILayout.Height(20))) { myWindowRect.IsGUIOn = false; }
            GUILayout.EndHorizontal();

            if (!myWindowRect.IsOpen)
            {

            }
            else
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);

                GUILayout.Label(scene_name);
                if (GUILayout.Button("Apply " + isOn)) { Apply(); configScene[scene_name, "isOn"] = isOn = !isOn; }
                if (GUILayout.Button("Apply Repeat" + " , " + isRepeat)) isRepeat = !isRepeat;

                GUILayout.Label("Mode");
                selected = GUILayout.SelectionGrid(selected, wrapModes, 2);
                if (GUI.changed)
                {
                    if (selectedf != selected)
                    {
                        wrapMode = (WrapMode)Enum.Parse(typeof(WrapMode), wrapModes[selected]);
                        configMode[scene_name, "WrapMode"] = selected;
                        Apply();
                        selectedf = selected;
                    }
                }

                GUILayout.Label("Auto Apply Scene");
                foreach (var item in configScene.GetList("isOn"))
                {
                    if (GUILayout.Button(item.Key + " , " + item.Value.Value)) item.Value.Value = !item.Value.Value;
                }

                GUILayout.EndScrollView();
            }

            GUI.DragWindow();
            GUI.enabled = true;
        }

        private void Apply()
        {
            foreach (var maid in GameMain.Instance?.CharacterMgr?.GetStockMaidList())
            {
                if (maid != null && maid.Visible)
                {
                    maid.GetAnimation().wrapMode = wrapMode;
                    //maid.GetAnimation().Play();
                }
            }
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
            //myWindowRect?.save();
        }

    }
}
