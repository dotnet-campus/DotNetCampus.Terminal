using System.Net.Sockets;

namespace DotNetCampus.Terminal.Utils;

/// <summary>
/// 网络连接工具类
/// </summary>
public static class NetworkUtils
{
    /// <summary>
    /// 异步测试TCP连接
    /// </summary>
    /// <param name="hostName">主机名或IP地址</param>
    /// <param name="port">端口号</param>
    /// <param name="timeoutSeconds">连接超时时间（秒），默认10秒</param>
    /// <returns>连接成功返回true，否则返回false</returns>
    public static async Task<bool> TestTcpConnectionAsync(string hostName, int port, int timeoutSeconds = 10)
    {
        // 验证输入参数
        if (string.IsNullOrWhiteSpace(hostName))
        {
            return false;
        }

        if (port <= 0 || port > 65535)
        {
            return false;
        }

        if (timeoutSeconds <= 0)
        {
            timeoutSeconds = 10;
        }

        try
        {
            using var tcpClient = new TcpClient();

            // 设置连接超时时间
            var connectTask = tcpClient.ConnectAsync(hostName, port);
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(timeoutSeconds));

            var completedTask = await Task.WhenAny(connectTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                // 连接超时
                return false;
            }

            // 检查连接任务是否成功完成
            if (connectTask.IsFaulted)
            {
                return false;
            }

            // 验证连接状态
            return tcpClient.Connected;
        }
        catch (ArgumentException)
        {
            // 无效的主机名或端口
            return false;
        }
        catch (SocketException)
        {
            // 网络连接错误（如主机不可达、端口关闭等）
            return false;
        }
        catch (ObjectDisposedException)
        {
            // TcpClient已被释放
            return false;
        }
        catch (InvalidOperationException)
        {
            // TcpClient状态无效
            return false;
        }
        catch (Exception)
        {
            // 其他未预期的异常
            return false;
        }
    }
}
