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
      var error = dataErrorInfo.Error;
      if (!string.IsNullOrEmpty(error))
        yield return error;
      var propertyInfoArray = dataErrorInfo.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
      for (var index = 0; index < propertyInfoArray.Length; ++index)
      {
        var propertyInfo = propertyInfoArray[index];
        if (propertyInfo.CanWrite)
        {
          var str = dataErrorInfo[propertyInfo.Name];
          if (!string.IsNullOrEmpty(str))
            yield return str;
        }
      }
    }
  }
}
