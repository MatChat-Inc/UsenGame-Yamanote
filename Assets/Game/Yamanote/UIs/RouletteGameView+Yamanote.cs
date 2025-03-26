using Luna;
using Luna.UI.Navigation;
using USEN.Games.Common;
using USEN.Games.Yamanote;

namespace USEN.Games.Roulette
{
    public partial class RouletteGameView
    {
        private void PopupConfirmView()
        {
            Navigator.ShowModal<PopupOptionsView2>(
                builder: (popup) =>
                {
                    popup.onOption1 = () => Navigator.Pop();
                    popup.onOption2 = () => Navigator.PopUntil<YamanoteGameOverView>();
#if UNITY_ANDROID
                    popup.onOption3 = () => Android.Back();
#else
                    popup.onOption3 = () => Application.Quit();
#endif
                });
        }
    }
}