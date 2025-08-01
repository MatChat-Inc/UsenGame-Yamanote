// Created by LunarEclipse on 2024-6-21 1:53.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Luna;
using Luna.Extensions;
using Luna.UI;
using Luna.UI.Navigation;
using Modules.UI.Misc;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using USEN.Games.Common;
using USEN.Games.Roulette;

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

        private void Awake()
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            bottomPanel.exitButton.onClick.AddListener(OnExitButtonClicked);
            
            EventSystem.current.SetSelectedGameObject(startButton.gameObject);
        }

        private async void Start()
        {
            UsenEvents.OnRemoconHomeButtonClicked += OnHomeButtonClicked;
            
            // Load data
            _dao = YamanoteDAO.Instance;
            _dao.UpdateTable(categoriesJson.text);
            _categories = _dao.GetCategories();
            
            // Audio volume
            BgmManager.Volume = YamanotePreferences.BgmVolume;
            SFXManager.Volume = YamanotePreferences.SfxVolume;
            
            // Show loading indicator before necessary assets are loaded
            await UniTask.Yield(PlayerLoopTiming.PreLateUpdate);
            Navigator.ShowModal<RoundedCircularLoadingIndicator>();
            
            // Load audios
            // R.Audios.BgmYamanote.Load().Then(BgmManager.Play);
            var clip = await R.Audios.BgmYamanote.Load();
            BgmManager.Play(clip);
            
            // Load audios
            await Assets.Load("USEN.Games.Common", "Audio");
            await Assets.Load("USEN.Games.Roulette", "Audio");
            await Assets.Load(GetType().Namespace, "Audio");

            Navigator.PopToRoot();
        }

        private void OnEnable()
        {
            // Train tween animation
            trainImage.transform.DOLocalMoveX(-5000, 1.5f).SetEase(Ease.OutSine);
            
            // Network request
            RouletteManager.Instance.Sync();
            API.GetRandomSetting().ContinueWith(task => {
                RoulettePreferences.DisplayMode = (RouletteDisplayMode) task.Result.random;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetButtonDown("Cancel")) 
                OnExitButtonClicked();

#if DEBUG
            if (Input.GetKeyDown(KeyCode.F1))
            {
                
            }
#endif
        }

        public void OnStartButtonClicked()
        {
            Navigator.Push<YamanoteCategoryView>((view) => view.Categories = _categories);
            
            // if (YamanotePreferences.DisplayMode == YamanoteDisplayMode.Random)
            //     PlayRandomGame();
            // else Navigator.Push<YamanoteCategoryView>((view) => view.Categories = _categories);
        }
        
        public void OnSettingsButtonClicked()
        {
            Navigator.Push<YamanoteSettingsView>();
        }
        
        private void OnExitButtonClicked()
        {
// #if UNITY_ANDROID
//             Android.Back();
// #endif
            Application.Quit();
        }
        
        private void OnHomeButtonClicked(object sender, EventArgs e)
        {
            Application.Quit();
        }
        
        public void PlayRandomGame()
        {
            var questions = _dao.GetQuestions().Shuffle().ToList();
            Navigator.Push<YamanoteGameView>((view) => view.Questions = questions);
        }
    }
}