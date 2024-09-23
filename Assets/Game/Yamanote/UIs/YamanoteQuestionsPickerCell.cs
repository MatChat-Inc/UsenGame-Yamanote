using TMPro;
using UnityEngine;

namespace USEN.Games.Yamanote
{
    public class YamanoteQuestionsPickerCell: ListViewCell<string>
    {
        public TextMeshProUGUI text;
        
        public override string Data
        {
            get => data;
            set
            {
                data = value;
                text.text = value;
            }
        }
    }
}