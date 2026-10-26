using System;
using System.Drawing;
using System.Windows.Forms;

namespace AwakeWin.UI.Controls
{
    public class FluentComboBox : ComboBox
    {
        public FluentComboBox()
        {
            this.DrawMode = DrawMode.OwnerDrawFixed;
            this.DropDownStyle = ComboBoxStyle.DropDownList;
            this.ItemHeight = 24;
            this.BackColor = Color.FromArgb(39, 39, 42);
            this.ForeColor = Color.FromArgb(244, 244, 246);
            this.FlatStyle = FlatStyle.Flat;
            this.Font = new Font("Segoe UI", 9f, FontStyle.Regular);
            this.Cursor = Cursors.Hand;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            Graphics g = e.Graphics;
            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            // Fondo oscuro suave (elimina el azul chillón de Windows)
            Color bgColor = isSelected ? Color.FromArgb(55, 65, 81) : Color.FromArgb(39, 39, 42);
            using (SolidBrush bgBrush = new SolidBrush(bgColor))
            {
                g.FillRectangle(bgBrush, e.Bounds);
            }

            string text = this.Items[e.Index].ToString();
            Color textColor = isSelected ? Color.FromArgb(52, 211, 153) : Color.FromArgb(244, 244, 246);
            using (SolidBrush textBrush = new SolidBrush(textColor))
            {
                Rectangle textRect = new Rectangle(e.Bounds.X + 8, e.Bounds.Y, e.Bounds.Width - 12, e.Bounds.Height);
                using (StringFormat sf = new StringFormat { LineAlignment = StringAlignment.Center })
                {
                    g.DrawString(text, this.Font, textBrush, textRect, sf);
                }
            }
        }
    }
}
