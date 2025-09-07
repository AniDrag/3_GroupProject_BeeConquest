using System;
using UnityEngine;

public class Graphics : MonoBehaviour
{

    int _rezolutionX = 1920;
    int _rezolutionY = 1080;
    Resolution[] resolutions;

    //bool _fullScreen = false;
    //int _fps = 60;
    FullScreenMode _moceScreen;
    private void Awake()
    {
        resolutions = Screen.resolutions;
        _moceScreen = Screen.fullScreenMode;
    }
    #region Other math 
    void AutomaticSceenRezolution()
    {
        _rezolutionX = Screen.width;
        _rezolutionY = Screen.height;
    }

    public void SetFOV(float value)
    {
        Camera.main.fieldOfView = value;
    }

    #endregion

    #region Public functions
    public void SETT_ResolutionSetting(int index)
    {
        switch (index)
        {
            default:
                AutomaticSceenRezolution();
                break;
            case 1:
                _rezolutionX = 1920;
                _rezolutionX = 1080;
                break;
            case 2:
                _rezolutionX = 1366;
                _rezolutionX = 768;
                break;
            case 3:
                _rezolutionX = 1536;
                _rezolutionX = 864;
                break;
            case 4:
                _rezolutionX = 1280;
                _rezolutionX = 720;
                break;
            case 5:
                _rezolutionX = 1440;
                _rezolutionX = 900;
                break;
        }
        Screen.SetResolution(_rezolutionX, _rezolutionY, _moceScreen);
        
    }
    public void SETT_DisplayModeSetting(int index)
    {
        switch (index)
        {
            default:
                _moceScreen = FullScreenMode.ExclusiveFullScreen; break;
            case 1:
                _moceScreen = FullScreenMode.FullScreenWindow; break;
            case 2:
                _moceScreen = FullScreenMode.Windowed; break;
        }
        Screen.fullScreenMode = _moceScreen;
    }
    public void SETT_FPS( float index)
    {
        if ((int)index < 25)
            Application.targetFrameRate = -1; // Unlimited
        else
            Application.targetFrameRate = (int)index; // set to frame rate
    }
    void SETT_FOV()
    {

    }

    void SETT_TextureQuality()
    {

    }
    public void SETT_ShadowQuality(int index)
    {
        // Example: Low = disable shadows, Medium = hard shadows, High = soft shadows
        switch (index)
        {
            case 0:
                QualitySettings.shadows = ShadowQuality.Disable; break;
            case 1:
                QualitySettings.shadows = ShadowQuality.HardOnly; break;
            case 2:
                QualitySettings.shadows = ShadowQuality.All; break;
        }
    }
    void SETT_AntiAliasing()
    {
        //FXAA (Fast Approximate Anti-Aliasing) low
        //Anti allising MSAA (Multisample Anti-Aliasing) med
        //TAA (Temporal Anti-Aliasing) high

    }
    void SETT_AmbientOcclusion()
    {

    }
    public void SETT_VSync(int index)
    {
        if (index == 1)
        {
            QualitySettings.vSyncCount = 1;
            Debug.Log("V sync on");
        }
        else
        {
            QualitySettings.vSyncCount = 0;
            Debug.Log("V sync off");
        }

    }
    void SETT_MotionBlurr()
    {

    }
    void SETT_Bloom()
    {

    }
    void SETT_FilmGrain()
    {

    }
    void SETT_CromaticAberration()
    {

    }
    #endregion
}
