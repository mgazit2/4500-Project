using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Photon.Pun;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviourPunCallbacks
{
    PhotonView PV;
    public GameObject pauseMenu;
    public AudioMixer AM;
    static bool pauseActive = false;

    public Dropdown resDropDown; // resolution drop down
    Resolution[] resolutions;
    void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    // Used mainly for fetching possible screen resolutions from user pc
    void Start()
    {
        resolutions = Screen.resolutions;

        List<string> resOptions = new List<string>();

        resDropDown.ClearOptions();

        int defaultResIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            resOptions.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                defaultResIndex = i;
            }
        }

        resDropDown.AddOptions(resOptions);
        resDropDown.value = defaultResIndex;
        resDropDown.RefreshShownValue();
    }

    void Update()
    {
        /*if (!PV.IsMine)
        {
            return;
        }*/

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseActive)
            {
                pauseMenu.SetActive(false);
                pauseActive = false;
            }
            else
            {
                pauseMenu.SetActive(true);
                pauseActive = true;
            }
            
        }
    }

    public void Resume()
    {
        if (pauseActive)
        {
            pauseMenu.SetActive(false);
            pauseActive = false;
        }
        else
        {
            pauseMenu.SetActive(true);
            pauseActive = true;
        }
    }

    public void QuitGame()
    {
        PhotonNetwork.LeaveRoom();
        /*Debug.Log("Game is exiting");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif*/
    }

    public void SetVolume(float vol)
    {
        AM.SetFloat("_volume", vol);
    }

    public void SetQuality(int quality)
    {
        QualitySettings.SetQualityLevel(quality);
    }

    public void SetFullscreen(bool full)
    {
        Screen.fullScreen = full;
        Debug.Log("Screen size changed");
    }

    public void SetRes(int resIndex)
    {
        Resolution resolution = resolutions[resIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public override void OnLeftRoom()
    {
        
        PhotonNetwork.LoadLevel(0);
    }
}
