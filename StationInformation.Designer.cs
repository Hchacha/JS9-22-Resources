namespace WindowsFormsApp1
{
    partial class StationInformation
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
            this.stationDataGridView = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.stationDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // stationDataGridView
            // 
            this.stationDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.stationDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.stationDataGridView.Location = new System.Drawing.Point(0, 0);
            this.stationDataGridView.Name = "stationDataGridView";
            this.stationDataGridView.RowTemplate.Height = 23;
            this.stationDataGridView.Size = new System.Drawing.Size(800, 450);
            this.stationDataGridView.TabIndex = 0;
            // 
            // StationInformation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.stationDataGridView);
            this.Name = "StationInformation";
            this.Text = "京沪高铁线路沿线车站信息";
            this.Load += new System.EventHandler(this.StationInformation_Load);
            ((System.ComponentModel.ISupportInitialize)(this.stationDataGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView stationDataGridView;
    }
}