using MSHServiceMyFox;

namespace MSHService
{
    partial class MSHServiceMyFox
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur de composants

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.timerToken = new System.Windows.Forms.Timer(this.components);
            this.temperatureChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.bindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            this.tempHistoryDS1 = new TempHistoryDS();
            ((System.ComponentModel.ISupportInitialize)(this.temperatureChart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tempHistoryDS1)).BeginInit();
            this.SuspendLayout();
            // 
            // timerToken
            // 
            this.timerToken.Enabled = true;
            this.timerToken.Interval = 3600000;
            this.timerToken.Tick += new System.EventHandler(this.timerToken_Tick);
            // 
            // temperatureChart
            // 
            chartArea1.Name = "ChartArea1";
            this.temperatureChart.ChartAreas.Add(chartArea1);
            this.temperatureChart.DataSource = this.bindingSource1;
            legend1.Name = "Legend1";
            this.temperatureChart.Legends.Add(legend1);
            this.temperatureChart.Location = new System.Drawing.Point(11, 129);
            this.temperatureChart.Name = "temperatureChart";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            series1.XValueMember = "temperature";
            series1.YValueMembers = "date";
            this.temperatureChart.Series.Add(series1);
            this.temperatureChart.Size = new System.Drawing.Size(625, 340);
            this.temperatureChart.TabIndex = 0;
            this.temperatureChart.Text = "chart1";
            // 
            // bindingSource1
            // 
            this.bindingSource1.DataMember = "temperatureHistory";
            this.bindingSource1.DataSource = this.tempHistoryDS1;
            // 
            // tempHistoryDS1
            // 
            this.tempHistoryDS1.DataSetName = "TempHistoryDS";
            this.tempHistoryDS1.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // MSHServiceMyFox
            // 
            this.AutoSize = true;
            this.Controls.Add(this.temperatureChart);
            this.Name = "MSHServiceMyFox";
            this.Size = new System.Drawing.Size(650, 595);
            ((System.ComponentModel.ISupportInitialize)(this.temperatureChart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tempHistoryDS1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer timerToken;
        private System.Windows.Forms.DataVisualization.Charting.Chart temperatureChart;
        private System.Windows.Forms.BindingSource bindingSource1;
        private global::MSHServiceMyFox.TempHistoryDS tempHistoryDS1;
    }
}
