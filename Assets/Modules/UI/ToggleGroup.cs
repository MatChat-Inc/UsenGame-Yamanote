using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.UI
{
    [DisallowMultipleComponent]
    [Icon("ToggleGroup Icon")]
    [AddComponentMenu("UI/Toggle Group", 32)]
    public class ToggleGroup: UnityEngine.UI.ToggleGroup
    {
        public List<Toggle> Toggles => base.m_Toggles;
        
        public void Add(Toggle toggle)
        {
            base.RegisterToggle(toggle);
        }

        public void Remove(Toggle toggle)
        {
            base.UnregisterToggle(toggle);
        }
        
        public void ToggleOn(int index)
        {
            if (index < 0 || index >= Toggles.Count) return;
            Toggles[index].isOn = true;
        }
        
        public void Bind(Slider slider)
        {
            slider.maxValue = Toggles.Count - 1;
            slider.onValueChanged.AddListener((value) => {
                var index = Mathf.RoundToInt(value);
                ToggleOn(index);
            });
            
            foreach (var toggle in Toggles)
                toggle.onValueChanged.AddListener((isOn) => {
                    if (isOn)
                    {   // Clicking on the toggle will change the slider value
                        var index = Toggles.IndexOf(toggle);
                        slider.value = index;
                    }
                });
        }
    }
}