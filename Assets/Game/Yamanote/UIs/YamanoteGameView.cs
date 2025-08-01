// Created by LunarEclipse on 2024-7-21 19:44.

using System;
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
using UnityEngine.UI;
using USEN.Games.Common;
using USEN.Games.Roulette;
using Object = UnityEngine.Object;

namespace USEN.Games.Yamanote
{
    public class YamanoteGameView : Widget
    {
        public ImageShaderController cloudController;
        public ImageShaderController buildingsController;
        public Button startButton;
        public CanvasGroup questionsView;
        public YamanoteQuestionsPicker questionsPicker;
        public Image questionBackground;
        public Image highlightMask;
        public CanvasGroup accelerationGroup;
        public ParticleSystem highlightParticles;
        public ParticleSystem rainParticles;
        public BottomPanel bottomPanel;
        
        [HideInInspector]
        public float questionInterval = 8;
        [HideInInspector]
        public float accelerationInterval = 18;
        
        public Sprite rouletteBackground;
        
        public bool pickingQuestionsAutomatically = false;
        
        private bool _shouldPickingQuestionsAutomatically = false;
        private bool _isPicking;
        private bool _isAccelerating;
        private bool _loopFlag = true;

        private float _startTime = float.MaxValue;
        private float _pauseTime;
        
        // private int _counter = 0;
        
        private PlayableDirector _accelerationDirector;
        
        private List<YamanoteQuestion> _questions;
        public List<YamanoteQuestion> Questions
        {
            get => _questions;
            set
            {
                _questions = value;
                questionsPicker.itemCount = Int32.MaxValue;
                questionsPicker.ItemBuilder = (index) => {
                    if (_questions.Count == 0) return "";
                    return _questions[index.Mod(_questions.Count)].Content;
                };
            }
        }

        private float ElapsedTime => Time.time - _startTime;

        private void Awake()
        {
            _accelerationDirector = GetComponent<PlayableDirector>();
            _shouldPickingQuestionsAutomatically = pickingQuestionsAutomatically;
        }

        private void Start()
        {
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
            
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                StopRain();
            }
            
            if (CheckAcceleration())
            {
                Accelerate();
            }
        }

        private void OnEnable()
        {
            BgmManager.Resume();
            
            _startTime += Time.time - _pauseTime;
            
            if (_shouldPickingQuestionsAutomatically)
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
            SFXManager.StopAll();
            BgmManager.Pause();
            
            _pauseTime = Time.time;
            
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
            _loopFlag = false;
            
            BgmManager.Play(R.Audios.BgmYamanote);
        }

        public void OnStartButtonClicked()
        {
            Debug.Log("Start Yamanote Game");
            StartGame();
        } 
        
        private void OnExitButtonClicked()
        {
            PopupConfirmView();
        }

        private void OnConfirmButtonClicked()
        {
            
        }

        private async void OnRedButtonClicked()
        {
            _accelerationDirector.time = 0;
            _accelerationDirector.Stop();
            
            BgmManager.Stop();
            await Navigator.Push<YamanoteGameOverView>();
            BgmManager.Play(R.Audios.BgmYamanoteGame);
        }

        private void OnBlueButtonClicked()
        {
            PickNextQuestion();
        }
        
        private async void OnGreenButtonClicked()
        {
            var currentBgm = BgmManager.CurrentBgm;

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
            
            BgmManager.Play(currentBgm);
        }

        private async void OnYellowButtonClicked()
        {
            BgmManager.Pause();
            await Navigator.Push<CommendView>();
            BgmManager.Resume();
            _startTime = Time.time;
        }
        
        public async void StartGame()
        {
            _startTime = Time.time;
            
            startButton.gameObject.SetActive(false);
            questionsView.gameObject.SetActive(true);
            bottomPanel.confirmButton.gameObject.SetActive(false);
            
            BgmManager.Play(R.Audios.BgmYamanoteGame);
            SFXManager.Play(R.Audios.SfxConfirm);
            
            await PlayStartupAnimation(0.5f);
            
            await UniTask.Delay(TimeSpan.FromSeconds(0.75f));

            // StartPickingQuestionsAutomatically();
            
            // await PickNextRandomQuestion();
            ShowControlButtons();
        } 
        
        public void ResetGame(bool resetQuestion = true)
        {
            _startTime = Time.time;
            _isPicking = false;
            _isAccelerating = false;
            _loopFlag = true;
            
            if (resetQuestion)
                questionsPicker.ScrollTo(0, 0);
            questionsPicker.Alpha = 0;
            questionsView.alpha = 0;
            accelerationGroup.alpha = 0;
            questionsView.gameObject.SetActive(false);
            startButton.gameObject.SetActive(true);
            bottomPanel.confirmButton.gameObject.SetActive(true);
            bottomPanel.redButton.gameObject.SetActive(false);
            bottomPanel.blueButton.gameObject.SetActive(false);
            // bottomPanel.greenButton.gameObject.SetActive(false);
            bottomPanel.yellowButton.gameObject.SetActive(false);
            cloudController.speed = new Vector2(-0.05f, 0f);
            buildingsController.speed = new Vector2(-0.5f, 0f);

            StopRain();
        }

        public async Task PickNextQuestion()
        {
            if (_isPicking) return;
            
            bottomPanel.redButton.gameObject.SetActive(false);
            bottomPanel.blueButton.gameObject.SetActive(false);
            
            _startTime = Time.time;
            
            // ++_counter;
            // if (CheckAcceleration())
            // {
            //     Accelerate();
            //     await UniTask.Delay(TimeSpan.FromSeconds(3));
            // }

            if (_isAccelerating)
            {
                Decelerate();
                await UniTask.Delay(TimeSpan.FromSeconds(0.5));
            }
            
            // Play sound effects
            SFXManager.Play(R.Audios.SfxYamanoteChangingQuestion);
            
            // Pick next question
            _isPicking = true;
            pickingQuestionsAutomatically = false;
            await ScrollToNextRandomQuestion();
            SFXManager.Play(R.Audios.SfxYamanoteChangeQuestion);
            if (!_isAccelerating)
                PlayNewQuestionAnimation();
            await UniTask.Delay(TimeSpan.FromSeconds(3));
            pickingQuestionsAutomatically = _shouldPickingQuestionsAutomatically;
            _isPicking = false;
            
            bottomPanel.redButton.gameObject.SetActive(true);
            bottomPanel.blueButton.gameObject.SetActive(true);
        }
        
        public async Task ScrollToNextQuestion()
        {
            await questionsPicker.ScrollTo(questionsPicker.FirstVisibleIndex + 1, 2);
        }
        
        public async Task ScrollToNextRandomQuestion()
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
                // ++_counter;
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
        
        // private void StartPickingQuestionsAutomatically()
        // {
        //     StartCoroutine(PickQuestionsAutomatically());
        // }
        //
        // private void StopPickingQuestionsAutomatically()
        // {
        //     StopCoroutine(PickQuestionsAutomatically());
        // }
        //
        // private IEnumerator PickQuestionsAutomatically()
        // {
        //     while (true)
        //     {
        //         yield return new WaitForSeconds(questionInterval);
        //
        //         if (pickingQuestionsAutomatically)
        //         {
        //             questionsPicker.PickNextQuestion();
        //             if (!_isAccelerating)
        //                 PlayNewQuestionAnimation();
        //         }
        //     }
        // }

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
            bottomPanel.blueButton.gameObject.SetActive(YamanotePreferences.DisplayMode != YamanoteDisplayMode.Normal);
            bottomPanel.redButton.gameObject.SetActive(true);
            // bottomPanel.greenButton.gameObject.SetActive(true);
            bottomPanel.yellowButton.gameObject.SetActive(true);
        }
        
        private void HideControlButtons()
        {
            bottomPanel.blueButton.gameObject.SetActive(false);
            bottomPanel.redButton.gameObject.SetActive(false);
            // bottomPanel.greenButton.gameObject.SetActive(false);
            bottomPanel.yellowButton.gameObject.SetActive(false);
        }
        
        private async void PopupConfirmView()
        {
            var orginalVolume = BgmManager.Volume;
            
            await Navigator.ShowModal<PopupOptionsView>(
                builder: (popup) =>
                {
                    popup.onOption1 = () => Navigator.Pop();
                    popup.onOption2 = () => Navigator.PopToRoot();
#if UNITY_ANDROID
                    // popup.onOption3 = () => Android.Back();
                    popup.onOption3 = () => Application.Quit();
#else
                    popup.onOption3 = () => Application.Quit();
#endif
                    BgmManager.Resume();
                    BgmManager.SetVolume(BgmManager.Volume * 0.6f, 0.3f);
                });
            
            BgmManager.SetVolume(orginalVolume, 0.3f);
        }
        
        private bool CheckAcceleration()
        {
            if (ElapsedTime > accelerationInterval && !_isAccelerating)
                return true;
            return false;
        }
        
        private async void Accelerate()
        {
            if (_isAccelerating) return;
            
            // Config
            _isAccelerating = true;
            questionInterval = 5;
            pickingQuestionsAutomatically = false;
            
            // Tweens
            questionsView.DOFade(0, 0.3f);
            accelerationGroup.DOFade(1, 0.3f);
            DOTween.To(() => cloudController.speed.x, x => cloudController.speed = new Vector2(x, 0), cloudController.speed.x * 1.5f, 2f);
            DOTween.To(() => buildingsController.speed.x, x => buildingsController.speed = new Vector2(x, 0), buildingsController.speed.x * 1.5f, 2f);
            
            // Play acceleration animation
            _accelerationDirector.Play();
            _accelerationDirector.SetSpeed(1);
            
            HideControlButtons();
            UniTask.WaitForSeconds(3).ContinueWith(() => {
                // pickingQuestionsAutomatically = true;
                questionsView.DOFade(1, 0.5f);
                SFXManager.PlayOccasionally(R.Audios.SfxYamanoteThunder, (2, 5));
                
                rainParticles.startColor = rainParticles.startColor.WithAlpha(0);
                rainParticles.Play();
                DOTween.To(() => rainParticles.startColor, x => rainParticles.startColor = x, rainParticles.startColor.WithAlpha(0.5f), 1f);
                
                ShowControlButtons();
            });
            
            BgmManager.Stop();
            SFXManager.Play(R.Audios.SfxYamanoteAccelerationStart);
            await Task.Delay(TimeSpan.FromSeconds(1.6f));
            if (this != null)
                BgmManager.Play(R.Audios.BgmYamanoteGameAcceleration);
        }
        
        private void Decelerate()
        {
            if (!_isAccelerating) return;
            
            // Config
            _isAccelerating = false;
            questionInterval = 8;
            pickingQuestionsAutomatically = _shouldPickingQuestionsAutomatically;
            
            // Tweens
            questionBackground.color = questionBackground.color.WithAlpha(0);
            questionBackground.DOFade(1, 0.6f);
            accelerationGroup.DOFade(0, 0.3f);
            DOTween.To(() => cloudController.speed.x, x => cloudController.speed = new Vector2(x, 0), cloudController.speed.x / 1.5f, 2f);
            DOTween.To(() => buildingsController.speed.x, x => buildingsController.speed = new Vector2(x, 0), buildingsController.speed.x / 1.5f, 2f);
            
            // Play deceleration animation
            _accelerationDirector.Play();
            _accelerationDirector.time = 3;
            _accelerationDirector.SetSpeed(-2);
            rainParticles.Stop();
            
            UniTask.WaitForSeconds(1.5f).ContinueWith(() => {
                _accelerationDirector.Stop();
                SFXManager.Stop(R.Audios.SfxYamanoteThunder);
            });
            
            BgmManager.Play(R.Audios.BgmYamanoteGame);
        }
        
        private async void StopRain()
        {
            _accelerationDirector.time = 0;
            rainParticles.Stop();
            if (_accelerationDirector.state != PlayState.Playing)
                _accelerationDirector.Play();
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
            _accelerationDirector.Stop();
        }
    }
}