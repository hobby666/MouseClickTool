using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MouseClickTool
{
    public partial class Form1 : Form
    {
        #region Windows API 导入

        // 获取窗口标题相关的 API
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", ExactSpelling = true)]
        private static extern IntPtr GetAncestor(IntPtr hwnd, uint gaFlags);

        private const uint GA_ROOT = 2; // 获取根窗口标志

        // 强制同步发送消息（比 PostMessage 更可靠）
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        // 获取控件的真实类名，用于透视绑定目标
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, System.Text.StringBuilder lpClassName, int nMaxCount);

        // 根据父窗口的相对坐标，获取该位置的子窗口句柄
        [DllImport("user32.dll")]
        private static extern IntPtr ChildWindowFromPoint(IntPtr hWndParent, POINT Point);

        // 将相对坐标转换为屏幕物理坐标
        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        // 全局物理鼠标移动与点击 API (用于默认不指定窗体的模式)
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

        // 获取指定坐标处的窗口句柄
        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(POINT Point);

        // 将屏幕坐标转换为相对窗口的客户区坐标
        [DllImport("user32.dll")]
        private static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public int mouseData;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        // 后台鼠标消息常量
        private const uint WM_LBUTTONDOWN = 0x0201;
        private const uint WM_LBUTTONUP = 0x0202;
        private const int MK_LBUTTON = 0x0001;

        // 全局物理鼠标消息常量
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;

        #endregion

        #region 字段

        private bool _isRunning = false;
        private bool _isCapturing = false;
        private CancellationTokenSource _cancellationTokenSource;
        private int _clickCount = 0;
        private LowLevelMouseProc _mouseProc;
        private IntPtr _mouseHookID = IntPtr.Zero;
        private List<ClickPoint> _clickPoints = new List<ClickPoint>();
        private int _selectedIndex = -1;

        // 目标窗口句柄
        private IntPtr _targetHandle = IntPtr.Zero;

        #endregion

        #region 构造函数

        public Form1()
        {
            InitializeComponent();
            InitializeControls();
            KeyboardHook.Start();
            KeyboardHook.HotKeyPressed += OnHotKeyPressed;
        }

        #endregion

        #region 初始化

        private void InitializeControls()
        {
            // 设置默认间隔
            nud_Interval.Value = 1000;
            lbl_CurrentPos.Text = "X: 0, Y: 0";

            // 初始化绑定提示
            if (this.Controls.ContainsKey("lbl_BindStatus"))
            {
                lbl_BindStatus.Text = "状态: 未绑定 \r\n(选中目标按 Alt+W 绑定窗体)";
                lbl_BindStatus.ForeColor = Color.Red;
            }

            UpdateUI();
            timer_MousePos.Start();

            // 设置快捷键提示
            lbl_HotkeyInfo.Text = "快捷键：Alt+F2 - 开始/停止\r\nAlt+F3 - 获取坐标\r\nAlt+W - 绑定后台窗口";
            lbl_HotkeyInfo.ForeColor = Color.Gray;
            lbl_CaptureHint.Text = "请按需勾选指定窗体！\r\n点击「获取坐标」后捕获坐标";

            // 绑定 ListView 事件
            lv_Points.SelectedIndexChanged += Lv_Points_SelectedIndexChanged;
            lv_Points.MouseDoubleClick += Lv_Points_MouseDoubleClick;

            UpdateButtonStates();
        }

        #endregion

        #region 点击点管理

        private class ClickPoint
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Interval { get; set; }
            public bool IsEnabled { get; set; } = true;
        }

        private void RefreshPointList()
        {
            lv_Points.Items.Clear();
            foreach (var point in _clickPoints)
            {
                var item = new ListViewItem(point.IsEnabled ? "✓" : "✗");
                item.SubItems.Add(point.X.ToString());
                item.SubItems.Add(point.Y.ToString());
                item.SubItems.Add(point.Interval.ToString());
                item.Tag = point;
                lv_Points.Items.Add(item);
            }
            UpdateButtonStates();
        }

        private void Lv_Points_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lv_Points.SelectedItems.Count > 0)
            {
                _selectedIndex = lv_Points.SelectedItems[0].Index;
                var point = _clickPoints[_selectedIndex];
                txt_X.Text = point.X.ToString();
                txt_Y.Text = point.Y.ToString();
                nud_Interval.Value = point.Interval;
                chk_Enabled.Checked = point.IsEnabled;
            }
            else
            {
                _selectedIndex = -1;
            }
            UpdateButtonStates();
        }

        private void Lv_Points_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lv_Points.SelectedItems.Count > 0)
            {
                EditSelectedPoint();
            }
        }

        private void EditSelectedPoint()
        {
            if (_selectedIndex < 0 || _selectedIndex >= _clickPoints.Count) return;

            var point = _clickPoints[_selectedIndex];
            point.X = int.Parse(txt_X.Text);
            point.Y = int.Parse(txt_Y.Text);
            point.Interval = (int)nud_Interval.Value;
            point.IsEnabled = chk_Enabled.Checked;
            RefreshPointList();

            string modeText = chk_BindWindow.Checked ? "相对" : "全局";
            AppendLog($"✅ 已更新点位 {_selectedIndex + 1}: ({modeText}X:{point.X}, {modeText}Y:{point.Y}) 间隔:{point.Interval}ms");
        }

        #endregion

        #region 按钮事件

        private void btn_Add_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txt_X.Text, out int x) || !int.TryParse(txt_Y.Text, out int y))
            {
                MessageBox.Show("请输入有效的坐标！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int interval = (int)nud_Interval.Value;
            _clickPoints.Add(new ClickPoint { X = x, Y = y, Interval = interval, IsEnabled = chk_Enabled.Checked });
            RefreshPointList();

            string modeText = chk_BindWindow.Checked ? "相对" : "全局";
            AppendLog($"✅ 添加{modeText}坐标点位 ({x}, {y}) 间隔:{interval}ms");
        }

        private void btn_Update_Click(object sender, EventArgs e)
        {
            EditSelectedPoint();
        }

        private void btn_Delete_Click(object sender, EventArgs e)
        {
            if (_selectedIndex < 0 || _selectedIndex >= _clickPoints.Count) return;

            var point = _clickPoints[_selectedIndex];
            _clickPoints.RemoveAt(_selectedIndex);
            _selectedIndex = -1;
            RefreshPointList();
            AppendLog($"🗑️ 删除点位 ({point.X}, {point.Y})");
        }

        private void btn_Clear_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要清空所有点位吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _clickPoints.Clear();
                RefreshPointList();
                AppendLog("🗑️ 已清空所有点位");
            }
        }

        private void btn_Up_Click(object sender, EventArgs e)
        {
            if (_selectedIndex > 0)
            {
                var temp = _clickPoints[_selectedIndex];
                _clickPoints[_selectedIndex] = _clickPoints[_selectedIndex - 1];
                _clickPoints[_selectedIndex - 1] = temp;
                _selectedIndex--;
                RefreshPointList();
                if (_selectedIndex >= 0 && _selectedIndex < lv_Points.Items.Count)
                {
                    lv_Points.Items[_selectedIndex].Selected = true;
                }
            }
        }

        private void btn_Down_Click(object sender, EventArgs e)
        {
            if (_selectedIndex >= 0 && _selectedIndex < _clickPoints.Count - 1)
            {
                var temp = _clickPoints[_selectedIndex];
                _clickPoints[_selectedIndex] = _clickPoints[_selectedIndex + 1];
                _clickPoints[_selectedIndex + 1] = temp;
                _selectedIndex++;
                RefreshPointList();
                if (_selectedIndex >= 0 && _selectedIndex < lv_Points.Items.Count)
                {
                    lv_Points.Items[_selectedIndex].Selected = true;
                }
            }
        }

        #endregion

        #region 坐标与句柄捕获

        // 辅助方法：获取窗口的文本/标题
        private string GetWindowTitle(IntPtr hwnd)
        {
            int length = GetWindowTextLength(hwnd);
            if (length == 0) return "";

            System.Text.StringBuilder sb = new System.Text.StringBuilder(length + 1);
            GetWindowText(hwnd, sb, sb.Capacity);
            return sb.ToString();
        }

        // 绑定目标窗口句柄逻辑 (自动绑定主程序)
        private void BindTargetWindow()
        {
            if (!chk_BindWindow.Checked)
            {
                AppendLog("⚠️ 当前为全局屏幕模式，无需绑定窗体。请勾选复选框开启后台模式。");
                return;
            }

            GetCursorPos(out POINT point);
            IntPtr hwnd = WindowFromPoint(point);

            if (hwnd != IntPtr.Zero)
            {
                IntPtr rootHwnd = GetAncestor(hwnd, GA_ROOT);
                if (rootHwnd == IntPtr.Zero) rootHwnd = hwnd;

                _targetHandle = rootHwnd;

                string windowTitle = GetWindowTitle(rootHwnd);
                if (string.IsNullOrWhiteSpace(windowTitle)) windowTitle = "无标题程序";

                // 【新增】限制标题最大显示长度（比如最多显示 10 个字符，超出的变成 ...）
                int maxLength = 7;
                string displayTitle = windowTitle.Length > maxLength ? windowTitle.Substring(0, maxLength) + ".." : windowTitle;

                if (this.Controls.ContainsKey("lbl_BindStatus") && lbl_BindStatus != null)
                {
                    lbl_BindStatus.Text = $"已绑定主程序：{displayTitle}";
                    lbl_BindStatus.ForeColor = Color.Green;
                    // ==================== 就是在这里添加 ====================
                    toolTip1.SetToolTip(lbl_BindStatus, $"已绑定主程序：{windowTitle}");
                    // ========================================================
                }
                AppendLog($"🔗 绑定主窗口: [{windowTitle}]，句柄: {rootHwnd.ToString("X")}");
            }
        }

        // 核心算法：从主窗口根据坐标层层向下寻找，找出真正被点击的底层控件
        private IntPtr ResolveChildHandle(IntPtr parentHandle, POINT parentPt, out POINT childPt)
        {
            IntPtr currentHandle = parentHandle;
            POINT currentPt = parentPt;

            while (true)
            {
                IntPtr childHandle = ChildWindowFromPoint(currentHandle, currentPt);

                if (childHandle == IntPtr.Zero || childHandle == currentHandle)
                {
                    break;
                }

                POINT screenPt = currentPt;
                ClientToScreen(currentHandle, ref screenPt);

                POINT nextPt = screenPt;
                ScreenToClient(childHandle, ref nextPt);

                currentHandle = childHandle;
                currentPt = nextPt;
            }

            childPt = currentPt;
            return currentHandle;
        }

        private void btn_GetPos_Click(object sender, EventArgs e)
        {
            if (chk_BindWindow.Checked && _targetHandle == IntPtr.Zero)
            {
                MessageBox.Show("您启用了后台指定窗体模式，请先选中目标窗口并按 [Alt+W] 绑定！", "未绑定窗口", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_isCapturing)
            {
                ExitCaptureMode();
                return;
            }
            EnterCaptureMode();
        }

        private void EnterCaptureMode()
        {
            _isCapturing = true;
            btn_GetPos.Text = "取消捕获 (Esc)";
            btn_GetPos.BackColor = Color.Orange;
            btn_GetPos.ForeColor = Color.White;
            lbl_CaptureHint.Text = chk_BindWindow.Checked ? "🖱️ 请在目标窗口内点击左键捕获相对坐标" : "🖱️ 请点击左键捕获屏幕全局坐标";
            lbl_CaptureHint.ForeColor = Color.Blue;
            this.Cursor = Cursors.Cross;

            StartMouseHook();
            AppendLog("进入坐标捕获模式 - 请点击目标位置");
        }

        private void ExitCaptureMode()
        {
            _isCapturing = false;
            btn_GetPos.Text = "获取坐标 (Alt+F3)";
            btn_GetPos.BackColor = Color.Green;
            btn_GetPos.ForeColor = Color.White;
            lbl_CaptureHint.Text = "点击「获取坐标」后，在目\r\n标位置点击左键即可捕获坐标";
            lbl_CaptureHint.ForeColor = Color.DarkGray;
            this.Cursor = Cursors.Default;

            StopMouseHook();
        }

        private void CaptureCoordinate(int screenX, int screenY)
        {
            if (!_isCapturing) return;

            if (chk_BindWindow.Checked && _targetHandle != IntPtr.Zero)
            {
                // 后台模式：转换为相对坐标
                POINT p = new POINT { X = screenX, Y = screenY };
                ScreenToClient(_targetHandle, ref p);

                txt_X.Text = p.X.ToString();
                txt_Y.Text = p.Y.ToString();
                lbl_CurrentPos.Text = $"相对X: {p.X}, 相对Y: {p.Y}";
                AppendLog($"✅ 捕获相对坐标: ({p.X}, {p.Y})");
            }
            else
            {
                // 全局模式：直接使用物理坐标
                txt_X.Text = screenX.ToString();
                txt_Y.Text = screenY.ToString();
                lbl_CurrentPos.Text = $"全局X: {screenX}, 全局Y: {screenY}";
                AppendLog($"✅ 捕获全局坐标: ({screenX}, {screenY})");
            }

            lbl_CurrentPos.BackColor = Color.LightGreen;
            Task.Delay(300).ContinueWith(_ =>
            {
                this.Invoke((MethodInvoker)delegate
                {
                    lbl_CurrentPos.BackColor = SystemColors.Control;
                });
            });

            ExitCaptureMode();
        }

        #endregion

        #region 鼠标钩子 (仅用于捕获)

        private void StartMouseHook()
        {
            _mouseProc = MouseHookCallback;
            _mouseHookID = SetWindowsHookEx(WH_MOUSE_LL, _mouseProc,
                GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);
        }

        private void StopMouseHook()
        {
            if (_mouseHookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_mouseHookID);
                _mouseHookID = IntPtr.Zero;
                _mouseProc = null;
            }
        }

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && _isCapturing)
            {
                if (wParam == (IntPtr)WM_LBUTTONDOWN)
                {
                    GetCursorPos(out POINT point);
                    this.Invoke((MethodInvoker)delegate
                    {
                        CaptureCoordinate(point.X, point.Y);
                    });
                    return (IntPtr)1;
                }
            }
            return CallNextHookEx(_mouseHookID, nCode, wParam, lParam);
        }

        #endregion

        #region 核心逻辑 (双轨制：后台无感点击 / 屏幕物理点击)

        // 辅助方法：生成 lParam
        private IntPtr MakeLParam(int x, int y)
        {
            return (IntPtr)((y << 16) | (x & 0xFFFF));
        }

        private async void btn_StartStop_Click(object sender, EventArgs e)
        {
            if (_isCapturing)
            {
                MessageBox.Show("请先退出坐标捕获模式！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (chk_BindWindow.Checked && _targetHandle == IntPtr.Zero)
            {
                MessageBox.Show("启用了后台指定窗体，请先按 Alt+W 绑定目标窗口！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_clickPoints.Count == 0)
            {
                MessageBox.Show("请先添加点击点位！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_isRunning)
            {
                _isRunning = true;
                UpdateUI();

                var enabledPoints = new List<ClickPoint>();
                foreach (var point in _clickPoints)
                {
                    if (point.IsEnabled)
                    {
                        enabledPoints.Add(point);
                    }
                }

                if (enabledPoints.Count == 0)
                {
                    MessageBox.Show("没有启用的点击点位！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _isRunning = false;
                    UpdateUI();
                    return;
                }

                _clickCount = 0;
                _cancellationTokenSource = new CancellationTokenSource();

                try
                {
                    int loopCount = 0;
                    if (this.Controls.ContainsKey("nud_LoopCount") && nud_LoopCount != null)
                    {
                        loopCount = (int)nud_LoopCount.Value;
                    }

                    await StartMultiClicking(enabledPoints, loopCount, _cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    AppendLog("⚠️ 用户手动中止了任务");
                }
                finally
                {
                    _isRunning = false;
                    UpdateUI();
                }
            }
            else
            {
                _cancellationTokenSource?.Cancel();
                _isRunning = false;
                UpdateUI();
            }
        }

        private async Task StartMultiClicking(List<ClickPoint> points, int maxLoopCount, CancellationToken token)
        {
            string loopTarget = maxLoopCount == 0 ? "无限" : maxLoopCount.ToString();
            string modeName = chk_BindWindow.Checked ? "纯后台穿透连点" : "全局屏幕物理连点";
            AppendLog($"▶ 开始 {modeName} - 共 {points.Count} 个点位，计划循环: {loopTarget} 次");

            _clickCount = 0;
            int currentLoop = 0;

            while (!token.IsCancellationRequested && (maxLoopCount == 0 || currentLoop < maxLoopCount))
            {
                for (int i = 0; i < points.Count; i++)
                {
                    if (token.IsCancellationRequested) break;

                    var point = points[i];

                    // 【逻辑分支】根据是否勾选决定点击模式
                    if (chk_BindWindow.Checked)
                    {
                        // 模式 1: 后台消息投递（无视遮挡）
                        POINT mainPt = new POINT { X = point.X, Y = point.Y };
                        IntPtr actualTargetHandle = ResolveChildHandle(_targetHandle, mainPt, out POINT actualChildPt);
                        IntPtr lParam = MakeLParam(actualChildPt.X, actualChildPt.Y);

                        SendMessage(actualTargetHandle, WM_LBUTTONDOWN, (IntPtr)MK_LBUTTON, lParam);
                        await Task.Delay(10, token);
                        SendMessage(actualTargetHandle, WM_LBUTTONUP, IntPtr.Zero, lParam);
                        AppendLog($"后台点击 {i + 1}: (相对X:{point.X}, 相对Y:{point.Y})");
                    }
                    else
                    {
                        // 模式 2: 全局物理鼠标移动与点击（伪无感瞬移）
                        GetCursorPos(out POINT originalPos);
                        SetCursorPos(point.X, point.Y);

                        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                        await Task.Delay(10, token);
                        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);

                        SetCursorPos(originalPos.X, originalPos.Y);
                        AppendLog($"全局点击 {i + 1}: (绝对X:{point.X}, 绝对Y:{point.Y})");
                    }

                    _clickCount++;

                    this.Invoke((MethodInvoker)delegate
                    {
                        int displayLoop = currentLoop + 1;
                        if (maxLoopCount > 0 && displayLoop > maxLoopCount) displayLoop = maxLoopCount;

                        lbl_ClickCount.Text = $"循环进度: {displayLoop}/{loopTarget} | 总点击: {_clickCount}";

                        if (i < lv_Points.Items.Count)
                        {
                            foreach (ListViewItem item in lv_Points.Items)
                            {
                                item.BackColor = SystemColors.Window;
                            }
                            lv_Points.Items[i].BackColor = Color.LightGreen;
                            lv_Points.Items[i].EnsureVisible();
                        }
                    });

                    await Task.Delay(point.Interval, token);
                }

                if (!token.IsCancellationRequested)
                {
                    currentLoop++;
                    AppendLog($"✅ 第 {currentLoop} 次列表循环完成");
                }
            }

            AppendLog($"⏹ 任务结束 - 共完成 {currentLoop} 次循环，总点击 {_clickCount} 次");
        }

        #endregion

        #region 辅助方法

        private void UpdateUI()
        {
            btn_StartStop.Text = _isRunning ? "停止 (Alt+F2)" : "开始 (Alt+F2)";
            btn_StartStop.BackColor = _isRunning ? Color.Red : Color.Green;
            btn_StartStop.ForeColor = Color.White;

            btn_Add.Enabled = !_isRunning;
            btn_Update.Enabled = !_isRunning && _selectedIndex >= 0;
            btn_Delete.Enabled = !_isRunning && _selectedIndex >= 0;
            btn_Clear.Enabled = !_isRunning && _clickPoints.Count > 0;
            btn_Up.Enabled = !_isRunning && _selectedIndex > 0;
            btn_Down.Enabled = !_isRunning && _selectedIndex >= 0 && _selectedIndex < _clickPoints.Count - 1;
            btn_GetPos.Enabled = !_isRunning;
            chk_Enabled.Enabled = !_isRunning;
            txt_X.Enabled = !_isRunning;
            txt_Y.Enabled = !_isRunning;
            nud_Interval.Enabled = !_isRunning;

            // 锁定复选框，防止运行中途切换模式
            chk_BindWindow.Enabled = !_isRunning;

            if (this.Controls.ContainsKey("nud_LoopCount") && nud_LoopCount != null)
            {
                nud_LoopCount.Enabled = !_isRunning;
            }
        }

        private void UpdateButtonStates()
        {
            btn_Update.Enabled = !_isRunning && _selectedIndex >= 0;
            btn_Delete.Enabled = !_isRunning && _selectedIndex >= 0;
            btn_Clear.Enabled = !_isRunning && _clickPoints.Count > 0;
            btn_Up.Enabled = !_isRunning && _selectedIndex > 0;
            btn_Down.Enabled = !_isRunning && _selectedIndex >= 0 && _selectedIndex < _clickPoints.Count - 1;
        }

        private void AppendLog(string message)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate { AppendLog(message); });
                return;
            }
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            txt_Log.AppendText($"[{timestamp}] {message}{Environment.NewLine}");
            txt_Log.ScrollToCaret();
        }

        #endregion

        #region 窗体事件

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && _isCapturing)
            {
                ExitCaptureMode();
                AppendLog("取消坐标捕获");
            }
            base.OnKeyDown(e);
        }

        private void OnHotKeyPressed(object sender, HotKeyEventArgs e)
        {
            if (e.IsAltPressed)
            {
                switch (e.KeyCode)
                {
                    case Keys.F2:
                        // 修复：当窗口在后台时，PerformClick 会失效，必须直接调用底层的 Click 逻辑方法
                        this.Invoke((MethodInvoker)delegate { btn_StartStop_Click(this, EventArgs.Empty); });
                        break;
                    case Keys.F3:
                        // 同理，直接调用方法
                        this.Invoke((MethodInvoker)delegate { btn_GetPos_Click(this, EventArgs.Empty); });
                        break;
                    case Keys.W:
                        this.Invoke((MethodInvoker)delegate { BindTargetWindow(); });
                        break;
                }
            }
        }

        private void timer_MousePos_Tick(object sender, EventArgs e)
        {
            GetCursorPos(out POINT point);

            // 根据复选框状态智能显示实时坐标
            if (chk_BindWindow.Checked && _targetHandle != IntPtr.Zero)
            {
                POINT p = point;
                ScreenToClient(_targetHandle, ref p);
                lbl_CurrentPos.Text = $"相对X: {p.X}, 相对Y: {p.Y}";
            }
            else
            {
                lbl_CurrentPos.Text = $"全局X: {point.X}, 全局Y: {point.Y}";
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            KeyboardHook.Stop();
            StopMouseHook();
            timer_MousePos.Stop();
            base.OnFormClosing(e);
        }

        #endregion

        #region Windows API 常量 

        private const int WH_MOUSE_LL = 14;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
        #endregion

        #region 热键事件参数
        public class HotKeyEventArgs : EventArgs
        {
            public Keys KeyCode { get; set; }
            public bool IsAltPressed { get; set; }
            public HotKeyEventArgs(Keys keyCode, bool isAltPressed)
            {
                KeyCode = keyCode;
                IsAltPressed = isAltPressed;
            }
        }
        #endregion
    }

    #region 键盘钩子类 (增加拦截 Alt+W)

    public static class KeyboardHook
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;

        private static LowLevelKeyboardProc _proc;
        private static IntPtr _hookID = IntPtr.Zero;

        public static event EventHandler<Form1.HotKeyEventArgs> HotKeyPressed;

        public static void Start()
        {
            _proc = HookCallback;
            _hookID = SetHook(_proc);
        }

        public static void Stop()
        {
            if (_hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookID);
                _hookID = IntPtr.Zero;
            }
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;

                bool altPressed = (GetAsyncKeyState(0x12) & 0x8000) != 0;

                if (altPressed)
                {
                    if (key == Keys.F2 || key == Keys.F3 || key == Keys.W)
                    {
                        HotKeyPressed?.Invoke(null, new Form1.HotKeyEventArgs(key, true));
                        return (IntPtr)1;
                    }
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    }
    #endregion
}