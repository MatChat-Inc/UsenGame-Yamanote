// Created by LunarEclipse on 2024-6-30 18:50.

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

namespace USEN.Games.Roulette
{
    public class RouletteGameView : Widget
    {
        public RouletteWheel rouletteWheel;
        public Button startButton;
        public Image backgroundImage;
        public BottomPanel bottomPanel;
        public TextMeshProUGUI confirmText;

        private AsyncOperationHandle<AudioClip>? _audioClipHandle;
        
        private bool _isStopping = false;

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
            AssetUtils.LoadAsync<CommendView>().ContinueWith(task =>
            {
                var go = task.Result;
                var commendView = go.GetComponent<CommendView>();
                if (commendView != null)
                    _audioClipHandle = commendView.PreloadAudio();
            }, TaskScheduler.FromCurrentSynchronizationContext());
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
            
            AssetUtils.Unload<CommendView>();
            if (_audioClipHandle != null)
                Addressables.Release(_audioClipHandle.Value);
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
            Navigator.Pop();
            
            if (Navigator.Instance.TopWidget is RouletteCategoryView categoryView)
            {
                await UniTask.NextFrame();
                categoryView.GotoRandomCategory();
            }
        }

        private void OnRedButtonClicked()
        {
            Navigator.PopUntil<RouletteCategoryView>();
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
            
            // Hide yellow button
            bottomPanel.yellowButton.gameObject.SetActive(false);
        }
        
        private void OnSpinEnd(string obj)
        {
            confirmText.text = "もう一度ルーレットを回す";
            
            // Show yellow button
            bottomPanel.yellowButton.gameObject.SetActive(true);
            
            _isStopping = false;
        }

        private async Task SpinWheel()
        {
            Debug.Log("Start button clicked.");

            // Hide buttons
            if (startButton.gameObject.activeSelf)
            {
                startButton.gameObject.SetActive(false);
                SFXManager.Play(R.Audios.SfxRouletteConfirm);
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
        }

        private void PopupConfirmView()
        {
            Navigator.ShowModal<PopupOptionsView>(
                builder: (popup) =>
                {
                    popup.onOption1 = () => Navigator.Pop();
                    popup.onOption2 = () => Navigator.PopToRoot(); //Navigator.PopUntil<RouletteStartView>();
                    popup.onOption3 = () => SceneManager.LoadScene("GameEntries");
                });
        }
    }
}