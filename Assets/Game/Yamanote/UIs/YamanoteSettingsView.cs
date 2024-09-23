// Created by LunarEclipse on 2024-7-18 9:26.

using Luna.UI;
using Luna.UI.Navigation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using USEN.Games.Common;
using ToggleGroup = Modules.UI.ToggleGroup;

namespace USEN.Games.Yamanote
{
    public class YamanoteSettingsView: Widget
    {
        public Slider questionDisplaySettingSlider;
        public ToggleGroup questionDisplaySettingToggles;
        public Button appInfoButton;
        public BottomPanel bottomPanel;

        private void Start()
        {
            var selectedIndex = (int) YamanotePreferences.DisplayMode;
            questionDisplaySettingToggles.ToggleOn(selectedIndex);
            questionDisplaySettingToggles.Bind(questionDisplaySettingSlider);
            
            questionDisplaySettingSlider.onValueChanged.AddListener((value) => {
                var index = Mathf.RoundToInt(value);
                questionDisplaySettingToggles.ToggleOn(index);
                YamanotePreferences.DisplayMode = (YamanoteDisplayMode) index;
            });
            
            appInfoButton.onClick.AddListener(OnClickAppInfoButton);
        }

        private void OnEnable()
        {
            EventSystem.current.SetSelectedGameObject(questionDisplaySettingSlider.gameObject);
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetButtonDown("Cancel")) {
                Navigator.Pop();
            }
        }
        
        void OnClickAppInfoButton() {
            Navigator.Push<AppInfoView>();
        }
    }
}