using NHibernate.Cfg.MappingSchema;
using System.Collections.Generic;
using System.Linq;

namespace NHibernate.Mapping.ByCode.Impl
{
    public static class ParamUtil
    {
        public static HbmParam[] GetParams(object parameters)
        {
            IDictionary<string, object> parametersDictionary = parameters as IDictionary<string, object>;
            if (parametersDictionary == null)
            {
                parameters.GetType().GetProperties().ToDictionary(pi => pi.Name, pi => pi.GetValue(parameters, null));
            }

            return parametersDictionary
                .Select(p => new HbmParam() { name = p.Key, Text = new string[] { ReferenceEquals(p.Value, null) ? "null" : p.Value.ToString() } })
                .ToArray();
        }
    }
}
