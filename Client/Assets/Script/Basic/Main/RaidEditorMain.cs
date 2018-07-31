using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

class RaidEditorMain : SingletonBehaviour<RaidEditorMain>
{
    public override void Awake()
    {
        base.Awake();
        Debuger.EnableLog = true;
        Application.targetFrameRate = 30;
        Input.multiTouchEnabled = true;
        Debuger.Log("GetQualityLevel = " + QualitySettings.GetQualityLevel());
        GameUtility.ResetCameraAspect();

        Screen.SetResolution(1600, 900, false); 

        SetDontDestroyOnLoadInMain();
        Debuger.Log(Application.persistentDataPath);

    }
    void Start()
    {
        InitModules();  //电脑上不走版本更新流程//
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
        ItemManager.GetInst().Init();
        EquipManager.GetInst().Init();
        PetManager.GetInst().Init();
        RaidManager.GetInst().Init();
        TaskManager.GetInst().Init();
/*        CombatManager.GetInst().Init();*/
/*        PressureManager.GetInst().Init();*/
        HomeManager.GetInst().Init();
        WorldMapManager.GetInst().Init();
        CutSceneManager.GetInst().Init();
        NpcManager.GetInst().Init();
        GuideManager.GetInst().Init();

        ModelResourceManager.GetInst().InitCommonResources();
        UIManager.GetInst().CloseUI("UI_ResourceLoading");

        Debuger.Log("配置加载完毕！");

        FinishLoadConfig();
    }

    public void FinishLoadConfig()
    {
        RaidConfigManager.GetInst().InitInRaidConfig(); //加载迷宫配置
        RaidEditor.GetInst().EnterBuildMode();
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
        NetworkManager.GetInst().Update();
        GameStateManager.GetInst().Update();
        ScreenLog.GetInst().Update();
/*        CombatManager.GetInst().Update();*/
    }

    void OnGUI()
    {
        ScreenLog.GetInst()._OnGUI();
    }

    public void ExitProcess()
    {
        HomeManager.GetInst().DestroyHome();
        WorldMapManager.GetInst().DestroyWorldMap();
        ModelResourceManager.GetInst().OnApplicationQuit();
        NetworkManager.GetInst().DisconnectToServer();
        ResourceManager.GetInst().UnloadAllAB();
        Debuger.Log("ExitProcess");
    }

    void OnApplicationQuit()
    {
        ExitProcess();
    }

}
