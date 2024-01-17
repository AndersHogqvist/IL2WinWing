namespace IL2WinWing
{
    partial class DebugWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            debugTextBox = new TextBox();
            SuspendLayout();
            // 
            // debugTextBox
            // 
            debugTextBox.Font = new Font("Consolas", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            debugTextBox.Location = new Point(12, 12);
            debugTextBox.Multiline = true;
            debugTextBox.Name = "debugTextBox";
            debugTextBox.ScrollBars = ScrollBars.Vertical;
            debugTextBox.Size = new Size(776, 426);
            debugTextBox.TabIndex = 0;
            // 
            // DebugWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(debugTextBox);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Name = "DebugWindow";
            Text = "IL2WinWing Debug";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox debugTextBox;
    }
}