// Copyright (c) .NET Core Community. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;

// ReSharper disable once CheckNamespace
namespace DotNetCore.CAP;

public class EFOptions
{
    public const string DefaultSchema = "cap";
    
    public const string DefaultPublishedTableName = "published";
    public const string DefaultReceivedTableName = "received";
    public const string DefaultLockTableName = "lock";

    /// <summary>
    /// Gets or sets the table name prefix to use when creating database objects.
    /// </summary>
    public string TableNamePrefix { get; set; } = DefaultSchema;
    
    /// <summary>
    /// Gets or sets the schema to use when creating published table.
    /// Default is <see cref="DefaultPublishedTableName" />.
    /// </summary>
    public string PublishedTableName { get; set; } = DefaultPublishedTableName;
    
    /// <summary>
    /// Gets or sets the schema to use when creating received table.
    /// Default is <see cref="DefaultReceivedTableName" />.
    /// </summary>
    public string ReceivedTableName { get; set; } = DefaultReceivedTableName;
    
    /// <summary>
    /// Gets or sets the schema to use when creating lock table.
    /// Default is <see cref="DefaultLockTableName" />.
    /// </summary>
    public string LockTableName { get; set; } = DefaultLockTableName;

    /// <summary>
    /// EF db context type.
    /// </summary>
    internal Type? DbContextType { get; set; }

    /// <summary>
    /// Data version
    /// </summary>
    internal string Version { get; set; } = "v1";
}