// Created by LunarEclipse on 2024-9-9 5:33.

using UnityEngine;

namespace USEN.Games.Yamanote
{
    public static class YamanotePreferences
    {
        public static YamanoteDisplayMode DisplayMode
        {
            get => (YamanoteDisplayMode) PlayerPrefs.GetInt("Yamanote.DisplayMode", 0);
            set => PlayerPrefs.SetInt("Yamanote.DisplayMode", (int) value);
        }
        
        public static int CommendationVideoOption
        {
            get => PlayerPrefs.GetInt("Yamanote.CommendationVideoOption", 0);
            set => PlayerPrefs.SetInt("Yamanote.CommendationVideoOption", value);
        }
        
        public static float BgmVolume
        {
            get => PlayerPrefs.GetFloat("Yamanote.BgmVolume", 1);
            set => PlayerPrefs.SetFloat("Yamanote.BgmVolume", value);
        }
        
        public static float SfxVolume
        {
            get => PlayerPrefs.GetFloat("Yamanote.SfxVolume", 1);
            set => PlayerPrefs.SetFloat("Yamanote.SfxVolume", value);
        }
    }
    
    public enum YamanoteDisplayMode
    {
        Normal,
        Random,
    }
}