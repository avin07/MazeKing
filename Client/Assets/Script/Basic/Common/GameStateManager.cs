using UnityEngine;
using System.Collections;
using Message;
public enum GAMESTATE
{
    INIT,
    LOGIN,
    OTHER,  //其他不重要的状态//
    RAID_EDITOR,
    RAID_PLAYING,
    RAID_COMBAT,
    RAID_CUTSCENE,
    RAID_CAMPING,
    HOME,   //家园
};

class GameStateManager : SingletonObject<GameStateManager>
{
    GAMESTATE m_GameState = GAMESTATE.INIT;
    public GAMESTATE GameState
    {
        get
        {
            return m_GameState;
        }
        set
        {
            m_GameState = value;
            Debug.LogWarning("GameState = " + value);
        }
    }

    float BaseTime = Time.realtimeSinceStartup;
    public void Update()
    {

        switch (GameState)
        {
            case GAMESTATE.RAID_EDITOR:
                {
                    //RaidEditor.GetInst().LateUpdate();
                }
                break;
            case GAMESTATE.RAID_PLAYING:
                {
                    RaidManager.GetInst().Update();
                }
                break;
            case GAMESTATE.RAID_COMBAT:
                {
                    //CombatManager.GetInst().Update();
                }
                break;
            case GAMESTATE.RAID_CUTSCENE:
                {
                    CutSceneManager.GetInst().Update();
                }
                break;
            case GAMESTATE.RAID_CAMPING:
                {
                    CampManager.GetInst().Update();
                }
                break;
            case GAMESTATE.HOME:
                {
                    HomeManager.GetInst().Update();
                    WorldMapManager.GetInst().Update();
                }
                break;
        }

        if (GameState > GAMESTATE.RAID_EDITOR)
        {

            UIRefreshManager.GetInst().OnUpdate();

            if (Time.realtimeSinceStartup - BaseTime < 1.0f)
            {
                return;
            }
            else
            {
                PetManager.GetInst().Update();
                WorldMapManager.GetInst().RefreshRestTime();
                HomeManager.GetInst().HotelRefreshTime();

                BaseTime = Time.realtimeSinceStartup;
            }
        }
    }


    float m_fHeartBeat = 0;
    IEnumerator HeartBeatUpdate()
    {
        while (true)
        {
            if (Time.realtimeSinceStartup - m_fHeartBeat > 5f)
            {
                if (!NetworkManager.GetInst().pipe.Connected)
                    yield break;

                NetworkManager.GetInst().SendMsgToServer(new Message.CSNetMsgMBT());
                m_fHeartBeat = Time.realtimeSinceStartup;
            }
            yield return null;
        }
    }

    public void TryLogin()
    {
        GameState = GAMESTATE.LOGIN;
        LoginManager.GetInst().TryLogin();
        AppMain.GetInst().StartCoroutine(HeartBeatUpdate());
    }

    public void TryConnect()
    {
        TryConnect(LoginManager.GetInst().IP);
    }
    public void TryConnect(string ip)
    {
        //NetworkManager.GetInst().SendPasswordLogin(LoginFake.GetInst().IP, LoginFake.GetInst().Port);
        if (NetworkManager.GetInst().ConnectToServer(ip, LoginManager.GetInst().Port))
        {
            TryLogin();
        }
    }

    public bool IsGameInRaid()
    {
        if (GameState >= GAMESTATE.RAID_PLAYING && GameState < GAMESTATE.HOME)
        {
            return true;
        }
        return false;
    }

    public bool IsGameCanScanPath()
    {
        if (GameState >= GAMESTATE.RAID_PLAYING && GameState <= GAMESTATE.HOME)
        {
            return true;
        }
        return false;
    }

    public int LoginScene = 1;

    public void EnterGame()
    {
        switch (LoginScene)
        {
            case 0:
                //HomeManager.GetInst().LoadHome();
                break;
            case 1:
                RaidManager.GetInst().SetupRaid(0, 1001001, 1, "20001&0&0&0&0&0&0&0");
                //NetworkManager.GetInst().SendMsgToServer(new CSMsgLoadUnfinishRaid());
                break;
            case 2:
                break;
        }
    }

}

public enum DayTime
{
    Dusk = 0,       //黄昏
    Night = 1,      //夜晚
    Day = 2,        //白天
    NO_LIGHT = 3,   //无光（家园只用前三个）
    MidNight = 4,
    LateNight = 5,
    DeepNight = 6,
}
