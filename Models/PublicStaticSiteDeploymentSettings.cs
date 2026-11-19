using System;
using System.Collections.Generic;
using System.Text;

namespace UpdateManager.CoreLibrary.Models;

public sealed class PublicStaticSiteDeploymentSettings
{
    public required string RepositoryDirectory { get; init; }

    public string WorkflowFile { get; init; } = "deploy.yml";
}