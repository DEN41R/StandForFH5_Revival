namespace StandForFH5Revival
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.creditsVal = new System.Windows.Forms.NumericUpDown();
            this.addCreditsBtn = new System.Windows.Forms.Button();
            this.wheelspinsVal = new System.Windows.Forms.NumericUpDown();
            this.addWheelspinsBtn = new System.Windows.Forms.Button();
            this.superWheelspinsVal = new System.Windows.Forms.NumericUpDown();
            this.addSuperWheelspinsBtn = new System.Windows.Forms.Button();
            this.xpVal = new System.Windows.Forms.NumericUpDown();
            this.addXpBtn = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.autoshowAvailableLabel = new System.Windows.Forms.Label();
            this.carIdsTextBox = new System.Windows.Forms.TextBox();
            this.autoshowAllfree = new System.Windows.Forms.Button();
            this.addAllCarsBtn = new System.Windows.Forms.Button();
            this.showAllCarsBtn = new System.Windows.Forms.Button();
            this.makeCarsFreeBtn = new System.Windows.Forms.Button();
            this.autoshowAvailableBox = new System.Windows.Forms.ComboBox();
            this.cursorToggle = new System.Windows.Forms.CheckBox();
            this.processWaitTimer = new System.Windows.Forms.Timer(this.components);
            this.patternscanTimer = new System.Windows.Forms.Timer(this.components);
            this.mainloopTimer = new System.Windows.Forms.Timer(this.components);
            this.processStartTimer = new System.Windows.Forms.Timer(this.components);
            this.statusStrip1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.creditsVal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.wheelspinsVal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.superWheelspinsVal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xpVal)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 248);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(514, 22);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(74, 17);
            this.toolStripStatusLabel1.Text = "Please wait...";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.groupBox1);
            this.flowLayoutPanel1.Controls.Add(this.groupBox2);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(12, 12);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(490, 230);
            this.flowLayoutPanel1.TabIndex = 4;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.creditsVal);
            this.groupBox1.Controls.Add(this.addCreditsBtn);
            this.groupBox1.Controls.Add(this.wheelspinsVal);
            this.groupBox1.Controls.Add(this.addWheelspinsBtn);
            this.groupBox1.Controls.Add(this.superWheelspinsVal);
            this.groupBox1.Controls.Add(this.addSuperWheelspinsBtn);
            this.groupBox1.Controls.Add(this.xpVal);
            this.groupBox1.Controls.Add(this.addXpBtn);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(269, 215);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Self";
            // 
            // creditsVal
            // 
            this.creditsVal.Location = new System.Drawing.Point(6, 25);
            this.creditsVal.Maximum = new decimal(new int[] {
            999999999,
            0,
            0,
            0});
            this.creditsVal.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.creditsVal.Name = "creditsVal";
            this.creditsVal.Size = new System.Drawing.Size(120, 20);
            this.creditsVal.TabIndex = 1;
            this.creditsVal.Value = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            // 
            // addCreditsBtn
            // 
            this.addCreditsBtn.Location = new System.Drawing.Point(131, 23);
            this.addCreditsBtn.Name = "addCreditsBtn";
            this.addCreditsBtn.Size = new System.Drawing.Size(130, 23);
            this.addCreditsBtn.TabIndex = 2;
            this.addCreditsBtn.Text = "Add Credits";
            this.addCreditsBtn.UseVisualStyleBackColor = true;
            this.addCreditsBtn.Click += new System.EventHandler(this.addCreditsBtn_Click);
            // 
            // wheelspinsVal
            // 
            this.wheelspinsVal.Location = new System.Drawing.Point(6, 65);
            this.wheelspinsVal.Maximum = new decimal(new int[] {
            999999999,
            0,
            0,
            0});
            this.wheelspinsVal.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.wheelspinsVal.Name = "wheelspinsVal";
            this.wheelspinsVal.Size = new System.Drawing.Size(120, 20);
            this.wheelspinsVal.TabIndex = 3;
            this.wheelspinsVal.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // addWheelspinsBtn
            // 
            this.addWheelspinsBtn.Location = new System.Drawing.Point(131, 63);
            this.addWheelspinsBtn.Name = "addWheelspinsBtn";
            this.addWheelspinsBtn.Size = new System.Drawing.Size(130, 23);
            this.addWheelspinsBtn.TabIndex = 4;
            this.addWheelspinsBtn.Text = "Add Wheelspins";
            this.addWheelspinsBtn.UseVisualStyleBackColor = true;
            this.addWheelspinsBtn.Click += new System.EventHandler(this.addWheelspinsBtn_Click);
            // 
            // superWheelspinsVal
            // 
            this.superWheelspinsVal.Location = new System.Drawing.Point(6, 105);
            this.superWheelspinsVal.Maximum = new decimal(new int[] {
            999999999,
            0,
            0,
            0});
            this.superWheelspinsVal.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.superWheelspinsVal.Name = "superWheelspinsVal";
            this.superWheelspinsVal.Size = new System.Drawing.Size(120, 20);
            this.superWheelspinsVal.TabIndex = 5;
            this.superWheelspinsVal.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // addSuperWheelspinsBtn
            // 
            this.addSuperWheelspinsBtn.Location = new System.Drawing.Point(131, 103);
            this.addSuperWheelspinsBtn.Name = "addSuperWheelspinsBtn";
            this.addSuperWheelspinsBtn.Size = new System.Drawing.Size(130, 23);
            this.addSuperWheelspinsBtn.TabIndex = 6;
            this.addSuperWheelspinsBtn.Text = "Add Super Wheelspins";
            this.addSuperWheelspinsBtn.UseVisualStyleBackColor = true;
            this.addSuperWheelspinsBtn.Click += new System.EventHandler(this.addSuperWheelspinsBtn_Click);
            // 
            // xpVal
            // 
            this.xpVal.Location = new System.Drawing.Point(6, 145);
            this.xpVal.Maximum = new decimal(new int[] {
            999999999,
            0,
            0,
            0});
            this.xpVal.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.xpVal.Name = "xpVal";
            this.xpVal.Size = new System.Drawing.Size(120, 20);
            this.xpVal.TabIndex = 7;
            this.xpVal.Value = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            // 
            // addXpBtn
            // 
            this.addXpBtn.Location = new System.Drawing.Point(131, 143);
            this.addXpBtn.Name = "addXpBtn";
            this.addXpBtn.Size = new System.Drawing.Size(130, 23);
            this.addXpBtn.TabIndex = 8;
            this.addXpBtn.Text = "Add XP";
            this.addXpBtn.UseVisualStyleBackColor = true;
            this.addXpBtn.Click += new System.EventHandler(this.addXpBtn_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.autoshowAvailableLabel);
            this.groupBox2.Controls.Add(this.carIdsTextBox);
            this.groupBox2.Controls.Add(this.autoshowAllfree);
            this.groupBox2.Controls.Add(this.addAllCarsBtn);
            this.groupBox2.Controls.Add(this.showAllCarsBtn);
            this.groupBox2.Controls.Add(this.makeCarsFreeBtn);
            this.groupBox2.Controls.Add(this.autoshowAvailableBox);
            this.groupBox2.Location = new System.Drawing.Point(278, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(200, 215);
            this.groupBox2.TabIndex = 9;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Autoshow";
            // 
            // autoshowAvailableLabel
            // 
            this.autoshowAvailableLabel.AutoSize = true;
            this.autoshowAvailableLabel.Location = new System.Drawing.Point(7, 19);
            this.autoshowAvailableLabel.Name = "autoshowAvailableLabel";
            this.autoshowAvailableLabel.Size = new System.Drawing.Size(44, 13);
            this.autoshowAvailableLabel.TabIndex = 1;
            this.autoshowAvailableLabel.Text = "Car IDs";
            // 
            // carIdsTextBox
            // 
            this.carIdsTextBox.Location = new System.Drawing.Point(10, 35);
            this.carIdsTextBox.Name = "carIdsTextBox";
            this.carIdsTextBox.Size = new System.Drawing.Size(178, 20);
            this.carIdsTextBox.TabIndex = 2;
            this.carIdsTextBox.Text = "1, 2, 3";
            // 
            // autoshowAllfree
            // 
            this.autoshowAllfree.Location = new System.Drawing.Point(10, 62);
            this.autoshowAllfree.Name = "autoshowAllfree";
            this.autoshowAllfree.Size = new System.Drawing.Size(178, 23);
            this.autoshowAllfree.TabIndex = 3;
            this.autoshowAllfree.Text = "Add Cars by ID";
            this.autoshowAllfree.UseVisualStyleBackColor = true;
            this.autoshowAllfree.Click += new System.EventHandler(this.autoshowAllfree_Click);
            // 
            // addAllCarsBtn
            // 
            this.addAllCarsBtn.Location = new System.Drawing.Point(10, 95);
            this.addAllCarsBtn.Name = "addAllCarsBtn";
            this.addAllCarsBtn.Size = new System.Drawing.Size(178, 23);
            this.addAllCarsBtn.TabIndex = 4;
            this.addAllCarsBtn.Text = "Add All Cars to Garage";
            this.addAllCarsBtn.UseVisualStyleBackColor = true;
            this.addAllCarsBtn.Click += new System.EventHandler(this.addAllCarsBtn_Click);
            // 
            // showAllCarsBtn
            // 
            this.showAllCarsBtn.Location = new System.Drawing.Point(10, 128);
            this.showAllCarsBtn.Name = "showAllCarsBtn";
            this.showAllCarsBtn.Size = new System.Drawing.Size(178, 23);
            this.showAllCarsBtn.TabIndex = 5;
            this.showAllCarsBtn.Text = "Show Rare Cars (Safe)";
            this.showAllCarsBtn.UseVisualStyleBackColor = true;
            this.showAllCarsBtn.Click += new System.EventHandler(this.showAllCarsBtn_Click);
            // 
            // makeCarsFreeBtn
            // 
            this.makeCarsFreeBtn.Location = new System.Drawing.Point(10, 161);
            this.makeCarsFreeBtn.Name = "makeCarsFreeBtn";
            this.makeCarsFreeBtn.Size = new System.Drawing.Size(178, 23);
            this.makeCarsFreeBtn.TabIndex = 6;
            this.makeCarsFreeBtn.Text = "Make All Cars Free";
            this.makeCarsFreeBtn.UseVisualStyleBackColor = true;
            this.makeCarsFreeBtn.Click += new System.EventHandler(this.makeCarsFreeBtn_Click);
            // 
            // autoshowAvailableBox
            // 
            this.autoshowAvailableBox.FormattingEnabled = true;
            this.autoshowAvailableBox.Items.AddRange(new object[] {
            "Hidden (Default)",
            "Shown",
            "Shown Exclusively"});
            this.autoshowAvailableBox.Location = new System.Drawing.Point(67, 15);
            this.autoshowAvailableBox.Name = "autoshowAvailableBox";
            this.autoshowAvailableBox.Size = new System.Drawing.Size(121, 21);
            this.autoshowAvailableBox.TabIndex = 0;
            this.autoshowAvailableBox.Text = "Hidden (Default)";
            this.autoshowAvailableBox.Visible = false;
            this.autoshowAvailableBox.SelectedIndexChanged += new System.EventHandler(this.autoshowAvailableBox_SelectedIndexChanged);
            // 
            // cursorToggle
            // 
            this.cursorToggle.AutoSize = true;
            this.cursorToggle.Location = new System.Drawing.Point(278, 230);
            this.cursorToggle.Name = "cursorToggle";
            this.cursorToggle.Size = new System.Drawing.Size(157, 17);
            this.cursorToggle.TabIndex = 0;
            this.cursorToggle.Text = "Hide Cursor When Focused";
            this.cursorToggle.UseVisualStyleBackColor = true;
            // 
            // processWaitTimer
            // 
            this.processWaitTimer.Interval = 200;
            this.processWaitTimer.Tick += new System.EventHandler(this.processWaitTimer_Tick);
            // 
            // patternscanTimer
            // 
            this.patternscanTimer.Interval = 10;
            this.patternscanTimer.Tick += new System.EventHandler(this.patternscanTimer_Tick);
            // 
            // mainloopTimer
            // 
            this.mainloopTimer.Interval = 200;
            this.mainloopTimer.Tick += new System.EventHandler(this.mainloopTimer_Tick);
            // 
            // processStartTimer
            // 
            this.processStartTimer.Interval = 30000;
            this.processStartTimer.Tick += new System.EventHandler(this.processStartTimer_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(514, 270);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.statusStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "StandForFH5Revival";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.creditsVal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.wheelspinsVal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.superWheelspinsVal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xpVal)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Timer processWaitTimer;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Timer patternscanTimer;
        private System.Windows.Forms.Timer mainloopTimer;
        private System.Windows.Forms.NumericUpDown creditsVal;
        private System.Windows.Forms.Button addCreditsBtn;
        private System.Windows.Forms.NumericUpDown wheelspinsVal;
        private System.Windows.Forms.Button addWheelspinsBtn;
        private System.Windows.Forms.NumericUpDown superWheelspinsVal;
        private System.Windows.Forms.Button addSuperWheelspinsBtn;
        private System.Windows.Forms.NumericUpDown xpVal;
        private System.Windows.Forms.Button addXpBtn;
        private System.Windows.Forms.CheckBox cursorToggle;
        private System.Windows.Forms.Label autoshowAvailableLabel;
        private System.Windows.Forms.ComboBox autoshowAvailableBox;
        private System.Windows.Forms.Timer processStartTimer;
        private System.Windows.Forms.TextBox carIdsTextBox;
        private System.Windows.Forms.Button autoshowAllfree;
        private System.Windows.Forms.Button addAllCarsBtn;
        private System.Windows.Forms.Button showAllCarsBtn;
        private System.Windows.Forms.Button makeCarsFreeBtn;
    }
}