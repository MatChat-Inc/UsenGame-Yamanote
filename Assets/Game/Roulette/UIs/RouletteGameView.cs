// Created by LunarEclipse on 2024-6-30 18:50.

using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Luna;
using Luna.UI;
using Luna.UI.Navigation;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using USEN.Games.Common;
using Random = UnityEngine.Random;

namespace USEN.Games.Roulette
{
    public partial class RouletteGameView : Widget
    {
        public RouletteWheel rouletteWheel;
        public ParticleSystem rouletteParticle;
        public Button startButton;
        public Image backgroundImage;
        public BottomPanel bottomPanel;
        public TextMeshProUGUI confirmText;

        private AsyncOperationHandle<AudioClip>? _audioClipHandle;
        
        private bool _isStopping = false;
        private Vector3 _originalPosition;
        private Vector3 _originalScale;

        public RouletteData RouletteData
        {
            get => rouletteWheel.RouletteData;
            set => rouletteWheel.RouletteData = value;
        }

        void OnEnable()
        {
            base.OnKey += OnKeyEvent;
            startButton.onClick.AddListener(OnStartButtonClicked);
            
            rouletteWheel.OnSpinStart += OnSpinStart;
            rouletteWheel.OnSpinEnd += OnSpinEnd;
            
            bottomPanel.onExitButtonClicked += OnExitButtonClicked;
            bottomPanel.onSelectButtonClicked += OnStartButtonClicked;
            bottomPanel.onConfirmButtonClicked += OnConfirmButtonClicked;
            bottomPanel.onRedButtonClicked += OnRedButtonClicked;
            bottomPanel.onBlueButtonClicked += OnBlueButtonClicked;
            bottomPanel.onYellowButtonClicked += OnYellowButtonClicked;
        }

        void OnDisable()
        {
            base.OnKey -= OnKeyEvent;
            startButton.onClick.RemoveListener(OnStartButtonClicked);
            
            rouletteWheel.OnSpinStart -= OnSpinStart;
            rouletteWheel.OnSpinEnd -= OnSpinEnd;
            
            bottomPanel.onExitButtonClicked -= OnExitButtonClicked;
            bottomPanel.onSelectButtonClicked -= OnStartButtonClicked;
            bottomPanel.onConfirmButtonClicked -= OnConfirmButtonClicked;
            bottomPanel.onRedButtonClicked -= OnRedButtonClicked;
            bottomPanel.onBlueButtonClicked -= OnBlueButtonClicked;
            bottomPanel.onYellowButtonClicked -= OnYellowButtonClicked;
        }

        private void Start()
        {
            _originalPosition = rouletteWheel.transform.parent.localPosition;
            _originalScale = rouletteWheel.transform.parent.localScale;
            
            // AssetUtils.LoadAsync<CommendView>().ContinueWith(task =>
            // {
            //     var go = task.Result;
            //     var commendView = go.GetComponent<CommendView>();
            //     if (commendView != null)
            //         _audioClipHandle = commendView.PreloadAudio();
            // }, TaskScheduler.FromCurrentSynchronizationContext());
            
#if !USEN_ROULETTE
            bottomPanel.redButton.gameObject.SetActive(false);
#endif
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetButtonDown("Cancel"))
            {
                OnExitButtonClicked();
            }
        }

        private void OnDestroy()
        {
            SFXManager.StopAll();
            
            // AssetUtils.Unload<CommendView>();
            // if (_audioClipHandle != null)
            //     Addressables.Release(_audioClipHandle.Value);
        }

        private KeyEventResult OnKeyEvent(KeyControl key, KeyEvent @event)
        {
            if (@event == KeyEvent.Down)
            {
                switch (key.keyCode)
                {
                    case Key.Enter:
                    case Key.Space:
                        OnStartButtonClicked();
                        break;
                }
            }

            return KeyEventResult.Unhandled;
        }

        private void OnStartButtonClicked()
        {
            if (rouletteWheel.IsSpinning)
                StopWheel();
            else SpinWheel();
        }

        private void OnConfirmButtonClicked()
        {
            if (rouletteWheel.IsSpinning)
                StopWheel();
            else SpinWheel();
        }

        private void OnExitButtonClicked()
        {
            PopupConfirmView();
        }

        private async void OnBlueButtonClicked()
        {
            switch (RoulettePreferences.DisplayMode)
            {
                case RouletteDisplayMode.Normal:
                    Navigator.Pop(RouletteData);
                    break;
                case RouletteDisplayMode.Random:
                    ResetRoulette();
                    RouletteData roulette;
                    do { 
                        roulette = RouletteManager.Instance.GetRandomRoulette(RouletteData.Category);
                    } while (roulette == RouletteData);
                    RouletteData = roulette;
                    break;
            }
        }

        private void OnRedButtonClicked()
        {
            SFXManager.Stop(R.Audios.SfxConfirm);
            SFXManager.Stop(R.Audios.SfxRouletteGameRotating);
            Navigator.PopUntil<RouletteCategoryView, RouletteData>(RouletteData);
        }

        private async void OnYellowButtonClicked()
        {
            BgmManager.Pause();
            await Navigator.Push<CommendView>();
            BgmManager.Resume();
        }
        
        private void OnSpinStart()
        {
            confirmText.text = "停止";
            
            // Hide bottom buttons
            bottomPanel.yellowButton.gameObject.SetActive(false);
            bottomPanel.blueButton.gameObject.SetActive(false);
#if USEN_ROULETTE
            bottomPanel.redButton.gameObject.SetActive(false);
#endif
        }
        
        private async void OnSpinEnd(int index, string obj)
        {
            confirmText.text = "もう一度ルーレットを回す";
            
            // Show bottom buttons
            bottomPanel.confirmButton.gameObject.SetActive(true);
            bottomPanel.blueButton.gameObject.SetActive(true);
#if USEN_ROULETTE
            bottomPanel.yellowButton.gameObject.SetActive(true);
            bottomPanel.redButton.gameObject.SetActive(true);      
#endif
            
            _isStopping = false;
            
            // Play sfx
            rouletteParticle.gameObject.SetActive(true);
            
            var sector = rouletteWheel.GetSector(index);
            var border = sector.transform.Find("Border").gameObject;
            var lineRenderer = border.GetComponent<LineRenderer>();
            border.SetActive(true);

            lineRenderer.material.SetColor("_Color", Color.clear);
            var tween = DOTween.To(
                () => lineRenderer.material.color,
                color => lineRenderer.material.SetColor("_Color", color), 
                new Color(1, 1, 1, 1), 
                0.5f); 
            
            await Task.Delay(2000);
            DOTween.To(
                () => lineRenderer.material.color,
                color => lineRenderer.material.SetColor("_Color", color), 
                Color.clear, 
                1f);
            
            rouletteParticle.gameObject.SetActive(false);
        }

        private async Task SpinWheel()
        {
            Debug.Log("Start button clicked.");

            // Hide buttons
            if (startButton.gameObject.activeSelf)
            {
                startButton.gameObject.SetActive(false);
                SFXManager.Play(R.Audios.SfxConfirm);
            }

            // Spin the wheel
            // rouletteWheel.SpinWheel();
            rouletteWheel.StartSpin();

            // Dotween move & scale
            // await UniTask.Delay((int)((rouletteWheel.spinDuration - 2) * 1000));
            // rouletteWheel.transform.parent.DOLocalMoveX(960, 1f).SetEase(Ease.InOutSine);
            // rouletteWheel.transform.parent.DOScale(3f, 1f).SetEase(Ease.InOutSine);
            
            // Play sfx
            SFXManager.PlayRepeatedly(R.Audios.SfxRouletteGameRotating);
        }
        
        private async Task StopWheel()
        {
            Debug.Log("Stop button clicked.");
            
            if (_isStopping) return;
            _isStopping = true;

            // Stop the wheel
            rouletteWheel.StopSpin();

            // Dotween move & scale
            // await UniTask.Delay(1000);
            rouletteWheel.transform.parent.DOLocalMoveX(960, 1f).SetEase(Ease.InOutSine);
            rouletteWheel.transform.parent.DOScale(3f, 1f).SetEase(Ease.InOutSine);
            
            // Stop sfx and play another sfx
            SFXManager.Stop(R.Audios.SfxRouletteGameRotating);
            SFXManager.Play(R.Audios.SfxRouletteGameDecelerating);
            
            bottomPanel.confirmButton.gameObject.SetActive(false);
        }
        
#if USEN_ROULETTE
        private void PopupConfirmView()
        {
            Navigator.ShowModal<PopupOptionsView>(
                builder: (popup) =>
                {
                    popup.onOption1 = () => Navigator.Pop(RouletteData);
                    popup.onOption2 = () =>
                    {
                        SFXManager.Stop();
                        Navigator.PopUntil<RouletteStartView, RouletteData>(RouletteData);
                    }; 
#if UNITY_ANDROID
                    popup.onOption3 = () => Android.Back();
#else
                    popup.onOption3 = () => Application.Quit();
#endif
                });
        }
#endif
        
        private void ResetRoulette()
        {
            rouletteWheel.transform.parent.localPosition = _originalPosition;
            rouletteWheel.transform.parent.localScale = _originalScale;
        }
    }
}