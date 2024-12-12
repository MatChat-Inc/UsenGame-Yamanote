// Created by LunarEclipse on 2024-7-21 19:44.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Games.Yamanote;
using Luna;
using Luna.Extensions;
using Luna.Extensions.Unity;
using Luna.UI;
using Luna.UI.Navigation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using USEN.Games.Common;
using USEN.Games.Roulette;

namespace USEN.Games.Yamanote
{
    public class YamanoteGameView : Widget
    {
        public ImageShaderController cloudController;
        public ImageShaderController buildingsController;
        public Button startButton;
        public CanvasGroup questionsView;
        public YamanoteQuestionsPicker questionsPicker;
        public Image highlightMask;
        public ParticleSystem highlightParticles;
        public BottomPanel bottomPanel;
        
        public float questionInterval = 8;
        
        public bool pickingQuestionsAutomatically = true;
        
        public Sprite rouletteBackground;
        
        private PlayableDirector _accelerationDirector;
        private bool _isAccelerating;
        private bool _loopFlag = true;

        private int _counter = 0;
        
        private List<YamanoteQuestion> _questions;
        public List<YamanoteQuestion> Questions
        {
            get => _questions;
            set
            {
                _questions = value;
                questionsPicker.itemCount = Int32.MaxValue;
                questionsPicker.ItemBuilder = (index) =>
                {
                    return _questions[index.Mod(_questions.Count)].Content;
                };
            }
        }

        private void Awake()
        {
            _accelerationDirector = GetComponent<PlayableDirector>();
        }

        private void Start()
        {
            BgmManager.Play(R.Audios.BgmYamanoteGame);

            cloudController.speed = new Vector2(-0.05f, 0f);
            buildingsController.speed = new Vector2(-0.5f, 0f);
            startButton.onClick.AddListener(OnStartButtonClicked);
            
            EventSystem.current.SetSelectedGameObject(startButton.gameObject);
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetButtonDown("Cancel")) {
                OnExitButtonClicked();
            }
            
            if (Input.GetKeyDown(KeyCode.Space)) {
                Accelerate();
            }
        }

        private void OnEnable()
        {
            BgmManager.Resume();
            pickingQuestionsAutomatically = true;
            
            bottomPanel.onExitButtonClicked += OnExitButtonClicked;
            bottomPanel.onSelectButtonClicked += OnStartButtonClicked;
            bottomPanel.onConfirmButtonClicked += OnConfirmButtonClicked;
            bottomPanel.onRedButtonClicked += OnRedButtonClicked;
            bottomPanel.onBlueButtonClicked += OnBlueButtonClicked;
            bottomPanel.onGreenButtonClicked += OnGreenButtonClicked;
            bottomPanel.onYellowButtonClicked += OnYellowButtonClicked;
        }

        private void OnDisable()
        {
            BgmManager.Pause();
            pickingQuestionsAutomatically = false;
            
            bottomPanel.onExitButtonClicked -= OnExitButtonClicked;
            bottomPanel.onSelectButtonClicked -= OnStartButtonClicked;
            bottomPanel.onConfirmButtonClicked -= OnConfirmButtonClicked;
            bottomPanel.onRedButtonClicked -= OnRedButtonClicked;
            bottomPanel.onBlueButtonClicked -= OnBlueButtonClicked;
            bottomPanel.onGreenButtonClicked -= OnGreenButtonClicked;
            bottomPanel.onYellowButtonClicked -= OnYellowButtonClicked;
        }
        
        private void OnDestroy()
        {
            Debug.Log("YamanoteGameView is destroyed.");
            
            _loopFlag = false;
            
            BgmManager.Play(R.Audios.BgmYamanote);
        }

        public async void OnStartButtonClicked()
        {
            startButton.gameObject.SetActive(false);
            questionsView.gameObject.SetActive(true);
            
            await PlayStartupAnimation(0.5f);
            
            await UniTask.Delay(TimeSpan.FromSeconds(0.75f));

            StartPickingQuestionsAutomatically();
            
            // await PickNextRandomQuestion();
            ShowControlButtons();
        } 
        
        private void OnExitButtonClicked()
        {
            PopupConfirmView();
        }

        private void OnConfirmButtonClicked()
        {
            
        }

        private void OnRedButtonClicked()
        {
            BgmManager.Stop();
            Navigator.Push<YamanoteGameOverView>();
        }

        private async void OnBlueButtonClicked()
        {
            if (!pickingQuestionsAutomatically) return;
            
            ++_counter;
            if (CheckAcceleration())
            {
                Accelerate();
                await UniTask.Delay(TimeSpan.FromSeconds(3));
            }
            
            // Play sound effects
            SFXManager.Play(R.Audios.SfxYamanoteChangingQuestion);
            
            // Pick next question
            pickingQuestionsAutomatically = false;
            await PickNextRandomQuestion();
            SFXManager.Play(R.Audios.SfxYamanoteChangeQuestion);
            if (!_isAccelerating)
                PlayNewQuestionAnimation();
            await UniTask.Delay(TimeSpan.FromSeconds(3));
            pickingQuestionsAutomatically = true;
        }
        
        private async void OnGreenButtonClicked()
        {
            if (RoulettePreferences.DisplayMode == RouletteDisplayMode.Random)
            {
                await Navigator.Push<USEN.Games.Roulette.RouletteGameView>(async (view) => {
                    view.RouletteData = RouletteManager.Instance.GetRandomRoulette();
                });
            }
            else await Navigator.Push<RouletteCategoryView>();
            
            await UniTask.NextFrame();
        }

        private async void OnYellowButtonClicked()
        {
            BgmManager.Pause();
            await Navigator.Push<CommendView>();
            BgmManager.Resume();
        }
        
        public async Task PickNextQuestion()
        {
            await questionsPicker.ScrollTo(questionsPicker.FirstVisibleIndex + 1, 2);
        }
        
        public async Task PickNextRandomQuestion()
        {
            var questionsCount = _questions.Count;
            var randomIndex = UnityEngine.Random.Range(0, questionsCount);
            randomIndex += questionsCount < 10 ? questionsCount : 0;
            await questionsPicker.ScrollTo(questionsPicker.FirstVisibleIndex + randomIndex, 2);
        }
        
        private void StartPickingQuestionsAutomatically()
        {
            async void PickQuestion()
            {
                ++_counter;
                await questionsPicker.PickNextQuestion();
                SFXManager.Play(R.Audios.SfxYamanoteChangeQuestion);
                if (!_isAccelerating)
                    PlayNewQuestionAnimation();
            }
            
            var cancellationToken = this.GetCancellationTokenOnDestroy();
            UniTask.Void(async (token) =>
            {
                while (_loopFlag)
                {
                    if (CheckAcceleration())
                        Accelerate();
                    
                    if (pickingQuestionsAutomatically)
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(questionInterval));
                        if (pickingQuestionsAutomatically)
                            PickQuestion();
                    }
                    else await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
                }
            }, cancellationToken);
        }
        
        private bool CheckAcceleration()
        {
            if (_counter >= 5 && !_isAccelerating)
                return true;
            return false;
        }

        private async Task PlayStartupAnimation(float duration = 1 /* In seconds */ )
        {
            // Questions view fade in
            DOTween.To(() => questionsView.alpha, x => questionsView.alpha = x, 1, duration);
            
            // Move questions view from bottom to top
            var rectTransform = questionsView.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(0, 100);
            rectTransform.DOAnchorPosY(405, duration).SetEase(Ease.OutSine);
            
            await Task.Delay(TimeSpan.FromSeconds(duration));
            
            questionsPicker.Alpha = 0;
            DOTween.To(() => questionsPicker.Alpha, x => questionsPicker.Alpha = x, 1, 0.3f);
        }
        
        private async Task PlayNewQuestionAnimation(float duration = 2.5f)
        {
            highlightMask.gameObject.SetActive(true);
            highlightMask.color = highlightMask.color.WithAlpha(0);
            
            // Fade in
            highlightMask.DOFade(1, duration * 0.2f);
            await UniTask.Delay(TimeSpan.FromSeconds(duration * 0.2f));
            
            // Highlight particles
            highlightParticles.Play();
            await UniTask.Delay(TimeSpan.FromSeconds(duration * 0.4f));
            highlightParticles.Stop();
            
            // Fade out
            highlightMask.DOFade(0, duration * 0.4f);
            await UniTask.Delay(TimeSpan.FromSeconds(duration * 0.4f));
            
            highlightMask.gameObject.SetActive(false);
        }
        
        private void ShowControlButtons()
        {
            bottomPanel.blueButton.gameObject.SetActive(true);
            bottomPanel.redButton.gameObject.SetActive(true);
            bottomPanel.greenButton.gameObject.SetActive(true);
            bottomPanel.yellowButton.gameObject.SetActive(true);
        }
        
        private void PopupConfirmView()
        {
            Navigator.ShowModal<PopupOptionsView>(
                builder: (popup) =>
                {
                    popup.onOption1 = () => Navigator.Pop();
                    popup.onOption2 = () => Navigator.PopToRoot();
                    popup.onOption3 = () => SceneManager.LoadScene("GameEntries");
                });
        }
        
        private void Accelerate()
        {
            // Config
            _isAccelerating = true;
            questionInterval = 5;
            pickingQuestionsAutomatically = false;
            
            // Tweens
            questionsView.DOFade(0, 0.3f);
            DOTween.To(() => cloudController.speed.x, x => cloudController.speed = new Vector2(x, 0), cloudController.speed.x * 1.5f, 2f);
            DOTween.To(() => buildingsController.speed.x, x => buildingsController.speed = new Vector2(x, 0), buildingsController.speed.x * 1.5f, 2f);
            
            // Play acceleration animation
            _accelerationDirector.Play();
            // _accelerationDirector.stopped += (director) => {
            //     pickingQuestionsAutomatically = true;
            //     questionsView.DOFade(1, 0.5f);
            // };
            
            UniTask.WaitForSeconds(2).ContinueWith(() => {
                pickingQuestionsAutomatically = true;
                questionsView.DOFade(1, 0.5f);
            });
            
            BgmManager.Play(R.Audios.BgmYamanoteGameAccelelation);
        }
    }
}