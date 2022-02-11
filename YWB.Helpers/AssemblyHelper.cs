using System.Reflection;

namespace YWB.Helpers;
public static class AssemblyHelper
{
    public static List<Type> GetTypesAssignableFrom<T>(this Assembly assembly) => assembly.GetTypesAssignableFrom(typeof(T));

    public static List<Type> GetTypesAssignableFrom(this Assembly assembly, Type compareType)
    {
        List<Type> ret = new();
        foreach (var type in assembly.DefinedTypes)
        {
            if (compareType.IsAssignableFrom(type) && compareType != type)
            {
                ret.Add(type);
            }
        }
        return ret;
    }
}
