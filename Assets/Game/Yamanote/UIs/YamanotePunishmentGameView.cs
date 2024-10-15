// Created by LunarEclipse on 2024-6-21 1:45.


using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Luna.UI;
using Luna.UI.Navigation;
using UnityEngine;
using UnityEngine.EventSystems;
using USEN.Games.Common;
using USEN.Games.Roulette;

namespace USEN.Games.Yamanote
{
    public class YamanotePunishmentGameView : Widget, IEventSystemHandler
    {
        public RouletteGameSelectionList rouletteGameSelectionList;
        public RouletteContentList rouletteContentList;
        public RouletteWheel rouletteWheel;
        public BottomPanel bottomPanel;
        
        private RouletteCategory _category;
        public RouletteCategory Category
        {
            get => _category;
            set
            {
                _category = value;
                rouletteGameSelectionList.Data = value.roulettes;

                if (value.title == "オリジナル")
                    bottomPanel.redButton.gameObject.SetActive(true);
                else bottomPanel.redButton.gameObject.SetActive(false);
            }
        }

        void Awake()
        {
            rouletteGameSelectionList.onCellSubmitted += (index, cell) => OnConfirmButtonClicked();
            rouletteContentList.onCellSubmitted += (index, cell) => OnConfirmButtonClicked();
        }

        private void OnEnable()
        {
            HideContentView();
            Category = _category;
            
            bottomPanel.onRedButtonClicked += OnRedButtonClicked;
            bottomPanel.onBlueButtonClicked += OnBlueButtonClicked;
        }

        private void OnDisable()
        {
            bottomPanel.onRedButtonClicked -= OnRedButtonClicked;
            bottomPanel.onBlueButtonClicked -= OnBlueButtonClicked;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetButtonDown("Cancel")) {
                OnExitButtonClicked();
            }
        }
        
        public void OnConfirmButtonClicked()
        {
            if (rouletteGameSelectionList.gameObject.activeSelf)
            {
                ShowContentView();
            }
            else if (rouletteContentList.gameObject.activeSelf)
            {
                Navigator.Push<Roulette.RouletteGameView>(async (view) =>
                {
                    await UniTask.NextFrame();
                    view.RouletteData = rouletteGameSelectionList.SelectedData;
                });
            }
        }

        public void OnExitButtonClicked()
        {
            if (rouletteGameSelectionList.gameObject.activeSelf)
            {
                Navigator.Pop();
            }
            else if (rouletteContentList.gameObject.activeSelf)
            {
                HideContentView();
            }
        }

        public void OnBlueButtonClicked()
        {
            // Edit roulette
            Navigator.Push<Roulette.RouletteEditView>((view) =>
            {
                view.Data = rouletteGameSelectionList.SelectedData;
            });
        }

        public async void OnRedButtonClicked()
        {
            // Create new roulette
            var roulette = new RouletteData();
            roulette.Title = "新規ルーレット";
            roulette.sectors = new List<RouletteSector>();
            for (int i = 0; i < 8; i++)
            {
                roulette.sectors.Add(new RouletteSector()
                {
                    content = $"",
                    weight = 1,
                    color = Color.HSVToRGB(1.0f / 8 * i, 0.5f, 1f),
                });
            }
            
            // Open edit view
            var result = await Navigator.Push<Roulette.RouletteEditView>((view) =>
            {
                view.Data = roulette;
            }) as RouletteData;
            
            // Add to category
            Category.roulettes.Insert(0, result);
        }
        
        private void ShowContentView()
        {
            rouletteGameSelectionList.gameObject.SetActive(false);
            rouletteContentList.Data = rouletteGameSelectionList.SelectedData.sectors;
            rouletteContentList.gameObject.SetActive(true);
        }
        
        private void HideContentView()
        {
            rouletteContentList.gameObject.SetActive(false);
            rouletteGameSelectionList.gameObject.SetActive(true);
        }
    }
}