using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

//using Script;
class AppMain : SingletonBehaviour<AppMain>
{
    public override void Awake()
    {
        base.Awake();
        Debuger.EnableLog = true;
        Application.targetFrameRate = 30;
        Input.multiTouchEnabled = true;
        Debuger.Log("GetQualityLevel = " + QualitySettings.GetQualityLevel());
        GameUtility.ResetCameraAspect();
#if UNITY_ANDROID
                if (Screen.width / (float)Screen.height < 1.775f)
                {
                        int height = (int)(Screen.height * (1136f/ Screen.width));
                        Screen.SetResolution(1136, height, false);
                }
                else
                {
                        int width = (int)(Screen.width * (640f / Screen.height));
                        Screen.SetResolution(width, 640, false);
                }
#endif
#if UNITY_STANDALONE
        Screen.SetResolution(1136, 640, false);
#endif

        SetDontDestroyOnLoadInMain();
        Debuger.Log(Application.persistentDataPath);

    }
    void Start()
    {
#if UNITY_STANDALONE_WIN
        InitModules();  //电脑上不走版本更新流程//
#elif UNITY_ANDROID || UNITY_IPHONE
                UIManager.GetInst().ShowUI<UI_ServerSelect>("UI_ServerSelect");
#endif
    }

    public void InitModules()
    {
        Debuger.Log("开始注册消息和加载配置表！！！");
        InputManager.GetInst().Init();
        LoginManager.GetInst().Init();
        UserManager.GetInst().Init();

        GlobalParams.Init();
        LanguageManager.Init();
        CommonDataManager.GetInst().Init();
        CharacterManager.GetInst().Init();
        SkillManager.GetInst().Init();
        ModelResourceManager.GetInst().Init();
        MonsterManager.GetInst().Init();
        ItemManager.GetInst().Init();
        EquipManager.GetInst().Init();
        PetManager.GetInst().Init();
        VillageManager.GetInst().Init();
        RaidManager.GetInst().Init();
        TaskManager.GetInst().Init();
        CombatManager.GetInst().Init();
        PressureManager.GetInst().Init();
        HomeManager.GetInst().Init();
        WorldMapManager.GetInst().Init();
        CutSceneManager.GetInst().Init();
        NpcManager.GetInst().Init();
        CampManager.GetInst().Init();
        //GuideManager.GetInst().Init();

        ModelResourceManager.GetInst().InitCommonResources();
        UIManager.GetInst().CloseUI("UI_ResourceLoading");

        Debuger.Log("配置加载完毕！");
        FinishLoadConfig();
    }

    public void FinishLoadConfig()
    {
        GameStateManager.GetInst().EnterGame();
//        GameStateManager.GetInst().TryConnect();
    }

    void SetDontDestroyOnLoadInMain()
    {
        GameObject[] initsObjects = GameObject.FindObjectsOfType<GameObject>(); //保存main场景的gameobject不被删除//
        foreach (GameObject go in initsObjects)
        {
            DontDestroyOnLoad(go);
        }
    }
    void Update()
    {
#if UNITY_EDITOR
        if (EditorApplication.isCompiling)
        {
            EditorApplication.isPlaying = false;
            return;
        }
#endif
        //NetworkManager.GetInst().Update();
        GameStateManager.GetInst().Update();
        ScreenLog.GetInst().Update();
        CombatManager.GetInst().Update();
    }

    void OnGUI()
    {
        ScreenLog.GetInst()._OnGUI();
    }

    void CheckQuit()
    {
#if UNITY_ANDROID
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Home))
                {
                        //退出程序
                        UnityEngine.Application.Quit();
                }
#endif
    }

    public void ExitProcess()
    {
        HomeManager.GetInst().DestroyHome();
        WorldMapManager.GetInst().DestroyWorldMap();
        ModelResourceManager.GetInst().OnApplicationQuit();
        NetworkManager.GetInst().DisconnectToServer();
        ResourceManager.GetInst().UnloadAllAB();
        Debuger.Log("ExitProcess");
        //System.Diagnostics.Process.GetCurrentProcess().Kill();   
    }

    void OnApplicationQuit()
    {
        ExitProcess();
    }

}
