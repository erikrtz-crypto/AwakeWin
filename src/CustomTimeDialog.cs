using System;
using System.Drawing;
using System.Windows.Forms;

namespace AwakeWin.UI
{
    public class CustomTimeDialog : Form
    {
        private NumericUpDown _numHours;
        private NumericUpDown _numMinutes;
        public int TotalMinutes { get; private set; }

        public CustomTimeDialog()
        {
            this.Text = "Tiempo Personalizado";
            this.Size = new Size(360, 225);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(24, 24, 27);
            this.ForeColor = Color.FromArgb(244, 244, 246);
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            Label lblTitle = new Label
            {
                Text = "Temporizador personalizado",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 211, 153),
                Location = new Point(20, 16),
                AutoSize = true
            };
            this.Controls.Add(lblTitle);

            Label lblPrompt = new Label
            {
                Text = "Indica el tiempo antes de suspender el equipo:",
                Location = new Point(22, 46),
                AutoSize = true,
                ForeColor = Color.FromArgb(161, 161, 170)
            };
            this.Controls.Add(lblPrompt);

            // Horas: 0 a 12
            _numHours = new NumericUpDown
            {
                Location = new Point(24, 76),
                Size = new Size(65, 28),
                Minimum = 0,
                Maximum = 12,
                Value = 1,
                BackColor = Color.FromArgb(39, 39, 42),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold)
            };
            this.Controls.Add(_numHours);

            Label lblHoursUnit = new Label
            {
                Text = "horas",
                Location = new Point(94, 80),
                AutoSize = true,
                ForeColor = Color.FromArgb(212, 212, 216)
            };
            this.Controls.Add(lblHoursUnit);

            // Minutos: 0 a 60
            _numMinutes = new NumericUpDown
            {
                Location = new Point(155, 76),
                Size = new Size(65, 28),
                Minimum = 0,
                Maximum = 60,
                Value = 30,
                BackColor = Color.FromArgb(39, 39, 42),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold)
            };
            this.Controls.Add(_numMinutes);

            Label lblMinUnit = new Label
            {
                Text = "minutos",
                Location = new Point(225, 80),
                AutoSize = true,
                ForeColor = Color.FromArgb(212, 212, 216)
            };
            this.Controls.Add(lblMinUnit);

            Button btnOk = new Button
            {
                Text = "Aceptar",
                DialogResult = DialogResult.OK,
                Location = new Point(145, 130),
                Size = new Size(95, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(5, 150, 105),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Click += (s, e) =>
            {
                int total = ((int)_numHours.Value * 60) + (int)_numMinutes.Value;
                if (total <= 0) total = 1;
                this.TotalMinutes = total;
                this.Close();
            };
            this.Controls.Add(btnOk);

            Button btnCancel = new Button
            {
                Text = "Cancelar",
                DialogResult = DialogResult.Cancel,
                Location = new Point(248, 130),
                Size = new Size(80, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(39, 39, 42),
                ForeColor = Color.FromArgb(212, 212, 216),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
        }
    }
}
