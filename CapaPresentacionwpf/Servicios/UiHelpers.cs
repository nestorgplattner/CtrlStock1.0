using System.Windows.Controls;
using System.Windows.Media;

namespace CapaPresentacionWPF.Servicios
{
    public static class UiHelpers
    {
        public static void SetPlaceholder(TextBox textBox, string placeholder, bool gotFocus)
        {
            if (gotFocus)
            {
                if (textBox.Text == placeholder)
                {
                    textBox.Text = "";
                    textBox.Foreground = Brushes.Black;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = placeholder;
                    textBox.Foreground = Brushes.Gray;
                }
            }
        }

        public static void ResetButtonBackground(params Button[] buttons)
        {
            foreach (var btn in buttons)
            {
                if (btn != null)
                    btn.Background = Brushes.LightGray;
            }
        }

        public static void SetButtonSelected(Button btn)
        {
            if (btn != null)
                btn.Background = Brushes.LightGreen;
        }
    }
}