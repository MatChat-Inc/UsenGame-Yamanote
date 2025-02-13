// Created by LunarEclipse on 2024-6-21 1:53.

using System.Threading.Tasks;
using DG.Tweening;
using Luna;
using Luna.Extensions;
using Luna.UI;
using Luna.UI.Navigation;
using UnityEngine;
using UnityEngine.EventSystems;
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
            SFXManager.Play(R.Audios.SfxYamanoteGameOver);
            SFXManager.PlayRepeatedly(R.Audios.SfxYamanoteRain, (-6, -3));
            Task.Delay(Random.Range(3000, 5000)).Then(_ => {
                SFXManager.PlayRepeatedly(R.Audios.SfxYamanoteWind, (3, 10));
            });
            
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