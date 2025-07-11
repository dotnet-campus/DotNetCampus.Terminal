namespace DotNetCampus.Terminal.Modules.SshManagement;

/// <summary>
/// SSH密钥部署结果
/// </summary>
public class KeyDeploymentResult
{
    /// <summary>
    /// 部署是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 错误消息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 部署步骤列表
    /// </summary>
    public List<string> Steps { get; set; } = [];

    /// <summary>
    /// 是否需要密码短语
    /// </summary>
    public bool RequiresPassphrase { get; set; }

    /// <summary>
    /// 私钥文件路径
    /// </summary>
    public string? PrivateKeyPath { get; set; }

    /// <summary>
    /// 添加步骤记录
    /// </summary>
    /// <param name="step">步骤描述</param>
    public void AddStep(string step)
    {
        Steps.Add($"[{DateTime.Now:HH:mm:ss}] {step}");
    }

    /// <summary>
    /// 添加带时间戳的错误步骤
    /// </summary>
    /// <param name="error">错误描述</param>
    public void AddError(string error)
    {
        Steps.Add($"[{DateTime.Now:HH:mm:ss}] ❌ {error}");
        ErrorMessage = error;
        Success = false;
    }

    /// <summary>
    /// 添加成功步骤
    /// </summary>
    /// <param name="message">成功消息</param>
    public void AddSuccess(string message)
    {
        Steps.Add($"[{DateTime.Now:HH:mm:ss}] ✅ {message}");
    }

    /// <summary>
    /// 添加警告步骤
    /// </summary>
    /// <param name="warning">警告消息</param>
    public void AddWarning(string warning)
    {
        Steps.Add($"[{DateTime.Now:HH:mm:ss}] ⚠️ {warning}");
    }

    /// <summary>
    /// 获取格式化的步骤字符串
    /// </summary>
    /// <returns>所有步骤的合并字符串</returns>
    public string GetFormattedSteps()
    {
        return string.Join(Environment.NewLine, Steps);
    }

    /// <summary>
    /// 清除所有步骤
    /// </summary>
    public void ClearSteps()
    {
        Steps.Clear();
    }

    /// <summary>
    /// 重置结果状态
    /// </summary>
    public void Reset()
    {
        Success = false;
        ErrorMessage = null;
        Steps.Clear();
        RequiresPassphrase = false;
        PrivateKeyPath = null;
    }
}
