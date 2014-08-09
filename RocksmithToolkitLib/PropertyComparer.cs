using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace RocksmithToolkitLib
{
    public class PropertyComparer<T> : IEqualityComparer<T>
    {

        private PropertyInfo[] _PropertyInfo;

        /// <summary>
        /// Creates a new instance of PropertyComparer.
        /// </summary>
        /// <param name="propertyName">The name of zero or more properties on type T to perform the comparison on.</param>
        public PropertyComparer(params string[] propertyName)
        {
            if (propertyName.Length == 0)
            {
                this._PropertyInfo = typeof(T).GetProperties();
            }
            else
            {
                //store a reference to the property info object for use during the comparison
                this._PropertyInfo = new PropertyInfo[propertyName.Length];
                for (var i = 0; i < this._PropertyInfo.Length; i++)
                {
                    _PropertyInfo[i] = typeof(T).GetProperty(propertyName[i], BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public);
                    if (_PropertyInfo[i] == null)
                    {
                        throw new ArgumentException(string.Format("{0} is not a property of type {1}.", propertyName[i], typeof(T)));
                    }
                }
            }
        }

        #region IEqualityComparer<T> Members

        public bool Equals(T x, T y)
        {
            //get the current value of the comparison property of x and of y
            for (var i = 0; i < this._PropertyInfo.Length; i++)
            {
                var type = _PropertyInfo[i].PropertyType;
                object xValue = _PropertyInfo[i].GetValue(x, null);
                object yValue = _PropertyInfo[i].GetValue(y, null);
                if(! Equals(xValue, yValue, type))
                    return false;
            }
            return true;
        }

        private static bool Equals(object x, object y, Type type) {
            if ((x == null) && y != null)
                return false;

                if (type.IsArray)
                {
                    var xx = ((Array)x);
                    var yy = ((Array)y);

                    if (xx.Length != yy.Length)
                        return false;

                    for (var j = 0; j < xx.Length; j++)
                    {
                        if (!Equals(xx.GetValue(j), yy.GetValue(j), type.GetElementType()))
                            return false;
                    }
                    return true;
                }
                if (!x.Equals( y))
                    return false;
            return true;
        }

        public int GetHashCode(T obj)
        {
            return 31;
        }
        #endregion

    }

}
