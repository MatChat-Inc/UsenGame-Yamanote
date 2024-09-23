// Created by LunarEclipse on 2024-6-21 1:53.

using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Luna.UI;
using Luna.UI.Navigation;
using Modules.UI.Misc;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using USEN.Games.Common;

namespace USEN.Games.Yamanote
{
    public class YamanoteHomeView : Widget
    {
        public Button startButton;
        public Button settingsButton;
        public Image trainImage;
        public BottomPanel bottomPanel;
        
        public TextAsset categoriesJson;
        
        private YamanoteDAO _dao;
        private List<YamanoteCategory> _categories;
        
        private void Start()
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            bottomPanel.exitButton.onClick.AddListener(OnExitButtonClicked);
            
            _dao = new();
            if (_dao.IsEmpty())
                _dao.InsertFromJsonList(categoriesJson.text);
            _categories = _dao.GetCategories();
            
            EventSystem.current.SetSelectedGameObject(startButton.gameObject);
        }

        private void OnEnable()
        {
            // Train tween animation
            trainImage.transform.DOLocalMoveX(-5000, 1.5f).SetEase(Ease.OutSine);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetButtonDown("Cancel")) 
                OnExitButtonClicked();
        }

        public void OnStartButtonClicked()
        {
            var displayMode = YamanotePreferences.DisplayMode;
            if (displayMode == YamanoteDisplayMode.Random)
                PlayRandomGame();
            else Navigator.Push<YamanoteCategoryView>((view) => view.Categories = _categories);
        }
        
        public void OnSettingsButtonClicked()
        {
            Navigator.Push<YamanoteSettingsView>();
        }
        
        private void OnExitButtonClicked()
        {
            SceneManager.LoadScene("GameEntries");
        }
        
        public void PlayRandomGame()
        {
            var questions = _dao.GetQuestions().Shuffle().ToList();
            Navigator.Push<YamanoteGameView>((view) => view.Questions = questions);
        }
    }
}