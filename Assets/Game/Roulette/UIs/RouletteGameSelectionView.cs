// Created by LunarEclipse on 2024-6-21 1:45.

using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Luna;
using Luna.UI;
using Luna.UI.Navigation;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using USEN.Games.Common;

namespace USEN.Games.Roulette
{
    public class RouletteGameSelectionView : Widget, IEventSystemHandler
    {
        public TextMeshProUGUI titleText;
        public RouletteGameSelectionList rouletteGameSelectionList;
        public RouletteContentList rouletteContentList;
        public RouletteWheel rouletteWheel;
        public BottomPanel bottomPanel;
        
        [HideInInspector] 
        public bool selectLast = false; 
        
        private EditMode _editMode;
        
        private RouletteManager _manager;
        private RouletteCategory _category;
        public RouletteCategory Category
        {
            get => _category;
            set
            {
                _category = value;
                rouletteGameSelectionList.Data = value.roulettes;
                
                CheckRoulette();

                if (value.title == "オリジナル")
                {
                    _editMode = EditMode.Editable;
                    bottomPanel.redButton.gameObject.SetActive(true);
#if !USEN_ROULETTE
                    bottomPanel.blueButton.gameObject.SetActive(true);              
#endif
                }
                else
                {
                    _editMode = EditMode.Readonly;
                    bottomPanel.redButton.gameObject.SetActive(false);
                    bottomPanel.yellowButton.gameObject.SetActive(false);
#if !USEN_ROULETTE
                    bottomPanel.blueButton.gameObject.SetActive(false);              
#endif
                }

                titleText.text = value.title;
            }
        }
        
        private GameObject RouletteGameObject => rouletteWheel.transform.parent.gameObject;
        
        private bool IsOriginal => Category.title == "オリジナル";

        private void Awake()
        {
            rouletteGameSelectionList.onCellSelected += (index, cell) => rouletteWheel.RouletteData = cell.Data;
            rouletteGameSelectionList.onCellSubmitted += (index, cell) => OnConfirmButtonClicked();
            rouletteContentList.onCellSubmitted += (index, cell) => OnConfirmButtonClicked();
            
            _manager = RouletteManager.Instance;
        }

        private void Start()
        {
            if (selectLast && rouletteGameSelectionList.Data.Count > 0)
                UniTask.DelayFrame(2).ContinueWith(() => {
                    if (gameObject.activeInHierarchy)
                        rouletteGameSelectionList.Select(rouletteGameSelectionList.Data.Count - 1);
                });
            else rouletteGameSelectionList.Select(0);
        }

        private void OnEnable()
        {
            HideContentView();
            bottomPanel.onRedButtonClicked += OnRedButtonClicked;
            bottomPanel.onBlueButtonClicked += OnBlueButtonClicked;
            bottomPanel.onYellowButtonClicked += OnYellowButtonClicked;

            if (selectLast && rouletteGameSelectionList.Initialized)
                UniTask.DelayFrame(2).ContinueWith(() => {
                    rouletteGameSelectionList.FocusOnCell(rouletteGameSelectionList.Data.Count - 1, 0.5f);
                    selectLast = false;
                });
        }

        private void OnDisable()
        {
            bottomPanel.onRedButtonClicked -= OnRedButtonClicked;
            bottomPanel.onBlueButtonClicked -= OnBlueButtonClicked;
            bottomPanel.onYellowButtonClicked -= OnYellowButtonClicked;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetButtonDown("Cancel")) {
                OnExitButtonClicked();
            }
            
            CheckRoulette();
        }
        
        public void OnConfirmButtonClicked()
        {
            if (rouletteGameSelectionList.gameObject.activeSelf)
            {
                ShowContentView();
            }
            else if (rouletteContentList.gameObject.activeSelf)
            {
                Navigator.Push<RouletteGameView>(async (view) =>
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

        public async void OnBlueButtonClicked()
        {
            // Edit roulette
            var result = await Navigator.Push<RouletteEditView>((view) => {
                view.Data = rouletteGameSelectionList.SelectedData;
                view.ShouldCreateNew = !IsOriginal;
            }) as RouletteData;
            
            // Add to category and save
            if (result != null)
            {
                result.Category = "オリジナル";
                var seletedIndex = rouletteGameSelectionList.SelectedIndex;
                
                if (IsOriginal)
                {
                    Category.roulettes[rouletteGameSelectionList.SelectedIndex] = result;
                    rouletteWheel.RouletteData = result;
                    rouletteGameSelectionList.Reload();
                }
                
                // Jump back to original category if not in original category
                if (_editMode == EditMode.Readonly && Category.roulettes.Count > 0)
                {
                    var categoryView = Navigator.BackTo<RouletteCategoryView>();
                    categoryView?.GotoOriginalCategory(view => {
                        view.selectLast = true;
                    });
                }
                else
                {
                    await UniTask.DelayFrame(2);
                    rouletteGameSelectionList.Select(seletedIndex);
                }
            }
        }

        public async void OnRedButtonClicked()
        {
            // Open edit view
            var result = await Navigator.Push<RouletteEditView>() as RouletteData;
            
            // Add to category and save
            if (result != null)
            {
                // Set color for each sector
                for (int i = 0; i < result.sectors.Count; i++)
                {
                    var sector = result.sectors[i];
                    sector.id = i;
                    sector.color = RouletteData.GetSectorColor(i, result.sectors.Count);
                }
                
                if (IsOriginal)
                {
                    Category.roulettes.Add(result);
                    rouletteGameSelectionList.Reload();
                    rouletteWheel.RouletteData = result;
                }
                
                result.Category = "オリジナル";
                
                // _manager.AddRoulette(result);
            }
            
            CheckRoulette();

            await UniTask.NextFrame();
            rouletteGameSelectionList.Select(Category.roulettes.Count - 1);
        }
        
        public void OnYellowButtonClicked()
        {
            if (rouletteGameSelectionList.gameObject.activeSelf &&
                rouletteGameSelectionList.Data.Count > 0)
            {
                _manager.DeleteRoulette(rouletteGameSelectionList.SelectedData);
                rouletteGameSelectionList.Remove(rouletteGameSelectionList.SelectedIndex);
            }

            if (rouletteContentList.gameObject.activeSelf &&
                rouletteContentList.Data.Count > 2)
            {
                rouletteContentList.Remove(rouletteContentList.SelectedIndex);
                _manager.UpdateRoulette(rouletteGameSelectionList.SelectedData);
            }
            
            if (rouletteGameSelectionList.Data.Count > 0)
                rouletteWheel.RouletteData = rouletteGameSelectionList.SelectedData;
            else rouletteWheel.RouletteData = null;

            CheckRoulette();
        }
        
        private void ShowContentView()
        {
            rouletteGameSelectionList.gameObject.SetActive(false);
            rouletteContentList.Data = rouletteGameSelectionList.SelectedData.sectors;
            rouletteContentList.gameObject.SetActive(true);
        }
        
        private void HideContentView()
        {
            if (rouletteContentList.gameObject.activeSelf)
            {
                rouletteContentList.gameObject.SetActive(false);
                rouletteGameSelectionList.gameObject.SetActive(true);
                rouletteGameSelectionList.Select(rouletteGameSelectionList.SelectedIndex);
                SFXManager.Stop();
                SFXManager.Play(R.Audios.SfxBack);
            }
        }
        
        private void CheckRoulette()
        {
            var hasRoulette = Category.roulettes.Count > 0;
            
            RouletteGameObject.SetActive(hasRoulette);

            if (Category.title == "オリジナル")
            {
                bottomPanel.blueButton.gameObject.SetActive(hasRoulette);
                bottomPanel.yellowButton.gameObject.SetActive(hasRoulette);
            }
        }
        
        private void ReloadCategory()
        {
            Category = _manager.GetCategory(Category.title);
        }
        
        private enum EditMode
        {
            Readonly,
            Editable,
        }
    }
}