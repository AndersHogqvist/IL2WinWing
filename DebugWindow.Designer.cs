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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DebugWindow));
            debugTextBox = new TextBox();
            toggleBtn = new Button();
            closeBtn = new Button();
            clearBtn = new Button();
            SuspendLayout();
            // 
            // debugTextBox
            // 
            debugTextBox.Font = new Font("Consolas", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            debugTextBox.Location = new Point(12, 12);
            debugTextBox.Multiline = true;
            debugTextBox.Name = "debugTextBox";
            debugTextBox.ScrollBars = ScrollBars.Vertical;
            debugTextBox.Size = new Size(776, 397);
            debugTextBox.TabIndex = 0;
            // 
            // toggleBtn
            // 
            toggleBtn.Location = new Point(12, 415);
            toggleBtn.Name = "toggleBtn";
            toggleBtn.Size = new Size(75, 23);
            toggleBtn.TabIndex = 1;
            toggleBtn.Text = "Start";
            toggleBtn.UseVisualStyleBackColor = true;
            toggleBtn.Click += toggleBtn_Click;
            // 
            // closeBtn
            // 
            closeBtn.Location = new Point(713, 415);
            closeBtn.Name = "closeBtn";
            closeBtn.Size = new Size(75, 23);
            closeBtn.TabIndex = 2;
            closeBtn.Text = "Close";
            closeBtn.UseVisualStyleBackColor = true;
            closeBtn.Click += closeBtn_Click;
            // 
            // clearBtn
            // 
            clearBtn.Location = new Point(93, 415);
            clearBtn.Name = "clearBtn";
            clearBtn.Size = new Size(75, 23);
            clearBtn.TabIndex = 3;
            clearBtn.Text = "Clear";
            clearBtn.UseVisualStyleBackColor = true;
            clearBtn.Click += clearBtn_Click;
            // 
            // DebugWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(clearBtn);
            Controls.Add(closeBtn);
            Controls.Add(toggleBtn);
            Controls.Add(debugTextBox);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "DebugWindow";
            Text = "IL2WinWing Debug";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox debugTextBox;
        private Button toggleBtn;
        private Button closeBtn;
        private Button clearBtn;
    }
}