﻿// Decompiled with JetBrains decompiler
// Type: Essence.Core.ComponentModel.ICustomType
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

namespace Essence.Core.ComponentModel
{
  public interface ICustomType
  {
    string FullName { get; }

    string Name { get; }

    string DisplayName { get; }

    string Description { get; }

    bool IsSubclassOf(ICustomType customType);
  }
}