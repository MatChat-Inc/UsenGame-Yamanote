using System;
using System.Collections.Generic;
using System.Linq;
using Luna.UI;
using Luna.UI.Navigation;
using UnityEngine;
using USEN.Games.Common;

namespace USEN.Games.Yamanote
{
    public class YamanoteCategoryView : Widget
    {
        public YamanoteCategoryList listView;
        public BottomPanel bottomPanel;

        public List<YamanoteCategory> Categories
        {
            get => listView.Data;
            set => listView.Data = value;
        }

        void Start()
        {
            listView.FocusOnCell(0);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetButtonDown("Cancel")) 
                Navigator.Pop();
        }

    }
}
