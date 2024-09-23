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
    }
    
    public enum YamanoteDisplayMode
    {
        Normal,
        Random,
    }
}