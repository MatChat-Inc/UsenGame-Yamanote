// Created by LunarEclipse on 2024-7-18 9:26.

using System;
using Luna;
using Luna.UI;
using Luna.UI.Navigation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using USEN.Games.Common;
using USEN.Games.Roulette;
using ToggleGroup = Modules.UI.ToggleGroup;

namespace USEN.Games.Yamanote
{
    public class YamanoteSettingsView: Widget
    {
        public ToggleGroup questionDisplaySettingsToggles;
        public Slider questionDisplaySettingsSlider;
        public ToggleGroup rouletteDisplaySettingsToggles;
        public Slider rouletteDisplaySettingsSlider;
        public ToggleGroup commendationVideoSettingsToggles;
        public Slider commendationVideoSettingsSlider;
        public Slider bgmVolumeSlider;
        public Slider sfxVolumeSlider;
        public Text bgmVolumeText;
        public Text sfxVolumeText;
        public Button appInfoButton;
        public BottomPanel bottomPanel;

        private void Start()
        {
            // Current display setting
            var yamanoteDisplayIndex = (int) YamanotePreferences.DisplayMode;
            questionDisplaySettingsSlider.maxValue = questionDisplaySettingsToggles.Toggles.Count - 1;
            questionDisplaySettingsSlider.value = yamanoteDisplayIndex;
            questionDisplaySettingsSlider.onValueChanged.AddListener(OnYamanoteDisplaySettingsSliderValueChanged);
            
            questionDisplaySettingsToggles.ToggleOn(yamanoteDisplayIndex);
            questionDisplaySettingsToggles.Bind(questionDisplaySettingsSlider);
            
            // Roulette display setting
            
            var rouletteDisplayIndex = (int) RoulettePreferences.DisplayMode;
            rouletteDisplaySettingsSlider.maxValue = rouletteDisplaySettingsToggles.Toggles.Count - 1;
            rouletteDisplaySettingsSlider.value = rouletteDisplayIndex;
            rouletteDisplaySettingsSlider.onValueChanged.AddListener(OnRouletteDisplaySettingsSliderValueChanged);
            
            rouletteDisplaySettingsToggles.ToggleOn(rouletteDisplayIndex);
            rouletteDisplaySettingsToggles.Bind(rouletteDisplaySettingsSlider);
            
            // Commendation video setting
            commendationVideoSettingsSlider.onValueChanged.AddListener(OnCommendationVideoSettingsSliderValueChanged);
            commendationVideoSettingsSlider.value = YamanotePreferences.CommendationVideoOption;
            commendationVideoSettingsToggles.ToggleOn(YamanotePreferences.CommendationVideoOption);
            
            // Audio volume
            bgmVolumeText.text = $"{YamanotePreferences.BgmVolume * 100:0}";
            bgmVolumeSlider.value = BgmManager.Volume * 10;
            bgmVolumeSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
            
            sfxVolumeText.text = $"{YamanotePreferences.SfxVolume * 100:0}";
            sfxVolumeSlider.value = SFXManager.Volume * 10;
            sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            
            // App info
            appInfoButton.onClick.AddListener(OnClickAppInfoButton);
            
            // Bottom panel
            bottomPanel.exitButton.onClick.AddListener(() => Navigator.Pop());
            
            EventSystem.current.SetSelectedGameObject(questionDisplaySettingsSlider.gameObject);
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetButtonDown("Cancel")) 
                Navigator.Pop();
        }
        
        private void OnYamanoteDisplaySettingsSliderValueChanged(float arg0)
        {
            var index = Convert.ToInt32(arg0);
            // questionDisplaySettingsToggles.ToggleOn(Convert.ToInt32(index));
            YamanotePreferences.DisplayMode = (YamanoteDisplayMode) index;
        }
        
        private void OnRouletteDisplaySettingsSliderValueChanged(float arg0)
        {
            var index = Convert.ToInt32(arg0);
            // basicDisplaySettingsToggles.ToggleOn(Convert.ToInt32(index));
            RoulettePreferences.DisplayMode = (RouletteDisplayMode) index;
            API.UpdateRandomSetting(RoulettePreferences.DisplayMode == RouletteDisplayMode.Random);
        }
        
        private void OnCommendationVideoSettingsSliderValueChanged(float arg0)
        {
            YamanotePreferences.CommendationVideoOption = Convert.ToInt32(arg0);
        }
        
        private void OnBgmVolumeChanged(float value)
        {
            BgmManager.SetVolume(value * 0.1f);
            YamanotePreferences.BgmVolume = value * 0.1f;
            bgmVolumeText.text = $"{value * 10:0}";
        }
        
        private void OnSfxVolumeChanged(float value)
        {
            SFXManager.SetVolume(value * 0.1f);
            YamanotePreferences.SfxVolume = value * 0.1f;
            sfxVolumeText.text = $"{value * 10:0}";
            SFXManager.Play(R.Audios.SfxBack);
        }
        
        private void OnClickAppInfoButton() 
        {
            Navigator.Push<AppInfoView>();
        }
    }
}