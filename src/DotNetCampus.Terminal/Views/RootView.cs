using Terminal.Gui.Views;
using Terminal.Gui.Input;
using Terminal.Gui.App;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using DotNetCampus.Terminal.Modules.Configurations;
using Terminal.Gui.ViewBase;

namespace DotNetCampus.Terminal.Views
{
    /// <summary>
    /// 主界面视图，负责整体布局和顶级界面管理
    /// </summary>
    public partial class RootView
    {
        private readonly ConfigurationManager _configurationManager;
        private MenuBarv2 _menuBar;
        private StatusBar _statusBar;
        private FrameView _deviceListFrame;
        private FrameView _syncStatusFrame;
        private ListView _deviceListView;
        private ListView _syncStatusView;
        private TextField _commandInput;

        public RootView(ConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager ?? throw new ArgumentNullException(nameof(configurationManager));
            InitializeComponent();
            SetupMenuBar();
            SetupStatusBar();
            SetupMainLayout();
        }

        /// <summary>
        /// 设置顶部菜单栏
        /// </summary>
        [MemberNotNull(nameof(_menuBar))]
        private void SetupMenuBar()
        {
            _menuBar = new MenuBarv2([
                new MenuBarItemv2
                {
                    Title = "_文件",
                    PopoverMenu = new PopoverMenu([
                        new MenuItemv2
                        {
                            Title = "_新建设备配置",
                            Key = Key.N.WithCtrl,
                            Action = HandleNewDeviceConfig,
                        },
                        new MenuItemv2
                        {
                            Title = "_打开配置文件",
                            Key = Key.O.WithCtrl,
                            Action = HandleOpenConfig,
                        },
                        new Line(),
                        new MenuItemv2
                        {
                            Title = "_退出",
                            Key = Key.Q.WithCtrl,
                            Action = () => Application.RequestStop(),
                        },
                    ]),
                },
                new MenuBarItemv2
                {
                    Title = "_设备",
                    PopoverMenu = new PopoverMenu([
                        new MenuItemv2
                        {
                            Title = "_连接设备",
                            Key = Key.C.WithCtrl,
                            Action = HandleConnectDevice,
                        },
                        new MenuItemv2
                        {
                            Title = "_断开连接",
                            Key = Key.D.WithCtrl,
                            Action = HandleDisconnectDevice,
                        },
                        new Line(),
                        new MenuItemv2
                        {
                            Title = "_刷新设备列表",
                            Key = Key.F5,
                            Action = RefreshDeviceList,
                        },
                    ]),
                },
                new MenuBarItemv2
                {
                    Title = "_同步",
                    PopoverMenu = new PopoverMenu([
                        new MenuItemv2
                        {
                            Title = "_开始同步",
                            Key = Key.S.WithCtrl,
                            Action = HandleStartSync,
                        },
                        new MenuItemv2
                        {
                            Title = "_停止同步",
                            Key = Key.T.WithCtrl,
                            Action = HandleStopSync,
                        },
                    ]),
                },
                new MenuBarItemv2
                {
                    Title = "_帮助",
                    PopoverMenu = new PopoverMenu([
                        new MenuItemv2
                        {
                            Title = "_关于",
                            Action = ShowAboutDialog,
                        },
                    ]),
                },
            ]);

            Add(_menuBar);
        }

        /// <summary>
        /// 设置底部状态栏
        /// </summary>
        [MemberNotNull(nameof(_statusBar))]
        private void SetupStatusBar()
        {
            _statusBar = new StatusBar([
                new Shortcut
                {
                    Title = "_连接",
                    Key = Key.F2,
                    Action = HandleConnectDevice,
                },
                new Shortcut
                {
                    Title = "_同步",
                    Key = Key.F3,
                    Action = HandleStartSync,
                },
                new Shortcut
                {
                    Title = "_帮助",
                    Key = Key.F1,
                    Action = ShowAboutDialog,
                },
                new Shortcut
                {
                    Title = "_退出",
                    Key = Key.F10,
                    Action = () => Application.RequestStop(),
                },
            ]);

            Add(_statusBar);
        }

        /// <summary>
        /// 设置主要布局
        /// </summary>
        [MemberNotNull(nameof(_deviceListFrame), nameof(_syncStatusFrame), nameof(_deviceListView), nameof(_syncStatusView), nameof(_commandInput))]
        private void SetupMainLayout()
        {
            // 设备列表框架
            _deviceListFrame = new FrameView
            {
                Title = "设备列表",
                X = 0,
                Y = 1, // 菜单栏下方
                Width = Dim.Percent(50),
                Height = Dim.Fill(2), // 为状态栏和命令输入框留空间
            };

            _deviceListView = new ListView
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
            };

            _deviceListFrame.Add(_deviceListView);
            Add(_deviceListFrame);

            // 同步状态框架
            _syncStatusFrame = new FrameView
            {
                Title = "同步状态",
                X = Pos.Right(_deviceListFrame),
                Y = 1, // 菜单栏下方
                Width = Dim.Fill(),
                Height = Dim.Fill(2),
            };

            _syncStatusView = new ListView
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
            };

            _syncStatusFrame.Add(_syncStatusView);
            Add(_syncStatusFrame);

            // 全局命令输入框
            _commandInput = new TextField
            {
                X = 0,
                Y = Pos.Bottom(_deviceListFrame),
                Width = Dim.Fill(),
                Height = 1,
                Text = "输入命令...",
            };

            // 使用KeyDown事件处理回车键
            _commandInput.KeyDown += OnCommandInputKeyDown;
            Add(_commandInput);

            // 初始化数据
            RefreshDeviceList();
            RefreshSyncStatus();
        }

        /// <summary>
        /// 处理命令输入按键
        /// </summary>
        private void OnCommandInputKeyDown(object? sender, Key e)
        {
            if (e == Key.Enter)
            {
                var command = _commandInput.Text ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(command))
                {
                    ExecuteCommand(command);
                    _commandInput.Text = string.Empty;
                }
            }
        }

        /// <summary>
        /// 处理命令输入文本变化
        /// </summary>
        private void OnCommandInputTextChanging(object? sender, ResultEventArgs<string> e)
        {
            // 这个方法暂时保留，但不使用
        }

        /// <summary>
        /// 执行全局命令
        /// </summary>
        private void ExecuteCommand(string command)
        {
            try
            {
                // TODO: 实现命令解析和执行逻辑
                MessageBox.Query("命令执行", $"执行命令: {command}", "确定");
            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery("命令执行错误", ex.Message, "确定");
            }
        }

        #region 菜单事件处理

        private void HandleNewDeviceConfig()
        {
            MessageBox.Query("新建设备", "创建新设备配置功能待实现", "确定");
        }

        private void HandleOpenConfig()
        {
            MessageBox.Query("打开配置", "打开配置文件功能待实现", "确定");
        }

        private void HandleConnectDevice()
        {
            MessageBox.Query("连接设备", "连接设备功能待实现", "确定");
        }

        private void HandleDisconnectDevice()
        {
            MessageBox.Query("断开连接", "断开设备连接功能待实现", "确定");
        }

        private void HandleStartSync()
        {
            MessageBox.Query("开始同步", "开始文件同步功能待实现", "确定");
        }

        private void HandleStopSync()
        {
            MessageBox.Query("停止同步", "停止文件同步功能待实现", "确定");
        }

        private void ShowAboutDialog()
        {
            MessageBox.Query("关于",
                "DotNetCampus Terminal v1.0\n" +
                "基于 Terminal.Gui 的远程设备连接管理工具\n" +
                "使用 .NET 9.0 开发",
                "确定");
        }

        #endregion

        #region 数据刷新

        private void RefreshDeviceList()
        {
            // TODO: 从配置管理器获取设备列表
            var devices = new ObservableCollection<string>
            {
                "开发服务器 (192.168.1.100)",
                "测试服务器 (192.168.1.101)",
                "生产服务器 (192.168.1.102)",
            };

            _deviceListView.SetSource(devices);
        }

        private void RefreshSyncStatus()
        {
            // TODO: 获取实际同步状态
            var syncItems = new ObservableCollection<string>
            {
                "本地项目 -> 开发服务器 [空闲]",
                "配置文件 -> 测试服务器 [同步中]",
                "日志文件 <- 生产服务器 [完成]",
            };

            _syncStatusView.SetSource(syncItems);
        }

        #endregion
    }
}
