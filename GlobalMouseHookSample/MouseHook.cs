using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static GloablMouseHookSample.Win32API;

namespace GloablMouseHookSample
{
    internal delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

    public class MouseHook
    {
        public event MouseEventHandler OnMouseGeneralEvent = delegate { };
        public event MouseEventHandler OnMouseLeftButtonClick = delegate { };
        public event MouseEventHandler OnMouseRightButtonClick = delegate { };
        public event MouseEventHandler OnMouseXButton1Click = delegate { };
        public event MouseEventHandler OnMouseXButton2Click = delegate { };

        private LowLevelMouseProc _proc;
        private IntPtr _hookID = IntPtr.Zero;
        private int processId = 0;
        private const int WH_MOUSE_LL = 14;

        public MouseHook() 
        {
            _proc = new LowLevelMouseProc(HookCallback);
        }

        public void Start()
        {
            _hookID = SetHook(_proc);
        }

        public void stop()
        {
            UnhookWindowsHookEx(_hookID);
        }

        private IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                processId = curProcess.Id;
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                MouseMessages mouseMessages = (MouseMessages)wParam;
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                POINT p = hookStruct.pt; // Mouse corrdinates
                MouseButtons clickButton;
                IntPtr window = GetForegroundWindow();
                int currentWindowProcessId;
                Win32API.GetWindowThreadProcessId(window, out currentWindowProcessId);
                // make sure foreground window process id is same as our application
                if (currentWindowProcessId == processId)
                {
                    RECT rect = new RECT();
                    bool result = GetWindowRect(window, out rect);
                    // make sure mouse position is within application window
                    if (result && (p.x > rect.Left && p.x < rect.Right) && (p.y > rect.Top && p.y < rect.Bottom) )
                    {
                        switch (mouseMessages)
                        {
                            case MouseMessages.WM_LBUTTONDOWN:
                                clickButton = MouseButtons.Left;
                                OnMouseLeftButtonClick(null, new MouseEventArgs(clickButton, 1, p.x, p.y, 0));
                                break;
                            case MouseMessages.WM_RBUTTONDOWN:
                                clickButton = MouseButtons.Right;
                                OnMouseRightButtonClick(null, new MouseEventArgs(clickButton, 1, p.x, p.y, 0));
                                break;
                            case MouseMessages.WM_XBUTTONDOWN:
                                if (GET_XBUTTON_WPARAM(hookStruct.mouseData) == XBUTTONS.XBUTTON1)
                                {
                                    clickButton = MouseButtons.XButton1;
                                    OnMouseXButton1Click(null, new MouseEventArgs(clickButton, 1, p.x, p.y, 0));
                                    // return 1 means not pass the event to next callback
                                    // return 0 will pass the event to next callback
                                    return new IntPtr(1);
                                }
                                else
                                {
                                    clickButton = MouseButtons.XButton2;
                                    OnMouseXButton2Click(null, new MouseEventArgs(clickButton, 1, p.x, p.y, 0));
                                    return new IntPtr(1);
                                }
                            default:
                                clickButton = MouseButtons.None;
                                OnMouseGeneralEvent(null, new MouseEventArgs(clickButton, 0, p.x, p.y, 0));
                                break;
                        }
                    }
                }
                
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private enum MouseMessages
        {
            WM_LBUTTONDOWN = 0x0201,
            //WM_LBUTTONUP = 0x0202,
            //WM_MOUSEMOVE = 0x0200,
            //WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            //WM_RBUTTONUP = 0x0205,
            WM_XBUTTONDOWN = 0x020B,
            //WM_XBUTTONUP = 0x020C
        }

    }
}
