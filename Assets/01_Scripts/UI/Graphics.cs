using TMPro;
using UnityEngine;

public class Graphics : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown screenRezolutionDropDown;
    private int _rezolutionX = 1920;
    private int _rezolutionY = 1080;
    private Resolution[] resolutions;
    private FullScreenMode _modeScreen;
    private void Awake()
    {
        _modeScreen = Screen.fullScreenMode;
        AutomaticScreenResolution();
    }
    #region Helpers
    void AutomaticScreenResolution()
    {
        _rezolutionX = Screen.width;
        _rezolutionY = Screen.height;
        resolutions = Screen.resolutions;
        screenRezolutionDropDown.ClearOptions();

        int currentIndex = 0;
        var options = new System.Collections.Generic.List<string>();

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = $"{resolutions[i].width}x{resolutions[i].height} {resolutions[i].refreshRateRatio.value}Hz";
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height &&
                resolutions[i].refreshRateRatio.Equals(Screen.currentResolution.refreshRateRatio))
            {
                currentIndex = i;
            }
        }

        screenRezolutionDropDown.AddOptions(options);
        screenRezolutionDropDown.value = currentIndex;
        screenRezolutionDropDown.RefreshShownValue();
    }

    
    #endregion

    #region Public functions
    public void SETT_ResolutionSetting(int index)
    {
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreenMode, res.refreshRateRatio);
        /* switch (index)
         {
             default: AutomaticScreenResolution(); break;
             case 1: _rezolutionX = 1920; _rezolutionY = 1080; break;
             case 2: _rezolutionX = 1366; _rezolutionY = 768; break;
             case 3: _rezolutionX = 1536; _rezolutionY = 864; break;
             case 4: _rezolutionX = 1280; _rezolutionY = 720; break;
             case 5: _rezolutionX = 1440; _rezolutionY = 900; break;
         }
         Screen.SetResolution(_rezolutionX, _rezolutionY, _modeScreen);*/

    }
    public void SETT_DisplayModeSetting(int index)
    {
        switch (index)
        {
            default:
                _modeScreen = FullScreenMode.ExclusiveFullScreen; break;
            case 1:
                _modeScreen = FullScreenMode.FullScreenWindow; break;
            case 2:
                _modeScreen = FullScreenMode.Windowed; break;
        }
        Screen.fullScreenMode = _modeScreen;
    }
    public void SETT_FPS( float index)
    {
        if ((int)index < 25)
            Application.targetFrameRate = -1; // Unlimited
        else
            Application.targetFrameRate = (int)index;
    }
    public void SETT_FOV(float value) // is a slider
    {
        if (Camera.main != null)
            Camera.main.fieldOfView = value;
    }


    public void SETT_TextureQuality(int index)// High medium low
    {
        switch (index)
        {
            default:QualitySettings.globalTextureMipmapLimit = 0;break; // 1/1
            case 1: QualitySettings.globalTextureMipmapLimit = 1;break; // 1/2
            case 2: QualitySettings.globalTextureMipmapLimit = 2;break; // 1/4
            case 3: QualitySettings.globalTextureMipmapLimit = 3;break; // 1/8
            case 4: QualitySettings.globalTextureMipmapLimit = 4;break; // 1/16
        }
    }
    public void SETT_ShadowQuality(int index)
    {
        switch (index)
        {
            case 0: QualitySettings.shadows = ShadowQuality.Disable; break;
            case 1: QualitySettings.shadows = ShadowQuality.HardOnly; break;
            case 2: QualitySettings.shadows = ShadowQuality.All; break;
        }
    }
    public void SETT_AntiAliasing(int index)
    {
        //Off
        //FXAA (Fast Approximate Anti-Aliasing) low
        //Anti allising MSAA (Multisample Anti-Aliasing) med MSAA
        //TAA (Temporal Anti-Aliasing) high
        // QualitySettings.antiAliasing 0, 2, 4, 8
        switch (index)
        {
            case 0: QualitySettings.antiAliasing = 0; break;
            case 1: QualitySettings.antiAliasing = 2; break;
            case 2: QualitySettings.antiAliasing = 4; break;
            case 3: QualitySettings.antiAliasing = 8; break;
        }

    }
    public void SETT_VSync(int index)
    {
        if (index == 1)
        {
            QualitySettings.vSyncCount = 1;
            Debug.Log("VSync On");
        }
        else
        {
            QualitySettings.vSyncCount = 0;
            Debug.Log("VSync Off");
        }
    }
    void SETT_AmbientOcclusion() // off or medium
    {

    }
    void SETT_MotionBlurr() //On/ off
    {

    }
    void SETT_Bloom()//On/ off
    {

    }
    void SETT_FilmGrain()//On/ off
    {

    }
    void SETT_CromaticAberration()//On/ off
    {

    }
    #endregion
}
