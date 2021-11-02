// Decompiled with JetBrains decompiler
// Type: Essence.Core.ComponentModel.ComponentModelExtensions
// Assembly: Essence.Core, Version=4.0.0.30534, Culture=neutral, PublicKeyToken=null
// MVID: EADC86D6-B806-4644-B499-D7F487995E73
// Assembly location: C:\Users\anon\Documents\GitHub\coh3-archive-viewer\CoH3.ArchiveViewer\bin\Release\AOE4\Essence.Core.dll

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Essence.Core.ComponentModel
{
  public static class ComponentModelExtensions
  {
    public static bool HasErrors(this IDataErrorInfo dataErrorInfo) => dataErrorInfo.GetErrors().Any<string>();

    public static IEnumerable<string> GetErrors(this IDataErrorInfo dataErrorInfo)
    {
      string error = dataErrorInfo.Error;
      if (!string.IsNullOrEmpty(error))
        yield return error;
      PropertyInfo[] propertyInfoArray = dataErrorInfo.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
      for (int index = 0; index < propertyInfoArray.Length; ++index)
      {
        PropertyInfo propertyInfo = propertyInfoArray[index];
        if (propertyInfo.CanWrite)
        {
          string str = dataErrorInfo[propertyInfo.Name];
          if (!string.IsNullOrEmpty(str))
            yield return str;
        }
      }
      propertyInfoArray = (PropertyInfo[]) null;
    }
  }
}
