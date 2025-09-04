<#
.SYNOPSIS
    FXC 编译 FX/HLSL 文件成 CSO，支持自定义入口和输出文件名
#>

param(
    [string]$InputFile = "CustomBloomBlend.hlsl",   # 输入 HLSL/FX 文件
    [string]$EntryPoint = "main",                   # 编译入口函数
    [string]$OutputFile = "../CustomBloomBlend.cso"   # 输出 CSO 文件
)

# 检查 FXC 是否存在
$fxcPath = "fxc.exe"
if (-not (Get-Command $fxcPath -ErrorAction SilentlyContinue)) {
    Write-Error "找不到 fxc.exe，请确保已安装 Windows SDK 并在 PATH 中"
    exit 1
}

# 生成命令
$cmd = "$fxcPath /T fx_2_0 /E $EntryPoint /Fo `"$OutputFile`" `"$InputFile`""
Write-Host "执行命令: $cmd"

# 执行 FXC
Invoke-Expression $cmd

# 检查输出文件是否生成
if (Test-Path $OutputFile) {
    Write-Host "编译成功: $OutputFile"
} else {
    Write-Error "编译失败"
}
