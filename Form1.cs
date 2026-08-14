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

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

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

        // 鼠标事件常量
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
            UpdateUI();
            timer_MousePos.Start();

            // 设置快捷键提示
            lbl_HotkeyInfo.Text = "快捷键：Alt+F2 - 开始/停止，\r\n Alt+F3 - 获取坐标";
            lbl_HotkeyInfo.ForeColor = Color.Gray;
            lbl_CaptureHint.Text = "点击「获取坐标」后,\r\n在目标位置点击鼠标左键即可捕获";

            // 绑定 ListView 事件
            lv_Points.SelectedIndexChanged += Lv_Points_SelectedIndexChanged;
            lv_Points.MouseDoubleClick += Lv_Points_MouseDoubleClick;

            // 添加默认点位
            AddDefaultPoints();

            // 更新按钮状态
            UpdateButtonStates();
        }

        private void AddDefaultPoints()
        {
            _clickPoints.Add(new ClickPoint { X = 100, Y = 100, Interval = 1000, IsEnabled = true });
            _clickPoints.Add(new ClickPoint { X = 200, Y = 200, Interval = 1500, IsEnabled = true });
            _clickPoints.Add(new ClickPoint { X = 300, Y = 300, Interval = 2000, IsEnabled = true });
            RefreshPointList();
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
                // 双击编辑
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
            AppendLog($"✅ 已更新点位 {_selectedIndex + 1}: ({point.X}, {point.Y}) 间隔:{point.Interval}ms");
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
            AppendLog($"✅ 添加点位 ({x}, {y}) 间隔:{interval}ms");
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

        #region 坐标捕获

        private void btn_GetPos_Click(object sender, EventArgs e)
        {
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
            lbl_CaptureHint.Text = "🖱️ 请移动鼠标到目标位置，\r\n然后点击左键捕获坐标";
            lbl_CaptureHint.ForeColor = Color.Blue;
            this.Cursor = Cursors.Cross;

            StartMouseHook();
            AppendLog("进入坐标捕获模式 - 请在目标位置点击鼠标左键");
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

        private void CaptureCoordinate(int x, int y)
        {
            if (!_isCapturing) return;

            txt_X.Text = x.ToString();
            txt_Y.Text = y.ToString();
            lbl_CurrentPos.Text = $"X: {x}, Y: {y}";

            lbl_CurrentPos.BackColor = Color.LightGreen;
            Task.Delay(300).ContinueWith(_ =>
            {
                this.Invoke((MethodInvoker)delegate
                {
                    lbl_CurrentPos.BackColor = SystemColors.Control;
                });
            });

            AppendLog($"✅ 捕获坐标: ({x}, {y})");
            ExitCaptureMode();
        }

        #endregion

        #region 鼠标钩子

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

        #region 核心逻辑

        private async void btn_StartStop_Click(object sender, EventArgs e)
        {
            if (_isCapturing)
            {
                MessageBox.Show("请先退出坐标捕获模式！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

                // 获取启用的点位
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
                    await StartMultiClicking(enabledPoints, _cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    // 用户取消
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

        private async Task StartMultiClicking(List<ClickPoint> points, CancellationToken token)
        {
            AppendLog($"开始多位置自动点击 - 共 {points.Count} 个点位");

            int totalClicks = 0;

            while (!token.IsCancellationRequested)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    if (token.IsCancellationRequested) break;

                    var point = points[i];

                    // 移动到目标位置
                    SetCursorPos(point.X, point.Y);
                    await Task.Delay(10, token);

                    // 执行点击
                    mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                    await Task.Delay(30, token);
                    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);

                    totalClicks++;
                    _clickCount++;

                    this.Invoke((MethodInvoker)delegate
                    {
                        lbl_ClickCount.Text = $"点击次数: {_clickCount}";
                        // 高亮当前点击的点位
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

                    AppendLog($"🖱️ 点击点位 {i + 1}: ({point.X}, {point.Y}) 间隔:{point.Interval}ms");

                    // 等待该点位的间隔时间
                    await Task.Delay(point.Interval, token);
                }
            }

            AppendLog($"停止多位置点击 - 共点击 {_clickCount} 次");
        }

        #endregion

        #region 辅助方法

        private void UpdateUI()
        {
            btn_StartStop.Text = _isRunning ? "停止 (Alt+F2)" : "开始 (Alt+F2)";
            btn_StartStop.BackColor = _isRunning ? Color.Red : Color.Green;
            btn_StartStop.ForeColor = Color.White;

            // 启用/禁用控件
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
            if (e.KeyCode == Keys.F2 && e.IsAltPressed)
            {
                this.Invoke((MethodInvoker)delegate { btn_StartStop.PerformClick(); });
            }
            else if (e.KeyCode == Keys.F3 && e.IsAltPressed)
            {
                this.Invoke((MethodInvoker)delegate { btn_GetPos.PerformClick(); });
            }
        }

        private void timer_MousePos_Tick(object sender, EventArgs e)
        {
            GetCursorPos(out POINT point);
            lbl_CurrentPos.Text = $"X: {point.X}, Y: {point.Y}";
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
        private const int WM_LBUTTONDOWN = 0x0201;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

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

    #region 键盘钩子类

    public static class KeyboardHook
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

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
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;

                bool altPressed = (GetAsyncKeyState(0x12) & 0x8000) != 0;

                if (altPressed && key == Keys.F2)
                {
                    HotKeyPressed?.Invoke(null, new Form1.HotKeyEventArgs(Keys.F2, true));
                    return (IntPtr)1;
                }
                else if (altPressed && key == Keys.F3)
                {
                    HotKeyPressed?.Invoke(null, new Form1.HotKeyEventArgs(Keys.F3, true));
                    return (IntPtr)1;
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    }

    #endregion
}