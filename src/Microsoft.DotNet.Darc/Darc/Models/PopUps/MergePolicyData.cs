// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Microsoft.DotNet.Darc.Models.PopUps;

public class MergePolicyData
{
    [YamlMember(Alias = "Name")]
    public string Name { get; set; }
    [YamlMember(Alias = "Properties")]
    public Dictionary<string, object> Properties { get; set; }
}
