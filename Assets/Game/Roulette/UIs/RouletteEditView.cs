// Created by LunarEclipse on 2024-7-11 23:55.

using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Luna;
using Luna.UI;
using Luna.UI.Navigation;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;
using USEN.Games.Common;
using Random = UnityEngine.Random;

namespace USEN.Games.Roulette
{
    public class RouletteEditView : Widget
    {
        public TextMeshProUGUI title;
        public TMP_InputField gameTitle;
        public RouletteEditListCell titleCell;
        public Button sectorCounterButton;
        public TextMeshProUGUI sectorCounter;
        public RouletteEditList sectorListView;
        public BottomPanel bottomPanel;
        
        private RouletteData _data;
        
        private bool _isEditing = false;
        private bool IsEditing => EventSystem.current.currentSelectedGameObject?.GetComponent<TMP_InputField>()?.isFocused ?? false;
        
        public RouletteData Data
        {
            get => _data;
            set
            {
                _data = new RouletteData(value);
                _data.Category = "オリジナル";
                
                if (title.text == "")
                {
                    if (value.sectors.Count > 0)
                        title.text = "編集";
                    else title.text = "新規作成";
                }
                gameTitle.text = value.Title;
                sectorListView.Data = _data.sectors;
                sectorCounter.text = $"{value.sectors.Count}";
            }
        }
        
        public bool ShouldCreateNew { get; set; } = false;

        protected void Awake()
        {
            gameTitle.onValueChanged.AddListener((value) => {
                Data.Title = value;
            });
            
            sectorListView.onCellCreated += (index, cell) => {
                cell.onInputValueChanged += (i, cell, value) => {
                    Data.sectors[i].content = value;
                };
            };
            
            // listView.onCellSubmitted += async (index, cell) =>
            // {
            //     await UniTask.NextFrame();
            //     cell.inputField.Select();
            // };
            
            bottomPanel.onBlueButtonClicked += OnBlueButtonClicked;
            bottomPanel.onRedButtonClicked += OnRedButtonClicked;
            bottomPanel.onYellowButtonClicked += OnYellowButtonClicked; // Delete the selected sector when the yellow button is clicked
        }

        private async void Start()
        {
            if (Data == null) ShouldCreateNew = true;
            if (ShouldCreateNew)
            {
                if (Data == null)
                    Data = CreateNewRoulette();
                else Data.GenerateNewID();
            }
            
            SetNavigation();
            
            await UniTask.DelayFrame(1);
            gameTitle.DeactivateInputField();
        }

        private void OnEnable()
        {
            EventSystem.current.SetSelectedGameObject(titleCell.gameObject);
            base.OnKey += OnKeyEvent;
        }

        private void OnDisable()
        {
            base.OnKey -= OnKeyEvent;
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetButtonDown("Cancel")) {
                if (!_isEditing) Navigator.Pop();
            }

            if (EventSystem.current.currentSelectedGameObject == sectorCounterButton.gameObject)
            {
                if (Input.GetKeyDown(KeyCode.RightArrow))
                    AddSector();
                else if (Input.GetKeyDown(KeyCode.LeftArrow))
                    RemoveSector();
            }
            
            // if(sectorListView.Selected)
            //     bottomPanel.yellowButton.gameObject.SetActive(true);
            // else bottomPanel.yellowButton.gameObject.SetActive(false);
        }

        private KeyEventResult OnKeyEvent(KeyControl key, KeyEvent keyEvent)
        {
            // Debug.Log($"[RouletteEditView] Key pressed: {key.keyCode} with event: {keyEvent}");
            var cell = sectorListView.cells.First()?.gameObject;
            Debug.Log($"[RouletteEditView] Current selected: {EventSystem.current.currentSelectedGameObject}");
            // Debug.Log($"[RouletteEditView] IsEditing: {IsEditing}");
            
            if (keyEvent == KeyEvent.Down)
            {
                if (key.keyCode == Key.UpArrow && (
                    EventSystem.current.currentSelectedGameObject == sectorCounterButton.gameObject ||
                    EventSystem.current.currentSelectedGameObject == sectorListView.cells.First()?.gameObject))
                    SFXManager.Play(R.Audios.SfxSelect);
                
                if (key.keyCode == Key.DownArrow && 
                    EventSystem.current.currentSelectedGameObject == titleCell.gameObject)
                    SFXManager.Play(R.Audios.SfxSelect);
            }
            
            if (keyEvent == KeyEvent.Down)
            {
                _isEditing = EventSystem.current.currentSelectedGameObject?.GetComponent<TMP_InputField>()?.isFocused ?? false;
            }
            
            return KeyEventResult.Unhandled;
        }
        
        private async void OnBlueButtonClicked()
        {
            SaveChanges();
            
            Navigator.PushReplacement<RouletteGameView>(async (view) =>
            {
                Navigator.Instance.PreviousRoute.LastSelected = null;
                await UniTask.NextFrame();
                view.RouletteData = Data;
            });
        }
        
        private void OnRedButtonClicked()
        {
            SaveChanges();
            
            Navigator.Pop(Data);
        }
        
        private void OnYellowButtonClicked()
        {
            if (sectorListView.Selected)
            {
                // Data.sectors.RemoveAt(sectorListView.SelectedIndex);
                sectorListView.Remove(sectorListView.SelectedIndex);
                sectorCounter.text = $"{sectorListView.Count}";
            }
        }

        public async void AddSector()
        {
            if (Data.sectors.Count >= 10) return;
            
            var newSector = new RouletteSector();
            sectorListView.Add(newSector, 0);
            sectorCounter.text = $"{sectorListView.Count}";
            
            // // Set color for each sector
            for (int i = 0; i < Data.sectors.Count; i++)
            {
                var sector = Data.sectors[i];
                sector.id = i;
                sector.color = RouletteData.GetSectorColor(i, Data.sectors.Count);
            }
            
            await UniTask.DelayFrame(1);
            SetNavigation();
        }
        
        public async void RemoveSector()
        {
            if (Data.sectors.Count <= 2) return;

            sectorListView.Remove(0, false);
            sectorCounter.text = $"{sectorListView.Count}";
            
            await UniTask.DelayFrame(1);
            SetNavigation();
        }
        
        private void SetNavigation()
        {
            if (sectorListView.cells.Count == 0) return;
            
            Navigation navigation1 = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = titleCell,
                selectOnDown = sectorListView.cells[0],
            };
            sectorCounterButton.navigation = navigation1;
            
            Navigation navigation2 = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = sectorCounterButton,
                selectOnDown = sectorListView.cells.Count > 1 ? sectorListView.cells[1] : null,
            };
            sectorListView.cells[0].navigation = navigation2;
            sectorListView.cells[0].inputField.navigation = navigation2;
        }
        
        private Color RandomColor(float saturation = 1, float brightness = 1)
        {
            return Color.HSVToRGB(Random.value, saturation, brightness);
        }

        private RouletteData CreateNewRoulette()
        {
            var roulette = new RouletteData();
            roulette.Title = "新規ルーレット";
            roulette.Category = "オリジナル";
            roulette.sectors = new List<RouletteSector>();
            for (int i = 0; i < 8; i++)
            {
                roulette.sectors.Add(new RouletteSector()
                {
                    content = $"",
                    weight = 1,
                    color = RouletteData.GetSectorColor(i, 8)
                });
            }
            return roulette;
        }

        private void SaveChanges()
        {
            var rm = RouletteManager.Instance;
            if (rm == null) return;
                
            if (ShouldCreateNew)
            {
                if (!rm.ExistsRoulette(Data))
                    rm.AddRoulette(Data);
                else Debug.LogWarning($"[RouletteEditView] Roulette already exists: {Data.Title}");
            }
            else rm.UpdateRoulette(Data);
        }
    }
}