namespace MouseClickTool
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        // 控件声明
        private System.Windows.Forms.GroupBox gb_Position;
        private System.Windows.Forms.Label lbl_X;
        private System.Windows.Forms.TextBox txt_X;
        private System.Windows.Forms.Label lbl_Y;
        private System.Windows.Forms.TextBox txt_Y;
        private System.Windows.Forms.Button btn_GetPos;
        private System.Windows.Forms.Label lbl_CurrentPos;
        private System.Windows.Forms.GroupBox gb_Interval;
        private System.Windows.Forms.Label lbl_Interval;
        private System.Windows.Forms.NumericUpDown nud_Interval;
        private System.Windows.Forms.Label lbl_Ms;
        private System.Windows.Forms.Button btn_StartStop;
        private System.Windows.Forms.TextBox txt_Log;
        private System.Windows.Forms.Label lbl_ClickCount;
        private System.Windows.Forms.Timer timer_MousePos;
        private System.Windows.Forms.Label lbl_HotkeyInfo;
        private System.Windows.Forms.Label lbl_CaptureHint;
        private System.Windows.Forms.CheckBox chk_Enabled;
        private System.Windows.Forms.Button btn_Add;
        private System.Windows.Forms.Button btn_Update;
        private System.Windows.Forms.Button btn_Delete;
        private System.Windows.Forms.Button btn_Clear;
        private System.Windows.Forms.Button btn_Up;
        private System.Windows.Forms.Button btn_Down;
        private System.Windows.Forms.ListView lv_Points;
        private System.Windows.Forms.ColumnHeader col_Enabled;
        private System.Windows.Forms.ColumnHeader col_X;
        private System.Windows.Forms.ColumnHeader col_Y;
        private System.Windows.Forms.ColumnHeader col_Interval;
        private System.Windows.Forms.GroupBox gb_PointList;
        private System.Windows.Forms.GroupBox gb_Edit;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.gb_Position = new System.Windows.Forms.GroupBox();
            this.btn_GetPos = new System.Windows.Forms.Button();
            this.lbl_CurrentPos = new System.Windows.Forms.Label();
            this.lbl_CaptureHint = new System.Windows.Forms.Label();
            this.gb_Edit = new System.Windows.Forms.GroupBox();
            this.chk_Enabled = new System.Windows.Forms.CheckBox();
            this.btn_Add = new System.Windows.Forms.Button();
            this.btn_Update = new System.Windows.Forms.Button();
            this.btn_Delete = new System.Windows.Forms.Button();
            this.btn_Clear = new System.Windows.Forms.Button();
            this.btn_Up = new System.Windows.Forms.Button();
            this.btn_Down = new System.Windows.Forms.Button();
            this.lbl_Y = new System.Windows.Forms.Label();
            this.txt_Y = new System.Windows.Forms.TextBox();
            this.lbl_X = new System.Windows.Forms.Label();
            this.txt_X = new System.Windows.Forms.TextBox();
            this.gb_Interval = new System.Windows.Forms.GroupBox();
            this.lbl_Ms = new System.Windows.Forms.Label();
            this.nud_Interval = new System.Windows.Forms.NumericUpDown();
            this.lbl_Interval = new System.Windows.Forms.Label();
            this.gb_PointList = new System.Windows.Forms.GroupBox();
            this.lv_Points = new System.Windows.Forms.ListView();
            this.col_Enabled = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.col_X = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.col_Y = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.col_Interval = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btn_StartStop = new System.Windows.Forms.Button();
            this.txt_Log = new System.Windows.Forms.TextBox();
            this.lbl_ClickCount = new System.Windows.Forms.Label();
            this.timer_MousePos = new System.Windows.Forms.Timer(this.components);
            this.lbl_HotkeyInfo = new System.Windows.Forms.Label();
            this.gb_Position.SuspendLayout();
            this.gb_Edit.SuspendLayout();
            this.gb_Interval.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_Interval)).BeginInit();
            this.gb_PointList.SuspendLayout();
            this.SuspendLayout();
            // 
            // gb_Position
            // 
            this.gb_Position.Controls.Add(this.btn_GetPos);
            this.gb_Position.Controls.Add(this.lbl_CurrentPos);
            this.gb_Position.Location = new System.Drawing.Point(9, 11);
            this.gb_Position.Margin = new System.Windows.Forms.Padding(2);
            this.gb_Position.Name = "gb_Position";
            this.gb_Position.Padding = new System.Windows.Forms.Padding(2);
            this.gb_Position.Size = new System.Drawing.Size(184, 96);
            this.gb_Position.TabIndex = 0;
            this.gb_Position.TabStop = false;
            this.gb_Position.Text = "屏幕坐标获取";
            // 
            // btn_GetPos
            // 
            this.btn_GetPos.BackColor = System.Drawing.Color.Green;
            this.btn_GetPos.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.btn_GetPos.Location = new System.Drawing.Point(9, 20);
            this.btn_GetPos.Margin = new System.Windows.Forms.Padding(2);
            this.btn_GetPos.Name = "btn_GetPos";
            this.btn_GetPos.Size = new System.Drawing.Size(145, 36);
            this.btn_GetPos.TabIndex = 0;
            this.btn_GetPos.Text = "获取坐标 (Alt+F3)";
            this.btn_GetPos.UseVisualStyleBackColor = false;
            this.btn_GetPos.Click += new System.EventHandler(this.btn_GetPos_Click);
            // 
            // lbl_CurrentPos
            // 
            this.lbl_CurrentPos.AutoSize = true;
            this.lbl_CurrentPos.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold);
            this.lbl_CurrentPos.Location = new System.Drawing.Point(6, 67);
            this.lbl_CurrentPos.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbl_CurrentPos.Name = "lbl_CurrentPos";
            this.lbl_CurrentPos.Size = new System.Drawing.Size(59, 17);
            this.lbl_CurrentPos.TabIndex = 1;
            this.lbl_CurrentPos.Text = "X: 0, Y: 0";
            // 
            // lbl_CaptureHint
            // 
            this.lbl_CaptureHint.Font = new System.Drawing.Font("微软雅黑", 8F);
            this.lbl_CaptureHint.ForeColor = System.Drawing.Color.DarkGray;
            this.lbl_CaptureHint.Location = new System.Drawing.Point(13, 110);
            this.lbl_CaptureHint.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbl_CaptureHint.Name = "lbl_CaptureHint";
            this.lbl_CaptureHint.Size = new System.Drawing.Size(180, 48);
            this.lbl_CaptureHint.TabIndex = 2;
            this.lbl_CaptureHint.Text = "点击「获取坐标」后，\r\n在目标位置点左键捕获";
            // 
            // gb_Edit
            // 
            this.gb_Edit.Controls.Add(this.chk_Enabled);
            this.gb_Edit.Controls.Add(this.btn_Add);
            this.gb_Edit.Controls.Add(this.btn_Update);
            this.gb_Edit.Controls.Add(this.btn_Delete);
            this.gb_Edit.Controls.Add(this.btn_Clear);
            this.gb_Edit.Controls.Add(this.btn_Up);
            this.gb_Edit.Controls.Add(this.btn_Down);
            this.gb_Edit.Controls.Add(this.lbl_Y);
            this.gb_Edit.Controls.Add(this.txt_Y);
            this.gb_Edit.Controls.Add(this.lbl_X);
            this.gb_Edit.Controls.Add(this.txt_X);
            this.gb_Edit.Controls.Add(this.gb_Interval);
            this.gb_Edit.Location = new System.Drawing.Point(4, 159);
            this.gb_Edit.Margin = new System.Windows.Forms.Padding(2);
            this.gb_Edit.Name = "gb_Edit";
            this.gb_Edit.Padding = new System.Windows.Forms.Padding(2);
            this.gb_Edit.Size = new System.Drawing.Size(187, 162);
            this.gb_Edit.TabIndex = 1;
            this.gb_Edit.TabStop = false;
            this.gb_Edit.Text = "编辑点位";
            // 
            // chk_Enabled
            // 
            this.chk_Enabled.AutoSize = true;
            this.chk_Enabled.Checked = true;
            this.chk_Enabled.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_Enabled.Location = new System.Drawing.Point(9, 86);
            this.chk_Enabled.Margin = new System.Windows.Forms.Padding(2);
            this.chk_Enabled.Name = "chk_Enabled";
            this.chk_Enabled.Size = new System.Drawing.Size(48, 16);
            this.chk_Enabled.TabIndex = 10;
            this.chk_Enabled.Text = "启用";
            this.chk_Enabled.UseVisualStyleBackColor = true;
            // 
            // btn_Add
            // 
            this.btn_Add.Location = new System.Drawing.Point(9, 106);
            this.btn_Add.Margin = new System.Windows.Forms.Padding(2);
            this.btn_Add.Name = "btn_Add";
            this.btn_Add.Size = new System.Drawing.Size(45, 22);
            this.btn_Add.TabIndex = 9;
            this.btn_Add.Text = "添加";
            this.btn_Add.UseVisualStyleBackColor = true;
            this.btn_Add.Click += new System.EventHandler(this.btn_Add_Click);
            // 
            // btn_Update
            // 
            this.btn_Update.Location = new System.Drawing.Point(62, 106);
            this.btn_Update.Margin = new System.Windows.Forms.Padding(2);
            this.btn_Update.Name = "btn_Update";
            this.btn_Update.Size = new System.Drawing.Size(45, 22);
            this.btn_Update.TabIndex = 8;
            this.btn_Update.Text = "更新";
            this.btn_Update.UseVisualStyleBackColor = true;
            this.btn_Update.Click += new System.EventHandler(this.btn_Update_Click);
            // 
            // btn_Delete
            // 
            this.btn_Delete.Location = new System.Drawing.Point(115, 106);
            this.btn_Delete.Margin = new System.Windows.Forms.Padding(2);
            this.btn_Delete.Name = "btn_Delete";
            this.btn_Delete.Size = new System.Drawing.Size(45, 22);
            this.btn_Delete.TabIndex = 7;
            this.btn_Delete.Text = "删除";
            this.btn_Delete.UseVisualStyleBackColor = true;
            this.btn_Delete.Click += new System.EventHandler(this.btn_Delete_Click);
            // 
            // btn_Clear
            // 
            this.btn_Clear.Location = new System.Drawing.Point(9, 133);
            this.btn_Clear.Margin = new System.Windows.Forms.Padding(2);
            this.btn_Clear.Name = "btn_Clear";
            this.btn_Clear.Size = new System.Drawing.Size(45, 22);
            this.btn_Clear.TabIndex = 6;
            this.btn_Clear.Text = "清空";
            this.btn_Clear.UseVisualStyleBackColor = true;
            this.btn_Clear.Click += new System.EventHandler(this.btn_Clear_Click);
            // 
            // btn_Up
            // 
            this.btn_Up.Location = new System.Drawing.Point(62, 133);
            this.btn_Up.Margin = new System.Windows.Forms.Padding(2);
            this.btn_Up.Name = "btn_Up";
            this.btn_Up.Size = new System.Drawing.Size(45, 22);
            this.btn_Up.TabIndex = 5;
            this.btn_Up.Text = "上移";
            this.btn_Up.UseVisualStyleBackColor = true;
            this.btn_Up.Click += new System.EventHandler(this.btn_Up_Click);
            // 
            // btn_Down
            // 
            this.btn_Down.Location = new System.Drawing.Point(115, 133);
            this.btn_Down.Margin = new System.Windows.Forms.Padding(2);
            this.btn_Down.Name = "btn_Down";
            this.btn_Down.Size = new System.Drawing.Size(45, 22);
            this.btn_Down.TabIndex = 4;
            this.btn_Down.Text = "下移";
            this.btn_Down.UseVisualStyleBackColor = true;
            this.btn_Down.Click += new System.EventHandler(this.btn_Down_Click);
            // 
            // lbl_Y
            // 
            this.lbl_Y.AutoSize = true;
            this.lbl_Y.Location = new System.Drawing.Point(93, 20);
            this.lbl_Y.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbl_Y.Name = "lbl_Y";
            this.lbl_Y.Size = new System.Drawing.Size(17, 12);
            this.lbl_Y.TabIndex = 3;
            this.lbl_Y.Text = "Y:";
            // 
            // txt_Y
            // 
            this.txt_Y.Location = new System.Drawing.Point(112, 18);
            this.txt_Y.Margin = new System.Windows.Forms.Padding(2);
            this.txt_Y.Name = "txt_Y";
            this.txt_Y.Size = new System.Drawing.Size(60, 21);
            this.txt_Y.TabIndex = 2;
            this.txt_Y.Text = "100";
            // 
            // lbl_X
            // 
            this.lbl_X.AutoSize = true;
            this.lbl_X.Location = new System.Drawing.Point(8, 20);
            this.lbl_X.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbl_X.Name = "lbl_X";
            this.lbl_X.Size = new System.Drawing.Size(17, 12);
            this.lbl_X.TabIndex = 1;
            this.lbl_X.Text = "X:";
            // 
            // txt_X
            // 
            this.txt_X.Location = new System.Drawing.Point(27, 18);
            this.txt_X.Margin = new System.Windows.Forms.Padding(2);
            this.txt_X.Name = "txt_X";
            this.txt_X.Size = new System.Drawing.Size(60, 21);
            this.txt_X.TabIndex = 0;
            this.txt_X.Text = "100";
            // 
            // gb_Interval
            // 
            this.gb_Interval.Controls.Add(this.lbl_Ms);
            this.gb_Interval.Controls.Add(this.nud_Interval);
            this.gb_Interval.Controls.Add(this.lbl_Interval);
            this.gb_Interval.Location = new System.Drawing.Point(4, 44);
            this.gb_Interval.Margin = new System.Windows.Forms.Padding(2);
            this.gb_Interval.Name = "gb_Interval";
            this.gb_Interval.Padding = new System.Windows.Forms.Padding(2);
            this.gb_Interval.Size = new System.Drawing.Size(167, 40);
            this.gb_Interval.TabIndex = 0;
            this.gb_Interval.TabStop = false;
            this.gb_Interval.Text = "点击间隔";
            // 
            // lbl_Ms
            // 
            this.lbl_Ms.AutoSize = true;
            this.lbl_Ms.Location = new System.Drawing.Point(112, 18);
            this.lbl_Ms.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbl_Ms.Name = "lbl_Ms";
            this.lbl_Ms.Size = new System.Drawing.Size(29, 12);
            this.lbl_Ms.TabIndex = 2;
            this.lbl_Ms.Text = "毫秒";
            // 
            // nud_Interval
            // 
            this.nud_Interval.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nud_Interval.Location = new System.Drawing.Point(32, 15);
            this.nud_Interval.Margin = new System.Windows.Forms.Padding(2);
            this.nud_Interval.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nud_Interval.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nud_Interval.Name = "nud_Interval";
            this.nud_Interval.Size = new System.Drawing.Size(75, 21);
            this.nud_Interval.TabIndex = 1;
            this.nud_Interval.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // lbl_Interval
            // 
            this.lbl_Interval.AutoSize = true;
            this.lbl_Interval.Location = new System.Drawing.Point(4, 18);
            this.lbl_Interval.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbl_Interval.Name = "lbl_Interval";
            this.lbl_Interval.Size = new System.Drawing.Size(35, 12);
            this.lbl_Interval.TabIndex = 0;
            this.lbl_Interval.Text = "间隔:";
            // 
            // gb_PointList
            // 
            this.gb_PointList.Controls.Add(this.lv_Points);
            this.gb_PointList.Location = new System.Drawing.Point(197, 14);
            this.gb_PointList.Margin = new System.Windows.Forms.Padding(2);
            this.gb_PointList.Name = "gb_PointList";
            this.gb_PointList.Padding = new System.Windows.Forms.Padding(2);
            this.gb_PointList.Size = new System.Drawing.Size(323, 272);
            this.gb_PointList.TabIndex = 2;
            this.gb_PointList.TabStop = false;
            this.gb_PointList.Text = "点位列表";
            // 
            // lv_Points
            // 
            this.lv_Points.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.col_Enabled,
            this.col_X,
            this.col_Y,
            this.col_Interval});
            this.lv_Points.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lv_Points.FullRowSelect = true;
            this.lv_Points.GridLines = true;
            this.lv_Points.HideSelection = false;
            this.lv_Points.Location = new System.Drawing.Point(2, 16);
            this.lv_Points.Margin = new System.Windows.Forms.Padding(2);
            this.lv_Points.Name = "lv_Points";
            this.lv_Points.Size = new System.Drawing.Size(319, 254);
            this.lv_Points.TabIndex = 0;
            this.lv_Points.UseCompatibleStateImageBehavior = false;
            this.lv_Points.View = System.Windows.Forms.View.Details;
            // 
            // col_Enabled
            // 
            this.col_Enabled.Text = "启用";
            this.col_Enabled.Width = 50;
            // 
            // col_X
            // 
            this.col_X.Text = "X";
            this.col_X.Width = 80;
            // 
            // col_Y
            // 
            this.col_Y.Text = "Y";
            this.col_Y.Width = 80;
            // 
            // col_Interval
            // 
            this.col_Interval.Text = "间隔(ms)";
            this.col_Interval.Width = 100;
            // 
            // btn_StartStop
            // 
            this.btn_StartStop.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold);
            this.btn_StartStop.Location = new System.Drawing.Point(5, 336);
            this.btn_StartStop.Margin = new System.Windows.Forms.Padding(2);
            this.btn_StartStop.Name = "btn_StartStop";
            this.btn_StartStop.Size = new System.Drawing.Size(186, 36);
            this.btn_StartStop.TabIndex = 3;
            this.btn_StartStop.Text = "开始 (Alt+F2)";
            this.btn_StartStop.UseVisualStyleBackColor = true;
            this.btn_StartStop.Click += new System.EventHandler(this.btn_StartStop_Click);
            // 
            // txt_Log
            // 
            this.txt_Log.Location = new System.Drawing.Point(199, 285);
            this.txt_Log.Margin = new System.Windows.Forms.Padding(2);
            this.txt_Log.Multiline = true;
            this.txt_Log.Name = "txt_Log";
            this.txt_Log.ReadOnly = true;
            this.txt_Log.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txt_Log.Size = new System.Drawing.Size(319, 161);
            this.txt_Log.TabIndex = 4;
            // 
            // lbl_ClickCount
            // 
            this.lbl_ClickCount.AutoSize = true;
            this.lbl_ClickCount.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold);
            this.lbl_ClickCount.Location = new System.Drawing.Point(4, 390);
            this.lbl_ClickCount.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbl_ClickCount.Name = "lbl_ClickCount";
            this.lbl_ClickCount.Size = new System.Drawing.Size(70, 17);
            this.lbl_ClickCount.TabIndex = 5;
            this.lbl_ClickCount.Text = "点击次数: 0";
            // 
            // timer_MousePos
            // 
            this.timer_MousePos.Tick += new System.EventHandler(this.timer_MousePos_Tick);
            // 
            // lbl_HotkeyInfo
            // 
            this.lbl_HotkeyInfo.AutoSize = true;
            this.lbl_HotkeyInfo.Location = new System.Drawing.Point(4, 418);
            this.lbl_HotkeyInfo.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbl_HotkeyInfo.Name = "lbl_HotkeyInfo";
            this.lbl_HotkeyInfo.Size = new System.Drawing.Size(173, 24);
            this.lbl_HotkeyInfo.TabIndex = 6;
            this.lbl_HotkeyInfo.Text = "快捷键：Alt+F2 - 开始/停止，\r\nAlt+F3 - 获取坐标";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(525, 457);
            this.Controls.Add(this.lbl_HotkeyInfo);
            this.Controls.Add(this.lbl_ClickCount);
            this.Controls.Add(this.lbl_CaptureHint);
            this.Controls.Add(this.txt_Log);
            this.Controls.Add(this.btn_StartStop);
            this.Controls.Add(this.gb_PointList);
            this.Controls.Add(this.gb_Edit);
            this.Controls.Add(this.gb_Position);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "鼠标自动点击工具 - 多位置版";
            this.gb_Position.ResumeLayout(false);
            this.gb_Position.PerformLayout();
            this.gb_Edit.ResumeLayout(false);
            this.gb_Edit.PerformLayout();
            this.gb_Interval.ResumeLayout(false);
            this.gb_Interval.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_Interval)).EndInit();
            this.gb_PointList.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}