
using ThinkGeo.Core;

namespace MBTilesExtractor
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
            this.chkExtractingData = new System.Windows.Forms.CheckBox();
            this.Map1 = new ThinkGeo.UI.WinForms.MapView();
            this.SuspendLayout();
            // 
            // chkExtractingData
            // 
            this.chkExtractingData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkExtractingData.AutoSize = true;
            this.chkExtractingData.Location = new System.Drawing.Point(858, 22);
            this.chkExtractingData.Name = "chkExtractingData";
            this.chkExtractingData.Size = new System.Drawing.Size(118, 17);
            this.chkExtractingData.TabIndex = 4;
            this.chkExtractingData.Text = "Extract Vector Tiles";
            this.chkExtractingData.UseVisualStyleBackColor = true;
            this.chkExtractingData.CheckedChanged += new System.EventHandler(this.chkExtractingData_CheckedChanged);
            // 
            // Map1
            // 
            this.Map1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Map1.BackColor = System.Drawing.Color.White;
            this.Map1.CurrentScale = 0D;
            this.Map1.Location = new System.Drawing.Point(2, 1);
            this.Map1.MapResizeMode = MapResizeMode.PreserveScale;
            this.Map1.MapUnit = ThinkGeo.Core.GeographyUnit.DecimalDegree;
            this.Map1.MaximumScale = 1.7976931348623157E+308D;
            this.Map1.MinimumScale = 200D;
            this.Map1.Name = "Map1";
            this.Map1.RestrictExtent = null;
            this.Map1.RotatedAngle = 0F;
            this.Map1.Size = new System.Drawing.Size(983, 673);
            this.Map1.TabIndex = 11;
            this.Map1.Text = "Map1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 674);
            this.Controls.Add(this.chkExtractingData);
            this.Controls.Add(this.Map1);
            this.Name = "Form1";
            this.Text = "ThinkGeo Offline Maps Viewer and Extractor";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.CheckBox chkExtractingData;
        private ThinkGeo.UI.WinForms.MapView Map1;
    }
}

