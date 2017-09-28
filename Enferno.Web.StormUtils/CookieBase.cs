using System;
using System.Linq;
using System.Web;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;
using Enferno.Public.Logging;

namespace Enferno.Web.StormUtils
{
    public abstract class CookieBase
    {
        private const string nullParameter = "!null!";

        public NameValueCollection Items = new NameValueCollection();

        private bool WasEncrypted { get; set; }

        public override string ToString()
        {
            var itemValues = Items.AllKeys.Select(key => string.Format("{0}={1}", key, Items[key])).ToList();
            return string.Join(":", itemValues);
        }

        public void SetDirty()
        {
            originalCookieValue = "";
        }

        private string originalCookieValue;

        public void LoadCookie(string name)
        {
            var cookie = StormContext.GetCookie(name);
            if (cookie != null)
            {
                WasEncrypted = IsEncrypted(cookie.Value);
                var data = Decrypt(cookie);
                Log.LogEntry.Categories(CategoryFlags.Debug).Message("Loaded cookie {0}: {1}", name, data).WriteVerbose();
            }
            StormContext.SetItem(name, this);
            originalCookieValue = ToString();
        }

        public void SaveCookie(string name, DateTime? expires)
        {
            try
            {
                if (ToString() == originalCookieValue && WasEncrypted == StormContext.Configuration.EncryptCookie) return;
                var data = Encrypt();
                var cookie = new HttpCookie(name, data);
                if(StormContext.IsSsl) { 
                    cookie.HttpOnly = true;
                    cookie.Secure = true;
                }
                if (expires.HasValue)
                {
                    if (expires.Value > DateTime.Now.AddYears(1))
                    {
                        expires = DateTime.Now.AddYears(1);
                    }
                    cookie.Expires = expires.Value;
                }
                StormContext.AddCookie(cookie);
                originalCookieValue = ToString(); // Avoid saving the same cookie twice
                Log.LogEntry.Categories(CategoryFlags.Debug).Message("Saved cookie {0}: {1}", name, data).WriteVerbose();
            }
            catch { }
        }

        private string Encrypt()
        {
            var parameters = "";
            foreach (var fi in this.GetType().GetFields())
            {
                object fieldObj = fi.GetValue(this);
                if (fieldObj == null)
                    fieldObj = nullParameter;
                if (fi.FieldType.Name == "NameValueCollection")
                {
                    foreach (string key in ((NameValueCollection)fieldObj).Keys)
                    {
                        parameters = String.Format("{0}{1}:{2}={3}&", parameters, fi.Name, key, HttpUtility.UrlEncode(((NameValueCollection)fieldObj)[key]));
                    }
                }
                else if (fieldObj is IList && !(fieldObj is String))
                {
                    foreach (object o in (IList)fieldObj)
                    {
                        parameters = String.Format("{0}{1}={2}&", parameters, fi.Name, HttpUtility.UrlPathEncode(o.ToString()));
                    }
                }
                else
                {
                    parameters = String.Format("{0}{1}={2}&", parameters, fi.Name, HttpUtility.UrlPathEncode(fieldObj.ToString()));
                }
            }
            if (parameters.Length > 0) parameters = parameters.TrimEnd('&');

            return StormContext.Configuration.EncryptCookie ? MachineKeyEncryption.Encode(parameters) : parameters;
        }

        private string Decrypt(HttpCookie cookie)
        {
            if(string.IsNullOrEmpty(cookie.Value)) return "";

            var encodedCookieValue = cookie.Value;
            var fields = GetType().GetFields().ToDictionary(fi => fi.Name);
            string parameters;
            try
            {
                parameters = WasEncrypted ? MachineKeyEncryption.Decode(encodedCookieValue) : encodedCookieValue;
            }
            catch
            {
                SaveCookie(cookie.Name, DateTime.Now.AddDays(-1D));
                return "Failed: null";
            }
            foreach (string parameter in parameters.Split('&'))
            {
                string[] nameValue = parameter.Split('=');
                if (nameValue.Length == 2)
                {
                    string name = nameValue[0];
                    if (name.Contains(":"))
                    { // NameValueCollection
                        string key = name.Split(':')[1];
                        name = name.Split(':')[0];
                        if (fields.ContainsKey(name))
                        {
                            var info = fields[name];
                            var type = info.FieldType;
                            var value = HttpUtility.UrlDecode(nameValue[1]);
                            var add = type.GetMethod("Add", new[] { typeof(string), typeof(string) });
                            add.Invoke(info.GetValue(this), new object[] { key, value });
                        }
                    }
                    else if (fields.ContainsKey(name))
                    {
                        var info = fields[name];
                        var type = info.FieldType;
                        var fieldObj = info.GetValue(this);
                        var value = HttpUtility.UrlDecode(nameValue[1]);
                        if (fieldObj is IList)
                        {
                            // Find Type of underlying objects and set "type" to this type instead. I have no idea how to find the type in any other way. This works anyway.
                            string typeString = fieldObj.GetType().FullName.Replace("System.Collections.Generic.List`1[[", "");
                            if (typeString.IndexOf(",") > 0)
                            {
                                typeString = typeString.Substring(0, typeString.IndexOf(","));
                            }
                            type = Type.GetType(typeString);
                        }
                        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            if (nameValue[1] == nullParameter)
                            {
                                value = null;
                            }
                            else
                            {
                                var typeCol = type.GetGenericArguments();
                                if (typeCol.Length > 0)
                                {
                                    type = typeCol[0];
                                }
                            }
                        }
                        if (type.IsEnum)
                        {
                            SetProperty(info, fieldObj, Enum.Parse(type, value));
                        }
                        else
                        {
                            if (value == null)
                            {
                                SetProperty(info, fieldObj, value);
                            }
                            else
                            {
                                if (type.Name == "String")
                                {
                                    SetProperty(info, fieldObj, value == nullParameter ? null : value);
                                }
                                else if (type.Name == "Guid")
                                {
                                    SetProperty(info, fieldObj, new Guid(value));
                                }
                                else
                                {
                                    try
                                    {
                                        object result = type.InvokeMember("Parse", BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.Static, null, null, new object[] { value });
                                        SetProperty(info, fieldObj, result);
                                    }
                                    catch
                                    {
                                        Trace.WriteLine(string.Format("The type {0} does not have a parse-method. Correct the code please...", type.FullName));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return parameters;
        }

        private static bool IsEncrypted(string encodedCookie)
        {
            if (string.IsNullOrWhiteSpace(encodedCookie)) return StormContext.Configuration.EncryptCookie;
            return !encodedCookie.Contains("=");
        }

        private void SetProperty(FieldInfo info, object fieldObj, object value)
        {
            var obj = fieldObj as IList;
            if (obj != null)
            {
                obj.Add(value);
            }
            else
            {
                info.SetValue(this, value);
            }
        }
    }
}
