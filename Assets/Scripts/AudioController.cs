using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioController : MonoBehaviour
{
    public AudioMixer soundsMixer;
    public AudioMixer musicMixer;
    public Toggle toggleMusic;
    public Toggle toggleSound;
    public Toggle toggleMusicPause;
    public Toggle toggleSoundPause;
    public int maxShotSounds; // Максимальное количество звуков выстрела одновременно
    [HideInInspector]
    public List<AudioSource> soundsList = new List<AudioSource>();

    private int soundsKey;
    private int musicKey;

    void Start()
    {
        SetAudio();
    }

    private void SetAudio()
    {
        soundsKey = PlayerPrefs.GetInt("soundsKey", 1);
        SetSounds();
        musicKey = PlayerPrefs.GetInt("musicKey", 1);
        SetMusic();
    }

    public void PlaySound(AudioSource sound)
    {
        int soundsCount = 0;

        foreach(AudioSource i in soundsList)
        {
            if (sound.clip == i.clip)
            {
                soundsCount++;
            }
        }
        if(soundsCount <= maxShotSounds)
        {
            soundsList.Add(sound);
            sound.Play();
            StartCoroutine(FinishPlaySound(sound));
        }
        
    }

    IEnumerator FinishPlaySound(AudioSource sound)
    {
        yield return new WaitForSeconds(sound.clip.length);
        soundsList.Remove(sound);
    }

    public void SetSounds()
    {
        if (soundsKey == 1)
        {
            soundsMixer.SetFloat("soundsVolume", 0f); // Включаем все звуки
            toggleSound.isOn = true;
            toggleSoundPause.isOn = true;
        }
        else
        {
            soundsMixer.SetFloat("soundsVolume", -80f);  // Выключаем все звуки
            toggleSound.isOn = false;
            toggleSoundPause.isOn = false;
        }
    }

    public void SetMusic()
    {
        if (musicKey == 1)
        {
            musicMixer.SetFloat("musicVolume", 0f); // Включаем всю музыку
            toggleMusic.isOn = true;
            toggleMusicPause.isOn = true;
        }
        else
        {
            musicMixer.SetFloat("musicVolume", -80f);  // Выключаем всю музыку
            toggleMusic.isOn = false;
            toggleMusicPause.isOn = false;
        }
    }

    public void ToggleSounds()
    {

        if (soundsKey == 1)
        {
            soundsMixer.SetFloat("soundsVolume", -80f);  // Выключаем все звуки
            toggleSound.isOn = false;
            toggleSoundPause.isOn = false;
            soundsKey = 0;
        }
        else
        {
            soundsMixer.SetFloat("soundsVolume", 0f); // Включаем все звуки
            toggleSound.isOn = true;
            toggleSoundPause.isOn = true;
            soundsKey = 1;
        }
        PlayerPrefs.SetInt("soundsKey", soundsKey);
    }

    public void ToggleMusic()
    {
        if (musicKey == 1)
        {
            musicMixer.SetFloat("musicVolume", -80f);  // Выключаем всю музыку
            toggleMusic.isOn = false;
            toggleMusicPause.isOn = false;
            musicKey = 0;
        }
        else
        {
            musicMixer.SetFloat("musicVolume", 0f); // Включаем всю музыку
            toggleMusic.isOn = true;
            toggleMusicPause.isOn = true;
            musicKey = 1;
        }
        PlayerPrefs.SetInt("musicKey", musicKey);
    }

    void OnApplicationPause(bool pauseStatus)
    {
        Debug.Log("OnApplicationPause = " + pauseStatus);

        if (!pauseStatus)
        {
            SetAudio();
        }
    }
}
