// Created by LunarEclipse on 2024-6-21 1:53.

using DG.Tweening;
using Luna.UI;
using Luna.UI.Navigation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace USEN.Games.Yamanote
{
    public class YamanoteGameOverView : Widget
    {
        public Button startButton;
        public Button settingsButton;
        public Image trainImage;
        
        private void Start()
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        }

        private void OnEnable()
        {
            if (startButton != null)
            {
                EventSystem.current.SetSelectedGameObject(startButton.gameObject);
            }
            
            // Train tween animation
            trainImage.transform.DOLocalMoveX(-5000, 1.5f).SetEase(Ease.OutSine);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetButtonDown("Cancel")) 
                OnExitButtonClicked();
        }

        private void OnExitButtonClicked()
        {
            Navigator.PopToRoot();
        }

        public void OnStartButtonClicked()
        {
            Navigator.Push<YamanoteCategoryView>();
        }
        
        public void OnSettingsButtonClicked()
        {
            Navigator.Push<YamanoteSettingsView>();
        }
    }
}