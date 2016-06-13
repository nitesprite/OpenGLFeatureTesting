namespace OpenTKSandbox
{
    partial class Form1
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
            this.glControl1 = new OpenTK.GLControl();
            this.pnlTimeslice = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // glControl1
            // 
            this.glControl1.AutoSize = true;
            this.glControl1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.glControl1.BackColor = System.Drawing.Color.Black;
            this.glControl1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.glControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glControl1.Location = new System.Drawing.Point(0, 0);
            this.glControl1.Margin = new System.Windows.Forms.Padding(4);
            this.glControl1.Name = "glControl1";
            this.glControl1.Size = new System.Drawing.Size(478, 325);
            this.glControl1.TabIndex = 0;
            this.glControl1.VSync = false;
            this.glControl1.Load += new System.EventHandler(this.OnGL_Load);
            this.glControl1.Paint += new System.Windows.Forms.PaintEventHandler(this.OnGL_Paint);
            //this.glControl1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OnMouseClick);
            //this.glControl1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnMouseDown);
            //this.glControl1.MouseEnter += new System.EventHandler(this.OnMouseEnter);
            //this.glControl1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
            //this.glControl1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnMouseUp);
            this.glControl1.Resize += new System.EventHandler(this.OnGL_Resize);
            // 
            // pnlTimeslice
            // 
            this.pnlTimeslice.Controls.Add(this.glControl1);
            this.pnlTimeslice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTimeslice.Location = new System.Drawing.Point(0, 0);
            this.pnlTimeslice.Name = "pnlTimeslice";
            this.pnlTimeslice.Size = new System.Drawing.Size(1175, 828);
            this.pnlTimeslice.TabIndex = 1;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1175, 828);
            this.Controls.Add(this.pnlTimeslice);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private OpenTK.GLControl glControl1;
        private System.Windows.Forms.Panel pnlTimeslice;
    }
}
}

