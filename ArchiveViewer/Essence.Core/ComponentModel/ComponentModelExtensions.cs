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
