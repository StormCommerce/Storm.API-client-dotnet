using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Enferno.Web.StormUtils {
	/// <summary>
	/// Is used to make urls with parameters to simplify navigation on site
	/// </summary>
	public class Link {
		string _appRelativeFilePath;

		/// <summary>
		/// Creates an empty link object 
		/// </summary>
		public Link() {
			_parameters = new Dictionary<string, string>(3);
		}

		/// <summary>
		/// Public constructor that sets the AppRelativeFilePath of the Link object
		/// </summary>
		/// <param name="url">The url with relative filePath and query to set the object to</param>
		public Link(object url) : this(url != null ? url.ToString() : null) { }

		/// <summary>
		/// Public constructor that sets the AppRelativeFilePath of the Link object
		/// </summary>
		/// <param name="url">The url with relative filePath and query to set the object to</param>
		public Link(string url)
			: this() {
			if(url != null) {
				if(url.Contains("?")) {
					_appRelativeFilePath = url.Substring(0, url.IndexOf('?'));
					string query = url.Substring(url.IndexOf('?') + 1);
					foreach(string p in query.Split('&')) {
						string[] nameValue = p.Split('=');
						if(nameValue.Length > 1) {
							this.Set(nameValue[0], nameValue[1]);
						} else
						{
						    _appRelativeFilePath = String.Concat(_appRelativeFilePath, _appRelativeFilePath.Contains("?") ? "&" : "?", nameValue[0]);
						}
					}
				} else {
					_appRelativeFilePath = url;
				}
			}
		}

        public static string ImageUrl(string image)
        {
            return ImageUrl(image, "jpg");
        }
        public static string ImageUrl(string image, string extension)
        {
            Guid key;
            if (Guid.TryParse(image, out key)) return string.Format("{0}{1}.{2}", StormContext.Configuration.ImageUrl, image, extension);
            return image;
        }

        public static string ImageUrl(string image, int? preset) 
        {
            return ImageUrl(image, preset, "jpg");
        }

        public static string ImageUrl(string image, int? preset, string extension)
        {
            Guid key;
            if (Guid.TryParse(image, out key))
            {
                return (ImageUrl(key, preset, extension));
            }
            if (!image.Contains("?preset=") && preset.HasValue)
            {
                image += String.Format("?preset={0}", preset);
            }
            return image;
        }

        public static string ImageUrl(Guid? imageKey, int? preset)
        {
            return ImageUrl(imageKey, preset, "jpg");
        }

        public static string ImageUrl(Guid? imageKey, int? preset, string extension)
        {
            var p = preset.HasValue ? String.Format("?preset={0}", preset) : ""; 
            if (imageKey.HasValue)
            {
                return string.Format("{0}{1}.{2}{3}", StormContext.Configuration.ImageUrl, imageKey, extension,
                    p);
            }
            return null;
        }

	    /// <summary>
		/// Returns how many parameters this link has.
		/// </summary>
		public int Count {
			get { return _parameters.Count; }
		}

		/// <summary>
		/// The virtual application relative file path without parameters
		/// </summary>
		public string AppRelativeFilePath {
			get { return _appRelativeFilePath; }
			set { _appRelativeFilePath = value; }
		}
		/// <summary>
		/// The actual file path without parameters
		/// </summary>
		public string FilePath {
			get { return ResolveUrl(_appRelativeFilePath); }
		}

		/// <summary>
		/// Gets a string with formatted Url and all parameters 
		/// </summary>
		/// <returns>String with url</returns> 
		public string Url {
			get { return ResolveUrl(AppRelativeUrl); }
		}

		public string FullUrl {
			get {
				string port = "";
				int p = HttpContext.Current.Request.Url.Port;
				if(p != 80 && p != 443) {
					port = String.Concat(":", p);
				}
				return String.Concat(HttpContext.Current.Request.Url.Scheme, "://", HttpContext.Current.Request.Url.Host.TrimEnd('/'), port, Url);
			}
		}

		/// <summary>
		/// Gets a string with type suffix  of link
		/// </summary>
		/// <returns>String with type</returns> 
		public string Type {
			get {
				int index = AppRelativeFilePath.LastIndexOf('.');
				if(index > 0 && AppRelativeFilePath.Length > index) {
					return AppRelativeFilePath.Substring(index + 1);
				}
				return null;
			}
		}

		/// <summary>
		/// Gets a string with formatted Url and all parameters 
		/// </summary>
		/// <returns>String with url</returns> 
		public override string ToString() {
			return Url;
		}

		/// <summary>
		/// Gets an array of parameter names
		/// </summary>
		/// <returns>String with url</returns> 
		public string[] Keys {
			get {
				var keys = new List<string>(_parameters.Count);
			    keys.AddRange(_parameters.Keys);
			    return keys.ToArray();
			}
		}

		/// <summary>
		/// Returns a Link object filled from the current request FilePath
		/// </summary>
		public static Link CurrentInternal {
			get {
				var link = new Link
				{
				    AppRelativeFilePath = HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath
				};
			    foreach (var key in HttpContext.Current.Request.QueryString.AllKeys.Where(key => key != null))
				{
				    link._parameters.Add(key, HttpContext.Current.Request.QueryString[key]);
				}
				return link;
			}
		}

		/// <summary>
		/// Returns a Link object filled from the current request FilePath
		/// </summary>
		public static Link Current {
			get {
				var link = new Link();
				var server = HttpContext.Current.Server;
				var request = HttpContext.Current.Request;
				var url = request.RawUrl;
				if(url.StartsWith(request.Url.Scheme)) {
					url = url.Substring(request.Url.Scheme.Length + 3);
				}
				if(url.StartsWith(request.Url.Host)) {
					url = url.Substring(request.Url.Host.Length);
				}
				if(url.StartsWith(":")) {
					url = url.Substring(request.Url.Port.ToString().Length + 1);
				}
				if(url.StartsWith(request.ApplicationPath)) {
					url = url.Substring(request.ApplicationPath.Length);
				}
				if(url.StartsWith("/")) {
					url = url.Substring(1);
				}
				string query = "";
				if(url.Contains("?")) {
					query = url.Substring(url.IndexOf('?') + 1);
					url = url.Substring(0, url.IndexOf('?'));
					foreach(string param in query.Split('&')) {
						string[] keyValue = param.Split('=');
						string key = keyValue[0];
						if(!String.IsNullOrEmpty(key)) {
							string value = null;
							if(keyValue.Length > 1) {
								value = server.UrlDecode(keyValue[1]);
							}
							link.Set(key, value);
						}
					}
				}
				link.AppRelativeFilePath = String.Concat("~/", url);
				return link;
			}
		}

	    readonly Dictionary<string, string> _parameters;
		/// <summary>
		/// The query string parameters
		/// </summary>
		public string this[string key] {
			get {
				if(_parameters.ContainsKey(key))
					return _parameters[key];
				return null;
			}
			set { _parameters[key] = value; }
		}

		public Link SetFromQuery() {
			foreach(string key in HttpContext.Current.Request.QueryString.AllKeys) {
				if(key != null && key != "emi") {
					this._parameters[key] = HttpContext.Current.Request.QueryString[key];
				}
			}
			return this;
		}

		public Link Set(string key, string value) {
			if(value != null) {
				this._parameters[key] = value;
			} else {
				this.Clear(key);
			}
			return this;
		}

		public Link Set(string key, object value) {
			if(value != null) {
				this._parameters[key] = value.ToString();
			} else {
				this.Clear(key);
			}
			return this;
		}

		public Link SetAnchor(string anchor) {
			string[] parts = this.Url.Split('#');
			return new Link(String.Concat(parts[0], "#", anchor));
		}

		public Link Clear() {
			_parameters.Clear();
			return this;
		}

		public Link Clear(string key) {
			if(key != null) {
				if(key.StartsWith("*") || key.EndsWith("*")) {
					var removeKeys = this._parameters.Keys.Where(find => (key.StartsWith("*") && find.EndsWith(key.Remove(0, 1))) || (key.EndsWith("*") && find.StartsWith(key.Remove(key.Length - 1, 1)))).ToList();
				    foreach(string remove in removeKeys) {
						this._parameters.Remove(remove);
					}
				} else {
					this._parameters.Remove(key);
				}
			}
			return this;
		}

		/// <summary>
		/// Gets a string with all parameters 
		/// </summary>
		/// <returns>String with parameters</returns> 
		public string QueryString {
			get {
				if(_parameters.Keys.Count > 0) {
					var sb = new StringBuilder();
					var server = HttpContext.Current.Server;
					foreach(var key in _parameters.Keys) {
						sb.AppendFormat("{0}={1}&", server.UrlEncode(key), server.UrlEncode(_parameters[key]));
					}
					if(sb.Length > 0) {
						sb.Remove(sb.Length - 1, 1); // Remove last '&'
					}
					return sb.ToString();
				}
				return String.Empty;
			}
		}

		/// <summary>
		/// Gets a string with formatted application relative Url and all parameters 
		/// </summary>
		/// <returns>String with application relative url</returns> 
		public string AppRelativeUrl {
			get {
				if(_appRelativeFilePath == null) {
					return null;
				}
				var sb = new StringBuilder();
			    sb.Append(AppRelativeFilePath);
				if(_parameters.Count > 0)
				{
				    sb.Append(_appRelativeFilePath.Contains("?") ? "&" : "?");
				    sb.Append(QueryString);
				}
			    return sb.ToString();
			}
		}

		/// <summary>
		/// Makes a server transfer to the url
		/// </summary>
		public void Transfer() {
			HttpContext.Current.Server.Transfer(ToString(), true);
		}

		/// <summary>
		/// Makes a redirect to the url
		/// </summary>
		public void Redirect() {
			Redirect(false);
		}

		/// <summary>
		/// Makes a redirect to the url
		/// <param name="isPermanent">If this is a permanent redirect: StatusCode = 301.</param>
		/// </summary>
		public void Redirect(bool isPermanent)
		{
		    if (HttpContext.Current == null) return;

			if(isPermanent)
            {
				HttpContext.Current.Response.StatusCode = 301;
				HttpContext.Current.Response.AddHeader("Location", ToString());
                HttpContext.Current.ApplicationInstance.CompleteRequest();
			} 
            else
            {
                HttpContext.Current.Response.Redirect(ToString(), false);
                HttpContext.Current.ApplicationInstance.CompleteRequest();
			}
		}

		/// <summary>
		/// Returns a site relative HTTP path from a partial path starting out with a ~.
		/// Same syntax that ASP.Net internally supports but this method can be used
		/// outside of the Page framework.
		/// 
		/// Works like Control.ResolveUrl including support for ~ syntax
		/// but returns an absolute URL.
		/// </summary>
		/// <param name="originalUrl">Any Url including those starting with ~</param>
		/// <returns>relative url</returns>
		private static string ResolveUrl(string originalUrl) {
			if(string.IsNullOrEmpty(originalUrl))
				return originalUrl;

			// *** Absolute path - just return
			if(originalUrl.IndexOf("://") != -1)
				return originalUrl;

			// *** We don't start with the '~' -> we don't process the Url
			if(!originalUrl.StartsWith("~")) {
				return originalUrl;
			}

			// *** Fix up path for ~ root app dir directory
			// VirtualPathUtility blows up if there is a 
			// query string, so we have to account for this.
			var queryStringStartIndex = originalUrl.IndexOf('?');
		    if (queryStringStartIndex == -1) return VirtualPathUtility.ToAbsolute(originalUrl);

		    var queryString = originalUrl.Substring(queryStringStartIndex);
		    var baseUrl = originalUrl.Substring(0, queryStringStartIndex);

		    return string.Concat(VirtualPathUtility.ToAbsolute(baseUrl), queryString);
		}
	}
}
