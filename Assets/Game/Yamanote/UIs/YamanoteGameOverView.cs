// Created by LunarEclipse on 2024-6-21 1:53.

using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Luna;
using Luna.Extensions;
using Luna.UI;
using Luna.UI.Navigation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;
using USEN.Games.Common;
using USEN.Games.Roulette;
using Random = UnityEngine.Random;

namespace USEN.Games.Yamanote
{
    public class YamanoteGameOverView : Widget
    {
        public Button startButton;
        public Button settingsButton;
        public Image trainImage;
        public BottomPanel bottomPanel;
        
        public PlayableDirector director;
        public TimelineAsset[] timelines;
        
        private bool _isPlayingAnimation = false;
        
        private void Start()
        {
            SFXManager.Play(R.Audios.SfxYamanoteGameOver);
            PlaySfx();
            
            startButton.onClick.AddListener(OnStartButtonClicked);
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        }

        private void OnEnable()
        {
            if (startButton != null)
            {
                EventSystem.current.SetSelectedGameObject(startButton.gameObject);
            }
            
            bottomPanel.onRedButtonClicked += OnRedButtonClicked;
            bottomPanel.onBlueButtonClicked += OnBlueButtonClicked;
            bottomPanel.onGreenButtonClicked += OnGreenButtonClicked;
            bottomPanel.onExitButtonClicked += OnExitButtonClicked;
            
            // Train tween animation
            trainImage.transform.DOLocalMoveX(-5000, 1.5f).SetEase(Ease.OutSine);
        }
        
        private void OnDisable()
        {
            bottomPanel.onRedButtonClicked -= OnRedButtonClicked;
            bottomPanel.onBlueButtonClicked -= OnBlueButtonClicked;
            bottomPanel.onGreenButtonClicked -= OnGreenButtonClicked;
            bottomPanel.onExitButtonClicked -= OnExitButtonClicked;
        }

        private void OnDestroy()
        {
            SFXManager.StopAll();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetButtonDown("Cancel")) 
                OnExitButtonClicked();
        }
        
        private void OnRedButtonClicked()
        {
            // もう一度同じお題で遊ぶ
            var questionsView = Navigator.BackTo<YamanoteGameView>();
            questionsView.ResetGame(resetQuestion: false);
        }

        private async void OnBlueButtonClicked()
        {            
            // 次のお題
            switch (YamanotePreferences.DisplayMode)
            {
                case YamanoteDisplayMode.Normal:
                    Navigator.PopUntil<YamanoteCategoryView>();
                    break;
                case YamanoteDisplayMode.Random:
                    var gameView = Navigator.BackTo<YamanoteGameView>();
                    await UniTask.NextFrame();
                    gameView.PickNextQuestion();
                    break;
            }

        }
        
        private async void OnGreenButtonClicked()
        {
            SFXManager.StopAll();
            
            await Navigator.Push<RouletteGameSelectionView>((view) => {
                view.Category = RouletteManager.Instance.GetCategory("バツゲーム");
                // BgmManager.Resume();
                R.Audios.BgmRouletteLoop.PlayAsBgm();
                
                if (RoulettePreferences.DisplayMode == RouletteDisplayMode.Random)
                { 
                    Navigator.Push<USEN.Games.Roulette.RouletteGameView>(async (view) => {
                        view.RouletteData = RouletteManager.Instance.GetRandomRoulette();
                    });
                }
            });
            
            BgmManager.Stop();
            
            await UniTask.NextFrame();
            PlaySfx();
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

        public void PlaySfx()
        {
            SFXManager.PlayRepeatedly(R.Audios.SfxYamanoteRain, (-6, -3));
            Task.Delay(Random.Range(3000, 5000)).Then(_ => {
                if (this != null && isActiveAndEnabled)
                    SFXManager.PlayRepeatedly(R.Audios.SfxYamanoteWind, (3, 10));
            });

            if (!_isPlayingAnimation)
                PlayLightningRepeatedly((5, 10));
        }
        
        public async void PlayLightningRepeatedly((float min, float max) interval)
        {
            var i = 0;

            _isPlayingAnimation = true;
            while (this != null && isActiveAndEnabled)
            {
                var delay = UnityEngine.Random.Range(interval.min, interval.max);
                var timeline = timelines[i++.Mod(2)];
                director.Play(timeline);
                await Task.Delay(TimeSpan.FromSeconds(delay));
            }
            _isPlayingAnimation = false;
        }
    }
}