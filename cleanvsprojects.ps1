function Get-ScriptDirectory
{
    $Invocation = (Get-Variable MyInvocation -Scope 1).Value;
    if($Invocation.PSScriptRoot)
    {
        $Invocation.PSScriptRoot;
    }
    Elseif($Invocation.MyCommand.Path)
    {
        Split-Path $Invocation.MyCommand.Path
    }
    else
    {
        $Invocation.InvocationName.Substring(0,$Invocation.InvocationName.LastIndexOf("\"));
    }
}


## https://blogs.msdn.microsoft.com/rbrundritt/2014/09/18/cleaning-up-visual-studio-project-folders-using-powershell/
$root = Get-ScriptDirectory
write-host "Cleaning:" $root
Get-ChildItem $root -include bin,obj,bld,Backup,_UpgradeReport_Files,Debug,Release,ipch -Recurse | Where-Object {$_.FullName -notlike "*node_modules*" -and $_.FullName -notlike "*.git*"} | foreach{ "Removing: $($_.FullName)"; Remove-Item $_.fullname -Force -Recurse}